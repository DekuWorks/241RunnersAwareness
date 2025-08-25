# 🏃 241 Runners Awareness

**Advanced missing persons tracking and community safety platform for the Houston area**

[![Netlify Status](https://api.netlify.com/api/v1/badges/your-site-id/deploy-status)](https://app.netlify.com/sites/241runnersawareness/deploys)
[![GitHub Actions](https://github.com/DekuWorks/241RunnersAwareness/workflows/Branch%20Protection%20Checks/badge.svg)](https://github.com/DekuWorks/241RunnersAwareness/actions)

## 🌟 Overview

241 Runners Awareness is a comprehensive platform designed to help locate missing persons and improve community safety in the Houston metropolitan area. The platform combines advanced DNA tracking technology, interactive mapping, and community engagement tools to support law enforcement and families in their search efforts.

**Live Site**: https://241runnersawareness.org

## ✨ Key Features

### 🧬 **DNA Tracking System**
- Advanced DNA sample collection and analysis
- Integration with national databases (NAMUS, CODIS)
- Real-time DNA matching and alerts
- Comprehensive reporting system
- Partner laboratory network

### 🗺️ **Interactive Map Dashboard**
- Real-time tracking of missing persons cases
- Houston area coverage with 50-mile radius
- Advanced filtering and search capabilities
- Statistics and analytics dashboard
- Heat map visualization

### 👥 **Community Engagement**
- User registration and case reporting
- Community alerts and notifications
- Volunteer coordination tools
- Educational resources and safety tips

### 🔐 **Admin Dashboard**
- Case management system
- User administration
- Analytics and reporting
- Content management

## 🏗️ Architecture

### Frontend
- **Static Site**: HTML, CSS, JavaScript (Vanilla)
- **React App**: Modern admin interface
- **Responsive Design**: Mobile-first approach
- **Progressive Web App**: Offline capabilities

### Backend
- **.NET 9.0 API**: RESTful services
- **Entity Framework**: Database management
- **JWT Authentication**: Secure user sessions
- **SignalR**: Real-time notifications

### Database
- **SQLite**: Development and testing
- **Azure SQL**: Production deployment
- **Migrations**: Automated schema updates

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ 
- .NET 9.0 SDK
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/DekuWorks/241RunnersAwareness.git
   cd 241RunnersAwareness
   ```

2. **Run the setup script**
   ```bash
   .\manage-project.ps1 setup
   ```

3. **Start development environment**
   ```bash
   .\manage-project.ps1 dev
   ```

4. **Open in browser**
   - Main site: `index.html`
   - Admin: `admin-dashboard.html`
   - Map: `map.html`
   - DNA tracking: `dna-tracking.html`

## 📋 Project Management

The project includes a comprehensive management script that handles all common tasks:

```bash
# Development
.\manage-project.ps1 dev      # Start development environment

# Testing
.\manage-project.ps1 test     # Run all tests and validation

# Deployment
.\manage-project.ps1 deploy   # Deploy to production

# Maintenance
.\manage-project.ps1 clean    # Clean up temporary files
.\manage-project.ps1 setup    # Initial project setup
.\manage-project.ps1 help     # Show help
```

## 🗂️ Project Structure

```
241RunnersAwareness/
├── 📁 frontend/              # React admin application
├── 📁 backend/               # .NET API backend
├── 📁 admin/                 # Admin subdomain files
├── 📁 docs/                  # Documentation and assets
├── 📁 .github/               # GitHub Actions and configs
├── 🧬 dna-tracking.html      # DNA tracking page
├── 🗺️ map.html              # Interactive map dashboard
├── 👤 admin-dashboard.html   # Admin dashboard
├── 🏠 index.html             # Main homepage
├── 📄 cases.html             # Cases listing
├── 📝 report-case.html       # Case reporting form
├── 🎨 styles.css             # Main stylesheet
├── ⚙️ manage-project.ps1     # Project management script
└── 📖 README.md              # This file
```

## 🔧 Configuration

### Environment Variables
Copy `env.example` to `.env` and configure:

```env
# Database
DATABASE_CONNECTION_STRING=your_connection_string

# Authentication
JWT_SECRET=your_jwt_secret
JWT_EXPIRY_HOURS=24

# Email/SMS
SMTP_HOST=your_smtp_host
SMTP_PORT=587
SMTP_USERNAME=your_email
SMTP_PASSWORD=your_password

# External APIs
GOOGLE_MAPS_API_KEY=your_google_maps_key
```

### Database Setup
```bash
# Run migrations
cd backend
dotnet ef database update
```

## 🧪 Testing

```bash
# Run all tests
.\manage-project.ps1 test

# Backend tests only
cd backend
dotnet test

# Frontend tests only
cd frontend
npm test
```

## 🚀 Deployment

### Automatic Deployment
The project is automatically deployed to Netlify when changes are pushed to the main branch.

### Manual Deployment
```bash
# Deploy to production
.\manage-project.ps1 deploy
```

### Deployment Targets
- **Static Site**: Netlify (https://241runnersawareness.org)
- **Backend API**: Azure App Service
- **Database**: Azure SQL Database

## 🔒 Security

### Branch Protection
- Pull request reviews required
- Automated testing and validation
- Code quality checks
- Security scanning

### Data Protection
- JWT token authentication
- Encrypted data transmission
- HIPAA-compliant DNA data handling
- Regular security audits

## 📊 Features in Detail

### DNA Tracking
- **Sample Collection**: Buccal swabs, blood samples, hair follicles
- **Analysis**: STR markers, SNP analysis, haplotype determination
- **Databases**: NAMUS, CODIS, local law enforcement integration
- **Reporting**: Comprehensive analysis reports with recommendations

### Interactive Map
- **Real-time Data**: Live updates from multiple sources
- **Advanced Filtering**: Status, time range, location-based
- **Statistics**: Case counts, resolution rates, trends
- **Visualization**: Marker clustering, heat maps, custom icons

### Community Features
- **Case Reporting**: Anonymous and authenticated reporting
- **Alerts**: Email and SMS notifications
- **Volunteer Coordination**: Task assignment and tracking
- **Educational Resources**: Safety tips and prevention guides

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow the existing code style
- Add tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting

## 📞 Support

### Documentation
- [Branch Protection Setup](BRANCH_PROTECTION_SETUP.md)
- [API Documentation](docs/api.md)
- [Deployment Guide](docs/deployment.md)

### Contact
- **Email**: support@241runnersawareness.org
- **Website**: https://241runnersawareness.org
- **GitHub Issues**: [Create an issue](https://github.com/DekuWorks/241RunnersAwareness/issues)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Houston law enforcement agencies
- NAMUS and CODIS database teams
- Partner laboratories and forensic experts
- Community volunteers and supporters
- Open source contributors

---

**Made with ❤️ for the Houston community**

*Supporting missing persons and their families since 2025*


