# 🚀 241 Runners Awareness - Developer Setup Guide

## **📋 Overview**

This guide helps developers set up access to the 241 Runners Awareness Azure backend and database.

## **🌐 API Endpoints**

### **Production API (Azure)**
- **Base URL**: `https://241runnersawareness-api.azurewebsites.net`
- **Health Check**: `https://241runnersawareness-api.azurewebsites.net/health`
- **Swagger Documentation**: `https://241runnersawareness-api.azurewebsites.net/swagger`
- **API Endpoints**: `https://241runnersawareness-api.azurewebsites.net/api/*`

### **Development API (Local)**
- **Base URL**: `http://localhost:5113`
- **Health Check**: `http://localhost:5113/health`
- **Swagger Documentation**: `http://localhost:5113/swagger`

## **🗄️ Database Access**

### **Azure SQL Database**
- **Server**: `241runners-sql-server-2025.database.windows.net`
- **Database**: `RunnersDb`
- **Username**: `sqladmin`
- **Password**: `YourStrongPassword123!`
- **Location**: Central US

### **Connection String**
```
Server=tcp:241runners-sql-server-2025.database.windows.net,1433;
Initial Catalog=RunnersDb;
Persist Security Info=False;
User ID=sqladmin;
Password=YourStrongPassword123!;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

## **👥 Adding Developer Access**

### **Step 1: Get Your IP Address**
Visit: https://whatismyipaddress.com/ or run:
```bash
curl ifconfig.me
```

### **Step 2: Add Your IP to Firewall**
Run the PowerShell script:
```powershell
.\backend\add-developer-access.ps1 -DeveloperName "YourName" -IpAddress "YOUR_IP_ADDRESS"
```

Example:
```powershell
.\backend\add-developer-access.ps1 -DeveloperName "JohnDoe" -IpAddress "192.168.1.100"
```

### **Step 3: Test Database Connection**
Use SQL Server Management Studio (SSMS) or Azure Data Studio:
- Server: `241runners-sql-server-2025.database.windows.net`
- Authentication: SQL Server Authentication
- Login: `sqladmin`
- Password: `YourStrongPassword123!`

## **🔧 Frontend Configuration**

### **Environment Variables**
The frontend is configured to use:
- **Development**: `http://localhost:5113/api`
- **Production**: `https://241runnersawareness-api.azurewebsites.net/api`

### **Local Development**
1. Clone the repository
2. Install dependencies: `npm install`
3. Start development server: `npm run dev`
4. Frontend will automatically connect to local backend

### **Production Testing**
1. Build the frontend: `npm run build`
2. Deploy to your hosting platform
3. Frontend will connect to Azure API automatically

## **🛠️ Backend Development**

### **Local Setup**
1. Navigate to `backend/` directory
2. Update `appsettings.Development.json` with local database connection
3. Run: `dotnet run`
4. API will be available at `http://localhost:5113`

### **Database Migrations**
```bash
cd backend
dotnet ef migrations add MigrationName
dotnet ef database update
```

### **Deploy to Azure**
```bash
cd backend
dotnet publish -c Release -o ./publish
Compress-Archive -Path "./publish/*" -DestinationPath "deployment.zip"
az webapp deployment source config-zip --resource-group "241runnersawareness-rg" --name "241runnersawareness-api" --src "deployment.zip"
```

## **🔐 Authentication**

### **JWT Configuration**
- **Issuer**: `241RunnersAwareness`
- **Audience**: `241RunnersAwareness`
- **Secret Key**: Configured in Azure App Settings
- **Token Expiry**: 7 days
- **Refresh Token Expiry**: 30 days

### **Testing Authentication**
1. Use Swagger UI: `https://241runnersawareness-api.azurewebsites.net/swagger`
2. Register/Login to get JWT token
3. Use token in Authorization header: `Bearer YOUR_TOKEN`

## **📊 Monitoring & Health Checks**

### **Health Endpoints**
- **Overall Health**: `/health`
- **Database Health**: `/health/database`
- **Memory Usage**: `/health/memory`

### **Azure Monitoring**
- **Application Insights**: Available in Azure Portal
- **Logs**: Available in App Service logs
- **Metrics**: CPU, Memory, Response times

## **🚨 Troubleshooting**

### **Common Issues**

#### **Database Connection Failed**
- Check if your IP is added to firewall
- Verify connection string
- Ensure SQL Server is running

#### **CORS Errors**
- Check if frontend domain is in CORS policy
- Verify API URL configuration
- Check browser console for errors

#### **Authentication Issues**
- Verify JWT configuration
- Check token expiration
- Ensure correct issuer/audience

### **Support**
- **Backend Issues**: Check Azure App Service logs
- **Database Issues**: Check SQL Server firewall rules
- **Frontend Issues**: Check browser console and network tab

## **📝 Development Workflow**

### **1. Local Development**
```bash
# Backend
cd backend
dotnet run

# Frontend (new terminal)
cd frontend
npm run dev
```

### **2. Testing**
- Use Swagger UI for API testing
- Use browser dev tools for frontend testing
- Test database connections with SSMS

### **3. Deployment**
- Backend: Deploy to Azure using deployment script
- Frontend: Build and deploy to hosting platform
- Database: Run migrations as needed

## **🔗 Useful Links**

- **Azure Portal**: https://portal.azure.com
- **App Service**: https://241runnersawareness-api.azurewebsites.net
- **Swagger UI**: https://241runnersawareness-api.azurewebsites.net/swagger
- **Health Check**: https://241runnersawareness-api.azurewebsites.net/health
- **SQL Server**: 241runners-sql-server-2025.database.windows.net

---

**Need Help?** Contact the development team or check the Azure Portal for detailed logs and monitoring information.
