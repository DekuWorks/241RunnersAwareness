#!/bin/bash

# ============================================
# 241 RUNNERS AWARENESS - STAGING SLOT CONFIGURATION
# ============================================
# 
# Script to properly configure the staging slot for .NET API deployment
# This ensures staging has proper isolation from production

set -e  # Exit on any error

echo "🔧 Configuring 241 Runners API Staging Slot..."

# Configuration
APP_SERVICE_NAME="241runners-api"
RESOURCE_GROUP="241raLinux_group"
STAGING_SLOT="staging"

# Check if Azure CLI is logged in
if ! az account show &> /dev/null; then
    echo "❌ Azure CLI not logged in. Please run 'az login' first."
    exit 1
fi

echo "✅ Azure CLI authenticated"

# Check if staging slot exists
echo "🔍 Checking if staging slot exists..."
if ! az webapp show --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME --slot $STAGING_SLOT &> /dev/null; then
    echo "❌ Staging slot '$STAGING_SLOT' does not exist."
    echo "Please create the staging slot first in Azure Portal or run:"
    echo "az webapp deployment slot create --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME --slot $STAGING_SLOT"
    exit 1
fi

echo "✅ Staging slot found"

# Configure .NET-specific settings for staging
echo "⚙️ Configuring .NET settings for staging slot..."
az webapp config set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --slot $STAGING_SLOT \
    --always-on true \
    --use-32bit-worker-process false \
    --net-framework-version "v8.0" \
    --startup-file "dotnet 241RunnersAPI.dll"

# Set application settings for staging
echo "⚙️ Setting application settings for staging..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --slot $STAGING_SLOT \
    --settings \
        ASPNETCORE_ENVIRONMENT="Staging" \
        ASPNETCORE_URLS="http://0.0.0.0:8080" \
        WEBSITES_ENABLE_APP_SERVICE_STORAGE="false" \
        WEBSITES_PORT="8080" \
        WEBSITES_CONTAINER_START_TIME_LIMIT="1800"

# Set JWT settings for staging (isolated from production)
echo "🔐 Setting JWT configuration for staging..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --slot $STAGING_SLOT \
    --settings \
        JWT_KEY="241RunnersAwareness2024-Staging-SuperSecure-JWT-Key-For-Authentication-With-Enhanced-Security-Staging-Environment-Only" \
        JWT_ISSUER="241RunnersAwareness-Staging" \
        JWT_AUDIENCE="241RunnersAwareness-Staging"

# Set connection string (using same as production for now, but can be changed to separate staging DB)
echo "🗄️ Setting database connection string..."
az webapp config connection-string set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --slot $STAGING_SLOT \
    --connection-string-type SQLServer \
    --settings DefaultConnection="Server=tcp:241runners-sql-2025.database.windows.net,1433;Initial Catalog=241RunnersAwarenessDB;Persist Security Info=False;User ID=sqladmin;Password=241RunnersAwareness2024!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configure CORS for staging (if needed)
echo "🌐 Configuring CORS for staging..."
az webapp cors add \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --slot $STAGING_SLOT \
    --allowed-origins "https://241runnersawareness.org" "https://www.241runnersawareness.org" "http://localhost:5173"

# Restart the staging slot to apply all changes
echo "🔄 Restarting staging slot to apply configuration..."
az webapp restart \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --slot $STAGING_SLOT

echo "⏳ Waiting for staging slot to restart..."
sleep 30

# Test the staging deployment
echo "🧪 Testing staging deployment..."
STAGING_URL="https://$APP_SERVICE_NAME-$STAGING_SLOT.azurewebsites.net"

echo "Testing /healthz endpoint..."
if curl -f "$STAGING_URL/healthz" > /dev/null 2>&1; then
    echo "✅ /healthz endpoint working"
else
    echo "❌ /healthz endpoint failed"
    exit 1
fi

echo "Testing /readyz endpoint..."
if curl -f "$STAGING_URL/readyz" > /dev/null 2>&1; then
    echo "✅ /readyz endpoint working"
else
    echo "❌ /readyz endpoint failed"
    exit 1
fi

echo "Testing /api/health endpoint..."
if curl -f "$STAGING_URL/api/health" > /dev/null 2>&1; then
    echo "✅ /api/health endpoint working"
else
    echo "❌ /api/health endpoint failed"
    exit 1
fi

# Verify staging environment
echo "Verifying staging environment configuration..."
HEALTH_RESPONSE=$(curl -s "$STAGING_URL/api/health")
if echo "$HEALTH_RESPONSE" | grep -q "Staging"; then
    echo "✅ Staging environment confirmed"
else
    echo "❌ Staging environment not detected"
    echo "Response: $HEALTH_RESPONSE"
    exit 1
fi

echo ""
echo "🎉 Staging slot configuration completed successfully!"
echo ""
echo "📋 Configuration Summary:"
echo "  • App Service: $APP_SERVICE_NAME"
echo "  • Staging Slot: $STAGING_SLOT"
echo "  • Environment: Staging"
echo "  • .NET Version: 8.0"
echo "  • Startup Command: dotnet 241RunnersAPI.dll"
echo "  • JWT Issuer: 241RunnersAwareness-Staging"
echo "  • Database: Connected"
echo ""
echo "🌐 Staging URL: $STAGING_URL"
echo "📚 Swagger UI: $STAGING_URL/swagger"
echo "❤️ Health Check: $STAGING_URL/healthz"
echo ""
echo "✅ Ready for staging deployments!"
