# 241 Runners Awareness

A comprehensive platform for raising awareness about missing persons cases, specifically focused on the 241 area code region. The system provides both public-facing information and secure admin tools for case management.

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
1. **SignalR Connection**: Admin clients connect to SignalR hub
2. **Event Broadcasting**: CRUD operations trigger real-time events
3. **Client Updates**: Connected clients receive updates instantly
4. **Polling Fallback**: If WebSocket fails, clients fall back to polling
5. **Debounced Processing**: Multiple events are batched for efficiency

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
   - Frontend: http://localhost:8000
   - API: https://localhost:7001
   - Admin: http://localhost:8000/admin

### Environment Configuration

Create a `config.json` file in the root directory:
```json
{
  "API_BASE_URL": "https://your-api-url.azurewebsites.net/api"
}
```

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
