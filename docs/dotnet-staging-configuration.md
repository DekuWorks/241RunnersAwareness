# .NET API Staging Slot Configuration Guide

## 🎯 **Complete Configuration for .NET API Deployment**

### **Quick Setup (Recommended)**
Use the automated configuration script:
```bash
./scripts/configure-staging-slot.sh
```

### **Manual Configuration Steps**

#### **Step 1: General Settings**
**Location**: Staging Slot → Configuration → General settings

```
Stack: .NET
Major version: 8
Minor version: 0
Startup command: dotnet 241RunnersAPI.dll
Platform: 64 Bit
Always On: On
HTTP Version: 2.0
ARR Affinity: Off
```

#### **Step 2: Application Settings**
**Location**: Staging Slot → Configuration → Application settings

##### **Required .NET Settings:**
```
ASPNETCORE_ENVIRONMENT = Staging
ASPNETCORE_URLS = http://0.0.0.0:8080
WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
WEBSITES_PORT = 8080
WEBSITES_CONTAINER_START_TIME_LIMIT = 1800
```

##### **JWT Configuration (Staging Isolation):**
```
JWT_ISSUER = 241RunnersAwareness-Staging
JWT_AUDIENCE = 241RunnersAwareness-Staging
JWT_KEY = 241RunnersAwareness2024-Staging-SuperSecure-JWT-Key-For-Authentication-With-Enhanced-Security-Staging-Environment-Only
```

#### **Step 3: Connection Strings**
**Location**: Staging Slot → Configuration → Connection strings

```
DefaultConnection = [Your production connection string - same as production for now]
```

#### **Step 4: Deployment Center Configuration**
**Location**: Staging Slot → Deployment Center → Settings

```
Source: GitHub
Organization: DekuWorks
Repository: 241RunnersAwareness
Branch: main
Build provider: GitHub Actions
Workflow file: .github/workflows/api-deploy.yml
```

#### **Step 5: CORS Configuration**
**Location**: Staging Slot → API → CORS

```
Allowed Origins:
- https://241runnersawareness.org
- https://www.241runnersawareness.org
- http://localhost:5173
```

## 🚀 **Automated Deployment Process**

### **GitHub Actions Configuration**
The staging slot is automatically configured and deployed via GitHub Actions:

1. **Automatic Configuration**: The workflow automatically sets staging-specific settings
2. **Staging Deployment**: Code is deployed to the staging slot first
3. **Smoke Testing**: Automated tests verify staging deployment
4. **Production Deployment**: After staging validation, code is deployed to production

### **Required GitHub Secrets**
Ensure these secrets are configured in your GitHub repository:

```
AZURE_CLIENT_ID = [Your Azure Service Principal Client ID]
AZURE_TENANT_ID = [Your Azure Tenant ID]
AZURE_SUBSCRIPTION_ID = [Your Azure Subscription ID]
AZURE_WEBAPP_NAME = 241runners-api
AZURE_RESOURCE_GROUP = 241raLinux_group
STAGING_CONNECTION_STRING = [Your staging database connection string]
```

### **Deployment Flow**
1. Push to `main` branch triggers deployment
2. Staging slot is configured with proper settings
3. Code is deployed to staging slot
4. Smoke tests run against staging
5. If staging tests pass, code is deployed to production
6. Production smoke tests run

## 🔧 **Configuration Checklist**

### **Before Deployment:**
- [ ] Stack set to .NET 8
- [ ] Startup command: `dotnet 241RunnersAPI.dll`
- [ ] ASPNETCORE_ENVIRONMENT = Staging
- [ ] JWT settings configured (staging-specific)
- [ ] Connection string set
- [ ] Deployment Center configured
- [ ] Platform: 64 Bit
- [ ] Always On: Enabled
- [ ] CORS configured for staging

### **After Configuration:**
- [ ] Save all settings
- [ ] Restart staging slot
- [ ] Wait for deployment
- [ ] Test endpoints
- [ ] Verify staging environment isolation

## 🧪 **Testing After Configuration**

### **Expected Endpoints:**
```
https://241runners-api-staging.azurewebsites.net/healthz
https://241runners-api-staging.azurewebsites.net/readyz
https://241runners-api-staging.azurewebsites.net/api/health
https://241runners-api-staging.azurewebsites.net/api/cases/publiccases
```

### **Expected Responses:**
- `/healthz`: `{"status":"ok","time":"..."}`
- `/readyz`: `{"status":"ok","db":"connected","latencyMs":...}`
- `/api/health`: `{"status":"healthy","timestamp":"...","environment":"Staging"}`
- `/api/cases/publiccases`: `{"success":true,"cases":[],"pagination":{...}}`

## 🚨 **Troubleshooting**

### **If endpoints still return 404:**
1. Check if startup command is correct: `dotnet 241RunnersAPI.dll`
2. Verify ASPNETCORE_ENVIRONMENT = Staging
3. Check if the application is actually deployed
4. Look at deployment logs in Deployment Center

### **If endpoints return 500:**
1. Check connection strings
2. Verify JWT settings
3. Check application logs
4. Verify database connectivity

### **If deployment fails:**
1. Check GitHub Actions for errors
2. Verify repository permissions
3. Check Azure App Service logs
4. Try manual deployment

## 📋 **Deployment Process**

1. **Configure all settings** as listed above
2. **Save configuration**
3. **Restart staging slot**
4. **Wait for GitHub Actions deployment** (5-10 minutes)
5. **Test endpoints**
6. **Verify functionality**

## 🎯 **Success Criteria**

The staging slot is properly configured when:
- ✅ All health endpoints return 200 OK
- ✅ API endpoints return proper JSON responses
- ✅ Environment shows "Staging"
- ✅ JWT authentication works
- ✅ Database connectivity works
