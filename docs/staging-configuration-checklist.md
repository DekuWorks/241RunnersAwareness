# Staging Slot Configuration Checklist

## 🚀 **Quick Setup (Automated)**
Run the automated configuration script:
```bash
./scripts/configure-staging-slot.sh
```

## ✅ **Required Configuration Settings**

### **General Settings**
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

### **Application Settings**
```
ASPNETCORE_ENVIRONMENT = Staging
ASPNETCORE_URLS = http://0.0.0.0:8080
WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
WEBSITES_PORT = 8080
WEBSITES_CONTAINER_START_TIME_LIMIT = 1800
```

### **JWT Settings (for staging isolation)**
```
JWT_ISSUER = 241RunnersAwareness-Staging
JWT_AUDIENCE = 241RunnersAwareness-Staging
JWT_KEY = 241RunnersAwareness2024-Staging-SuperSecure-JWT-Key-For-Authentication-With-Enhanced-Security-Staging-Environment-Only
```

### **Connection Strings**
```
DefaultConnection = [Production SQL Connection String - same as production for now]
```

### **CORS Settings**
```
Allowed Origins:
- https://241runnersawareness.org
- https://www.241runnersawareness.org
- http://localhost:5173
```

## 🔍 **Verification Steps**

### **1. Check Application Settings**
- Go to staging slot → Configuration → Application settings
- Verify all settings are present and correct
- Click "Save" if any changes were made

### **2. Check Deployment Center**
- Go to staging slot → Deployment Center
- Verify source is set to GitHub
- Verify repository: DekuWorks/241RunnersAwareness
- Verify branch: main
- Verify build provider: GitHub Actions

### **3. Check Deployment Status**
- Look for any error messages in Deployment Center
- Check if the latest deployment completed successfully
- Look for any failed deployments

### **4. Restart After Configuration Changes**
- After making configuration changes, restart the staging slot
- Go to staging slot → Overview → Restart
- Wait 2-3 minutes for restart to complete

## 🧪 **Testing After Configuration**

### **Expected Endpoints**
```
https://241runners-api-staging.azurewebsites.net/healthz
https://241runners-api-staging.azurewebsites.net/readyz
https://241runners-api-staging.azurewebsites.net/api/health
https://241runners-api-staging.azurewebsites.net/api/cases/publiccases
```

### **Expected Responses**
- `/healthz`: `{"status":"ok","time":"..."}`
- `/readyz`: `{"status":"ok","db":"connected","latencyMs":...}`
- `/api/health`: `{"status":"healthy","timestamp":"...","environment":"Staging"}`
- `/api/cases/publiccases`: `{"success":true,"cases":[],"pagination":{...}}`

## 🚨 **Troubleshooting**

### **If endpoints return 404:**
1. Check if application is deployed
2. Verify ASPNETCORE_ENVIRONMENT = Staging
3. Restart the staging slot
4. Check deployment logs

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

## 📞 **Support**

If issues persist:
1. Check Azure App Service logs
2. Check GitHub Actions logs
3. Verify all configuration settings
4. Contact development team
