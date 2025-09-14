#!/bin/bash

# ============================================
# 241 RUNNERS AWARENESS - API DEPLOYMENT SCRIPT
# ============================================
# 
# Production deployment script for Azure App Service
# Ensures proper configuration and deployment

set -e  # Exit on any error

echo "🚀 Starting 241 Runners API Deployment..."

# Configuration
API_PROJECT_PATH="./241RunnersAPI"
PUBLISH_PATH="./publish"
ZIP_NAME="api-production-deployment.zip"
APP_SERVICE_NAME="241runners-api-v2"
RESOURCE_GROUP="241raLinux_group"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf $PUBLISH_PATH
rm -f $ZIP_NAME

# Build and publish the API
echo "🔨 Building and publishing API..."
cd $API_PROJECT_PATH
dotnet publish -c Release -o ../$PUBLISH_PATH --self-contained false
cd ..

# Create deployment zip (contents of publish folder, not the folder itself)
echo "📦 Creating deployment package..."
cd $PUBLISH_PATH
zip -r ../$ZIP_NAME . -x "*.pdb" "*.xml" "*.log" "*.tmp" "runtimes/*"
cd ..

echo "✅ Deployment package created: $ZIP_NAME"

# Deploy to Azure App Service
echo "☁️ Deploying to Azure App Service..."
az webapp deploy \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --src-path $ZIP_NAME \
    --type zip

# Set environment variables for Linux App Service
echo "⚙️ Setting environment variables..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --settings \
        ASPNETCORE_URLS="http://0.0.0.0:8080" \
        ASPNETCORE_ENVIRONMENT="Production" \
        DefaultConnection="Server=tcp:241runners-sql-2025.database.windows.net,1433;Initial Catalog=241RunnersAwarenessDB;Persist Security Info=False;User ID=sqladmin;Password=241RunnersAwareness2024!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
        JWT_KEY="241RunnersAwareness2024-Production-SuperSecure-JWT-Key-For-Authentication-With-Enhanced-Security" \
        JWT_ISSUER="241RunnersAwareness" \
        JWT_AUDIENCE="241RunnersAwareness"

# Configure App Service settings
echo "⚙️ Configuring App Service settings..."
az webapp config set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --always-on true \
    --use-32bit-worker-process false \
    --net-framework-version "v8.0"

# Restart the app service
echo "🔄 Restarting App Service..."
az webapp restart \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME

echo "⏳ Waiting for deployment to complete..."
sleep 30

# Verify deployment
echo "🔍 Verifying deployment..."
echo "Testing /api/health endpoint (database-free)..."
curl -f "https://$APP_SERVICE_NAME.azurewebsites.net/api/health" || echo "❌ Simple health endpoint failed"

echo "Testing /swagger endpoint..."
curl -f "https://$APP_SERVICE_NAME.azurewebsites.net/swagger" || echo "❌ Swagger endpoint failed"

echo "Testing /api/auth/health endpoint..."
curl -f "https://$APP_SERVICE_NAME.azurewebsites.net/api/auth/health" || echo "❌ Health endpoint failed"

echo "Testing /health endpoint..."
curl -f "https://$APP_SERVICE_NAME.azurewebsites.net/health" || echo "❌ Health endpoint failed"

# Post-deploy smoke test
echo "🧪 Running post-deploy smoke test..."
HEALTH_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "https://$APP_SERVICE_NAME.azurewebsites.net/api/health")
if [ "$HEALTH_RESPONSE" = "200" ]; then
    echo "✅ Smoke test passed - API is responding correctly"
else
    echo "❌ Smoke test failed - API returned status code: $HEALTH_RESPONSE"
    echo "🔍 Check the App Service logs for more details"
    exit 1
fi

echo "✅ API deployment completed successfully!"
echo "🌐 API URL: https://$APP_SERVICE_NAME.azurewebsites.net"
echo "📚 Swagger UI: https://$APP_SERVICE_NAME.azurewebsites.net/swagger"
echo "❤️ Health Check: https://$APP_SERVICE_NAME.azurewebsites.net/health"
echo "🔍 Application Insights: Monitor exceptions and performance in Azure Portal"
