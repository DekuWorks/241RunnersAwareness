#!/bin/bash

# ============================================
# DEPLOY TO LIVE - STATIC SITE + BACKEND
# ============================================
# 
# This script commits and pushes all changes,
# then triggers deployment to GitHub Pages and Azure.
# 
# Usage: ./deploy-live.sh

echo "🚀 DEPLOYING TO LIVE - STATIC SITE + BACKEND"
echo "============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if we have changes to commit
if git diff-index --quiet HEAD --; then
    echo -e "${YELLOW}📝 No changes to commit. Repository is clean.${NC}"
else
    echo -e "${BLUE}📝 Committing changes...${NC}"
    
    # Add all files
    git add .
    
    # Commit with timestamp
    COMMIT_MESSAGE="Deploy to live: Static site + Backend - $(date '+%Y-%m-%d %H:%M:%S')"
    git commit -m "$COMMIT_MESSAGE"
    
    echo -e "${GREEN}✅ Changes committed: $COMMIT_MESSAGE${NC}"
fi

# Push to GitHub
echo -e "${BLUE}🚀 Pushing to GitHub...${NC}"
git push origin main

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Successfully pushed to GitHub!${NC}"
else
    echo -e "${RED}❌ Failed to push to GitHub${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}🎉 DEPLOYMENT TRIGGERED!${NC}"
echo "====================================="
echo ""
echo "📋 What's happening:"
echo "• Static site deploying to GitHub Pages"
echo "• Backend deploying to Azure"
echo "• Custom domain: https://241runnersawareness.org"
echo ""
echo "⏱️  Deployment typically takes 2-5 minutes"
echo ""
echo "🔗 Monitor deployments:"
echo "• GitHub Actions: https://github.com/DekuWorks/241RunnersAwareness/actions"
echo "• Live site: https://241runnersawareness.org"
echo "• Backend API: https://241runnersawareness-api.azurewebsites.net"
echo ""
echo "📊 Check deployment status in 2 minutes:"
echo "   curl -f https://241runnersawareness.org"
echo "   curl -f https://241runnersawareness-api.azurewebsites.net/health"
echo ""
echo -e "${YELLOW}✨ Your 241 Runners Awareness platform is going live!${NC}" 