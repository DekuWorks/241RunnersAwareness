# 🔧 STEP-BY-STEP DATABASE FIX

## 🚨 **SQL SCRIPT FAILED - TROUBLESHOOTING GUIDE**

Since the SQL script failed, let's fix this step by step to identify the exact issue.

### **Step 1: Check What Tables Already Exist**

First, let's see what tables are already in your database. Run this query:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### **Step 2: Create Tables One by One**

Instead of running the entire script, let's create tables one by one to identify the exact error.

#### **Create Cases Table First:**

```sql
-- Create Cases table
CREATE TABLE Cases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CaseNumber NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    Category NVARCHAR(100),
    Location NVARCHAR(255),
    Latitude DECIMAL(10,8),
    Longitude DECIMAL(11,8),
    CreatedBy INT NOT NULL,
    AssignedTo INT,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    ClosedAt DATETIME2,
    IsPublic BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    Tags NVARCHAR(500),
    Notes NVARCHAR(MAX),
    Evidence NVARCHAR(MAX),
    ContactInfo NVARCHAR(255),
    ContactPhone NVARCHAR(20),
    ContactEmail NVARCHAR(255),
    Resolution NVARCHAR(MAX)
);
```

#### **Add Foreign Key Constraints:**

```sql
-- Add foreign key constraints
ALTER TABLE Cases ADD CONSTRAINT FK_Cases_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
ALTER TABLE Cases ADD CONSTRAINT FK_Cases_AssignedTo FOREIGN KEY (AssignedTo) REFERENCES Users(Id);
```

#### **Create Devices Table:**

```sql
-- Create Devices table
CREATE TABLE Devices (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DeviceId NVARCHAR(255) NOT NULL,
    DeviceType NVARCHAR(50) NOT NULL,
    DeviceName NVARCHAR(255),
    Platform NVARCHAR(50),
    Version NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    LastSeen DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PushToken NVARCHAR(500)
);

-- Add foreign key constraint
ALTER TABLE Devices ADD CONSTRAINT FK_Devices_UserId FOREIGN KEY (UserId) REFERENCES Users(Id);
```

#### **Create Reports Table:**

```sql
-- Create Reports table
CREATE TABLE Reports (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Type NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Draft',
    GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Data NVARCHAR(MAX),
    FilePath NVARCHAR(500),
    IsPublic BIT NOT NULL DEFAULT 0
);

-- Add foreign key constraint
ALTER TABLE Reports ADD CONSTRAINT FK_Reports_UserId FOREIGN KEY (UserId) REFERENCES Users(Id);
```

#### **Create FileUploads Table:**

```sql
-- Create FileUploads table
CREATE TABLE FileUploads (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    OriginalFileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Add foreign key constraint
ALTER TABLE FileUploads ADD CONSTRAINT FK_FileUploads_UserId FOREIGN KEY (UserId) REFERENCES Users(Id);
```

### **Step 3: Insert Sample Data**

After creating the tables, insert some sample data:

```sql
-- Insert sample case
INSERT INTO Cases (CaseNumber, Title, Description, Status, Priority, Category, CreatedBy, Location)
VALUES ('CASE-001', 'Missing Person Report', 'Report of missing individual in downtown area', 'Open', 'High', 'Missing Person', 1, 'Downtown Area');

-- Insert sample device
INSERT INTO Devices (UserId, DeviceId, DeviceType, DeviceName, Platform, Version)
VALUES (1, 'TEST-DEVICE-001', 'Mobile', 'Test Device', 'iOS', '17.0');

-- Insert sample report
INSERT INTO Reports (UserId, Title, Description, Type, Status)
VALUES (1, 'Monthly Activity Report', 'Monthly summary of all activities', 'Summary', 'Draft');
```

### **Step 4: Verify Tables Were Created**

Run this query to verify all tables exist:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Users', 'Cases', 'Runners', 'Notifications', 'Topics', 'Devices', 'Reports', 'FileUploads')
ORDER BY TABLE_NAME;
```

## 🔍 **COMMON ERROR SOLUTIONS**

### **Error 1: "Invalid column name"**
- **Solution**: Check if the Users table exists and has the correct column names
- **Fix**: Run `SELECT * FROM Users` to see the table structure

### **Error 2: "Foreign key constraint"**
- **Solution**: Create tables without foreign keys first, then add them
- **Fix**: Use the step-by-step approach above

### **Error 3: "Permission denied"**
- **Solution**: Make sure you're using the correct database user
- **Fix**: Check your connection string and permissions

### **Error 4: "Table already exists"**
- **Solution**: Check if tables already exist
- **Fix**: Use `DROP TABLE IF EXISTS` or skip existing tables

## 🚀 **TESTING AFTER FIX**

After creating the tables, test the API:

```bash
node verify-database-fix.js
```

## 📋 **TROUBLESHOOTING CHECKLIST**

- [ ] Check what tables already exist
- [ ] Create Cases table first
- [ ] Add foreign key constraints
- [ ] Create Devices table
- [ ] Create Reports table
- [ ] Create FileUploads table
- [ ] Insert sample data
- [ ] Verify all tables exist
- [ ] Test API endpoints

## 🎯 **EXPECTED RESULTS**

After successfully creating the tables:
- Cases API: ✅ 200 status
- Devices API: ✅ 200 status
- My Cases API: ✅ 200 status
- Overall Success Rate: 60%+ (5-6/9 endpoints working)

**This step-by-step approach will help identify exactly where the error occurs and fix it!**
