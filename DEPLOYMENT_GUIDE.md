# 241 Runners Awareness - Deployment Guide

## 🚀 Quick Deployment Setup

This guide will help you deploy the 241 Runners Awareness system to production.

## �� Prerequisites

### Required Accounts
- GitHub account with repository access
- Azure account with App Service access
- Domain name (241runnersawareness.org)

### Required Tools
- Azure CLI (for API deployment)
- Git (for version control)

## 🔧 Step 1: Configure GitHub Actions Secrets

### Frontend Deployment (GitHub Pages)
1. Go to your GitHub repository
2. Navigate to Settings → Secrets and variables → Actions
3. Add the following secrets:

```
GITHUB_TOKEN (automatically provided)
```

### API Deployment (Azure App Service)
1. Go to your GitHub repository
2. Navigate to Settings → Secrets and variables → Actions
3. Add the following secrets:

```
AZURE_WEBAPP_PUBLISH_PROFILE
```

**To get the Azure publish profile:**
1. Go to Azure Portal
2. Navigate to your App Service
3. Go to "Get publish profile" and download the file
4. Copy the contents and paste as the secret value

## 🌐 Step 2: Configure Domain and DNS

### GitHub Pages Configuration
1. Go to repository Settings → Pages
2. Set source to "GitHub Actions"
3. Add custom domain: `241runnersawareness.org`
4. Add CNAME record: `www.241runnersawareness.org`

### DNS Configuration
Add these DNS records:
```
Type: CNAME
Name: www
Value: your-username.github.io

Type: A
Name: @
Value: 185.199.108.153
Value: 185.199.109.153
Value: 185.199.110.153
Value: 185.199.111.153
```

## 🔐 Step 3: Configure Azure App Service

### Environment Variables
Add these environment variables to your Azure App Service:

```
JWT_KEY=your-super-secret-key-that-is-at-least-32-characters-long
JWT_ISSUER=241RunnersAwareness
JWT_AUDIENCE=241RunnersAwareness
DefaultConnection=your-azure-sql-connection-string
```

### CORS Configuration
Update CORS settings in Azure App Service:
```
Allowed Origins: https://241runnersawareness.org,https://www.241runnersawareness.org
Allowed Methods: GET,POST,PUT,DELETE,OPTIONS
Allowed Headers: Authorization,Content-Type,X-CSRF-Token,X-Client
Allow Credentials: true
```

### SignalR Configuration
1. Enable SignalR in Azure App Service
2. Configure WebSocket support
3. Set up sticky sessions if needed

## �� Step 4: Deploy

### Automatic Deployment
1. Push to main branch
2. GitHub Actions will automatically:
   - Build and deploy frontend to GitHub Pages
   - Build and deploy API to Azure App Service

### Manual Deployment

#### Frontend
```bash
# Build the frontend
./scripts/build.sh

# The dist/ directory contains the built files
# GitHub Actions will deploy these automatically
```

#### API
```bash
# Build and publish API
cd 241RunnersAwarenessAPI
dotnet publish -c Release -o ./publish

# Deploy to Azure (if using Azure CLI)
az webapp deployment source config-zip \
  --resource-group your-resource-group \
  --name your-app-service-name \
  --src ./publish.zip
```

## 🧪 Step 5: Testing

### Health Checks
Test these endpoints after deployment:

```
Frontend: https://241runnersawareness.org
API Health: https://your-api-url.azurewebsites.net/healthz
API Ready: https://your-api-url.azurewebsites.net/readyz
```

### Admin Login Test
1. Go to https://241runnersawareness.org/admin/login.html
2. Use admin credentials to log in
3. Verify dashboard loads with real-time updates

### Real-time Updates Test
1. Open two admin sessions
2. Make changes in one session
3. Verify changes appear in the other session within 2 seconds

## 🔍 Step 6: Monitoring

### GitHub Actions
Monitor deployment status in:
- Repository → Actions tab
- Check for any failed workflows

### Azure App Service
Monitor API health in:
- Azure Portal → App Service → Monitoring
- Check Application Insights for errors

### Frontend Monitoring
- Check GitHub Pages deployment status
- Test service worker updates
- Verify PWA functionality

## 🛠️ Troubleshooting

### Common Issues

#### Frontend Not Loading
- Check DNS configuration
- Verify GitHub Pages settings
- Check CNAME file in repository

#### API Connection Issues
- Verify CORS configuration
- Check environment variables
- Test health endpoints

#### Real-time Updates Not Working
- Check SignalR configuration
- Verify WebSocket support
- Check browser console for errors

#### Authentication Issues
- Verify JWT configuration
- Check token expiration
- Verify admin user exists

### Debug Commands

#### Check API Health
```bash
curl https://your-api-url.azurewebsites.net/healthz
curl https://your-api-url.azurewebsites.net/readyz
```

#### Check Frontend
```bash
curl -I https://241runnersawareness.org
```

#### Check Service Worker
```bash
# Open browser dev tools
# Go to Application → Service Workers
# Check registration status
```

## 📞 Support

### Getting Help
- Check GitHub Issues for known problems
- Review Azure App Service logs
- Check browser console for errors

### Emergency Contacts
- Security issues: security@241runnersawareness.org
- Technical issues: Create GitHub issue
- General support: Contact through website

## 🎉 Success Criteria

Your deployment is successful when:
- ✅ Frontend loads at https://241runnersawareness.org
- ✅ API responds at health endpoints
- ✅ Admin login works
- ✅ Real-time updates function
- ✅ Service worker updates work
- ✅ Mobile responsive design works

---

**Ready to deploy! 🚀**

Follow these steps in order, and your 241 Runners Awareness system will be live and fully functional.
