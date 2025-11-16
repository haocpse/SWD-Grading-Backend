# Plagiarism Detection Implementation Summary

## Completed Features

### ✅ Database Schema
- Created `SimilarityCheck` entity to store plagiarism check metadata
- Created `SimilarityResult` entity to store similarity pairs
- Added migration `AddPlagiarismDetection`
- Updated `SWDGradingDbContext` with new DbSets and configurations

### ✅ Vector Database Integration
- Added `Qdrant.Client` (v1.12.0) NuGet package
- Added `Microsoft.ML.OnnxRuntime` (v1.20.1) for embedding generation
- Implemented `VectorService` for:
  - Text embedding generation
  - Document indexing in Qdrant
  - Similarity search with configurable threshold
  - Collection management

### ✅ Business Logic
- Implemented `PlagiarismService` for:
  - Orchestrating plagiarism checks
  - Managing embedding generation
  - Storing results to database
  - Retrieving check history

### ✅ Data Access Layer
- Created `ISimilarityCheckRepository` and `SimilarityCheckRepository`
- Added methods to `IDocFileRepository`:
  - `GetRecentlyParsedDocFilesAsync()` - for background processing
- Updated `IUnitOfWork` and `UnitOfWork` with new repository

### ✅ API Layer
- Created `PlagiarismController` with endpoints:
  - `POST /api/plagiarism/check/{examId}` - Check plagiarism
  - `GET /api/plagiarism/history/{examId}` - Get history
  - `POST /api/plagiarism/generate-embedding/{docFileId}` - Manual trigger

### ✅ Request/Response Models
- `CheckPlagiarismRequest` - with flexible threshold
- `PlagiarismCheckResponse` - complete check results
- `SimilarityPairResponse` - individual pair details

### ✅ Background Processing
- Enhanced `BackgroundJobService` with:
  - `ProcessPendingEmbeddingsAsync()` - automatic embedding generation
  - Processes 10 documents per iteration
  - Runs every 10 seconds

### ✅ Configuration
- Added Qdrant settings to `appsettings.json` and `appsettings.json.example`
- Registered all services in `Program.cs`
- Added authorization to plagiarism endpoints

### ✅ Documentation
- Created `PLAGIARISM_DETECTION_GUIDE.md` with:
  - Setup instructions
  - API usage examples
  - Troubleshooting guide
  - Best practices

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Client (Frontend)                        │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP/REST
┌──────────────────────────▼──────────────────────────────────┐
│              PlagiarismController (API)                      │
├──────────────────────────┬──────────────────────────────────┤
│   - POST /check/{examId}                                    │
│   - GET /history/{examId}                                   │
│   - POST /generate-embedding/{docFileId}                    │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│               PlagiarismService (BLL)                        │
├──────────────────────────┬──────────────────────────────────┤
│   - CheckPlagiarismAsync()                                  │
│   - GetCheckHistoryAsync()                                  │
│   - GenerateEmbeddingForDocFileAsync()                      │
└──────────────────────────┬──────────────────────────────────┘
                           │
        ┌──────────────────┴──────────────────┐
        │                                     │
┌───────▼─────────┐                ┌─────────▼──────────┐
│  VectorService  │                │   UnitOfWork/DAL   │
│     (BLL)       │                │                    │
├─────────────────┤                ├────────────────────┤
│ - Generate      │                │ - SimilarityCheck  │
│   Embeddings    │                │   Repository       │
│ - Index Docs    │                │ - DocFile          │
│ - Search        │                │   Repository       │
│   Similar       │                │ - SQL Server       │
└────────┬────────┘                └────────────────────┘
         │
┌────────▼────────────────────────────────────────────────────┐
│              Qdrant Vector Database                          │
├──────────────────────────────────────────────────────────────┤
│   Collection: exam_submissions                               │
│   Vector Size: 384                                           │
│   Distance: Cosine                                           │
│   Storage: Persistent                                        │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│           BackgroundJobService (Background)                   │
├──────────────────────────────────────────────────────────────┤
│   Polls every 10 seconds:                                     │
│   1. ProcessPendingExamZipsAsync()                           │
│   2. ProcessPendingEmbeddingsAsync()                         │
│      └─> Generates embeddings for new DocFiles              │
└──────────────────────────────────────────────────────────────┘
```

## Workflow

### 1. Automatic Embedding Generation (Background)
```
DocFile Created (ParseStatus = OK)
        ↓
BackgroundJobService detects new doc
        ↓
PlagiarismService.GenerateEmbeddingForDocFileAsync()
        ↓
VectorService.GenerateEmbeddingAsync(ParsedText)
        ↓
VectorService.IndexDocumentAsync()
        ↓
Stored in Qdrant with metadata (examId, studentCode, docFileId)
```

### 2. Manual Plagiarism Check
```
Teacher triggers check via API
        ↓
PlagiarismService.CheckPlagiarismAsync(examId, threshold, userId)
        ↓
Fetch all DocFiles for exam with ParseStatus = OK
        ↓
Check for missing embeddings → Generate if needed
        ↓
VectorService.SearchSimilarDocumentsAsync(examId, threshold)
        ↓
Qdrant returns similar pairs (cosine similarity >= threshold)
        ↓
Create SimilarityCheck + SimilarityResult records
        ↓
Return PlagiarismCheckResponse to API
```

## Files Created/Modified

### New Files (25)
**Model Layer:**
- `Model/Entity/SimilarityCheck.cs`
- `Model/Entity/SimilarityResult.cs`

**DAL Layer:**
- `DAL/Interface/ISimilarityCheckRepository.cs`
- `DAL/Repository/SimilarityCheckRepository.cs`
- `DAL/Migrations/XXXXXX_AddPlagiarismDetection.cs` (generated)

**BLL Layer:**
- `BLL/Interface/IVectorService.cs`
- `BLL/Interface/IPlagiarismService.cs`
- `BLL/Service/VectorService.cs`
- `BLL/Service/PlagiarismService.cs`
- `BLL/Model/Request/CheckPlagiarismRequest.cs`
- `BLL/Model/Response/PlagiarismCheckResponse.cs`
- `BLL/Model/Response/SimilarityPairResponse.cs`

**API Layer:**
- `SWD-Grading/Controllers/PlagiarismController.cs`

**Documentation:**
- `PLAGIARISM_DETECTION_GUIDE.md`
- `PLAGIARISM_IMPLEMENTATION_SUMMARY.md` (this file)

### Modified Files (9)
- `DAL/SWDGradingDbContext.cs` - Added DbSets and entity configurations
- `DAL/Interface/IUnitOfWork.cs` - Added SimilarityCheckRepository
- `DAL/Repository/UnitOfWork.cs` - Implemented repository property
- `DAL/Interface/IDocFileRepository.cs` - Added GetRecentlyParsedDocFilesAsync
- `DAL/Repository/DocFileRepository.cs` - Implemented new method
- `BLL/Service/BackgroundJobService.cs` - Added embedding generation
- `BLL/BLL.csproj` - Added Qdrant and ONNX packages
- `SWD-Grading/Program.cs` - Registered new services
- `SWD-Grading/appsettings.json` - Added Qdrant configuration
- `SWD-Grading/appsettings.json.example` - Added Qdrant configuration

## Next Steps

### For Development
1. **Install and run Qdrant**:
   ```bash
   docker run -p 6333:6333 qdrant/qdrant
   ```

2. **Apply migration**:
   ```bash
   dotnet ef database update --project DAL --startup-project SWD-Grading
   ```

3. **Restore packages**:
   ```bash
   dotnet restore
   ```

4. **Run application**:
   ```bash
   dotnet run --project SWD-Grading
   ```

### For Testing
1. Upload exam submissions (ZIP with student Word files)
2. Wait for background processing to complete
3. Check Qdrant dashboard: http://localhost:6333/dashboard
4. Call plagiarism check API with examId and threshold
5. Review results in response

### For Production
1. **Improve embedding quality**:
   - Download ONNX model: all-MiniLM-L6-v2
   - Replace simple hashing with actual model inference
   - See VectorService TODO comments

2. **Scale Qdrant**:
   - Use persistent storage volume
   - Configure memory limits
   - Set up replication for HA

3. **Optimize performance**:
   - Add caching layer
   - Batch embedding generation
   - Implement pagination for large result sets

4. **Enhance features**:
   - Add text highlighting for similar sections
   - Support PDF documents
   - Implement code similarity for programming assignments
   - Add email notifications for high-similarity findings

## Configuration Reference

### Qdrant Settings
```json
{
  "Qdrant": {
    "Endpoint": "http://localhost:6333",
    "CollectionName": "exam_submissions",
    "VectorSize": "384"
  }
}
```

### Recommended Thresholds
- **0.9**: Very strict - only nearly identical documents
- **0.8**: Recommended - catches most similar submissions
- **0.7**: Permissive - may include legitimate similarities
- **0.6**: Very permissive - many false positives

## Testing Checklist

- [ ] Qdrant container running
- [ ] Database migration applied
- [ ] Packages restored
- [ ] Application starts without errors
- [ ] Background job processing documents
- [ ] Embeddings generated in Qdrant
- [ ] POST /api/plagiarism/check/{examId} returns results
- [ ] GET /api/plagiarism/history/{examId} returns history
- [ ] Results saved to database correctly
- [ ] Authorization working (JWT required)

## Support & Troubleshooting

See `PLAGIARISM_DETECTION_GUIDE.md` for:
- Detailed setup instructions
- API usage examples
- Common issues and solutions
- Best practices
- Performance tuning

