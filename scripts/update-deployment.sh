#!/bin/bash

# ============================================
# 241 RUNNERS AWARENESS - UPDATE DEPLOYMENT SCRIPT
# ============================================
# 
# This script updates the existing API deployment

set -e  # Exit on any error

# Configuration
RESOURCE_GROUP="241runnersawareness-rg"
APP_SERVICE_NAME="241runners-api"
SQL_SERVER_NAME="241runners-sql-2025"
SQL_DATABASE_NAME="241RunnersAwarenessDB"
SQL_ADMIN_USER="sqladmin"
SQL_ADMIN_PASSWORD="241RunnersAwareness2024!"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Azure CLI is installed and user is logged in
check_azure_cli() {
    log_info "Checking Azure CLI..."
    
    if ! command -v az &> /dev/null; then
        log_error "Azure CLI is not installed. Please install it first."
        exit 1
    fi
    
    if ! az account show &> /dev/null; then
        log_error "Not logged in to Azure CLI. Please run 'az login' first."
        exit 1
    fi
    
    log_success "Azure CLI is ready"
}

# Configure App Service settings
configure_app_service() {
    log_info "Configuring App Service settings..."
    
    # Set .NET 8 runtime
    az webapp config set \
        --name $APP_SERVICE_NAME \
        --resource-group $RESOURCE_GROUP \
        --linux-fx-version "DOTNET|8.0"
    
    # Disable ARR Affinity (not available in this CLI version)
    # az webapp config set \
    #     --name $APP_SERVICE_NAME \
    #     --resource-group $RESOURCE_GROUP \
    #     --arr-affinity false
    
    # Set application settings
    az webapp config appsettings set \
        --name $APP_SERVICE_NAME \
        --resource-group $RESOURCE_GROUP \
        --settings \
            ASPNETCORE_ENVIRONMENT=Production \
            Jwt__Key="your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners" \
            Jwt__Issuer="241RunnersAwareness" \
            Jwt__Audience="241RunnersAwareness" \
            ConnectionStrings__DefaultConnection="Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DATABASE_NAME;Persist Security Info=False;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    log_success "App Service configured"
}

# Configure CORS
configure_cors() {
    log_info "Configuring CORS..."
    
    az webapp cors add \
        --name $APP_SERVICE_NAME \
        --resource-group $RESOURCE_GROUP \
        --allowed-origins "https://241runnersawareness.org" "https://www.241runnersawareness.org"
    
    log_success "CORS configured"
}

# Build and deploy the application
deploy_application() {
    log_info "Building and deploying application..."
    
    # Navigate to API directory
    cd 241RunnersAPI
    
    # Restore packages
    log_info "Restoring NuGet packages..."
    dotnet restore
    
    # Build the application
    log_info "Building application..."
    dotnet build --configuration Release
    
    # Publish the application
    log_info "Publishing application..."
    dotnet publish --configuration Release --output ./publish
    
    # Create deployment package
    log_info "Creating deployment package..."
    cd publish
    zip -r ../api-deployment.zip .
    cd ..
    
    # Deploy to Azure
    log_info "Deploying to Azure App Service..."
    az webapp deployment source config-zip \
        --name $APP_SERVICE_NAME \
        --resource-group $RESOURCE_GROUP \
        --src ./api-deployment.zip
    
    # Clean up
    rm -rf ./publish
    rm -f ./api-deployment.zip
    
    cd ..
    
    log_success "Application deployed successfully"
}

# Test the deployment
test_deployment() {
    log_info "Testing deployment..."
    
    # Get the App Service URL
    APP_URL=$(az webapp show \
        --name $APP_SERVICE_NAME \
        --resource-group $RESOURCE_GROUP \
        --query "defaultHostName" \
        --output tsv)
    
    log_info "App Service URL: https://$APP_URL"
    
    # Test health endpoint
    log_info "Testing health endpoint..."
    if curl -f -s "https://$APP_URL/api/auth/health" > /dev/null; then
        log_success "Health endpoint is responding"
    else
        log_error "Health endpoint is not responding"
    fi
    
    # Test CORS
    log_info "Testing CORS..."
    if curl -f -s -H "Origin: https://241runnersawareness.org" "https://$APP_URL/cors-test" > /dev/null; then
        log_success "CORS is working"
    else
        log_warning "CORS test failed"
    fi
    
    log_success "Deployment test completed"
}

# Main deployment function
main() {
    log_info "Starting Azure deployment update for 241 Runners Awareness API"
    
    check_azure_cli
    configure_app_service
    configure_cors
    deploy_application
    test_deployment
    
    log_success "Deployment update completed successfully!"
    log_info "API URL: https://$APP_SERVICE_NAME.azurewebsites.net"
    log_info "Swagger UI: https://$APP_SERVICE_NAME.azurewebsites.net/swagger"
    log_info "Health Check: https://$APP_SERVICE_NAME.azurewebsites.net/api/auth/health"
}

# Run main function
main "$@"
