# 🚀 241 Runners Awareness - Deployment Guide

This guide will help you deploy the 241 Runners Awareness application to Azure and set up automatic deployments.

## 📋 Prerequisites

- Azure subscription
- GitHub repository connected to Azure
- .NET 8.0 SDK (for local development)

## 🏗️ Architecture

- **Frontend**: Static HTML/CSS/JS hosted on Netlify (GitHub Pages)
- **Backend**: ASP.NET Core API hosted on Azure App Service
- **Database**: Azure SQL Database (configured in backend)
- **Domain**: `241runnersawareness.org`

## 🚀 Quick Deployment

### Step 1: Deploy Backend to Azure

#### Option A: Azure Portal (Recommended)

1. Go to [Azure Portal](https://portal.azure.com)
2. Find your App Service: `241runnersawareness-api`
3. Go to **Deployment Center**
4. Connect to your GitHub repository
5. Set **Source path** to `/backend`
6. Set **Branch** to `main`
7. Click **Save** to deploy

#### Option B: GitHub Actions (Automatic)

The repository includes a GitHub Actions workflow that automatically deploys the backend when you push changes to the `main` branch.

**To enable automatic deployment:**

1. Go to your Azure App Service
2. Go to **Deployment Center**
3. Download the publish profile
4. Go to your GitHub repository settings
5. Add a new secret: `AZURE_WEBAPP_PUBLISH_PROFILE`
6. Paste the publish profile content as the secret value

### Step 2: Set Up Admin Users

Once the backend is deployed, run the admin setup script:

```bash
node setup-azure-admin-users.js
```

This will create the following admin users:

| Email | Password | Role |
|-------|----------|------|
| dekuworks1@gmail.com | marcus2025 | admin |
| danielcarey9770@gmail.com | daniel2025 | admin |
| danielcarey9770@yahoo.com | daniel2025 | admin |

### Step 3: Verify Deployment

1. **Backend Health Check**: `https://241runnersawareness-api.azurewebsites.net/health`
2. **Admin Login**: `https://241runnersawareness.org/admin-login.html`
3. **Main Site**: `https://241runnersawareness.org`

## 🔧 Manual Deployment

### Backend Deployment

```bash
# Build the backend
cd backend
dotnet publish -c Release -o ./publish

# Deploy to Azure (if you have Azure CLI)
az webapp deployment source config-zip --resource-group 241runnersawareness-rg --name 241runnersawareness-api --src ./publish.zip
```

### Frontend Deployment

The frontend is automatically deployed via Netlify when you push to the `main` branch.

## 📁 Project Structure

```
241RunnersAwareness/
├── backend/                 # ASP.NET Core API
│   ├── Controllers/         # API endpoints
│   ├── DBContext/          # Database models and context
│   ├── Services/           # Business logic
│   └── Program.cs          # Application entry point
├── .github/workflows/      # GitHub Actions
├── admin/index.html        # Admin interface
├── admin-login.html        # Admin login page
├── admin-management.html   # Admin user management
├── index.html             # Main landing page
├── netlify.toml           # Netlify configuration
└── CNAME                  # Custom domain
```

## 🔐 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/change-password` - Change password

### Admin Management
- `GET /api/admin/admins` - Get all admin users
- `POST /api/admin/create-admin` - Create new admin
- `POST /api/admin/reset-admin-password` - Reset admin password
- `GET /api/admin/dashboard-stats` - Dashboard statistics

### Cases
- `GET /api/cases/mine` - Get user's cases
- `POST /api/cases` - Create new case

## 🛠️ Troubleshooting

### Backend Issues

1. **Build Errors**: Check that .NET 8.0 is installed
2. **Database Connection**: Verify connection string in Azure App Service settings
3. **CORS Issues**: Check CORS configuration in `Program.cs`

### Frontend Issues

1. **API Connection**: Verify `API_BASE_URL` in `auth-utils.js`
2. **Admin Login**: Ensure admin users are created in the backend
3. **Static Assets**: Check Netlify deployment status

### Common Commands

```bash
# Check backend health
curl https://241runnersawareness-api.azurewebsites.net/health

# Test admin login
curl -X POST https://241runnersawareness-api.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dekuworks1@gmail.com","password":"marcus2025"}'

# Create admin user
node setup-azure-admin-users.js
```

## 🔄 Continuous Deployment

The project is set up for continuous deployment:

1. **Frontend**: Automatically deploys via Netlify on push to `main`
2. **Backend**: Automatically deploys via GitHub Actions on push to `main` (if configured)

## 📞 Support

For deployment issues:
1. Check the GitHub Actions logs
2. Verify Azure App Service logs
3. Test API endpoints manually
4. Ensure all environment variables are set correctly

## 🔒 Security Notes

- Admin passwords are hashed using BCrypt
- JWT tokens are used for authentication
- CORS is configured for the production domain
- HTTPS is enforced on all endpoints
# Deployment triggered at Fri Aug 29 19:13:28 EDT 2025
