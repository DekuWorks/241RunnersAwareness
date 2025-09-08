# 🚀 Next Steps: Deploy API to Azure

## Current Status
✅ **Completed:**
- Fixed Program.cs middleware order and Swagger configuration
- Created GitHub Actions workflow for automated deployment
- Set up proper .NET 8.0 runtime configuration
- Configured startup command: `dotnet 241RunnersAPI.dll`

❌ **Current Issue:**
The API is returning 404/403 errors due to deployment issues with manual Azure CLI commands.

## 🎯 Recommended Solution: GitHub Actions Deployment

The most reliable approach is to use the GitHub Actions workflow we created. Here's how to set it up:

### Step 1: Set up GitHub Repository Secrets

1. **Go to your GitHub repository**
2. **Navigate to Settings → Secrets and variables → Actions**
3. **Click "New repository secret"**
4. **Add the following secret:**

   **Name:** `AZURE_WEBAPP_PUBLISH_PROFILE`
   
   **Value:** Copy the entire XML content from this publish profile:
   ```xml
   <publishData><publishProfile profileName="241runners-api - Web Deploy" publishMethod="MSDeploy" publishUrl="241runners-api.scm.azurewebsites.net:443" msdeploySite="241runners-api" userName="REDACTED" userPWD="REDACTED" destinationAppUrl="http://241runners-api.azurewebsites.net" SQLServerDBConnectionString="REDACTED" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile><publishProfile profileName="241runners-api - FTP" publishMethod="FTP" publishUrl="ftps://waws-prod-dm1-275.ftp.azurewebsites.windows.net/site/wwwroot" ftpPassiveMode="True" userName="REDACTED" userPWD="REDACTED" destinationAppUrl="http://241runners-api.azurewebsites.net" SQLServerDBConnectionString="REDACTED" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile><publishProfile profileName="241runners-api - Zip Deploy" publishMethod="ZipDeploy" publishUrl="241runners-api.scm.azurewebsites.net:443" userName="REDACTED" userPWD="REDACTED" destinationAppUrl="http://241runners-api.azurewebsites.net" SQLServerDBConnectionString="REDACTED" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile></publishData>
   ```

### Step 2: Commit and Push Changes

```bash
git add .
git commit -m "Fix API deployment and add GitHub Actions workflow"
git push origin main
```

### Step 3: Monitor Deployment

1. **Go to the Actions tab** in your GitHub repository
2. **Watch the "Deploy API to Azure App Service" workflow** run
3. **Check the logs** if there are any issues

### Step 4: Verify Deployment

Once the workflow completes successfully, test these endpoints:

- **Health Check:** https://241runners-api.azurewebsites.net/api/auth/health
- **Swagger UI:** https://241runners-api.azurewebsites.net/swagger
- **API Info:** https://241runners-api.azurewebsites.net/api/info

## 🔧 Alternative: Manual Fix (If GitHub Actions Fails)

If you prefer to fix the current deployment manually:

### Option 1: Reset App Service
```bash
# Stop the app service
az webapp stop --name 241runners-api --resource-group 241runnersawareness-rg

# Start the app service
az webapp start --name 241runners-api --resource-group 241runnersawareness-rg

# Wait 2 minutes, then test
curl https://241runners-api.azurewebsites.net/api/auth/health
```

### Option 2: Recreate App Service
```bash
# Delete the current app service
az webapp delete --name 241runners-api --resource-group 241runnersawareness-rg

# Recreate it
az webapp create --name 241runners-api --resource-group 241runnersawareness-rg --plan 241runners-basic-plan --runtime "DOTNET|8.0"

# Set startup command
az webapp config set --name 241runners-api --resource-group 241runnersawareness-rg --startup-file "dotnet 241RunnersAPI.dll"

# Deploy using GitHub Actions or manual deployment
```

## 🎯 Expected Results

After successful deployment, you should see:

1. **Health endpoint returns:** `{"status":"Healthy"}`
2. **Swagger UI loads** at `/swagger`
3. **API responds** to all endpoints
4. **Frontend can connect** to the API

## 🚨 Troubleshooting

If you encounter issues:

1. **Check GitHub Actions logs** for detailed error messages
2. **Verify the publish profile secret** is set correctly
3. **Ensure the repository has the latest code** pushed
4. **Check Azure App Service logs** in the Azure portal

## 📞 Need Help?

If you run into issues, the GitHub Actions workflow will provide detailed logs that will help identify the problem. The automated deployment is much more reliable than manual CLI commands.

---

**Next:** Once the API is deployed and working, we can proceed with:
- Frontend integration testing
- End-to-end testing
- Performance monitoring
- Documentation updates
