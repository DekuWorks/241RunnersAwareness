# 🚀 GitHub Pages Setup Guide

## Step 1: Enable GitHub Pages

1. Go to your GitHub repository: `https://github.com/yourusername/241RunnersAwareness`
2. Click on **Settings** tab
3. Scroll down to **Pages** section in the left sidebar
4. Under **Source**, select **GitHub Actions**
5. This will allow the workflow to deploy to GitHub Pages

## Step 2: Configure Custom Domain (Optional)

If you want to use a custom domain like `241runnersawareness.org`:

1. In the **Pages** settings, enter your custom domain
2. Add a `CNAME` file to your repository root:

```bash
# Create CNAME file
echo "241runnersawareness.org" > CNAME
```

3. Configure DNS with your domain provider:
   - Add a CNAME record pointing to `yourusername.github.io`
   - Or add an A record pointing to GitHub Pages IP addresses

## Step 3: Set Up GitHub Secrets

### For Backend Deployment:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Add the following secrets:

#### AZURE_WEBAPP_PUBLISH_PROFILE
1. Go to Azure Portal → App Service → 241runnersawareness-api
2. Click **Get publish profile**
3. Download the file and copy its contents
4. Add as secret: `AZURE_WEBAPP_PUBLISH_PROFILE`

#### AZURE_SQL_CONNECTION_STRING
```
Server=241runners-sql-server-2025.database.windows.net;Database=RunnersDb;User Id=sqladmin;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;Command Timeout=30;
```

## Step 4: Configure Frontend Environment

Update your frontend environment configuration:

```javascript
// frontend/src/config/environment.js
const config = {
  development: {
    API_BASE_URL: 'http://localhost:5113/api',
    GOOGLE_CLIENT_ID: '933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com',
    APP_URL: 'http://localhost:5173',
    NODE_ENV: 'development'
  },
  
  production: {
    API_BASE_URL: 'https://241runnersawareness-api.azurewebsites.net/api',
    GOOGLE_CLIENT_ID: '933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com',
    APP_URL: 'https://241runnersawareness.org', // or your GitHub Pages URL
    NODE_ENV: 'production'
  }
};
```

## Step 5: Test Deployment

1. Make a small change to trigger deployment
2. Go to **Actions** tab to monitor deployment
3. Check the deployed site

## Step 6: Verify Deployment

### Test Frontend:
```bash
# Test GitHub Pages URL
curl https://yourusername.github.io/241RunnersAwareness/

# Test custom domain (if configured)
curl https://241runnersawareness.org/
```

### Test Backend:
```bash
# Test API health
curl https://241runnersawareness-api.azurewebsites.net/health

# Test auth endpoint
curl https://241runnersawareness-api.azurewebsites.net/api/auth/test
```

## Troubleshooting

### Common Issues:

1. **GitHub Pages not working**:
   - Check if GitHub Pages is enabled
   - Verify the workflow ran successfully
   - Check the Actions tab for errors

2. **Custom domain not working**:
   - Verify DNS configuration
   - Check if CNAME file is in repository root
   - Wait for DNS propagation (up to 24 hours)

3. **API calls failing**:
   - Check CORS configuration in backend
   - Verify API URL in frontend config
   - Test API endpoints directly

4. **Build failures**:
   - Check GitHub Actions logs
   - Verify Node.js and .NET versions
   - Check for missing dependencies

### Debug Commands:

```bash
# Check GitHub Pages status
curl -I https://yourusername.github.io/241RunnersAwareness/

# Test API connectivity
curl -v https://241runnersawareness-api.azurewebsites.net/health

# Check CORS headers
curl -H "Origin: https://241runnersawareness.org" \
     -H "Access-Control-Request-Method: GET" \
     -H "Access-Control-Request-Headers: X-Requested-With" \
     -X OPTIONS \
     https://241runnersawareness-api.azurewebsites.net/api/auth/test
```

## Next Steps

1. **Monitor deployments** in GitHub Actions
2. **Set up monitoring** for your application
3. **Configure backups** for your database
4. **Set up alerts** for deployment failures
5. **Document your deployment process**

## Support

If you encounter issues:
1. Check GitHub Actions logs
2. Review Azure App Service logs
3. Test endpoints manually
4. Check DNS and domain configuration
