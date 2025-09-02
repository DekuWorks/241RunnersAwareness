# Database Setup Instructions

## Creating the Runners Table

This document provides instructions for creating the Runners table in the Azure SQL Database.

### Prerequisites

1. **Azure SQL Database Access**: You need access to the Azure SQL Database
2. **SQL Client**: Azure Data Studio, SQL Server Management Studio, or Azure Portal Query Editor
3. **Database Connection**: Connection string to the Azure SQL Database

### Connection Details

- **Server**: `241runners-sql-2025.database.windows.net`
- **Database**: `241RunnersAwarenessDB`
- **Username**: `241runners-admin`
- **Password**: `241Runners2025!`

### Option 1: Azure Portal Query Editor (Recommended)

1. **Login to Azure Portal**: Go to [portal.azure.com](https://portal.azure.com)
2. **Navigate to SQL Database**: Find the `241RunnersAwarenessDB` database
3. **Open Query Editor**: Click on "Query editor" in the left menu
4. **Connect**: Use the admin credentials to connect
5. **Execute Script**: Copy and paste the contents of `create_runners_table_simple.sql`
6. **Run**: Click "Run" to execute the script

### Option 2: Azure Data Studio

1. **Install Azure Data Studio**: Download from [docs.microsoft.com](https://docs.microsoft.com/en-us/sql/azure-data-studio/)
2. **Connect to Database**: Use the connection details above
3. **Open Script**: Open `create_runners_table_simple.sql`
4. **Execute**: Press F5 or click "Run" to execute

### Option 3: SQL Server Management Studio

1. **Connect to Database**: Use the connection details above
2. **Open Script**: Open `create_runners_table_simple.sql`
3. **Execute**: Press F5 or click "Execute" to run

### What the Script Does

1. **Creates Runners Table**: Creates the main table with all necessary columns
2. **Adds Indexes**: Creates performance indexes for common queries
3. **Inserts Sample Data**: Adds 3 sample runners for testing
4. **Verifies Creation**: Shows confirmation and table structure

### Expected Output

After running the script, you should see:

```
Status                                    TotalRunners
---------------------------------------- ------------
Runners table created successfully        3

COLUMN_NAME           DATA_TYPE    IS_NULLABLE CHARACTER_MAXIMUM_LENGTH
-------------------- ------------ ----------- -----------------------
Id                   int          NO          NULL
FirstName            nvarchar     NO          100
LastName             nvarchar     NO          100
RunnerId             nvarchar     NO          50
Age                  int          NO          NULL
Gender               nvarchar     YES         50
Status               nvarchar     NO          50
City                 nvarchar     NO          100
State                nvarchar     NO          50
Address              nvarchar     YES         500
Description          nvarchar     YES         500
ContactInfo          nvarchar     YES         200
DateReported         datetime2    NO          NULL
DateFound            datetime2    YES         NULL
LastSeen             datetime2    YES         NULL
DateOfBirth          datetime2    YES         NULL
Tags                 nvarchar     YES         500
IsActive             bit          NO          NULL
IsUrgent             bit          NO          NULL
CreatedAt            datetime2    NO          NULL
UpdatedAt            datetime2    YES         NULL
Height               nvarchar     YES         50
Weight               nvarchar     YES         50
HairColor            nvarchar     YES         50
EyeColor             nvarchar     YES         50
IdentifyingMarks     nvarchar     YES         500
MedicalConditions    nvarchar     YES         1000
Medications          nvarchar     YES         500
Allergies            nvarchar     YES         500
EmergencyContacts    nvarchar     YES         500
ReportedByUserId     int          YES         NULL
```

### Testing the API

After creating the table, test the API endpoints:

```bash
# Test health endpoint
curl -s "https://241runners-api.azurewebsites.net/api/auth/health" | jq .

# Test runners endpoint
curl -s "https://241runners-api.azurewebsites.net/api/runners" | jq .

# Test creating a new runner
curl -X POST "https://241runners-api.azurewebsites.net/api/runners" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"Runner","age":30,"gender":"Male","city":"Austin","state":"TX","description":"Test runner","contactInfo":"555-000-0000"}' | jq .
```

### Troubleshooting

#### Error: "Invalid object name 'Runners'"
- **Cause**: Table doesn't exist
- **Solution**: Run the SQL script to create the table

#### Error: "Cannot open server"
- **Cause**: Firewall blocking access
- **Solution**: Use Azure Portal Query Editor instead

#### Error: "Foreign key constraint"
- **Cause**: Users table doesn't exist
- **Solution**: Use the simplified script (`create_runners_table_simple.sql`)

### Next Steps

After successfully creating the table:

1. **Test API Endpoints**: Verify all endpoints work correctly
2. **Create Test Data**: Add more sample runners if needed
3. **User Testing**: Test the complete user flow
4. **Monitor Performance**: Check query performance with real data

### Files

- `create_runners_table.sql` - Full version with foreign key constraints
- `create_runners_table_simple.sql` - Simplified version without foreign keys
- `DATABASE_SETUP.md` - This instruction file 