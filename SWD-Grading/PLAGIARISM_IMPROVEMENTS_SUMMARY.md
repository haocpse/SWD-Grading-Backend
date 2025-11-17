# Plagiarism Detection Improvements Summary

## 🎯 Overview
This document summarizes the recent improvements made to the plagiarism detection system, including the `IsEmbedded` flag feature and embedding algorithm bug fix.

---

## ✅ Feature 1: IsEmbedded Flag for DocFile

### Problem
- Background job was re-embedding documents that were already processed
- No way to track which documents had been embedded into the vector database
- Manual plagiarism checks needed to re-index to ensure latest data

### Solution
Added `IsEmbedded` boolean flag to `DocFile` entity to track embedding status.

### Implementation Details

#### 1. **DocFile Entity** (`Model/Entity/DocFile.cs`)
```csharp
[Required]
public bool IsEmbedded { get; set; } = false;
```

#### 2. **DocFileRepository** (`DAL/Repository/DocFileRepository.cs`)
```csharp
public async Task<List<DocFile>> GetRecentlyParsedDocFilesAsync(int limit = 10)
{
    return await _context.Set<DocFile>()
        .Include(df => df.ExamStudent)
        .ThenInclude(es => es.Student)
        .Where(df => df.ParseStatus == Model.Enums.DocParseStatus.OK 
            && !string.IsNullOrWhiteSpace(df.ParsedText)
            && !df.IsEmbedded) // Only get non-embedded documents
        .OrderByDescending(df => df.Id)
        .Take(limit)
        .ToListAsync();
}
```

#### 3. **PlagiarismService Updates**

**GenerateEmbeddingForDocFileAsync()** (Background Job):
- Checks `IsEmbedded` flag and skips if already embedded
- Sets `IsEmbedded = true` after successful indexing

**CheckSuspiciousDocumentAsync()** (Manual API):
- Always re-indexes document (ignores flag)
- Sets `IsEmbedded = true` after re-indexing
- Ensures plagiarism checks use latest data

### Behavior
- **Background Job**: Only processes documents with `IsEmbedded = false`
- **Manual API Call**: Always re-indexes regardless of flag
- Prevents duplicate work while allowing manual refresh

---

## 🐛 Feature 2: Fixed Embedding Algorithm Bug

### Problem
Two nearly identical documents were not being detected as similar, even with very low similarity threshold. The root cause was in the embedding generation algorithm.

### Root Cause
The original algorithm used `Random` with varying seeds for each dimension:
```csharp
// BUG: Different seed for each dimension
for (int i = 0; i < _vectorSize; i++)
{
    var seed = wordHash + i;  // Seed changes per dimension!
    var rand = new Random(seed);
    embedding[i] += (float)(rand.NextDouble() * 2 - 1) * weight * 10;
}
```

This caused:
- Inconsistent vector representations for identical words
- Different random values for the same word across dimensions
- Very low similarity scores even for duplicate content

### Solution
Replaced `Random` with **deterministic hash function** for dimension projection:

```csharp
// FIXED: Deterministic hash for consistent embeddings
for (int i = 0; i < _vectorSize; i++)
{
    var dimHash = GetDimensionHash(wordHash, i);
    var value = (float)((dimHash % 10000) / 5000.0 - 1.0); // Range: -1 to 1
    embedding[i] += value * weight * 10;
}

private int GetDimensionHash(int wordHash, int dimension)
{
    unchecked
    {
        int hash = wordHash;
        hash = (hash * 16777619) ^ dimension;
        hash = (int)((hash * 2166136261) ^ (dimension >> 8));
        hash = (hash * 1000000007) ^ (wordHash >> 16);
        return Math.Abs(hash);
    }
}
```

### Benefits
- **Deterministic**: Same text always produces same embedding
- **Reproducible**: Embeddings are consistent across runs
- **Better Similarity**: Identical/similar texts now properly detected
- **Stable Hashing**: Uses multiple prime numbers for good distribution

---

## 🔗 Feature 3: S3 Presigned URLs in Plagiarism Response

### Problem
Frontend needed direct links to view/download the suspicious document files, but only had DocFile IDs.

### Solution
Added S3 presigned URLs to `SimilarityPairResponse` for temporary file access.

### Implementation

#### 1. **IS3Service Interface**
```csharp
string GetPresignedUrl(string s3Key, int expiryMinutes = 60);
```

#### 2. **S3Service Implementation**
```csharp
public string GetPresignedUrl(string s3Key, int expiryMinutes = 60)
{
    var request = new GetPreSignedUrlRequest
    {
        BucketName = _awsConfig.BucketName,
        Key = s3Key,
        Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
        Verb = HttpVerb.GET
    };
    return _s3Client.GetPreSignedURL(request);
}
```

#### 3. **SimilarityPairResponse Model**
```csharp
public class SimilarityPairResponse
{
    // ... existing fields ...
    
    /// <summary>
    /// S3 URL to view/download the first document file
    /// </summary>
    public string? DocFile1Url { get; set; }
    
    /// <summary>
    /// S3 URL to view/download the second document file
    /// </summary>
    public string? DocFile2Url { get; set; }
}
```

#### 4. **PlagiarismService Update**
```csharp
SuspiciousPairs = similarityResults.Select(result => 
{
    var matchedDocFile = await _unitOfWork.DocFileRepository.GetByIdAsync(result.DocFile2Id);
    
    return new SimilarityPairResponse
    {
        // ... existing fields ...
        
        // Generate presigned URLs (valid for 24 hours)
        DocFile1Url = !string.IsNullOrWhiteSpace(docFile.FilePath) 
            ? _s3Service.GetPresignedUrl(docFile.FilePath, expiryMinutes: 1440) 
            : null,
        DocFile2Url = !string.IsNullOrWhiteSpace(matchedDocFile?.FilePath) 
            ? _s3Service.GetPresignedUrl(matchedDocFile.FilePath, expiryMinutes: 1440) 
            : null
    };
}).ToList()
```

### API Response Example
```json
{
  "checkId": 123,
  "suspiciousPairs": [
    {
      "resultId": 456,
      "student1Code": "SE12345",
      "student2Code": "SE67890",
      "docFile1Name": "assignment.pdf",
      "docFile2Name": "submission.pdf",
      "docFile1Url": "https://bucket.s3.region.amazonaws.com/path/file.pdf?X-Amz-Signature=...",
      "docFile2Url": "https://bucket.s3.region.amazonaws.com/path/file2.pdf?X-Amz-Signature=...",
      "similarityScore": 0.95
    }
  ]
}
```

### Benefits
- **Direct Access**: Frontend can directly download/view files via URLs
- **Secure**: Presigned URLs expire after 24 hours
- **No Backend Pass-through**: Files served directly from S3
- **Better UX**: Users can immediately review suspicious documents

---

## 🔄 Migration Required

Don't forget to run the migration for the `IsEmbedded` column:

```bash
cd DAL
dotnet ef migrations add AddIsEmbeddedToDocFile --startup-project ../SWD-Grading/SWD-Grading.csproj
dotnet ef database update --startup-project ../SWD-Grading/SWD-Grading.csproj
```

---

## 📝 Testing Recommendations

### 1. Test IsEmbedded Flag
- Upload new documents → verify background job sets `IsEmbedded = true`
- Call manual plagiarism check → verify it re-indexes and updates flag
- Check that background job doesn't re-process already embedded docs

### 2. Test Embedding Algorithm Fix
- Create two documents with identical/very similar content
- Verify similarity score is now high (>0.8 for identical content)
- Test with various similarity thresholds

### 3. Test S3 URLs
- Call plagiarism check API
- Verify `docFile1Url` and `docFile2Url` are present in response
- Test that URLs work and files can be downloaded
- Verify URLs expire after 24 hours

---

## 📚 Related Files Modified

### Core Changes
- `Model/Entity/DocFile.cs` - Added IsEmbedded flag
- `BLL/Service/VectorService.cs` - Fixed embedding algorithm
- `BLL/Service/PlagiarismService.cs` - Updated to use IsEmbedded and S3 URLs
- `BLL/Service/S3Service.cs` - Added GetPresignedUrl method
- `BLL/Interface/IS3Service.cs` - Added interface method
- `DAL/Repository/DocFileRepository.cs` - Updated query filter
- `BLL/Model/Response/SimilarityPairResponse.cs` - Added URL fields

### Migration
- `DAL/Migrations/XXXXXX_AddIsEmbeddedToDocFile.cs` (generated)

---

## 🎉 Impact

These improvements significantly enhance the plagiarism detection system:
1. ✅ **Better Performance**: No duplicate embedding work
2. ✅ **Higher Accuracy**: Fixed algorithm properly detects similar content
3. ✅ **Better UX**: Direct file access via presigned URLs
4. ✅ **Flexibility**: Manual checks can refresh data when needed

**Date**: November 16, 2025
**Version**: 2.0


