# 241 Runners Awareness

A comprehensive platform for raising awareness about missing persons cases, specifically focused on the 241 area code region. The system provides both public-facing information and secure admin tools for case management.

## 🎯 Current Status: **OPERATIONAL** ✅

**Version:** 1.0.3 | **Last Updated:** January 27, 2025  
**API Health:** [![API Status](https://img.shields.io/badge/API-Operational-green)](https://241runners-api.azurewebsites.net/api/auth/health)  
**Frontend:** [![Frontend Status](https://img.shields.io/badge/Frontend-Live-blue)](https://241runnersawareness.org)

> 📊 **Detailed Status:** See [DEPLOYMENT_STATUS.md](./DEPLOYMENT_STATUS.md) for comprehensive system health and monitoring information.

## 🚀 Quick Start

### For Users
- **Visit:** [https://241runnersawareness.org](https://241runnersawareness.org)
- **Browse Cases:** View missing persons cases with advanced filtering
- **Install PWA:** Add to home screen for offline access

### For Developers
```bash
# Clone the repository
git clone https://github.com/your-username/241RunnersAwareness.git
cd 241RunnersAwareness

# Deploy to Azure (requires Azure CLI)
chmod +x scripts/deploy-azure.sh
./scripts/deploy-azure.sh

# Run integration tests
node scripts/integration-tests.js
```

### For Administrators
- **Admin Portal:** [https://241runnersawareness.org/admin](https://241runnersawareness.org/admin)
- **API Documentation:** [https://241runners-api.azurewebsites.net/swagger](https://241runners-api.azurewebsites.net/swagger)
- **Health Monitoring:** [https://241runners-api.azurewebsites.net/api/auth/health](https://241runners-api.azurewebsites.net/api/auth/health)

## 🏥 Health Monitoring

### Health Endpoints
- **Liveness Check:** `/healthz` - Basic app health (stays green while DB boots)
- **Readiness Check:** `/readyz` - Database connectivity with latency metrics
- **API Health:** `/api/health` - Comprehensive API status

### Quick Health Commands
```bash
# Basic health checks
curl -sS -I https://241runners-api.azurewebsites.net/healthz
curl -sS https://241runners-api.azurewebsites.net/readyz | jq .

# API health
curl -sS https://241runners-api.azurewebsites.net/api/health | jq .
```

## 🌍 Environments

### Production
- **API:** https://241runners-api.azurewebsites.net
- **Frontend:** https://241runnersawareness.org
- **Database:** Azure SQL (Production)
- **Monitoring:** Application Insights

### Staging
- **API:** https://241runners-api-staging.azurewebsites.net
- **Database:** Azure SQL (Staging)
- **Purpose:** Safe deployment testing before production

## 🔒 Security Configuration

### CORS Origins
- `https://241runnersawareness.org`
- `https://www.241runnersawareness.org`
- `http://localhost:5173` (development only)

### Admin Seeding
- Set `SEED_ADMIN_PWD` in Azure App Service Configuration for initial admin
- **⚠️ IMPORTANT:** Remove `SEED_ADMIN_PWD` after first admin login for security
- Default admin email: `admin@241runnersawareness.org`

### JWT Configuration
- `JWT_ISSUER`: JWT token issuer
- `JWT_AUDIENCE`: JWT token audience  
- `JWT_KEY`: 256-bit JWT signing key

## 📊 Monitoring & Operations

### Application Insights
- **Live Metrics:** Real-time request, dependency, and exception tracking
- **KQL Queries:** Custom queries for performance analysis
- **Alerts:** Automated alerts for health check failures, latency spikes, and exceptions

### Smoke Tests
```bash
# Authentication test
curl -sS -X POST https://241runners-api.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@241runnersawareness.org","password":"<REDACTED>"}' | jq .

# Admin stats (requires valid token)
curl -sS -H "Authorization: Bearer <ACCESS_TOKEN>" \
  https://241runners-api.azurewebsites.net/api/admin/stats | jq .
```

### Operations Runbook
- **Emergency Procedures:** See [docs/ops-runbook.md](./docs/ops-runbook.md)
- **Deployment Procedures:** Staging slot deployment and rollback
- **Common Issues:** Database, authentication, and migration troubleshooting
- **Performance Optimization:** Database and API optimization guidelines

## 🚀 Features

### Public Features
- **Case Directory**: Browse missing persons cases with filtering and search
- **Real-time Updates**: Live updates when new cases are added
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **PWA Support**: Install as a mobile app with offline capabilities

### Admin Features
- **Secure Dashboard**: Role-based access control for administrators
- **Real-time Collaboration**: Multiple admins can work simultaneously with live updates
- **Case Management**: Create, edit, and manage missing persons cases
- **User Management**: Admin user creation and role management
- **System Monitoring**: Health checks and performance monitoring

### Technical Features
- **Real-time Updates**: SignalR-powered live updates with polling fallback
- **Secure Authentication**: JWT-based authentication with automatic token refresh
- **API-First Design**: RESTful API with comprehensive error handling
- **Modern UI**: Clean, accessible interface with dark/light mode support
- **Performance Optimized**: Cached assets, lazy loading, and efficient data fetching

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   API Server    │    │   Database      │
│   (GitHub Pages)│◄──►│   (Azure App    │◄──►│   (Azure SQL)   │
│                 │    │    Service)     │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │
         │                       │
         ▼                       ▼
┌─────────────────┐    ┌─────────────────┐
│   CDN/Assets    │    │   SignalR Hub   │
│   (Cached)      │    │   (Real-time)   │
└─────────────────┘    └─────────────────┘
```

### Real-time Updates Flow
1. **SignalR Connection**: Admin clients connect to SignalR hub with JWT authentication
2. **Event Broadcasting**: CRUD operations trigger real-time events via AdminHub
3. **Client Updates**: Connected clients receive updates instantly (< 2 seconds)
4. **Polling Fallback**: If WebSocket fails, clients fall back to 30-second polling
5. **Debounced Processing**: Multiple events are batched for efficiency
6. **Connection Management**: Automatic reconnection with exponential backoff

### Security Architecture
- **JWT Authentication**: Secure token-based authentication with automatic refresh
- **Role-based Access**: Admin-only access to sensitive operations
- **CORS Protection**: Configured for specific domains only
- **Input Validation**: Server-side validation for all inputs
- **HTTPS Everywhere**: All communications encrypted in transit

## 🛠️ Technology Stack

### Frontend
- **HTML5/CSS3**: Semantic markup with modern CSS features
- **Vanilla JavaScript**: No frameworks for maximum performance
- **Service Workers**: Offline support and caching
- **SignalR Client**: Real-time communication
- **PWA**: Progressive Web App capabilities

### Backend
- **ASP.NET Core 8**: Modern web API framework
- **Entity Framework Core**: Database ORM with SQLite/Azure SQL
- **SignalR**: Real-time communication hub
- **JWT Authentication**: Secure token-based auth
- **Swagger/OpenAPI**: API documentation

### Infrastructure
- **GitHub Pages**: Static site hosting
- **Azure App Service**: API hosting
- **Azure SQL**: Database hosting
- **GitHub Actions**: CI/CD pipeline
- **Azure CLI**: Deployment automation

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ (for build tools)
- .NET 8 SDK (for API development)
- Git

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/241RunnersAwareness.git
   cd 241RunnersAwareness
   ```

2. **Set up the API**
   ```bash
   cd 241RunnersAwarenessAPI
   dotnet restore
   dotnet run
   ```

3. **Set up the frontend**
   ```bash
   # Install build tools
   npm install -g htmlhint eslint csslint
   
   # Run build script
   chmod +x scripts/build.sh
   ./scripts/build.sh
   
   # Serve locally (optional)
   python -m http.server 8000
   ```

4. **Access the application**
   - Frontend: https://241runnersawareness.org
   - API: https://241runners-api.azurewebsites.net
   - Admin: https://241runnersawareness.org/admin

### Environment Configuration

Create a `config.json` file in the root directory:
```json
{
  "API_BASE_URL": "https://your-api-url.azurewebsites.net/api"
}
```

## 🚀 Deployment

### Frontend Deployment (GitHub Pages)
The frontend automatically deploys to GitHub Pages when changes are pushed to the `main` branch.

**Manual Deployment:**
```bash
# Build assets with hashing
node scripts/build.js

# Deploy to GitHub Pages
npm run deploy
```

### API Deployment (Azure App Service)
The API automatically deploys to Azure App Service via GitHub Actions.

**Manual Deployment:**
```bash
# Build and publish
cd 241RunnersAwarenessAPI
dotnet publish -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group 241runners-rg \
  --name 241runners-api \
  --src ./publish.zip
```

### Environment Configuration
1. **Azure App Service Configuration:**
   - `DefaultConnection`: Azure SQL connection string
   - `JWT_KEY`: Secure JWT signing key (32+ characters)
   - `JWT_ISSUER`: JWT issuer (241RunnersAwareness)
   - `JWT_AUDIENCE`: JWT audience (241RunnersAwareness)

2. **GitHub Pages Configuration:**
   - Custom domain: `241runnersawareness.org`
   - HTTPS: Enabled
   - CNAME: Configured for both www and apex domains

### Health Monitoring
- **Health Check**: https://241runners-api.azurewebsites.net/healthz
- **Readiness Check**: https://241runners-api.azurewebsites.net/readyz
- **Status Badge**: [![Deployment Status](https://github.com/241RunnersAwareness/241RunnersAwareness/workflows/API%20Build%20and%20Deploy/badge.svg)](https://github.com/241RunnersAwareness/241RunnersAwareness/actions)

## 📁 Project Structure

```
241RunnersAwareness/
├── 241RunnersAwarenessAPI/          # Backend API
│   ├── Controllers/                 # API controllers
│   ├── Hubs/                       # SignalR hubs
│   ├── Services/                   # Business logic services
│   ├── Models/                     # Data models
│   ├── Data/                       # Database context
│   └── Migrations/                 # Database migrations
├── admin/                          # Admin interface
│   ├── admindash.html             # Main admin dashboard
│   ├── login.html                 # Admin login
│   └── assets/                    # Admin-specific assets
├── js/                            # JavaScript modules
│   ├── admin-auth.js              # Authentication utilities
│   ├── admin-realtime.js          # Real-time updates
│   └── update-banner.js           # Service worker updates
├── assets/                        # Static assets
│   ├── images/                    # Images and media
│   ├── styles/                    # CSS files
│   └── js/                        # Frontend JavaScript
├── scripts/                       # Build and deployment scripts
│   └── build.sh                   # Asset building script
├── .github/                       # GitHub configuration
│   ├── workflows/                 # CI/CD pipelines
│   └── ISSUE_TEMPLATE/            # Issue templates
└── docs/                          # Documentation
```

## 🔧 Configuration

### API Configuration
The API uses environment variables for configuration:

```bash
# Database
DefaultConnection="Data Source=app.db"

# JWT Authentication
JWT_KEY="your-super-secret-key-that-is-at-least-32-characters-long"
JWT_ISSUER="241RunnersAwareness"
JWT_AUDIENCE="241RunnersAwareness"
```

### Frontend Configuration
Update `config.json` for different environments:

```json
{
  "API_BASE_URL": "https://241runners-api.azurewebsites.net/api",
  "ENVIRONMENT": "production"
}
```

## 🚀 Deployment

### Automatic Deployment
The project uses GitHub Actions for automatic deployment:

- **Frontend**: Deploys to GitHub Pages on push to main
- **API**: Deploys to Azure App Service on push to main
- **Assets**: Automatically hashed and cached

### Manual Deployment

1. **Build the frontend**
   ```bash
   ./scripts/build.sh
   ```

2. **Deploy the API**
   ```bash
   cd 241RunnersAwarenessAPI
   dotnet publish -c Release
   # Deploy to Azure App Service
   ```

3. **Update DNS**
   - Point domain to GitHub Pages
   - Configure CNAME for www subdomain

## 🔒 Security

### Authentication
- JWT tokens with automatic refresh
- Role-based access control (Admin/User)
- Secure token storage in localStorage
- Session timeout and automatic logout

### Data Protection
- HTTPS everywhere
- Input validation and sanitization
- SQL injection prevention
- XSS protection
- CSRF protection

### Monitoring
- Security event logging
- Failed login attempt tracking
- Real-time security monitoring
- Regular security audits

## 🧪 Testing

### Frontend Testing
```bash
# Lint HTML files
htmlhint "**/*.html"

# Lint JavaScript files
eslint "**/*.js"

# Lint CSS files
csslint "**/*.css"
```

### API Testing
```bash
cd 241RunnersAwarenessAPI
dotnet test
```

### Manual Testing
1. **Admin Login**: Test authentication flow
2. **Real-time Updates**: Open multiple admin sessions
3. **Responsive Design**: Test on different devices
4. **Performance**: Check loading times and responsiveness

## 📊 Monitoring

### Health Endpoints
- `/healthz`: Basic health check
- `/readyz`: Readiness check with database
- `/api/data-version`: Data version for polling

### Real-time Monitoring
- SignalR connection status
- Admin session tracking
- System performance metrics
- Error rate monitoring

## 🤝 Contributing

### Development Workflow
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests and linting
5. Submit a pull request

### Code Standards
- Follow existing code style
- Add comments for complex logic
- Write tests for new features
- Update documentation

### Issue Reporting
Use the provided bug report template for issues:
- Include reproduction steps
- Provide environment details
- Add relevant logs
- Check for duplicates first

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Community**: Thanks to all contributors and users
- **Security Researchers**: For helping keep the platform secure
- **Open Source**: Built with amazing open source tools
- **Volunteers**: For their time and dedication

## 📞 Support

- **Issues**: Use GitHub Issues for bug reports
- **Security**: Email security@241runnersawareness.org
- **General**: Contact through the website

## 🔄 Changelog

### Version 2.0.0 (Current)
- ✅ Real-time admin updates with SignalR
- ✅ Enhanced authentication with token refresh
- ✅ Improved admin dashboard with live data
- ✅ Service worker for offline support
- ✅ Automated CI/CD pipeline
- ✅ Comprehensive security measures
- ✅ Performance optimizations

### Version 1.0.0
- ✅ Basic admin dashboard
- ✅ Public case directory
- ✅ JWT authentication
- ✅ Responsive design
- ✅ API endpoints

---

**Built with ❤️ for the 241 Runners Awareness community**
# Staging slot deployment test
# Staging deployment trigger Sat Sep 13 14:36:24 EDT 2025
