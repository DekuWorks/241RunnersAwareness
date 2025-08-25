# 🗄️ SQLite Development Guide - 241 Runners Awareness

## Overview
This guide helps your development team work with SQLite databases across Windows and Mac platforms using DB Browser for SQLite.

## 🚀 Quick Start

### 1. Install DB Browser for SQLite

**Windows:**
- Download from: https://sqlitebrowser.org/dl/
- Choose the latest stable version
- Run the installer

**Mac:**
```bash
# Using Homebrew
brew install --cask db-browser-for-sqlite

# Or download from: https://sqlitebrowser.org/dl/
```

### 2. Get the Database File

**Option A: Download from Azure (Recommended)**
```powershell
# Run the download script
.\download-database.ps1
```

**Option B: Manual Download**
1. Go to Azure Portal → App Services → 241runnersawareness-api
2. Navigate to "Advanced Tools" → "SSH"
3. Run: `ls -la RunnersDb.db`
4. Download the file using the file browser

### 3. Open Database in DB Browser

1. Launch DB Browser for SQLite
2. Click "Open Database"
3. Select `RunnersDb.db`
4. Explore the database structure

## 📊 Database Schema

### Tables Overview

| Table | Purpose | Key Fields |
|-------|---------|------------|
| **Users** | Authentication & user management | UserId, Email, Role, PasswordHash |
| **Individuals** | Missing persons data | IndividualId, FullName, Status, Location |
| **Cases** | Case management | CaseId, Title, Description, Status |
| **DNAReports** | DNA tracking | ReportId, IndividualId, Results |
| **EmergencyContacts** | Emergency contact info | ContactId, IndividualId, Phone |

### Key Relationships

```
Users (1) ←→ (Many) Cases
Individuals (1) ←→ (Many) DNAReports
Individuals (1) ←→ (Many) EmergencyContacts
Cases (1) ←→ (Many) Individuals
```

## 🔧 Development Workflow

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd 241RunnersAwareness
   ```

2. **Get the database file**
   ```powershell
   .\download-database.ps1
   ```

3. **Start the backend**
   ```bash
   cd backend
   dotnet run
   ```

4. **Start the frontend**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

### Database Changes

1. **Create a migration**
   ```bash
   cd backend
   dotnet ef migrations add MigrationName
   ```

2. **Apply to local database**
   ```bash
   dotnet ef database update
   ```

3. **Test changes in DB Browser**
   - Open the updated database
   - Verify new tables/columns
   - Test data relationships

4. **Deploy to production**
   ```bash
   dotnet ef database update
   # Then redeploy the application
   ```

## 🛠️ Common Tasks

### Viewing Data

**In DB Browser:**
1. Click "Browse Data" tab
2. Select the table you want to view
3. Use filters and sorting as needed

**Using SQL:**
```sql
-- View all users
SELECT * FROM Users;

-- View missing persons
SELECT * FROM Individuals WHERE Status = 'Missing';

-- View cases with user info
SELECT c.Title, u.FullName 
FROM Cases c 
JOIN Users u ON c.UserId = u.UserId;
```

### Adding Test Data

**Using DB Browser:**
1. Click "Browse Data" tab
2. Select the table
3. Click "New Record" button
4. Fill in the required fields
5. Click "Write Changes"

**Using SQL:**
```sql
-- Add a test user
INSERT INTO Users (UserId, Username, Email, FullName, PasswordHash, Role)
VALUES (NEWID(), 'testuser', 'test@example.com', 'Test User', 'hashedpassword', 'user');

-- Add a test individual
INSERT INTO Individuals (IndividualId, FullName, DateOfBirth, Gender, Status)
VALUES (NEWID(), 'John Doe', '1990-01-01', 'Male', 'Missing');
```

### Backup and Restore

**Backup:**
1. In DB Browser, go to "File" → "Export" → "Database to SQL file"
2. Save the SQL file
3. Share with team members

**Restore:**
1. In DB Browser, go to "File" → "Import" → "Database from SQL file"
2. Select the SQL file
3. Execute the import

## 🔐 Security Best Practices

### Database Security

1. **Never commit database files to Git**
   ```gitignore
   # Add to .gitignore
   *.db
   *.sqlite
   *.sqlite3
   ```

2. **Use environment variables for sensitive data**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=RunnersDb.db"
     }
   }
   ```

3. **Regular backups**
   - Download database weekly
   - Store backups securely
   - Test restore procedures

### Development Security

1. **Use test data only**
   - Never use real personal information
   - Use fake names and addresses
   - Generate test data programmatically

2. **Secure API keys**
   - Store keys in environment variables
   - Never hardcode in source code
   - Use different keys for dev/prod

## 🐛 Troubleshooting

### Common Issues

**Database locked:**
- Close DB Browser
- Restart the application
- Check for other processes using the file

**Migration errors:**
```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

**Connection issues:**
- Verify database file path
- Check file permissions
- Ensure no other processes are using the file

### Performance Tips

1. **Index frequently queried columns**
   ```sql
   CREATE INDEX idx_users_email ON Users(Email);
   CREATE INDEX idx_individuals_status ON Individuals(Status);
   ```

2. **Use appropriate data types**
   - Use TEXT for strings
   - Use INTEGER for numbers
   - Use BLOB for binary data

3. **Optimize queries**
   - Use WHERE clauses
   - Limit result sets
   - Use JOINs instead of subqueries

## 📱 Cross-Platform Compatibility

### File Sharing

1. **Use cloud storage** (Google Drive, Dropbox, OneDrive)
2. **Use Git LFS** for large database files
3. **Use shared network drives**

### Version Control

1. **Track schema changes** (migrations)
2. **Don't track data files**
3. **Use seed data scripts**

### Team Collaboration

1. **Regular sync meetings**
2. **Document schema changes**
3. **Share database snapshots**

## 🚀 Production Deployment

### Database Deployment

1. **Upload database file to Azure**
   ```bash
   az webapp files upload --name 241runnersawareness-api --resource-group 241runnersawareness-rg --source-path ./RunnersDb.db --target-path /home/site/wwwroot/RunnersDb.db
   ```

2. **Set environment variables**
   ```bash
   az webapp config appsettings set --name 241runnersawareness-api --resource-group 241runnersawareness-rg --settings ConnectionStrings__DefaultConnection="Data Source=RunnersDb.db"
   ```

3. **Restart the application**
   ```bash
   az webapp restart --name 241runnersawareness-api --resource-group 241runnersawareness-rg
   ```

### Monitoring

1. **Check database size**
2. **Monitor query performance**
3. **Backup regularly**
4. **Test restore procedures**

## 📚 Additional Resources

- [DB Browser for SQLite Documentation](https://sqlitebrowser.org/docs/)
- [SQLite Official Documentation](https://www.sqlite.org/docs.html)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)

## 🆘 Support

For technical support:
- Check the troubleshooting section above
- Review Azure App Service logs
- Contact the development team
- Check the project documentation

---

**Last Updated**: January 27, 2025  
**Version**: 1.0.0  
**Status**: Ready for Development Team Use
