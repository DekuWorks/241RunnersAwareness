# 241 Runners API - Staging Slot Deployment Guide

## 🎯 **Overview**

This guide provides complete instructions for configuring and deploying the .NET API to Azure App Service staging slot. The staging slot provides a safe environment for testing deployments before they go to production.

## 🏗️ **Architecture**

```
GitHub Repository (main branch)
    ↓
GitHub Actions Workflow
    ↓
Azure App Service Staging Slot
    ↓ (after validation)
Azure App Service Production
```

## 🚀 **Quick Start**

### **Option 1: Automated Configuration (Recommended)**
```bash
# Run the automated configuration script
./scripts/configure-staging-slot.sh
```

### **Option 2: Manual Configuration**
Follow the detailed steps in `docs/dotnet-staging-configuration.md`

## 📋 **Prerequisites**

### **Azure Resources**
- [ ] Azure App Service `241runners-api` exists
- [ ] Staging slot `staging` is created
- [ ] Azure SQL Database is accessible
- [ ] Azure CLI is installed and authenticated

### **GitHub Configuration**
- [ ] GitHub repository has proper secrets configured
- [ ] GitHub Actions workflow is set up
- [ ] Service principal has proper permissions

## 🔧 **Configuration Details**

### **Staging Slot Settings**

#### **General Configuration**
```
Stack: .NET
Major Version: 8
Minor Version: 0
Startup Command: dotnet 241RunnersAPI.dll
Platform: 64 Bit
Always On: Enabled
HTTP Version: 2.0
ARR Affinity: Disabled
```

#### **Application Settings**
```
ASPNETCORE_ENVIRONMENT = Staging
ASPNETCORE_URLS = http://0.0.0.0:8080
WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
WEBSITES_PORT = 8080
WEBSITES_CONTAINER_START_TIME_LIMIT = 1800
```

#### **JWT Configuration (Staging Isolation)**
```
JWT_ISSUER = 241RunnersAwareness-Staging
JWT_AUDIENCE = 241RunnersAwareness-Staging
JWT_KEY = 241RunnersAwareness2024-Staging-SuperSecure-JWT-Key-For-Authentication-With-Enhanced-Security-Staging-Environment-Only
```

#### **Database Connection**
```
DefaultConnection = [Production connection string - can be changed to separate staging DB]
```

#### **CORS Configuration**
```
Allowed Origins:
- https://241runnersawareness.org
- https://www.241runnersawareness.org
- http://localhost:5173
```

## 🔄 **Deployment Process**

### **Automatic Deployment (GitHub Actions)**

1. **Trigger**: Push to `main` branch
2. **Build**: .NET 8 application is built and tested
3. **Staging Configuration**: Staging slot is configured with proper settings
4. **Staging Deployment**: Code is deployed to staging slot
5. **Staging Testing**: Automated smoke tests run
6. **Production Deployment**: If staging tests pass, code is deployed to production
7. **Production Testing**: Final smoke tests run

### **Manual Deployment**

```bash
# Build and deploy manually
cd 241RunnersAPI
dotnet publish -c Release -o publish
az webapp deploy --resource-group 241raLinux_group --name 241runners-api --slot staging --src-path publish --type zip
```

## 🧪 **Testing**

### **Health Endpoints**
```
https://241runners-api-staging.azurewebsites.net/healthz
https://241runners-api-staging.azurewebsites.net/readyz
https://241runners-api-staging.azurewebsites.net/api/health
```

### **API Endpoints**
```
https://241runners-api-staging.azurewebsites.net/api/cases/publiccases
https://241runners-api-staging.azurewebsites.net/swagger
```

### **Expected Responses**
- `/healthz`: `{"status":"ok","time":"..."}`
- `/readyz`: `{"status":"ok","db":"connected","latencyMs":...}`
- `/api/health`: `{"status":"healthy","timestamp":"...","environment":"Staging"}`

## 🔒 **Security Considerations**

### **JWT Isolation**
- Staging uses separate JWT issuer/audience
- Staging JWT tokens cannot be used in production
- Production JWT tokens cannot be used in staging

### **Database Access**
- Currently uses same database as production
- Can be configured to use separate staging database
- Connection strings are environment-specific

### **CORS Configuration**
- Staging has same CORS policy as production
- Can be modified for testing different origins

## 🚨 **Troubleshooting**

### **Common Issues**

#### **404 Errors**
- Check if application is deployed
- Verify startup command: `dotnet 241RunnersAPI.dll`
- Check deployment logs in Azure Portal

#### **500 Errors**
- Check connection strings
- Verify JWT settings
- Check application logs
- Verify database connectivity

#### **Deployment Failures**
- Check GitHub Actions logs
- Verify Azure permissions
- Check App Service logs
- Try manual deployment

### **Debugging Steps**

1. **Check Azure App Service Logs**
   ```bash
   az webapp log tail --resource-group 241raLinux_group --name 241runners-api --slot staging
   ```

2. **Verify Configuration**
   ```bash
   az webapp config appsettings list --resource-group 241raLinux_group --name 241runners-api --slot staging
   ```

3. **Test Connectivity**
   ```bash
   curl -v https://241runners-api-staging.azurewebsites.net/healthz
   ```

## 📊 **Monitoring**

### **Application Insights**
- Staging slot has separate Application Insights configuration
- Monitor performance and errors
- Set up alerts for staging-specific issues

### **Health Checks**
- Automated health checks run after deployment
- Monitor `/healthz` and `/readyz` endpoints
- Set up alerts for health check failures

## 🔄 **Production Swap**

### **When Ready for Production**
1. **Final Staging Validation**
   - Run comprehensive tests
   - Verify all functionality
   - Check performance metrics

2. **Execute Swap**
   ```bash
   az webapp deployment slot swap --resource-group 241raLinux_group --name 241runners-api --slot staging --target-slot production
   ```

3. **Post-Swap Validation**
   - Test production endpoints
   - Monitor for issues
   - Verify functionality

4. **Rollback (if needed)**
   ```bash
   az webapp deployment slot swap --resource-group 241raLinux_group --name 241runners-api --slot staging --target-slot production
   ```

## 📚 **Additional Resources**

- [Azure App Service Deployment Slots](https://docs.microsoft.com/en-us/azure/app-service/deploy-staging-slots)
- [.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)

## 🆘 **Support**

If you encounter issues:
1. Check the troubleshooting section above
2. Review Azure App Service logs
3. Check GitHub Actions logs
4. Contact the development team

---

**Last Updated**: 2025-01-13  
**Version**: 1.0
