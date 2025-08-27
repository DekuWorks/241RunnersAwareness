# Azure Environment Variables Setup Guide

## Step-by-Step Instructions

### 1. Access Azure Portal
1. Go to [Azure Portal](https://portal.azure.com)
2. Sign in with your account

### 2. Navigate to App Service
1. Search for "App Services" in the search bar
2. Click on "App Services"
3. Find and click on "241runnersawareness-api"

### 3. Access Configuration
1. In the left sidebar, click on "Configuration"
2. Click on "Application settings" tab

### 4. Set Environment Variables
Add the following environment variables one by one:

#### Essential Variables (Required for startup):
```
ASPNETCORE_ENVIRONMENT = Production
DB_CONNECTION_STRING = Server=241runners-sql-server-2025.database.windows.net;Database=RunnersDb;User Id=sqladmin;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;Command Timeout=30;
JWT_SECRET_KEY = 241RunnersAwareness-Production-Secret-Key-2025-Change-In-Production-64-Chars-Minimum
```

#### JWT Configuration:
```
JWT_ISSUER = 241RunnersAwareness
JWT_AUDIENCE = 241RunnersAwareness
JWT_EXPIRY_IN_MINUTES = 60
JWT_REFRESH_TOKEN_EXPIRY_IN_DAYS = 30
```

#### Google OAuth:
```
GOOGLE_CLIENT_ID = 933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET = [Your actual Google client secret]
```

#### App Configuration:
```
APP_BASE_URL = https://241runnersawareness.org
APP_API_URL = https://241runnersawareness-api.azurewebsites.net
APP_ENVIRONMENT = Production
APP_VERSION = 1.0.0
```

#### Rate Limiting:
```
RATE_LIMITING_PERMIT_LIMIT = 100
RATE_LIMITING_WINDOW = 00:01:00
```

#### Health Checks:
```
HEALTH_CHECKS_MEMORY_THRESHOLD = 1024
```

#### Optional Services (Set if you have them):
```
SENDGRID_API_KEY = [Your SendGrid API key]
TWILIO_ACCOUNT_SID = [Your Twilio Account SID]
TWILIO_AUTH_TOKEN = [Your Twilio Auth Token]
TWILIO_FROM_NUMBER = [Your Twilio phone number]
```

### 5. Save and Restart
1. Click "Save" at the top of the configuration page
2. Click "Continue" when prompted about restarting the app
3. Wait for the app to restart (usually takes 1-2 minutes)

### 6. Test the API
After restart, test the API:
```
https://241runnersawareness-api.azurewebsites.net/health
```

## Troubleshooting

### If the app still fails to start:
1. Check the logs in Azure Portal:
   - Go to App Service → Log stream
   - Look for error messages

2. Common issues:
   - Database connection string format
   - Missing required environment variables
   - Invalid JWT secret key format

### If you need to check current settings:
1. Go to App Service → Configuration
2. All environment variables should be visible with their values

## Next Steps After Setup

1. Test user registration: `POST https://241runnersawareness-api.azurewebsites.net/api/auth/register`
2. Test user login: `POST https://241runnersawareness-api.azurewebsites.net/api/auth/login`
3. Deploy frontend applications
4. Test the complete application flow
