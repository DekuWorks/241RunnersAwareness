# 241RA API Backend

This is the .NET 8 Web API backend for the 241 Runners Awareness project.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, Express, or Developer Edition)

## Quick Start

### 1. Database Setup

The API requires a SQL Server database. You have several options:

#### Option A: Use LocalDB (Recommended for development)
```bash
# LocalDB is usually installed with Visual Studio or SQL Server Express
# The connection string in appsettings.json should work out of the box
```

#### Option B: Use SQL Server Express
1. Download and install SQL Server Express
2. Update the connection string in `appsettings.json` if needed

#### Option C: Use Azure SQL Database
1. Create an Azure SQL Database
2. Update the connection string in `appsettings.json`

### 2. Database Migration

Run the following commands to create the database:

```bash
cd apps/api/Api
dotnet ef database update
```

### 3. Run the API

```bash
cd apps/api/Api
dotnet run
```

The API will be available at `http://localhost:5113`

## Default Admin User

The API automatically creates an admin user on first run:
- Email: `admin@example.com`
- Password: `ChangeMe123!`

## API Endpoints

- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Refresh JWT token
- `GET /api/auth/me` - Get current user info
- `GET /api/admin/users` - List users (Admin only)
- `PUT /api/admin/users/{id}/disable` - Disable user (Admin only)

## Configuration

Update `appsettings.json` to configure:
- Database connection string
- JWT settings (Issuer, Audience, Key, etc.)
- CORS origins

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Check the connection string in `appsettings.json`
- Verify you have the necessary permissions

### JWT Key Issues
- Update the JWT Key in `appsettings.json` with a secure random string
- The default key should be replaced in production

### CORS Issues
- Update the CORS origins in `Program.cs` to match your frontend URLs
