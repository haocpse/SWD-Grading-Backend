# API Upload Student Solutions - Hướng dẫn sử dụng

## Tổng quan
API này cho phép upload toàn bộ folder `Student_Solutions` với tất cả files và subfolder structure.

## Cấu trúc Folder Student_Solutions

```
Student_Solutions/
├── Anhddhse170283/
│   ├── 0/
│   │   ├── solution.zip
│   │   └── SWD392_PE_SU25_SE170283_DaoDuongHungAnh.docx (optional)
│   └── history.dat
├── AnhKVSE182347/
│   ├── 0/
│   │   └── solution.zip
│   └── history.dat
└── ...
```

## API Endpoint

### Upload Student Solutions

**Method:** `POST`

**URL:** `/api/exam-upload/upload-solutions/{examId}`

**Parameters:**
- `examId` (path parameter): ID của exam trong database

**Request Body:**
- Content-Type: `multipart/form-data`
- Field name: `files` (multiple files)

## Cách test với Postman

### Bước 1: Chuẩn bị

1. Đảm bảo đã có Exam trong database với `ExamCode` và `Id`
2. Có folder `Student_Solutions` với cấu trúc đúng

### Bước 2: Upload bằng Postman

1. **Tạo request mới:**
   - Method: `POST`
   - URL: `http://localhost:5000/api/exam-upload/upload-solutions/1` (thay `1` bằng examId thực tế)

2. **Chọn Body tab:**
   - Chọn `form-data`

3. **Add files:**
   - Click vào ô "KEY", chọn type là `File`
   - Nhập key name: `files`
   - Click "Select Files" và chọn **NHIỀU FILES** từ folder Student_Solutions
   
4. **Important:** 
   - Postman sẽ giữ relative path trong `FileName`
   - Backend sẽ tự động recreate folder structure

### Bước 3: Alternative - Upload từ script

Nếu có quá nhiều files, có thể dùng script Python hoặc curl:

#### Python Script

```python
import requests
import os

def upload_student_solutions(exam_id, folder_path, api_url):
    files = []
    
    # Walk through all files in folder
    for root, dirs, filenames in os.walk(folder_path):
        for filename in filenames:
            full_path = os.path.join(root, filename)
            relative_path = os.path.relpath(full_path, folder_path)
            
            # Open file and add to list with relative path
            files.append(
                ('files', (relative_path, open(full_path, 'rb')))
            )
    
    # Upload
    response = requests.post(
        f'{api_url}/api/exam-upload/upload-solutions/{exam_id}',
        files=files
    )
    
    # Close all files
    for _, (_, file_obj) in files:
        file_obj.close()
    
    return response.json()

# Usage
result = upload_student_solutions(
    exam_id=1,
    folder_path='./Model/Student_Solutions',
    api_url='http://localhost:5000'
)

print(result)
```

#### PowerShell Script

```powershell
$examId = 1
$folderPath = ".\Model\Student_Solutions"
$apiUrl = "http://localhost:5000/api/exam-upload/upload-solutions/$examId"

# Get all files recursively
$files = Get-ChildItem -Path $folderPath -Recurse -File

# Create multipart form data
$boundary = [System.Guid]::NewGuid().ToString()
$headers = @{
    "Content-Type" = "multipart/form-data; boundary=$boundary"
}

# Build form data
$body = ""
foreach ($file in $files) {
    $relativePath = $file.FullName.Replace($folderPath, "").TrimStart("\")
    $fileContent = [System.IO.File]::ReadAllBytes($file.FullName)
    $encoding = [System.Text.Encoding]::GetEncoding("iso-8859-1")
    
    $body += "--$boundary`r`n"
    $body += "Content-Disposition: form-data; name=`"files`"; filename=`"$relativePath`"`r`n"
    $body += "Content-Type: application/octet-stream`r`n`r`n"
    $body += $encoding.GetString($fileContent)
    $body += "`r`n"
}
$body += "--$boundary--`r`n"

# Upload
Invoke-RestMethod -Uri $apiUrl -Method Post -Headers $headers -Body $body
```

## Response Format

### Success Response

```json
{
  "examZipId": 1,
  "status": "Processing",
  "message": "Uploaded 621 files successfully and processing has started. Check status using the provided ExamZipId."
}
```

### Error Response

```json
{
  "examZipId": 0,
  "status": "Error",
  "message": "No files uploaded"
}
```

## Check Processing Status

**Method:** `GET`

**URL:** `/api/exam-upload/status/{examZipId}`

**Response:**

```json
{
  "examZipId": 1,
  "parseStatus": "DONE",
  "processedCount": 305,
  "totalCount": 309,
  "errors": [
    "Error processing studentXYZ: folder '0' not found"
  ],
  "failedStudents": [
    "se170001",
    "se170002"
  ],
  "parseSummary": "Found 309 student folders\nProcessing complete:\nTotal: 309\nSuccess: 305\nErrors: 4"
}
```

## Processing Logic

1. **Upload Phase:**
   - API nhận tất cả files với relative paths
   - Save files vào temp folder maintaining structure
   - Create ExamZip record với status PENDING

2. **Background Processing:**
   - Scan tất cả student folders
   - Với mỗi folder:
     - Parse student code từ folder name
     - Tìm folder `0/`
     - Check file .docx trong folder `0/` trực tiếp
     - Check `solution.zip` trong folder `0/`
     - Upload cả .docx và solution.zip lên S3
     - Nếu có solution.zip, extract và tìm thêm .docx files
     - Parse text từ tất cả .docx files
     - Save vào DocFile với ParsedText

3. **Student Code Parsing:**
   - From: `Anhddhse170283` → Extract: `se170283`
   - From: `AnhKVSE182347` → Extract: `se182347`
   - Nếu không match pattern → use full folder name

## Important Notes

1. **File Limits:**
   - Default max: 500MB per request (configurable in appsettings.json)
   - Không limit số lượng files
   - Tất cả file types được accept (không chỉ .zip)

2. **Folder Structure:**
   - PHẢI có cấu trúc: `StudentFolder/0/solution.zip` hoặc `StudentFolder/0/*.docx`
   - File `history.dat` được ignore (không process)

3. **Processing Time:**
   - Background job chạy mỗi 10 giây
   - Processing time phụ thuộc số lượng students và file sizes
   - Check status API để theo dõi progress

4. **S3 Upload:**
   - Cần configure AWS credentials trong appsettings.json
   - Files được upload: `/{ExamCode}/{StudentCode}/solution.zip` và `/{ExamCode}/{StudentCode}/*.docx`

5. **Database Records:**
   - Auto-create Student nếu chưa tồn tại
   - Create ExamStudent cho mỗi student
   - Create DocFile cho mỗi .docx file với ParsedText

## Troubleshooting

### "No files uploaded"
- Check Postman: Field name phải là `files` (lowercase)
- Check: Đã select files chưa

### "Exam with ID X not found"
- Verify examId trong database
- Check connection string

### Files không maintain structure
- Postman tự động gửi relative path trong FileName
- Nếu dùng script, ensure relative path được set đúng

### Processing stuck at PENDING
- Check BackgroundJobService có running không
- Check logs trong console
- Verify temp folder có write permissions

## Testing Checklist

- [ ] Upload với examId hợp lệ
- [ ] Check ExamZipId trong response
- [ ] Call status API để verify processing
- [ ] Check database: ExamZip, ExamStudent, DocFile, Student
- [ ] Verify S3 bucket có files
- [ ] Check DocFile.ParsedText có content
- [ ] Test với student không có folder "0"
- [ ] Test với student không có solution.zip
- [ ] Test với student có .docx trực tiếp trong folder "0"

