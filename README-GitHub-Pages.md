# 241 Runners Awareness - GitHub Pages Deployment

This project is now deployed on GitHub Pages instead of Netlify.

## Deployment

The frontend is automatically deployed to GitHub Pages via GitHub Actions when changes are pushed to the `main` branch.

### GitHub Pages URL
- **Production**: https://dekuworks.github.io/241RunnersAwareness/
- **Repository**: https://github.com/DekuWorks/241RunnersAwareness

### How it works
1. Push changes to the `main` branch
2. GitHub Actions automatically builds and deploys to the `gh-pages` branch
3. GitHub Pages serves the content from the `gh-pages` branch

### Manual Deployment
If you need to manually deploy:
1. Go to repository Settings > Pages
2. Ensure source is set to "Deploy from a branch"
3. Select `gh-pages` branch and `/ (root)` folder
4. Click Save

## Backend API
The backend API is deployed on Azure at:
- **API Base URL**: https://241runnersawareness-api.azurewebsites.net/api

## Configuration
- Frontend configuration is in `config.json`
- CORS is configured to allow GitHub Pages domain
- All API calls point to the Azure backend

## Notes
- Removed Netlify configuration files
- Updated all API endpoints to use Azure backend
- GitHub Actions workflow handles automatic deployment 