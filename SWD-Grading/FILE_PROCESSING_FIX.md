# File Processing Fix - Background Job Service

## Vấn đề

Sau khi upload file ZIP, file **không được tự động giải nén và xử lý**. File chỉ được lưu vào database với status `PENDING` nhưng không có service nào xử lý chúng.

### Nguyên nhân

`BackgroundJobService` đã bị comment out trong `Program.cs` (dòng 165):

```csharp
// Background job service removed - using on-demand indexing instead
```

Service này chịu trách nhiệm:
1. **Poll** mỗi 10 giây để tìm ExamZips với status `PENDING`
2. **Tự động giải nén** và xử lý các file ZIP đã upload
3. **Extract** text từ Word documents
4. **Upload** files lên S3
5. **Cập nhật** status thành `DONE` hoặc `ERROR`

## Giải pháp đã áp dụng

Đã đăng ký lại `BackgroundJobService` trong `Program.cs`:

```csharp
// Register BackgroundJobService to automatically process uploaded ZIP files
builder.Services.AddHostedService<BackgroundJobService>();
```

## Cách hoạt động

### 1. Upload Flow
```
User uploads ZIP → ExamUploadService.InitiateUploadAsync()
    ↓
Save ZIP to temp storage
    ↓
Create ExamZip record with ParseStatus = PENDING
    ↓
Return ExamZipId immediately
```

### 2. Background Processing Flow
```
BackgroundJobService (every 10 seconds)
    ↓
Query ExamZips with ParseStatus = PENDING
    ↓
For each pending ExamZip:
    → FileProcessingService.ProcessStudentSolutionsAsync()
        → Extract ZIP file
        → Find Student_Solutions folder
        → For each student folder:
            → Find folder "0"
            → Process solution.zip and .docx files
            → Upload to S3
            → Extract text from Word documents
            → Save DocFile records
            → Update ExamStudent status
        → Update ExamZip status → DONE/ERROR
        → Cleanup temp files
```

### 3. BackgroundJobService chi tiết

**File:** `BLL/Service/BackgroundJobService.cs`

Thực hiện 2 background tasks:

#### Task 1: ProcessPendingExamZipsAsync()
- Tìm tất cả ExamZip với `ParseStatus = PENDING`
- Gọi `FileProcessingService.ProcessStudentSolutionsAsync()` cho mỗi file
- Log success/error cho mỗi file được xử lý

#### Task 2: ProcessPendingEmbeddingsAsync()
- Tìm các DocFile đã được parse thành công (`ParseStatus = OK`)
- Tạo embeddings để phục vụ plagiarism detection
- Giới hạn 10 documents mỗi lần để tránh overload

**Poll Interval:** 10 giây (có thể điều chỉnh trong constructor)

## Testing

### 1. Khởi động lại ứng dụng

```bash
cd SWD-Grading
dotnet run
```

Bạn sẽ thấy log:

```
Background Job Service started
```

### 2. Upload file ZIP

```bash
POST /api/exams/{examId}/upload-zip
Content-Type: multipart/form-data
file: [Student_Solutions.zip]
```

Response:
```json
{
  "examZipId": 123,
  "status": "Processing",
  "message": "File uploaded successfully and processing has started..."
}
```

### 3. Check logs

Sau ~10 giây, bạn sẽ thấy:

```
Found 1 pending exam zip(s) to process
Processing ExamZip ID: 123
Successfully processed ExamZip ID: 123
```

### 4. Check processing status

```bash
GET /api/exam-zips/{examZipId}/check-status
```

Response:
```json
{
  "examZipId": 123,
  "parseStatus": "DONE",
  "processedCount": 305,
  "totalCount": 309,
  "errors": [],
  "failedStudents": [],
  "parseSummary": "Found 309 student folders\n..."
}
```

## Các trường hợp lỗi thường gặp

### 1. File không được xử lý sau 10+ giây

**Kiểm tra:**
- BackgroundJobService có chạy không? Check logs
- ExamZip có status PENDING không? Query database
- Có exception nào trong logs không?

### 2. ParseStatus = ERROR

**Nguyên nhân thường gặp:**
- ZIP file không tồn tại tại đường dẫn đã lưu
- Folder "0" không tồn tại trong student folder
- Không tìm thấy file .docx hoặc solution.zip
- Lỗi khi upload lên S3 (check AWS credentials)
- Lỗi khi parse Word document (file corrupt)

**Xem chi tiết:** Check `ParseSummary` trong ExamZip record

### 3. Một số student không được xử lý

**Kiểm tra ParseSummary:**
```
Found 309 student folders
Error processing studentXYZ: Student with code 'studentXYZ' not found in database
Error processing studentABC: Folder '0' not found
...
Total: 309
Success: 305
Errors: 4
```

**Giải pháp:**
- Đảm bảo student code trong folder name khớp với database
- Đảm bảo mỗi student folder có folder "0" bên trong

## Cấu trúc thư mục yêu cầu

```
Student_Solutions.zip
└── Student_Solutions/
    ├── Anhddhse170283/
    │   └── 0/
    │       ├── solution.zip (optional)
    │       └── answer.docx (optional - direct .docx files)
    ├── Nguyenvhse170284/
    │   └── 0/
    │       └── solution.zip
    └── ...
```

**Lưu ý:**
- Folder "0" là **bắt buộc**
- Phải có ít nhất 1 trong 2:
  - File `solution.zip` chứa .docx files
  - File .docx trực tiếp trong folder "0"

## Performance

### Thông số

- **Poll interval:** 10 giây
- **Processing time:** ~5-15 giây cho 300-500 students
- **Max file size:** 500MB
- **S3 upload:** Parallel (multiple files cùng lúc)
- **Embedding generation:** 10 documents mỗi poll (rate limiting)

### Optimization tips

1. **Tăng poll interval** nếu server load cao:
   ```csharp
   private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30); // từ 10s → 30s
   ```

2. **Process multiple ExamZips parallel** (cẩn thận với database connection pool):
   ```csharp
   var tasks = pendingExamZips.Select(ez => 
       fileProcessingService.ProcessStudentSolutionsAsync(ez.Id));
   await Task.WhenAll(tasks);
   ```

3. **Increase embedding batch size** nếu có nhiều documents:
   ```csharp
   var recentDocFiles = await unitOfWork.DocFileRepository
       .GetRecentlyParsedDocFilesAsync(limit: 50); // từ 10 → 50
   ```

## Related Files

- `BLL/Service/BackgroundJobService.cs` - Background job implementation
- `BLL/Service/FileProcessingService.cs` - File extraction & processing logic
- `BLL/Service/ExamUploadService.cs` - Upload initiation
- `DAL/Repository/ExamZipRepository.cs` - ExamZip data access
- `DAL/Repository/DocFileRepository.cs` - DocFile data access
- `SWD-Grading/Program.cs` - Service registration

## Date Fixed

November 16, 2025


