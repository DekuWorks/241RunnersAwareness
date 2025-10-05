#!/bin/bash

echo "🚀 Manual API Deployment Script"
echo "================================"

# Configuration
RESOURCE_GROUP="241raLinux_group"
WEBAPP_NAME="241runners-api-v2"
PUBLISH_DIR="publish-api-fix"

echo "📦 Building API..."
cd 241RunnersAPI
dotnet publish -c Release -o ../$PUBLISH_DIR
cd ..

echo "📁 Creating deployment package..."
cd $PUBLISH_DIR
zip -r ../api-manual-deployment.zip . -x "*.pdb" "*.xml"
cd ..

echo "🚀 Deploying to Azure..."
az webapp deploy \
  --resource-group $RESOURCE_GROUP \
  --name $WEBAPP_NAME \
  --src-path api-manual-deployment.zip \
  --type zip

echo "⏳ Waiting for deployment to complete..."
sleep 30

echo "🧪 Testing deployment..."
curl -f https://241runners-api-v2.azurewebsites.net/health || echo "Health check failed"

echo "✅ Deployment complete!"
