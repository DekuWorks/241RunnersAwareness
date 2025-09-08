#!/bin/bash

# ============================================
# 241 RUNNERS AWARENESS - QUICK DEPLOYMENT
# ============================================
# 
# Quick deployment using az webapp deploy

set -e

# Configuration
RESOURCE_GROUP="241runnersawareness-rg"
APP_SERVICE_NAME="241runners-api"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_info "Starting quick deployment..."

# Navigate to API directory
cd 241RunnersAPI

# Build and deploy directly
log_info "Building and deploying application..."
dotnet publish --configuration Release --output ./publish

# Deploy using the newer command
log_info "Deploying to Azure..."
az webapp deploy \
    --name $APP_SERVICE_NAME \
    --resource-group $RESOURCE_GROUP \
    --src-path ./publish \
    --type zip

# Clean up
rm -rf ./publish

cd ..

log_success "Deployment completed!"

# Test the deployment
log_info "Testing deployment..."
APP_URL="https://$APP_SERVICE_NAME.azurewebsites.net"

# Test health endpoint
if curl -f -s "$APP_URL/api/auth/health" > /dev/null; then
    log_success "Health endpoint is responding"
else
    log_info "Health endpoint test failed - may need a moment to start"
fi

log_info "API URL: $APP_URL"
log_info "Swagger UI: $APP_URL/swagger"
log_info "Health Check: $APP_URL/api/auth/health"
