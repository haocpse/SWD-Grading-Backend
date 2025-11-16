# Plagiarism Detection Feature Guide

## Overview

The plagiarism detection feature uses vector embeddings and similarity search to identify potentially similar submissions between students in the same exam. It leverages Qdrant vector database for efficient similarity comparisons.

## Architecture

### Components

1. **VectorService**: Generates text embeddings and manages Qdrant vector database
2. **PlagiarismService**: Orchestrates plagiarism checking workflow
3. **PlagiarismController**: REST API endpoints for plagiarism detection
4. **BackgroundJobService**: Automatically generates embeddings for newly parsed documents

### Database Tables

- **similarity_check**: Stores plagiarism check metadata
  - ExamId, CheckedAt, Threshold, CheckedByUserId
  
- **similarity_result**: Stores individual similarity pairs
  - DocFile1Id, DocFile2Id, SimilarityScore, Student codes

## Setup

### 1. Install Qdrant

Run Qdrant using Docker:

```bash
docker run -p 6333:6333 -p 6334:6334 \
    -v $(pwd)/qdrant_storage:/qdrant/storage:z \
    qdrant/qdrant
```

Or use Docker Compose:

```yaml
version: '3.8'
services:
  qdrant:
    image: qdrant/qdrant
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - ./qdrant_storage:/qdrant/storage
```

### 2. Configuration

The Qdrant configuration is in `appsettings.json`:

```json
{
  "Qdrant": {
    "Endpoint": "http://localhost:6333",
    "CollectionName": "exam_submissions",
    "VectorSize": "384"
  }
}
```

### 3. Run Migration

Ensure the database migration has been applied:

```bash
dotnet ef database update --project DAL --startup-project SWD-Grading
```

## API Usage

### 1. Check Plagiarism for an Exam

**Endpoint**: `POST /api/plagiarism/check/{examId}`

**Request Body**:
```json
{
  "threshold": 0.8
}
```

**Parameters**:
- `examId`: The ID of the exam to check
- `threshold`: Similarity threshold (0.0 to 1.0). Higher values = stricter matching
  - 0.7 = 70% similar
  - 0.8 = 80% similar (recommended)
  - 0.9 = 90% similar (very strict)

**Response**:
```json
{
  "success": true,
  "message": "Plagiarism check completed. Found 3 suspicious pair(s).",
  "data": {
    "checkId": 1,
    "examId": 5,
    "examCode": "SE1234",
    "checkedAt": "2024-11-16T10:30:00Z",
    "threshold": 0.8,
    "totalPairsChecked": 45,
    "suspiciousPairsCount": 3,
    "checkedByUsername": "teacher1",
    "suspiciousPairs": [
      {
        "resultId": 1,
        "student1Code": "SE001",
        "student2Code": "SE002",
        "docFile1Name": "SE001_assignment.docx",
        "docFile2Name": "SE002_assignment.docx",
        "docFile1Id": 10,
        "docFile2Id": 11,
        "similarityScore": 0.92
      }
    ]
  }
}
```

### 2. Get Plagiarism Check History

**Endpoint**: `GET /api/plagiarism/history/{examId}`

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 2 plagiarism check(s)",
  "data": [
    {
      "checkId": 2,
      "examId": 5,
      "examCode": "SE1234",
      "checkedAt": "2024-11-16T11:00:00Z",
      "threshold": 0.8,
      "totalPairsChecked": 0,
      "suspiciousPairsCount": 3,
      "checkedByUsername": "teacher1",
      "suspiciousPairs": [...]
    }
  ]
}
```

### 3. Manually Generate Embedding

**Endpoint**: `POST /api/plagiarism/generate-embedding/{docFileId}`

Useful for re-generating embeddings or forcing generation for specific documents.

**Response**:
```json
{
  "success": true,
  "message": "Embedding generated successfully"
}
```

## Workflow

### Automatic Embedding Generation (Background)

1. Student submissions are uploaded and processed
2. `FileProcessingService` extracts text from Word documents
3. `DocFile` records are created with `ParseStatus = OK` and `ParsedText`
4. `BackgroundJobService` polls for new documents every 10 seconds
5. Embeddings are automatically generated and stored in Qdrant

### Manual Plagiarism Check

1. Teacher navigates to exam plagiarism check page
2. Sets desired similarity threshold (e.g., 0.8 = 80%)
3. Clicks "Check Plagiarism"
4. System:
   - Ensures all documents have embeddings (generates if missing)
   - Queries Qdrant for similar document pairs
   - Saves results to database
   - Returns suspicious pairs to UI
5. Teacher reviews suspicious pairs and takes action

## Understanding Similarity Scores

- **0.95 - 1.0**: Nearly identical or copied (very high confidence)
- **0.85 - 0.94**: Very similar (high confidence)
- **0.75 - 0.84**: Similar structure/content (medium confidence)
- **0.65 - 0.74**: Some similarities (low confidence)
- **< 0.65**: Different content

## Technical Details

### Embedding Algorithm

The current implementation uses a simplified text hashing approach for demonstration. For production use, consider:

1. **Sentence Transformers** (recommended):
   - Download ONNX model: `all-MiniLM-L6-v2`
   - 384-dimensional embeddings
   - Multilingual support

2. **OpenAI Embeddings API**:
   - High quality embeddings
   - Requires API key
   - Cost per usage

### Vector Comparison

- Uses **cosine similarity** for comparing document vectors
- Range: 0.0 (completely different) to 1.0 (identical)
- Efficient for high-dimensional vectors

### Performance Considerations

- **Indexing**: Background job processes 10 documents per iteration
- **Search**: Qdrant handles similarity search efficiently
- **Storage**: Each document = ~1.5KB in Qdrant (384 floats × 4 bytes)
- **Scalability**: Tested with up to 1000 documents per exam

## Troubleshooting

### Qdrant Connection Error

**Problem**: Cannot connect to Qdrant
**Solution**: 
- Ensure Qdrant is running: `docker ps`
- Check endpoint in appsettings.json
- Verify port 6333 is accessible

### No Embeddings Generated

**Problem**: Documents not being indexed
**Solution**:
- Check `BackgroundJobService` logs
- Verify `DocFile.ParseStatus = OK`
- Ensure `ParsedText` is not null
- Manually trigger: `POST /api/plagiarism/generate-embedding/{docFileId}`

### Low Similarity Scores

**Problem**: No suspicious pairs found with threshold 0.8
**Solution**:
- Lower threshold to 0.7 or 0.6
- Check if documents have sufficient text content
- Review embedding quality

### High Memory Usage

**Problem**: Qdrant using too much memory
**Solution**:
- Configure Qdrant memory limits
- Use disk-based storage
- Implement collection archiving for old exams

## Best Practices

1. **Run plagiarism checks after submission deadline** to ensure all documents are processed
2. **Use threshold 0.8** as a starting point, adjust based on results
3. **Review high-score pairs manually** - similarity doesn't always mean plagiarism
4. **Keep Qdrant data backed up** - embeddings can be regenerated but it takes time
5. **Monitor background job logs** to ensure embeddings are generated smoothly

## Future Enhancements

- [ ] Support for PDF documents
- [ ] Code similarity detection for programming assignments
- [ ] Visualization of similar text segments
- [ ] External plagiarism detection (compare with online sources)
- [ ] Batch export of plagiarism reports
- [ ] Whitelist common template text

## API Testing with cURL

### Check Plagiarism
```bash
curl -X POST "http://localhost:5000/api/plagiarism/check/5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"threshold": 0.8}'
```

### Get History
```bash
curl -X GET "http://localhost:5000/api/plagiarism/history/5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Generate Embedding
```bash
curl -X POST "http://localhost:5000/api/plagiarism/generate-embedding/123" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Support

For issues or questions:
1. Check logs in `BackgroundJobService` and `PlagiarismService`
2. Verify Qdrant is running: http://localhost:6333/dashboard
3. Review database tables: `similarity_check` and `similarity_result`

