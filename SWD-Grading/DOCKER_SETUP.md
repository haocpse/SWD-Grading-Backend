# Docker Setup Guide

This guide explains how to run the SWD Grading Backend using Docker Compose with SQL Server and Qdrant.

## Prerequisites

- Docker Desktop installed and running
- Docker Compose v3.8 or later

## Quick Start

1. **Create your environment file**:
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env` file with your actual credentials**:
   ```bash
   # Required: Update these values
   DB_PASSWORD=YourStr0ng!Pass
   JWT_SECRET_KEY=YourSuperSecretKeyForJWTAuthentication12345
   AWS_BUCKET_NAME=your-actual-bucket-name
   AWS_ACCESS_KEY=your-actual-access-key
   AWS_SECRET_KEY=your-actual-secret-key
   OPENAI_API_KEY=your-actual-openai-api-key
   ```

3. **Start all services**:
   ```bash
   docker-compose up -d
   ```

4. **Check logs**:
   ```bash
   docker-compose logs -f backend
   ```

5. **Access the application**:
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger (if enabled in Production)
   - Qdrant Dashboard: http://localhost:6333/dashboard
   - SQL Server: localhost:1433

## Services

### SQL Server
- **Image**: mcr.microsoft.com/mssql/server:2022-latest
- **Port**: 1433
- **Database**: SWDGradingDB
- **User**: sa
- **Password**: From `DB_PASSWORD` environment variable
- **Data Persistence**: `sqlserver_data` volume

### Qdrant
- **Image**: qdrant/qdrant:latest
- **Ports**: 6333 (REST API), 6334 (gRPC)
- **Collection**: exam_submissions
- **Data Persistence**: `qdrant_storage` volume

### Backend (.NET 8.0)
- **Port**: 8080
- **Environment**: Production
- **Auto-Migration**: Yes (runs on startup)
- **Dependencies**: Waits for SQL Server and Qdrant to be healthy

## Database Migrations

Migrations run automatically when the backend container starts. The process:

1. Backend waits for SQL Server to be ready
2. Installs EF Core tools if needed
3. Runs `dotnet ef database update`
4. Starts the application

If you need to run migrations manually:
```bash
docker-compose exec backend dotnet ef database update
```

## Environment Variables

All sensitive configuration is managed through environment variables. See `.env.example` for a complete list.

### Critical Variables
- `DB_PASSWORD`: SQL Server SA password
- `JWT_SECRET_KEY`: JWT signing key (keep secret!)
- `AWS_ACCESS_KEY` / `AWS_SECRET_KEY`: AWS S3 credentials
- `OPENAI_API_KEY`: OpenAI API key for AI features

### Optional Variables (with defaults)
- `JWT_ISSUER`: Default "SWDGradingAPI"
- `JWT_AUDIENCE`: Default "SWDGradingClient"
- `JWT_EXPIRY_MINUTES`: Default 60
- `AWS_REGION`: Default "ap-southeast-1"
- `QDRANT_COLLECTION_NAME`: Default "exam_submissions"
- `OPENAI_MODEL`: Default "gpt-4o-mini"

## Common Commands

### Start services
```bash
docker-compose up -d
```

### Stop services
```bash
docker-compose down
```

### Stop and remove volumes (WARNING: Deletes all data)
```bash
docker-compose down -v
```

### View logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f sqlserver
docker-compose logs -f qdrant
```

### Rebuild backend after code changes
```bash
docker-compose build backend
docker-compose up -d backend
```

### Access SQL Server from host
```bash
sqlcmd -S localhost,1433 -U sa -P YourStr0ng!Pass
```

### Execute commands in running container
```bash
docker-compose exec backend bash
docker-compose exec sqlserver bash
```

## Troubleshooting

### Backend fails to start
1. Check if SQL Server is healthy:
   ```bash
   docker-compose ps
   ```

2. View backend logs:
   ```bash
   docker-compose logs backend
   ```

3. Ensure `.env` file exists with correct values

### Database connection errors
1. Verify SQL Server password in `.env` matches docker-compose
2. Check SQL Server logs:
   ```bash
   docker-compose logs sqlserver
   ```

### Qdrant connection issues
1. Verify Qdrant is running:
   ```bash
   curl http://localhost:6333/health
   ```

2. Check Qdrant logs:
   ```bash
   docker-compose logs qdrant
   ```

### Migration failures
Migrations run automatically, but if they fail:
1. Check the backend logs for detailed error messages
2. Manually run migrations:
   ```bash
   docker-compose exec backend dotnet ef database update
   ```

### Port conflicts
If ports 1433, 6333, 6334, or 8080 are already in use:
1. Stop conflicting services
2. Or modify ports in `docker-compose.yml`

## Data Persistence

Data is stored in Docker volumes:
- `sqlserver_data`: SQL Server database files
- `qdrant_storage`: Qdrant vector storage

To backup data:
```bash
# SQL Server backup
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE SWDGradingDB TO DISK = '/var/opt/mssql/backup/SWDGradingDB.bak'"

# Copy backup to host
docker cp sqlserver:/var/opt/mssql/backup/SWDGradingDB.bak ./backup/
```

## Production Considerations

1. **Security**:
   - Never commit `.env` file to git
   - Use strong passwords for DB_PASSWORD and JWT_SECRET_KEY
   - Consider using Docker secrets in production
   - Enable HTTPS in production

2. **Performance**:
   - Increase SQL Server memory limits if needed
   - Monitor Qdrant storage size
   - Consider using external database in production

3. **Monitoring**:
   - Set up health check endpoints
   - Monitor container logs
   - Use proper logging aggregation

4. **Backups**:
   - Schedule regular database backups
   - Backup Qdrant volumes periodically
   - Test restore procedures

## Network Architecture

All services communicate through a shared Docker network (`swd-grading-network`):
- Backend → SQL Server: `sqlserver:1433`
- Backend → Qdrant: `http://qdrant:6333`
- Host → Backend: `localhost:8080`
- Host → SQL Server: `localhost:1433`
- Host → Qdrant: `localhost:6333` and `localhost:6334`

## Updating the Application

When you make code changes:

```bash
# 1. Rebuild the Docker image
docker-compose build backend

# 2. Recreate the backend container
docker-compose up -d backend

# 3. Watch logs to ensure successful startup
docker-compose logs -f backend
```

Migrations will run automatically on startup.

