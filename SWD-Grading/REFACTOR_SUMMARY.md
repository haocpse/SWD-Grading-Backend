# Refactor Summary: Upload Logic Changes

## Date: November 16, 2025

## Overview
Refactored the file upload and processing logic to only interact with the `DocFile` table. Student and ExamStudent records are now managed by other parts of the system.

## Changes Made

### 1. FileProcessingService.cs - ProcessStudentFolderAsync Method

#### Before:
- Extracted student code from folder name (e.g., `Anhddhse170283` → `se170283`)
- Created Student record if it didn't exist
- Created ExamStudent record if it didn't exist
- Updated ExamStudent.Status to NOT_FOUND when files were missing
- Used extracted student code for S3 paths

#### After:
- Uses full folder name as StudentCode (e.g., `Anhddhse170283`)
- Queries existing Student by StudentCode
- Queries existing ExamStudent by (ExamId, StudentId)
- Throws exception if Student or ExamStudent not found
- Throws exception if files are missing (no status updates)
- Uses full folder name for S3 paths
- Updates ExamStudent.Status to PARSED only on successful processing

### 2. Removed Methods
- `ExtractStudentCode(string folderName)` - No longer needed

### 3. Removed Dependencies
- `using System.Text.RegularExpressions;` - No longer needed

## New Logic Flow

```
For each student folder (e.g., "Anhddhse170283"):
1. Use folder name as StudentCode
2. Query Student by StudentCode
   → Not found: throw exception → logged as error, skip folder
3. Query ExamStudent by (ExamId, StudentId)
   → Not found: throw exception → logged as error, skip folder
4. Check folder "0" exists
   → Not found: throw exception → logged as error, skip folder
5. Process files:
   - Upload solution.zip to S3 (if exists)
   - Extract and find all .docx files
   - Upload each .docx to S3
   - Parse .docx content using DocumentFormat.OpenXml
   - Create DocFile records with ParsedText
6. Update ExamStudent.Status = PARSED
7. Update ExamStudent.Note = "Processed X Word file(s)"
```

## Error Handling

All errors are now thrown as exceptions and caught by the outer try-catch in `ProcessStudentSolutionsAsync`:
- Student not found
- ExamStudent not found
- Folder "0" not found
- No .docx files found
- S3 upload errors
- Word parsing errors

Errors are logged in:
- `ExamZip.ParseSummary` (overall summary)
- Console output (detailed logging)

## S3 Path Structure

Changed from: `{ExamCode}/{extractedStudentCode}/`
- Example: `EXAM001/se170283/`

Changed to: `{ExamCode}/{fullFolderName}/`
- Example: `EXAM001/Anhddhse170283/`

## Database Impact

### Tables Modified:
- **DocFile**: Records created with ExamStudentId (no change to schema)
- **ExamStudent**: Status updated to PARSED on success (no creation)

### Tables NOT Modified:
- **Student**: No longer created by upload process
- **ExamStudent**: No longer created by upload process

## Testing Checklist

- [ ] Verify Students exist in database before upload
- [ ] Verify ExamStudent records exist before upload
- [ ] Test upload with valid student folders
- [ ] Test upload with missing student (should log error, skip folder)
- [ ] Test upload with missing ExamStudent (should log error, skip folder)
- [ ] Test upload with missing folder "0" (should log error, skip folder)
- [ ] Test upload with no .docx files (should log error, skip folder)
- [ ] Verify S3 paths use full folder name
- [ ] Verify ExamStudent.Status updated to PARSED on success
- [ ] Verify DocFile records created with correct data
- [ ] Verify ParsedText contains Word document content

## Backward Compatibility

### Breaking Changes:
1. **StudentCode Format**: Now expects full folder name (e.g., `Anhddhse170283`) instead of extracted code (e.g., `se170283`)
2. **Pre-requisites**: Student and ExamStudent records must exist in database before upload
3. **Error Behavior**: Folders are skipped (not processed) if Student or ExamStudent not found

### Migration Required:
If existing Student records use extracted codes (e.g., `se170283`), you need to:
1. Update Student.StudentCode to use full names (e.g., `Anhddhse170283`)
2. OR update folder naming convention to match existing StudentCode format
3. Ensure all ExamStudent records exist before uploading solutions

## Dependencies

### No Changes Required:
- DAL/Repository (all methods remain unchanged)
- Model/Entity (all entities remain unchanged)
- Controllers (API endpoints remain unchanged)
- ExamUploadService (upload service remains unchanged)
- BackgroundJobService (background processing remains unchanged)

## Configuration

### AWS S3 Permissions Required:
- s3:PutObject (upload files)
- s3:GetObject (optional, for retrieval)
- s3:DeleteObject (optional, for cleanup)
- s3:ListBucket (optional, for listing)

### appsettings.json:
No changes required. Existing AWS configuration remains valid.

## Next Steps

1. Ensure Student table is populated with correct StudentCode values
2. Ensure ExamStudent records are created before uploading solutions
3. Update student folder naming convention if needed
4. Test upload process with sample data
5. Verify ExamStudent status updates correctly
6. Verify DocFile records contain parsed text

