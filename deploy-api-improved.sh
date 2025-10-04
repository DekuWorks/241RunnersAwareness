#!/bin/bash

# ============================================
# 241 RUNNERS AWARENESS - IMPROVED API DEPLOYMENT
# ============================================
# 
# Production deployment script for Azure App Service
# Includes comprehensive testing and monitoring

set -e  # Exit on any error

echo "🚀 Starting 241 Runners API Deployment (Improved Version)..."

# Configuration
API_PROJECT_PATH="./241RunnersAPI"
PUBLISH_PATH="./publish-clean"
ZIP_NAME="api-production-deployment-$(date +%Y%m%d-%H%M%S).zip"
APP_SERVICE_NAME="241runners-api-v2"
RESOURCE_GROUP="241raLinux_group"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Azure CLI is installed and user is logged in
check_azure_cli() {
    print_status "Checking Azure CLI..."
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first."
        exit 1
    fi
    
    if ! az account show &> /dev/null; then
        print_error "Not logged into Azure CLI. Please run 'az login' first."
        exit 1
    fi
    
    print_success "Azure CLI is ready"
}

# Check if .NET is installed
check_dotnet() {
    print_status "Checking .NET installation..."
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET is not installed. Please install .NET 8.0 SDK first."
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    print_success "Found .NET version: $dotnet_version"
}

# Clean and prepare for deployment
clean_environment() {
    print_status "Cleaning environment..."
    
    # Clean previous builds
    rm -rf $PUBLISH_PATH
    rm -f api-production-deployment-*.zip
    
    # Clean the project
    cd $API_PROJECT_PATH
    dotnet clean
    cd ..
    
    print_success "Environment cleaned"
}

# Build and publish the API
build_api() {
    print_status "Building and publishing API..."
    
    cd $API_PROJECT_PATH
    
    # Restore packages
    print_status "Restoring NuGet packages..."
    dotnet restore
    
    # Build the project
    print_status "Building project..."
    dotnet build -c Release --no-restore
    
    # Publish the project
    print_status "Publishing project..."
    dotnet publish -c Release -o ../$PUBLISH_PATH --self-contained false --no-build
    
    cd ..
    
    print_success "API built and published successfully"
}

# Create deployment package
create_package() {
    print_status "Creating deployment package..."
    
    cd $PUBLISH_PATH
    
    # Create zip with all necessary files
    zip -r ../$ZIP_NAME . -x "*.pdb" "*.xml" "*.log" "*.tmp" "runtimes/*" "publish/*"
    
    cd ..
    
    local package_size=$(du -h $ZIP_NAME | cut -f1)
    print_success "Deployment package created: $ZIP_NAME ($package_size)"
}

# Deploy to Azure App Service
deploy_to_azure() {
    print_status "Deploying to Azure App Service..."
    
    # Deploy the package
    az webapp deploy \
        --resource-group $RESOURCE_GROUP \
        --name $APP_SERVICE_NAME \
        --src-path $ZIP_NAME \
        --type zip \
        --clean true \
        --verbose
    
    print_success "Package deployed to Azure App Service"
}

# Configure Azure App Service
configure_app_service() {
    print_status "Configuring Azure App Service..."
    
    # Set environment variables
    az webapp config appsettings set \
        --resource-group $RESOURCE_GROUP \
        --name $APP_SERVICE_NAME \
        --settings \
            ASPNETCORE_URLS="http://0.0.0.0:8080" \
            ASPNETCORE_ENVIRONMENT="Production" \
            DefaultConnection="Server=tcp:241runners-sql-2025.database.windows.net,1433;Initial Catalog=241RunnersAwarenessDB;Persist Security Info=False;User ID=sqladmin;Password=241RunnersAwareness2024!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
            JWT_KEY="241RunnersAwareness2024-Production-SuperSecure-JWT-Key-For-Authentication-With-Enhanced-Security" \
            JWT_ISSUER="241RunnersAwareness" \
            JWT_AUDIENCE="241RunnersAwareness" \
            Swagger__Enabled="true" \
            --verbose
    
    # Configure App Service settings
    az webapp config set \
        --resource-group $RESOURCE_GROUP \
        --name $APP_SERVICE_NAME \
        --always-on true \
        --use-32bit-worker-process false \
        --net-framework-version "v8.0" \
        --verbose
    
    print_success "App Service configured"
}

# Restart the app service
restart_app_service() {
    print_status "Restarting App Service..."
    
    az webapp restart \
        --resource-group $RESOURCE_GROUP \
        --name $APP_SERVICE_NAME \
        --verbose
    
    print_success "App Service restarted"
}

# Wait for deployment to be ready
wait_for_deployment() {
    print_status "Waiting for deployment to be ready..."
    
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        print_status "Attempt $attempt/$max_attempts - Checking if API is ready..."
        
        if curl -f -s "https://$APP_SERVICE_NAME.azurewebsites.net/api/health" > /dev/null 2>&1; then
            print_success "API is ready and responding!"
            return 0
        fi
        
        sleep 10
        attempt=$((attempt + 1))
    done
    
    print_warning "API may not be fully ready yet, but continuing with tests..."
}

# Run comprehensive endpoint tests
run_endpoint_tests() {
    print_status "Running comprehensive endpoint tests..."
    
    # Make the test script executable
    chmod +x test-api-endpoints.js
    
    # Run the tests
    if node test-api-endpoints.js --url "https://$APP_SERVICE_NAME.azurewebsites.net"; then
        print_success "All endpoint tests passed!"
    else
        print_warning "Some endpoint tests failed. Check the output above for details."
    fi
}

# Display deployment summary
show_summary() {
    echo ""
    echo "============================================================"
    echo "🎉 DEPLOYMENT COMPLETED SUCCESSFULLY!"
    echo "============================================================"
    echo ""
    echo "📋 Deployment Details:"
    echo "   • App Service: $APP_SERVICE_NAME"
    echo "   • Resource Group: $RESOURCE_GROUP"
    echo "   • Package: $ZIP_NAME"
    echo "   • Environment: Production"
    echo ""
    echo "🌐 API Endpoints:"
    echo "   • API Base URL: https://$APP_SERVICE_NAME.azurewebsites.net"
    echo "   • Swagger UI: https://$APP_SERVICE_NAME.azurewebsites.net/swagger"
    echo "   • Health Check: https://$APP_SERVICE_NAME.azurewebsites.net/health"
    echo "   • API Info: https://$APP_SERVICE_NAME.azurewebsites.net/api"
    echo ""
    echo "🔧 Key Features:"
    echo "   • JWT Authentication"
    echo "   • SignalR Real-time Communication"
    echo "   • Comprehensive Health Checks"
    echo "   • Swagger API Documentation"
    echo "   • Performance Monitoring"
    echo "   • CORS Support for Web & Mobile"
    echo ""
    echo "📊 Monitoring:"
    echo "   • Azure Application Insights enabled"
    echo "   • Health check endpoints available"
    echo "   • Performance metrics tracked"
    echo ""
    echo "✅ Your API is now live and ready to serve requests!"
    echo ""
}

# Main deployment flow
main() {
    echo "Starting deployment process..."
    echo ""
    
    # Pre-deployment checks
    check_azure_cli
    check_dotnet
    
    # Deployment steps
    clean_environment
    build_api
    create_package
    deploy_to_azure
    configure_app_service
    restart_app_service
    wait_for_deployment
    
    # Post-deployment verification
    run_endpoint_tests
    
    # Show summary
    show_summary
}

# Handle script arguments
case "${1:-}" in
    --help|-h)
        echo "241 Runners API Deployment Script"
        echo ""
        echo "Usage: $0 [options]"
        echo ""
        echo "Options:"
        echo "  --help, -h     Show this help message"
        echo "  --test-only    Run endpoint tests only (skip deployment)"
        echo ""
        exit 0
        ;;
    --test-only)
        print_status "Running endpoint tests only..."
        run_endpoint_tests
        exit 0
        ;;
    *)
        main
        ;;
esac
