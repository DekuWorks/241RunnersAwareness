# 🚀 Quick Deployment Guide - 241 Runners Awareness

## Overview
This guide will help you deploy the complete 241 Runners Awareness platform to production in under 30 minutes.

## ✅ Prerequisites Checklist

Before starting, ensure you have:

- [ ] **Azure Account** with billing enabled
- [ ] **Azure CLI** installed and logged in (`az login`)
- [ ] **Netlify Account** and CLI access
- [ ] **Domain Access** (241runnersawareness.org DNS control)
- [ ] **Environment Variables** ready (see below)

## 🔑 Required Environment Variables

### Backend (Azure App Service):
```bash
JWT_SECRET_KEY=your-super-secure-jwt-secret-key-here
DB_PASSWORD=your-azure-sql-password
SENDGRID_API_KEY=your-sendgrid-api-key
TWILIO_ACCOUNT_SID=your-twilio-account-sid
TWILIO_AUTH_TOKEN=your-twilio-auth-token
TWILIO_PHONE_NUMBER=your-twilio-phone-number
GOOGLE_CLIENT_ID=933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-google-client-secret
SENTRY_DSN=your-sentry-dsn
```

### Frontend (Netlify):
```bash
API_BASE_URL=https://241runnersawareness-api.azurewebsites.net/api
GOOGLE_CLIENT_ID=933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com
APP_URL=https://241runnersawareness.org
```

## 🚀 Step-by-Step Deployment

### Step 1: Deploy Backend to Azure (15 minutes)

1. **Open PowerShell** in the project root directory

2. **Run the deployment script**:
   ```powershell
   .\deploy-to-production.ps1
   ```

3. **Follow the prompts** and wait for completion

4. **Verify backend deployment**:
   - Visit: `https://241runnersawareness-api.azurewebsites.net/health`
   - Should return: `{"status":"Healthy","timestamp":"..."}`

### Step 2: Deploy Frontend to Netlify (10 minutes)

1. **Run the frontend deployment script**:
   ```powershell
   .\deploy-frontend.ps1
   ```

2. **Follow the prompts** for Netlify setup

3. **Configure custom domain** when prompted:
   - Primary: `241runnersawareness.org`
   - WWW: `www.241runnersawareness.org`

### Step 3: Configure DNS (5 minutes)

1. **In your domain registrar**, add these DNS records:

   ```
   Type: CNAME
   Name: @
   Value: 241runnersawareness-web.azurestaticapps.net
   
   Type: CNAME  
   Name: www
   Value: 241runnersawareness-web.azurestaticapps.net
   
   Type: CNAME
   Name: api
   Value: 241runnersawareness-api.azurewebsites.net
   ```

2. **Wait for DNS propagation** (up to 24 hours, usually 15 minutes)

### Step 4: Final Configuration

1. **Set environment variables in Azure**:
   ```powershell
   az webapp config appsettings set --resource-group 241runnersawareness-rg --name 241runnersawareness-api --settings @backend/appsettings.Production.json
   ```

2. **Set environment variables in Netlify**:
   - Go to Netlify Dashboard → Site Settings → Environment Variables
   - Add the frontend environment variables listed above

## 🧪 Testing Your Deployment

### Backend Tests:
```bash
# Health Check
curl https://241runnersawareness-api.azurewebsites.net/health

# API Documentation
open https://241runnersawareness-api.azurewebsites.net/swagger

# Authentication Test
curl -X POST https://241runnersawareness-api.azurewebsites.net/api/auth/test
```

### Frontend Tests:
```bash
# Main Site
open https://241runnersawareness.org

# Map Functionality
open https://241runnersawareness.org/map.html

# Authentication
open https://241runnersawareness.org/login.html
```

### Test Accounts:
- **Admin**: `admin@241runners.org` / `admin123`
- **Test User**: `test@example.com` / `password123`
- **Lisa Thomas**: `lisa@241runners.org` / `lisa2025`

## 🔧 Troubleshooting

### Common Issues:

1. **Backend won't start**:
   - Check environment variables in Azure App Service
   - Verify database connection string
   - Check application logs in Azure Portal

2. **Frontend shows errors**:
   - Verify API_BASE_URL in Netlify environment variables
   - Check browser console for CORS errors
   - Ensure backend is running and accessible

3. **Map not loading**:
   - Check if mock-api.js is accessible
   - Verify Leaflet CSS/JS are loading
   - Check browser console for JavaScript errors

4. **Authentication issues**:
   - Verify JWT_SECRET_KEY is set
   - Check Google OAuth configuration
   - Ensure CORS settings include your domain

### Getting Help:

1. **Check logs**:
   - Azure App Service logs
   - Netlify deployment logs
   - Browser developer console

2. **Verify configuration**:
   - Environment variables
   - DNS settings
   - SSL certificates

3. **Contact support**:
   - Azure Support for backend issues
   - Netlify Support for frontend issues

## 📊 Post-Deployment Checklist

- [ ] All pages load correctly
- [ ] Authentication works (login/register)
- [ ] Map displays Houston data
- [ ] Case management features work
- [ ] Real-time notifications function
- [ ] Mobile responsiveness works
- [ ] SSL certificates are active
- [ ] Performance is acceptable
- [ ] Error monitoring is set up
- [ ] Backup procedures are configured

## 🎉 Success!

Your 241 Runners Awareness platform is now live at:
- **Main Site**: https://241runnersawareness.org
- **API**: https://241runnersawareness-api.azurewebsites.net
- **Documentation**: https://241runnersawareness-api.azurewebsites.net/swagger

## 📞 Support

For technical support:
- **Backend Issues**: Check Azure App Service logs
- **Frontend Issues**: Check Netlify deployment logs
- **Database Issues**: Check Azure SQL Database metrics
- **General Issues**: Review the troubleshooting section above

---

**Deployment Time**: ~30 minutes  
**Last Updated**: January 27, 2025  
**Status**: Ready for Production
