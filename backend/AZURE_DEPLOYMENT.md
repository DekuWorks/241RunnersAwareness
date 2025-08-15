# 🚀 Azure Deployment Guide - 241 Runners Awareness Backend

## 📋 **Prerequisites**

### **Required Tools**
- **Azure CLI** - [Install Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- **Azure Subscription** - Active Azure account
- **Git** - For source control
- **.NET 8 SDK** - For local builds

### **Azure Resources Needed**
- **Resource Group** - To organize resources
- **App Service Plan** - To host the web app
- **App Service** - To run the .NET API
- **Azure SQL Database** - For production database
- **Application Insights** - For monitoring (optional)

---

## 🔧 **Step 1: Azure CLI Setup**

### **Login to Azure**
```bash
# Login to Azure
az login

# Set your subscription (if you have multiple)
az account set --subscription "Your-Subscription-Name"
```

### **Verify Login**
```bash
# Check current account
az account show

# List available subscriptions
az account list --output table
```

---

## 🏗️ **Step 2: Create Azure Resources**

### **Create Resource Group**
```bash
# Create resource group
az group create \
  --name "241runnersawareness-rg" \
  --location "East US" \
  --tags "Project=241RunnersAwareness" "Environment=Production"
```

### **Create App Service Plan**
```bash
# Create App Service Plan (B1 = Basic tier, S1 = Standard tier)
az appservice plan create \
  --name "241runners-api-plan" \
  --resource-group "241runnersawareness-rg" \
  --sku "B1" \
  --is-linux
```

### **Create App Service**
```bash
# Create the web app
az webapp create \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --plan "241runners-api-plan" \
  --runtime "DOTNETCORE:8.0"
```

### **Create Azure SQL Database**
```bash
# Create SQL Server
az sql server create \
  --name "241runners-sql-server" \
  --resource-group "241runnersawareness-rg" \
  --location "East US" \
  --admin-user "sqladmin" \
  --admin-password "YourStrongPassword123!"

# Create database
az sql db create \
  --name "RunnersDb" \
  --resource-group "241runnersawareness-rg" \
  --server "241runners-sql-server" \
  --edition "Basic" \
  --capacity 5
```

### **Configure Firewall Rules**
```bash
# Allow Azure services to access SQL Server
az sql server firewall-rule create \
  --resource-group "241runnersawareness-rg" \
  --server "241runners-sql-server" \
  --name "AllowAzureServices" \
  --start-ip-address "0.0.0.0" \
  --end-ip-address "0.0.0.0"

# Allow your IP address (optional for development)
az sql server firewall-rule create \
  --resource-group "241runnersawareness-rg" \
  --server "241runners-sql-server" \
  --name "AllowMyIP" \
  --start-ip-address "YOUR_IP_ADDRESS" \
  --end-ip-address "YOUR_IP_ADDRESS"
```

---

## ⚙️ **Step 3: Configure Environment Variables**

### **Get Database Connection String**
```bash
# Get the connection string
az sql db show-connection-string \
  --server "241runners-sql-server" \
  --database "RunnersDb" \
  --client ado.net
```

### **Configure App Service Settings**
```bash
# Set environment variables
az webapp config appsettings set \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --settings \
    "ASPNETCORE_ENVIRONMENT=Production" \
    "ConnectionStrings__DefaultConnection=Server=tcp:241runners-sql-server.database.windows.net,1433;Initial Catalog=RunnersDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    "Jwt__SecretKey=your-super-secure-jwt-secret-key-change-this-in-production" \
    "Jwt__Issuer=241RunnersAwareness" \
    "Jwt__Audience=241RunnersAwareness" \
    "Jwt__ExpiryInMinutes=60" \
    "Jwt__RefreshTokenExpiryInDays=7" \
    "SendGrid__ApiKey=your-sendgrid-api-key" \
    "SendGrid__FromEmail=noreply@241runnersawareness.org" \
    "SendGrid__TemplateId=your-template-id" \
    "Twilio__AccountSid=your-twilio-account-sid" \
    "Twilio__AuthToken=your-twilio-auth-token" \
    "Twilio__FromNumber=+1234567890" \
    "Twilio__WebhookUrl=https://241runnersawareness-api.azurewebsites.net/api/notifications/webhook" \
    "Google__ClientId=your-google-client-id" \
    "Google__ClientSecret=your-google-client-secret" \
    "Google__RedirectUri=https://241runnersawareness-api.azurewebsites.net/api/auth/google/callback" \
    "AppSettings__Version=1.0.0" \
    "AppSettings__MaxFileUploadSize=10485760" \
    "AppSettings__AllowedFileTypes=jpg,jpeg,png,pdf,doc,docx" \
    "RateLimiting__PermitLimit=100" \
    "RateLimiting__Window=60" \
    "RateLimiting__SegmentsPerWindow=6" \
    "HealthChecks__DatabaseTimeout=30" \
    "HealthChecks__MemoryThreshold=1024"
```

---

## 🚀 **Step 4: Deploy the Application**

### **Method 1: Deploy from Local Build**
```bash
# Build and publish locally
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deployment.zip .

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group "241runnersawareness-rg" \
  --name "241runnersawareness-api" \
  --src "../deployment.zip"
```

### **Method 2: Deploy from Git (Recommended)**
```bash
# Configure Git deployment
az webapp deployment source config \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --repo-url "https://github.com/your-org/241RunnersAwareness.git" \
  --branch "main" \
  --manual-integration
```

### **Method 3: Use Azure DevOps Pipeline**
```bash
# The azure-deploy.yml file is already configured
# Push to your repository and Azure DevOps will handle deployment
```

---

## 🗄️ **Step 5: Database Migration**

### **Run Entity Framework Migrations**
```bash
# Connect to App Service and run migrations
az webapp ssh --name "241runnersawareness-api" --resource-group "241runnersawareness-rg"

# Inside the SSH session:
cd /home/site/wwwroot
dotnet ef database update --connection "Server=tcp:241runners-sql-server.database.windows.net,1433;Initial Catalog=RunnersDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

## 🔍 **Step 6: Verify Deployment**

### **Test Health Endpoint**
```bash
# Test the health endpoint
curl https://241runnersawareness-api.azurewebsites.net/health

# Expected response:
# {"status":"Healthy","checks":[...]}
```

### **Test API Documentation**
- Open: https://241runnersawareness-api.azurewebsites.net/swagger
- Verify all endpoints are accessible

### **Test Authentication**
```bash
# Test registration
curl -X POST https://241runnersawareness-api.azurewebsites.net/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'

# Test login
curl -X POST https://241runnersawareness-api.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

---

## 🔧 **Step 7: Configure Custom Domain (Optional)**

### **Add Custom Domain**
```bash
# Add custom domain
az webapp config hostname add \
  --webapp-name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --hostname "api.241runnersawareness.org"
```

### **Configure SSL Certificate**
```bash
# Upload SSL certificate (if you have one)
az webapp config ssl upload \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --certificate-file "path/to/certificate.pfx" \
  --certificate-password "your-certificate-password"
```

---

## 📊 **Step 8: Monitoring Setup**

### **Enable Application Insights**
```bash
# Create Application Insights
az monitor app-insights component create \
  --app "241runnersawareness-insights" \
  --location "East US" \
  --resource-group "241runnersawareness-rg" \
  --application-type web

# Get the instrumentation key
az monitor app-insights component show \
  --app "241runnersawareness-insights" \
  --resource-group "241runnersawareness-rg" \
  --query "instrumentationKey" \
  --output tsv
```

### **Configure Logging**
```bash
# Enable application logging
az webapp log config \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --web-server-logging filesystem

# Enable detailed error messages
az webapp log config \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --detailed-error-messages true \
  --failed-request-tracing true
```

---

## 🔒 **Step 9: Security Configuration**

### **Configure CORS**
```bash
# Set CORS policy
az webapp cors add \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --allowed-origins "https://241runnersawareness.org" "https://www.241runnersawareness.org"
```

### **Enable HTTPS Only**
```bash
# Redirect HTTP to HTTPS
az webapp update \
  --name "241runnersawareness-api" \
  --resource-group "241runnersawareness-rg" \
  --https-only true
```

---

## 📋 **Step 10: Post-Deployment Checklist**

### **✅ Verification Checklist**
- [ ] Health endpoint returns 200 OK
- [ ] Swagger UI accessible
- [ ] Database migrations completed
- [ ] Authentication endpoints working
- [ ] CORS policy configured
- [ ] HTTPS redirect working
- [ ] Logs being generated
- [ ] Application Insights collecting data
- [ ] Custom domain configured (if applicable)
- [ ] SSL certificate installed (if applicable)

### **✅ Performance Testing**
- [ ] API response time < 500ms
- [ ] Database queries < 100ms
- [ ] Memory usage within limits
- [ ] CPU usage acceptable

### **✅ Security Verification**
- [ ] JWT authentication working
- [ ] Rate limiting active
- [ ] Input validation working
- [ ] SQL injection protection verified

---

## 🔧 **Troubleshooting**

### **Common Issues**

#### **Application Won't Start**
```bash
# Check application logs
az webapp log tail --name "241runnersawareness-api" --resource-group "241runnersawareness-rg"

# SSH into the app service
az webapp ssh --name "241runnersawareness-api" --resource-group "241runnersawareness-rg"
```

#### **Database Connection Issues**
```bash
# Test database connection
az sql db show \
  --name "RunnersDb" \
  --server "241runners-sql-server" \
  --resource-group "241runnersawareness-rg"

# Check firewall rules
az sql server firewall-rule list \
  --server "241runners-sql-server" \
  --resource-group "241runnersawareness-rg"
```

#### **Performance Issues**
```bash
# Check app service metrics
az monitor metrics list \
  --resource "/subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/241runnersawareness-rg/providers/Microsoft.Web/sites/241runnersawareness-api" \
  --metric "CpuPercentage,MemoryPercentage" \
  --interval PT1M
```

---

## 💰 **Cost Optimization**

### **App Service Plan Sizing**
- **Development**: B1 (Basic) - ~$13/month
- **Production**: S1 (Standard) - ~$73/month
- **High Traffic**: P1V2 (Premium) - ~$146/month

### **Database Sizing**
- **Development**: Basic (5 DTU) - ~$5/month
- **Production**: Standard S1 (20 DTU) - ~$30/month
- **High Traffic**: Premium P1 (125 DTU) - ~$465/month

### **Cost Saving Tips**
1. Use Basic tier for development
2. Scale down during off-hours
3. Use Azure Hybrid Benefit if eligible
4. Monitor usage with Azure Cost Management

---

## 📞 **Support Resources**

### **Azure Documentation**
- [App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

### **Team Contacts**
- **Azure Admin**: [Your Name] - your.email@example.com
- **DevOps Team**: devops@241runnersawareness.org
- **Technical Support**: support@241runnersawareness.org

---

*Last Updated: January 27, 2025*
*Version: 1.0.0*
