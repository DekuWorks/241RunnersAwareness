# 🚀 241 Runners Awareness - Deployment Architecture

## ✅ **CURRENT STATUS: FULLY OPERATIONAL**

All systems are live and working:
- ✅ **Frontend**: GitHub Pages (https://241runnersawareness.org)
- ✅ **Backend**: Azure App Service (https://241runnersawareness-api.azurewebsites.net)
- ✅ **Database**: Azure SQL Database
- ✅ **API Endpoints**: All working correctly

## 🏗️ **Architecture Overview**

```
┌─────────────────────────────────────────────────────────────────┐
│                    GITHUB REPOSITORY                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   Frontend      │  │   Backend       │  │   GitHub        │ │
│  │   (React/Vite)  │  │   (.NET Core)   │  │   Actions       │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DEPLOYMENT PIPELINE                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   GitHub Pages  │  │   Azure App     │  │   Azure SQL     │ │
│  │   (Static Site) │  │   Service       │  │   Database      │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## 🌐 **Live URLs**

| Component | URL | Status |
|-----------|-----|--------|
| **Frontend** | https://241runnersawareness.org | ✅ Online |
| **Backend API** | https://241runnersawareness-api.azurewebsites.net | ✅ Online |
| **Health Check** | https://241runnersawareness-api.azurewebsites.net/health | ✅ Working |
| **Swagger Docs** | https://241runnersawareness-api.azurewebsites.net/swagger | ✅ Available |
| **Auth Test** | https://241runnersawareness-api.azurewebsites.net/api/auth/test | ✅ Working |

## 🔧 **GitHub Actions Workflows**

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

## 📋 **Required GitHub Secrets**

### For Backend Deployment:
```bash
AZURE_WEBAPP_PUBLISH_PROFILE=your-azure-publish-profile
AZURE_SQL_CONNECTION_STRING=Server=241runners-sql-server-2025.database.windows.net;Database=RunnersDb;User Id=sqladmin;Password=YourPassword;TrustServerCertificate=True;
```

## 🔧 **Environment Configuration**

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

## 🚀 **Deployment Process**

### Frontend Deployment
1. Push changes to `main` branch
2. GitHub Actions builds the React app
3. Deploys to GitHub Pages
4. Available at `https://241runnersawareness.org`

### Backend Deployment
1. Push changes to `main` branch
2. GitHub Actions builds .NET app
3. Runs tests
4. Deploys to Azure App Service
5. Runs database migrations
6. Tests API endpoints

## 🗄️ **Database Schema**
- **Database**: `RunnersDb`
- **Server**: `241runners-sql-server-2025.database.windows.net`
- **Tables**: Users, Cases, Individuals, DNAReports, etc.
- **Migrations**: Entity Framework Core migrations

## 🔒 **Security Features**
- **HTTPS**: All endpoints use HTTPS
- **CORS**: Configured for production domains
- **JWT**: Token-based authentication
- **Rate Limiting**: Configured on API endpoints
- **SQL Injection Protection**: Entity Framework Core

## 📊 **Monitoring & Health Checks**
- **Health Endpoint**: `/health`
- **Swagger Documentation**: `/swagger`
- **Auth Test**: `/api/auth/test`
- **Database Connection**: Monitored via health checks

## 🔄 **Backup & Recovery**
- **Database**: Azure SQL automatic backups
- **Code**: GitHub repository backup
- **Configuration**: Environment variables in Azure
- **Deployment History**: GitHub Actions logs

## 🛠️ **Troubleshooting Tools**

### Deployment Status Checker
```powershell
# Run the status checker
.\check-deployment-status.ps1 -TestAPI

# For verbose output
.\check-deployment-status.ps1 -Verbose -TestAPI
```

### Manual Testing Commands
```bash
# Test frontend
curl https://241runnersawareness.org

# Test backend health
curl https://241runnersawareness-api.azurewebsites.net/health

# Test auth endpoint
curl https://241runnersawareness-api.azurewebsites.net/api/auth/test
```

## 📈 **Performance & Scalability**
- **Frontend**: Static files served by GitHub Pages CDN
- **Backend**: Azure App Service with auto-scaling
- **Database**: Azure SQL Database with performance tiers
- **Caching**: Browser caching for static assets

## 🔄 **CI/CD Pipeline**
- **Automated Testing**: Runs on every push
- **Automated Deployment**: Deploys on successful tests
- **Rollback Capability**: Previous versions available
- **Environment Separation**: Development/Production

## 📞 **Support & Maintenance**
- **Monitoring**: Azure Application Insights
- **Logging**: Structured logging with Serilog
- **Alerts**: Azure Monitor alerts
- **Documentation**: Swagger API documentation

## 🎯 **Next Steps**
1. **Monitor deployments** in GitHub Actions
2. **Set up monitoring** for your application
3. **Configure backups** for your database
4. **Set up alerts** for deployment failures
5. **Document your deployment process**

---

## ✅ **VERIFICATION CHECKLIST**

- [x] Frontend accessible at https://241runnersawareness.org
- [x] Backend API responding at https://241runnersawareness-api.azurewebsites.net
- [x] Health check endpoint working
- [x] Auth controller responding
- [x] Database connection established
- [x] GitHub Actions workflows configured
- [x] CORS properly configured
- [x] HTTPS enabled on all endpoints
- [x] Custom domain configured
- [x] Swagger documentation available

**🎉 Your deployment architecture is fully operational and ready for production use!**
