#!/bin/bash

# ============================================
# RESTORE STATIC SITE STYLING
# ============================================
# 
# This script helps restore the original static site styling
# by triggering the new GitHub Actions workflow that deploys
# the static HTML files instead of the React frontend.
# 
# The original styling is in:
# - styles.css (main stylesheet)
# - index.html (main landing page)
# - All other .html files in the root directory
# 
# Usage: ./restore-static-site.sh

echo "🚀 Restoring Original Static Site Styling"
echo "=========================================="

# Check if we're in the right directory
if [ ! -f "index.html" ] || [ ! -f "styles.css" ]; then
    echo "❌ Error: Please run this script from the root directory of the repository"
    echo "   (where index.html and styles.css are located)"
    exit 1
fi

echo "✅ Found static site files:"
echo "   - index.html (main page)"
echo "   - styles.css (original styling)"
echo "   - All other HTML files"

# Check if the new workflow file exists
if [ ! -f ".github/workflows/static-site-deploy.yml" ]; then
    echo "❌ Error: Static site deployment workflow not found"
    echo "   Please ensure .github/workflows/static-site-deploy.yml exists"
    exit 1
fi

echo "✅ Found static site deployment workflow"

# Check if the React workflow is disabled
if grep -q "DISABLED" ".github/workflows/frontend-deploy.yml"; then
    echo "✅ React frontend deployment is disabled"
else
    echo "⚠️  Warning: React frontend deployment is still active"
    echo "   This might interfere with static site deployment"
fi

echo ""
echo "📋 Next Steps:"
echo "1. Commit and push these changes to GitHub:"
echo "   git add ."
echo "   git commit -m 'Restore original static site styling'"
echo "   git push origin main"
echo ""
echo "2. Check the GitHub Actions tab to monitor deployment:"
echo "   https://github.com/yourusername/241RunnersAwareness/actions"
echo ""
echo "3. Once deployed, your site will be available at:"
echo "   https://241runnersawareness.org"
echo "   or"
echo "   https://yourusername.github.io/241RunnersAwareness/"
echo ""
echo "🎨 The original styling includes:"
echo "   - Red background (#ff0000) - brand color"
echo "   - Dark mode toggle functionality"
echo "   - Responsive navigation with hamburger menu"
echo "   - PWA (Progressive Web App) features"
echo "   - All original HTML pages and functionality"

echo ""
echo "✨ Ready to restore the original styling!" 