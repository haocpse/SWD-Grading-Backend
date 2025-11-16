# Student Solutions Upload & Processing Feature

## Tổng quan
Feature này cho phép giảng viên upload folder `Student_Solutions` chứa bài làm của sinh viên. Hệ thống sẽ tự động:
- Giải nén các file ZIP lồng nhau
- Upload files lên AWS S3
- Parse nội dung file Word và lưu vào database
- Tự động tạo Student nếu chưa tồn tại
- Xử lý bất đồng bộ với background service

## Cấu trúc folder Student_Solutions
```
Student_Solutions.zip
├── Anhddhse170283/
│   └── 0/
│       └── solution.zip  → chứa report.docx
├── Anhlmnse184425/
│   └── 0/
│       └── solution.zip  → chứa assignment.docx
└── ...
```

## Cấu hình

### 1. Cập nhật appsettings.json

```json
{
  "AWS": {
    "BucketName": "your-s3-bucket-name",
    "Region": "ap-southeast-1",
    "AccessKey": "YOUR_AWS_ACCESS_KEY",
    "SecretKey": "YOUR_AWS_SECRET_KEY"
  },
  "FileUpload": {
    "MaxFileSizeMB": 500,
    "AllowedExtensions": [".zip"],
    "TempStoragePath": "temp/uploads"
  }
}
```

### 2. Tạo Exam trước khi upload

Cần đảm bảo có record Exam trong database với `ExamCode` phù hợp. Ví dụ:
```sql
INSERT INTO Exam (ExamCode, Title, Description, CreatedAt, UpdatedAt)
VALUES ('SWD391', 'Software Design Exam', 'Final exam for SWD course', GETDATE(), GETDATE());
```

## API Endpoints

### 1. Upload Student Solutions

**Endpoint:** `POST /api/exam-upload/upload-solutions/{examId}`

**Parameters:**
- `examId` (path): ID của exam
- `file` (form-data): ZIP file chứa Student_Solutions
- `examCode` (form-data): Exam code (vd: "SWD391")

**Request Example (Postman):**
```
POST http://localhost:5000/api/exam-upload/upload-solutions/1
Content-Type: multipart/form-data

file: [select Student_Solutions.zip]
examCode: SWD391
```

**Response:**
```json
{
  "examZipId": 1,
  "status": "Processing",
  "message": "File uploaded successfully and processing has started. Check status using the provided ExamZipId."
}
```

### 2. Check Processing Status

**Endpoint:** `GET /api/exam-upload/status/{examZipId}`

**Parameters:**
- `examZipId` (path): ID trả về từ API upload

**Request Example:**
```
GET http://localhost:5000/api/exam-upload/status/1
```

**Response:**
```json
{
  "examZipId": 1,
  "parseStatus": "DONE",
  "processedCount": 15,
  "totalCount": 20,
  "errors": [
    "Error processing Dangnpmse184193: solution.zip not found in folder '0'"
  ],
  "failedStudents": [
    "se184193",
    "se184194"
  ],
  "parseSummary": "Found 20 student folders\nProcessing complete:\nTotal: 20\nSuccess: 15\nErrors: 5"
}
```

## Parse Status Flow

### ExamZip.ParseStatus
- `PENDING`: Đang chờ xử lý
- `DONE`: Hoàn thành
- `ERROR`: Có lỗi xảy ra

### ExamStudent.Status
- `NOT_FOUND`: Không tìm thấy file hoặc có lỗi
- `PARSED`: Đã parse thành công
- `GRADED`: Đã chấm điểm

### DocFile.ParseStatus
- `NOT_FOUND`: Không tìm thấy file Word
- `OK`: Parse thành công
- `ERROR`: Lỗi khi parse

## S3 File Structure

Files sẽ được upload lên S3 theo cấu trúc:
```
s3://your-bucket/
├── SWD391/
│   ├── se170283/
│   │   ├── solution.zip
│   │   └── report.docx
│   ├── se184425/
│   │   ├── solution.zip
│   │   └── assignment.docx
│   └── ...
```

## Database Schema

### ExamZip
- `Id`: Primary key
- `ExamId`: Foreign key to Exam
- `ZipName`: Tên file gốc
- `ZipPath`: Đường dẫn file temp local
- `ExtractedPath`: Đường dẫn folder đã giải nén
- `ParseStatus`: PENDING/DONE/ERROR
- `ParseSummary`: Tóm tắt quá trình xử lý

### ExamStudent
- `Id`: Primary key
- `ExamId`: Foreign key to Exam
- `StudentId`: Foreign key to Student
- `Status`: NOT_FOUND/PARSED/GRADED
- `Note`: Ghi chú

### DocFile
- `Id`: Primary key
- `ExamStudentId`: Foreign key to ExamStudent
- `ExamZipId`: Foreign key to ExamZip
- `FileName`: Tên file Word
- `FilePath`: S3 URL
- `ParsedText`: Nội dung text đã parse từ Word
- `ParseStatus`: NOT_FOUND/OK/ERROR
- `ParseMessage`: Thông báo lỗi nếu có

### Student
Tự động tạo nếu chưa tồn tại:
- `StudentCode`: Parse từ tên folder (vd: "Anhddhse170283" → "se170283")
- `FullName`: Mặc định = tên folder
- `Email`: null
- `ClassName`: null

## Background Processing

### BackgroundJobService
- Tự động chạy khi application start
- Poll database mỗi 10 giây để tìm ExamZip với ParseStatus = PENDING
- Xử lý tuần tự từng ExamZip
- Log errors nhưng không stop service

### FileProcessingService
Xử lý từng student folder:
1. Parse student code từ tên folder
2. Check/Create Student record
3. Create ExamStudent record
4. Tìm folder "0/solution.zip"
5. Upload solution.zip lên S3
6. Giải nén solution.zip
7. Tìm tất cả file .docx
8. Upload .docx files lên S3
9. Parse text từ Word document
10. Create DocFile records với ParsedText
11. Update ExamStudent.Status

## Error Handling

### Common Errors
1. **Folder không có "0"**: ExamStudent.Status = NOT_FOUND
2. **Không có solution.zip**: ExamStudent.Status = NOT_FOUND
3. **Không có file .docx**: DocFile với ParseStatus = NOT_FOUND
4. **Word file corrupt**: DocFile.ParseStatus = ERROR
5. **S3 upload failed**: Retry 3 lần, sau đó mark ERROR

### Cleanup
- Temp files được tự động xóa sau khi xử lý
- Nếu có lỗi, temp files vẫn được cleanup

## Testing

### Test với Postman
1. Tạo Exam trong database
2. Chuẩn bị ZIP file theo đúng cấu trúc
3. POST upload API
4. GET status API để check progress
5. Verify S3 bucket có files
6. Check database: ExamZip, ExamStudent, DocFile, Student

### Sample Test Data
Tạo folder structure:
```
TestStudent_SE123456/
└── 0/
    └── solution.zip (chứa test.docx)
```

Zip thành `Test_Solutions.zip` và upload

## Troubleshooting

### Build errors
```bash
cd /path/to/SWD-Grading
dotnet build
```

### Check logs
- Application logs: Console output
- Background job logs: Search for "Background Job Service"
- Processing logs: Search for "Processing ExamZip"

### AWS Credentials
Đảm bảo:
- Access Key có quyền s3:PutObject, s3:GetObject
- Bucket tồn tại và có đúng region
- Bucket không có policy block public access (nếu cần public URLs)

## Performance

### Recommendations
- Limit file size: 500MB default
- Background processing: 1 ExamZip at a time
- Poll interval: 10 seconds (có thể điều chỉnh)
- S3 upload timeout: Default của AWS SDK

### Monitoring
- Check ExamZip.ParseStatus regularly
- Monitor ParseSummary cho errors
- Watch temp storage disk space

## Security Notes
- Temp files stored locally trước khi upload S3
- AWS credentials trong appsettings.json (nên dùng User Secrets hoặc Environment Variables cho production)
- Files uploaded to S3 với Private ACL
- Validate file extensions (.zip only)
- Max file size limit để tránh DOS

## Future Enhancements
- [ ] Parallel processing multiple ExamZips
- [ ] Real-time progress updates (SignalR)
- [ ] Retry mechanism for failed S3 uploads
- [ ] Email notifications khi processing done
- [ ] Support more document formats (.pdf, .doc)
- [ ] Batch delete old temp files
- [ ] Admin dashboard for monitoring

