# 🚀 241 Runners Awareness - Deployment Architecture

## Overview
This project uses a modern deployment architecture with:
- **Frontend**: Static site hosted on GitHub Pages
- **Backend**: ASP.NET Core API hosted on Azure App Service
- **Database**: Azure SQL Database
- **CI/CD**: GitHub Actions for automated deployment

## Architecture Diagram
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   GitHub Pages  │    │  Azure App       │    │  Azure SQL      │
│   (Frontend)    │◄──►│  Service         │◄──►│  Database       │
│                 │    │  (Backend API)   │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  Static Files   │    │  ASP.NET Core    │    │  SQL Server     │
│  (HTML/CSS/JS)  │    │  Web API         │    │  Database       │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## URLs
- **Frontend**: `https://241runnersawareness.org` (GitHub Pages)
- **Backend API**: `https://241runnersawareness-api.azurewebsites.net`
- **Database**: `241runners-sql-server-2025.database.windows.net`

## GitHub Actions Workflows

### 1. Frontend Deployment (`.github/workflows/frontend-deploy.yml`)
- **Trigger**: Changes to frontend files, HTML, CSS, JS
- **Actions**:
  - Build React/Vite application
  - Deploy to GitHub Pages
  - Test deployment

### 2. Backend Deployment (`.github/workflows/azure-deploy.yml`)
- **Trigger**: Changes to backend files
- **Actions**:
  - Build .NET application
  - Run tests
  - Deploy to Azure App Service
  - Run database migrations
  - Test API endpoints

## Required GitHub Secrets

### For Backend Deployment:
```bash
AZURE_WEBAPP_PUBLISH_PROFILE=your-azure-publish-profile
AZURE_SQL_CONNECTION_STRING=Server=241runners-sql-server-2025.database.windows.net;Database=RunnersDb;User Id=sqladmin;Password=YourPassword;TrustServerCertificate=True;
```

### For Frontend Deployment:
- GitHub Pages is automatically configured when the workflow runs

## Environment Variables

### Frontend (Vite)
```env
VITE_API_BASE_URL=https://241runnersawareness-api.azurewebsites.net/api
VITE_APP_URL=https://241runnersawareness.org
VITE_GOOGLE_CLIENT_ID=933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com
```

### Backend (Azure App Service)
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=241runners-sql-server-2025.database.windows.net;Database=RunnersDb;User Id=sqladmin;Password=YourPassword;TrustServerCertificate=True;
JWT__SecretKey=your-jwt-secret-key
JWT__Issuer=241RunnersAwareness
JWT__Audience=241RunnersAwareness
App__BaseUrl=https://241runnersawareness.org
App__ApiUrl=https://241runnersawareness-api.azurewebsites.net
```

## Deployment Process

### 1. Frontend Deployment
1. Push changes to `main` branch
2. GitHub Actions builds the React app
3. Deploys to GitHub Pages
4. Available at `https://241runnersawareness.org`

### 2. Backend Deployment
1. Push changes to `main` branch
2. GitHub Actions builds .NET app
3. Runs tests
4. Deploys to Azure App Service
5. Runs database migrations
6. Tests API endpoints

## Database Schema
- **Database**: `RunnersDb`
- **Tables**: Users, Cases, Individuals, DNAReports, etc.
- **Migrations**: Entity Framework Core migrations

## Monitoring & Health Checks
- **Health Endpoint**: `https://241runnersawareness-api.azurewebsites.net/health`
- **Swagger Docs**: `https://241runnersawareness-api.azurewebsites.net/swagger`
- **Auth Test**: `https://241runnersawareness-api.azurewebsites.net/api/auth/test`

## Security
- **HTTPS**: All endpoints use HTTPS
- **CORS**: Configured for production domains
- **JWT**: Token-based authentication
- **Rate Limiting**: Configured on API endpoints

## Backup & Recovery
- **Database**: Azure SQL automatic backups
- **Code**: GitHub repository backup
- **Configuration**: Environment variables in Azure

## Troubleshooting

### Common Issues:
1. **Database Connection**: Check Azure SQL firewall rules
2. **CORS Errors**: Verify allowed origins in backend config
3. **Build Failures**: Check GitHub Actions logs
4. **Deployment Failures**: Verify Azure credentials

### Debug Commands:
```bash
# Test backend health
curl https://241runnersawareness-api.azurewebsites.net/health

# Test auth endpoint
curl https://241runnersawareness-api.azurewebsites.net/api/auth/test

# Test frontend
curl https://241runnersawareness.org
```
