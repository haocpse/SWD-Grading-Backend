# API Upload Student Solutions - Hướng dẫn sử dụng

## 📦 Tổng quan
API này cho phép upload **1 file ZIP duy nhất** chứa toàn bộ folder `Student_Solutions` với tất cả bài làm của sinh viên.

## 📁 Cấu trúc ZIP File

```
Student_Solutions.zip           ← File này sẽ upload
└── Student_Solutions/          ← Folder chính (hoặc trực tiếp các student folders)
    ├── Anhddhse170283/
    │   ├── 0/
    │   │   ├── solution.zip
    │   │   └── SWD392_PE_SU25_SE170283.docx (optional)
    │   └── history.dat
    ├── AnhKVSE182347/
    │   ├── 0/
    │   │   └── solution.zip
    │   └── history.dat
    └── ...
```

**Chú ý:** ZIP có thể có hoặc không có folder `Student_Solutions` ở root. Backend sẽ tự động detect.

## 🚀 API Endpoint

### Upload Student Solutions ZIP

**Method:** `POST`

**URL:** `/api/exam-upload/upload-solutions/{examId}`

**Parameters:**
- `examId` (path): ID của exam trong database

**Request:**
- Content-Type: `multipart/form-data`
- Field name: `file` (single file)
- File type: `.zip` only
- Max size: 500MB (configurable)

**Response - Success:**
```json
{
  "examZipId": 1,
  "status": "Processing",
  "message": "File 'Student_Solutions.zip' uploaded successfully and processing has started. Check status using the provided ExamZipId."
}
```

**Response - Error:**
```json
{
  "examZipId": 0,
  "status": "Error",
  "message": "File type .rar is not allowed. Only .zip files are accepted."
}
```

## 📝 Cách test với Postman

### Bước 1: Chuẩn bị

1. Tạo Exam trong database:
```sql
INSERT INTO Exam (ExamCode, Title, Description, CreatedAt, UpdatedAt)
VALUES ('SWD392', 'Software Design Exam', 'Final exam', GETDATE(), GETDATE());
```

2. Zip folder Student_Solutions thành file `.zip`:
   - Windows: Right-click → "Compress to ZIP"
   - Mac: Right-click → "Compress"
   - Command line: `zip -r Student_Solutions.zip Student_Solutions/`

### Bước 2: Upload với Postman

1. **Tạo request:**
   - Method: `POST`
   - URL: `http://localhost:5000/api/exam-upload/upload-solutions/1`
   - (Thay `1` bằng ID exam thật)

2. **Setup Body:**
   - Chọn tab "Body"
   - Chọn "form-data"
   - Key: `file` (chọn type: File)
   - Value: Click "Select Files" → chọn `Student_Solutions.zip`

3. **Send request**

4. **Copy ExamZipId** từ response để check status

### Bước 3: Check Processing Status

**URL:** `GET /api/exam-upload/status/{examZipId}`

Example: `http://localhost:5000/api/exam-upload/status/1`

**Response:**
```json
{
  "examZipId": 1,
  "parseStatus": "DONE",  // hoặc "PENDING", "ERROR"
  "processedCount": 305,
  "totalCount": 309,
  "errors": [
    "Error processing studentXYZ: folder '0' not found"
  ],
  "failedStudents": ["se170001", "se170002"],
  "parseSummary": "Found 309 student folders\n..."
}
```

## 🔄 Processing Flow

### Phase 1: Upload
1. API nhận ZIP file
2. Validate: file type (.zip), size (<500MB), exam exists
3. Save ZIP vào temp storage
4. Create ExamZip record với status PENDING
5. Return ExamZipId ngay lập tức

### Phase 2: Background Processing
1. BackgroundJobService detect ExamZip PENDING (mỗi 10s)
2. Extract ZIP file
3. Tìm folder Student_Solutions (auto-detect)
4. Scan tất cả student folders
5. Với mỗi student folder:
   - Parse student code từ folder name
   - Check/Create Student record
   - Tìm folder `0/`
   - Process files:
     - Upload `solution.zip` (nếu có) lên S3
     - Upload `.docx` files trực tiếp lên S3
     - Extract `solution.zip` để tìm thêm `.docx`
     - Parse text từ tất cả `.docx` files
   - Save vào DocFile với ParsedText
6. Update ExamZip status → DONE/ERROR
7. Cleanup temp files và ZIP file

## 📊 Database Records

### Student
- Auto-created nếu chưa tồn tại
- StudentCode: parsed từ folder name
  - `Anhddhse170283` → `se170283`
  - `AnhKVSE182347` → `se182347`
- FullName: tên folder (ban đầu)

### ExamZip
- ZipName: tên file gốc
- ZipPath: path tới ZIP file temp
- ParseStatus: PENDING → DONE/ERROR
- ParseSummary: chi tiết quá trình xử lý

### ExamStudent
- Status: NOT_FOUND/PARSED/GRADED
- Note: thông tin xử lý

### DocFile
- FileName: tên file .docx
- FilePath: S3 URL
- **ParsedText**: nội dung text extracted từ Word
- ParseStatus: NOT_FOUND/OK/ERROR
- ParseMessage: error message nếu có

## 🌐 S3 File Structure

```
s3://your-bucket/
└── SWD392/                    ← ExamCode
    ├── se170283/              ← StudentCode
    │   ├── solution.zip
    │   └── SWD392_PE_SU25_SE170283.docx
    └── se182347/
        ├── solution.zip
        └── assignment.docx
```

## ⚙️ Configuration

### appsettings.json

```json
{
  "AWS": {
    "BucketName": "your-bucket-name",
    "Region": "ap-southeast-1",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key"
  },
  "FileUpload": {
    "MaxFileSizeMB": 500,
    "AllowedExtensions": [".zip"],
    "TempStoragePath": "temp/uploads"
  }
}
```

## ❗ Error Handling

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| "No file uploaded" | Không có file trong request | Check Postman field name là `file` |
| "File type .rar is not allowed" | Upload sai format | Chỉ accept .zip |
| "File size exceeds maximum" | File quá lớn | Compress hoặc tăng MaxFileSizeMB |
| "Exam with ID X not found" | ExamId không tồn tại | Tạo Exam trước |
| "folder '0' not found" | Student folder thiếu folder "0" | Check structure |
| "solution.zip not found" | Không có solution.zip | Có thể có .docx trực tiếp |

## 🧪 Testing Checklist

- [ ] Upload ZIP với examId hợp lệ
- [ ] Verify response có ExamZipId
- [ ] Call status API để check progress
- [ ] Wait cho ParseStatus = DONE
- [ ] Check database records:
  - [ ] ExamZip created
  - [ ] Students auto-created
  - [ ] ExamStudent records
  - [ ] DocFile records với ParsedText
- [ ] Verify S3 bucket có files
- [ ] Test edge cases:
  - [ ] Student không có folder "0"
  - [ ] Student không có solution.zip
  - [ ] Student có .docx trực tiếp
  - [ ] ZIP có folder Student_Solutions ở root
  - [ ] ZIP không có folder Student_Solutions (student folders ở root)

## 🐛 Troubleshooting

### Processing stuck ở PENDING
- Check BackgroundJobService có running không (check logs)
- Verify temp storage có write permissions
- Check database connection

### Files không upload lên S3
- Verify AWS credentials
- Check bucket name và region
- Verify bucket policy allows upload
- Check S3 service logs

### ParsedText là null
- Check Word file có bị corrupt không
- Verify DocumentFormat.OpenXml package installed
- Check logs cho errors khi parse Word

### Student code không đúng
- Check folder naming convention
- Regex pattern: `(se|SE)(\d+)`
- Nếu không match → dùng full folder name

## 📞 Support

Nếu có vấn đề:
1. Check application logs
2. Check BackgroundJobService logs
3. Verify database records
4. Check S3 bucket
5. Review ParseSummary trong ExamZip

## 🎯 Next Steps After Upload

1. Monitor processing status
2. Review failed students (if any)
3. Check DocFile.ParsedText cho grading
4. Start grading process với AI/Manual

