# 🚀 241 Runners Awareness - .NET Backend Maintenance Guide

## 📋 Table of Contents
1. [Performance Optimizations](#performance-optimizations)
2. [Database Maintenance](#database-maintenance)
3. [Security Best Practices](#security-best-practices)
4. [Monitoring & Health Checks](#monitoring--health-checks)
5. [Deployment Procedures](#deployment-procedures)
6. [Troubleshooting](#troubleshooting)
7. [Backup & Recovery](#backup--recovery)

---

## 🚀 Performance Optimizations

### Database Indexes Added
The following indexes have been added to improve query performance:

```sql
-- Individual queries
CREATE INDEX IX_Individuals_CaseStatus ON Individuals(CaseStatus);
CREATE INDEX IX_Individuals_State ON Individuals(State);
CREATE INDEX IX_Individuals_City ON Individuals(City);
CREATE INDEX IX_Individuals_LastSeenDate ON Individuals(LastSeenDate);
CREATE INDEX IX_Individuals_CreatedAt ON Individuals(CreatedAt);
CREATE INDEX IX_Individuals_Name ON Individuals(FirstName, LastName);
CREATE INDEX IX_Individuals_NAMUSCaseNumber ON Individuals(NAMUSCaseNumber);
CREATE INDEX IX_Individuals_LocalCaseNumber ON Individuals(LocalCaseNumber);
```

### Entity Framework Optimizations
- **No Tracking Queries**: Enabled for read-only operations
- **Connection Resilience**: Automatic retry on failure (3 attempts, 30s delay)
- **Command Timeout**: 30 seconds for all database operations
- **Multiple Active Result Sets**: Enabled for concurrent operations

### Response Compression
- **Brotli**: Primary compression for modern browsers
- **Gzip**: Fallback compression for older browsers
- **HTTPS Only**: Compression enabled for secure connections

### Rate Limiting
- **Global Limit**: 100 requests per minute per user/IP
- **Window**: 1-minute sliding window
- **Segments**: 10 segments for smooth distribution

---

## 🗄️ Database Maintenance

### Automated Cleanup Operations

#### 1. Test Data Removal
```bash
# API Endpoint
POST /api/cleanup/remove-test-data
Authorization: Bearer <admin-token>

# Removes:
# - Users with @test.com, @example.com, or +test in email
# - Individuals with test emails
# - Products with "TEST" in name/description/SKU
```

#### 2. Duplicate User Cleanup
```bash
# API Endpoint
POST /api/cleanup/remove-duplicates
Authorization: Bearer <admin-token>

# Removes duplicate users based on email
# Keeps the oldest record (earliest CreatedAt)
```

#### 3. Orphaned Records Cleanup
```bash
# API Endpoint
POST /api/cleanup/remove-orphaned
Authorization: Bearer <admin-token>

# Removes:
# - Emergency contacts without individuals
# - Case images without individuals
# - Case documents without individuals
```

#### 4. Full Cleanup Report
```bash
# API Endpoint
POST /api/cleanup/full-cleanup
Authorization: Bearer <admin-token>

# Returns detailed report:
{
  "success": true,
  "report": {
    "startedAt": "2025-01-27T10:00:00Z",
    "completedAt": "2025-01-27T10:00:05Z",
    "duration": "00:00:05",
    "testDataRemoved": 15,
    "duplicatesRemoved": 3,
    "orphanedRecordsRemoved": 8,
    "totalRecordsRemoved": 26
  }
}
```

### Manual SQL Cleanup Scripts

#### Test Data Cleanup
```sql
-- Remove test users
DELETE FROM Users 
WHERE Email LIKE '%@test.com' 
   OR Email LIKE '%@example.com' 
   OR Email LIKE '%+test%';

-- Remove test individuals
DELETE FROM Individuals 
WHERE Email LIKE '%@test.com' 
   OR Email LIKE '%@example.com' 
   OR Email LIKE '%+test%';

-- Remove test products
DELETE FROM Products 
WHERE Name LIKE '%TEST%' 
   OR Description LIKE '%TEST%' 
   OR Sku LIKE '%TEST%';
```

#### Duplicate Cleanup
```sql
-- Remove duplicate users (keep oldest)
DELETE FROM Users 
WHERE Id IN (
    SELECT u2.Id 
    FROM Users u1 
    JOIN Users u2 ON u1.Email = u2.Email 
    WHERE u1.Id < u2.Id
);
```

#### Orphaned Records Cleanup
```sql
-- Remove orphaned emergency contacts
DELETE FROM EmergencyContacts 
WHERE IndividualId NOT IN (SELECT Id FROM Individuals);

-- Remove orphaned case images
DELETE FROM CaseImages 
WHERE IndividualId NOT IN (SELECT Id FROM Individuals);

-- Remove orphaned case documents
DELETE FROM CaseDocuments 
WHERE IndividualId NOT IN (SELECT Id FROM Individuals);
```

---

## 🔒 Security Best Practices

### JWT Configuration
```json
{
  "Jwt": {
    "SecretKey": "CHANGE-IN-PRODUCTION-32+CHARACTERS",
    "Issuer": "241RunnersAwareness",
    "Audience": "241RunnersAwareness",
    "ExpiryInDays": 7,
    "RefreshTokenExpiryInDays": 30
  }
}
```

### Environment-Specific Settings
- **Development**: Use LocalDB with Windows Authentication
- **Staging**: Use SQL Server with service account
- **Production**: Use Azure SQL Database with managed identity

### CORS Configuration
```csharp
// Production domains only
"https://241runnersawareness.org",
"https://www.241runnersawareness.org",
"https://app.241runnersawareness.org"
```

---

## 📊 Monitoring & Health Checks

### Health Check Endpoints
```bash
# Overall health
GET /health

# Database connectivity
GET /health/ready

# Application liveness
GET /health/live
```

### Health Check Response
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database connection is working",
      "duration": "00:00:00.123"
    },
    {
      "name": "memory",
      "status": "Healthy",
      "description": "Memory usage is within limits",
      "duration": "00:00:00.001"
    }
  ]
}
```

### Logging Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    },
    "File": {
      "Path": "logs/app-{Date}.txt",
      "FileSizeLimitBytes": 10485760,
      "RetainedFileCountLimit": 30
    }
  }
}
```

---

## 🚀 Deployment Procedures

### Development Environment
```bash
# Start backend
cd backend
dotnet run

# Or use the provided scripts
./start-backend.ps1
```

### Staging Deployment
```bash
# Build and deploy
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish

# Run migrations
dotnet ef database update

# Start application
dotnet ./publish/241RunnersAwareness.BackendAPI.dll
```

### Production Deployment
```bash
# 1. Backup current database
sqlcmd -S your-server -d RunnersDb -E -Q "BACKUP DATABASE RunnersDb TO DISK = 'backup.bak'"

# 2. Deploy new version
dotnet publish --configuration Release --output ./publish

# 3. Run migrations
dotnet ef database update

# 4. Verify health checks
curl https://api.241runnersawareness.org/health

# 5. Start application
dotnet ./publish/241RunnersAwareness.BackendAPI.dll
```

---

## 🔧 Troubleshooting

### Common Issues

#### 1. Database Connection Issues
```bash
# Check connection string
# Verify SQL Server is running
# Check firewall settings
# Test with sqlcmd
sqlcmd -S your-server -d RunnersDb -E -Q "SELECT 1"
```

#### 2. Performance Issues
```bash
# Check slow queries
SELECT TOP 10 
    qs.execution_count,
    qs.total_elapsed_time / qs.execution_count as avg_elapsed_time,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1, 
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) as statement_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY avg_elapsed_time DESC;
```

#### 3. Memory Issues
```bash
# Check memory usage
GET /health

# Monitor with Application Insights
# Check for memory leaks in long-running operations
```

#### 4. Authentication Issues
```bash
# Verify JWT configuration
# Check token expiration
# Validate issuer/audience
# Test with Postman or curl
```

### Log Analysis
```bash
# Check application logs
tail -f logs/app-2025-01-27.txt

# Search for errors
grep -i "error" logs/app-*.txt

# Check for performance issues
grep -i "slow" logs/app-*.txt
```

---

## 💾 Backup & Recovery

### Automated Backups
```sql
-- Create backup job
USE msdb;
GO

EXEC dbo.sp_add_job
    @job_name = N'RunnersDb_Backup',
    @enabled = 1;

EXEC dbo.sp_add_jobstep
    @job_name = N'RunnersDb_Backup',
    @step_name = N'Backup Database',
    @subsystem = N'TSQL',
    @command = N'BACKUP DATABASE RunnersDb TO DISK = ''C:\Backups\RunnersDb_$(Get-Date -Format "yyyyMMdd_HHmmss").bak'' WITH COMPRESSION;';

-- Schedule daily backup at 2 AM
EXEC dbo.sp_add_schedule
    @schedule_name = N'Daily_2AM',
    @freq_type = 4, -- Daily
    @freq_interval = 1,
    @active_start_time = 020000; -- 2:00 AM

EXEC dbo.sp_attach_schedule
    @job_name = N'RunnersDb_Backup',
    @schedule_name = N'Daily_2AM';
```

### Recovery Procedures
```sql
-- Restore from backup
RESTORE DATABASE RunnersDb 
FROM DISK = 'C:\Backups\RunnersDb_20250127_020000.bak'
WITH REPLACE, RECOVERY;

-- Point-in-time recovery (if using full recovery model)
RESTORE DATABASE RunnersDb 
FROM DISK = 'C:\Backups\RunnersDb_20250127_020000.bak'
WITH NORECOVERY;

RESTORE LOG RunnersDb 
FROM DISK = 'C:\Backups\RunnersDb_20250127_030000.trn'
WITH RECOVERY, STOPAT = '2025-01-27 02:30:00';
```

### Data Export
```bash
# Export to CSV (using the CsvExportService)
GET /api/individuals/export
Authorization: Bearer <admin-token>

# SQL Server export
bcp "SELECT * FROM Individuals" queryout "individuals.csv" -c -t, -r\n -S your-server -d RunnersDb -E
```

---

## 📈 Performance Monitoring

### Key Metrics to Monitor
- **Response Time**: Average API response time < 500ms
- **Database Queries**: < 100ms for simple queries
- **Memory Usage**: < 1GB for typical load
- **CPU Usage**: < 70% average
- **Error Rate**: < 1% of requests

### Monitoring Tools
- **Application Insights**: Azure monitoring
- **SQL Server Profiler**: Database performance
- **Health Checks**: Built-in monitoring endpoints
- **Log Analytics**: Centralized logging

### Alerts
- Database connection failures
- High error rates (>5%)
- Memory usage > 80%
- Response time > 2 seconds
- Failed health checks

---

## 🎯 Best Practices Summary

### Daily Operations
1. ✅ Monitor health check endpoints
2. ✅ Review error logs
3. ✅ Check database performance
4. ✅ Verify backup completion

### Weekly Operations
1. ✅ Run cleanup operations
2. ✅ Review performance metrics
3. ✅ Update security patches
4. ✅ Test backup restoration

### Monthly Operations
1. ✅ Full system health review
2. ✅ Performance optimization
3. ✅ Security audit
4. ✅ Capacity planning

### Quarterly Operations
1. ✅ Disaster recovery testing
2. ✅ Security penetration testing
3. ✅ Performance benchmarking
4. ✅ Architecture review

---

## 📞 Support & Contacts

- **Technical Lead**: [Your Name]
- **Database Admin**: [DBA Name]
- **DevOps**: [DevOps Team]
- **Security**: [Security Team]

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)
- [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/)

---

*Last Updated: January 27, 2025*
*Version: 1.0.0*
