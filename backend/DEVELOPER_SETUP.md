# 🚀 241 Runners Awareness Backend - Developer Setup

## 📋 Table of Contents
1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Development Environment](#development-environment)
4. [API Documentation](#api-documentation)
5. [Database Setup](#database-setup)
6. [Testing](#testing)
7. [Deployment](#deployment)
8. [Contributing](#contributing)

---

## 🔧 Prerequisites

### Required Software
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** - LocalDB (included with Visual Studio) or SQL Server Express
- **Git** - [Download](https://git-scm.com/)
- **Visual Studio 2022** or **VS Code** - [Download](https://visualstudio.microsoft.com/)

### Optional Software
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Postman** - [Download](https://www.postman.com/) (for API testing)
- **Azure Data Studio** - [Download](https://docs.microsoft.com/en-us/sql/azure-data-studio/)

---

## ⚡ Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/your-org/241RunnersAwareness.git
cd 241RunnersAwareness/backend
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Run Database Migrations
```bash
dotnet ef database update
```

### 4. Start the Application
```bash
dotnet run
```

### 5. Verify Installation
- Open browser: `https://localhost:5001/health`
- Expected response: `{"status":"Healthy","checks":[...]}`

---

## 🛠️ Development Environment

### Using Docker (Recommended)
```bash
# Start all services (API + SQL Server + Redis)
docker-compose up --build

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

### Using Local Development
```bash
# Start SQL Server LocalDB
sqllocaldb start "MSSQLLocalDB"

# Run the application
dotnet run

# Or use the deployment script
./deploy.ps1 -Environment local
```

### Environment Variables
Create a `local.settings.json` file (not committed to git):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RunnersDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "your-local-development-secret-key"
  },
  "SendGrid": {
    "ApiKey": "your-sendgrid-key"
  },
  "Twilio": {
    "AccountSid": "your-twilio-sid",
    "AuthToken": "your-twilio-token"
  }
}
```

---

## 📚 API Documentation

### Swagger UI
- **Development**: `https://localhost:5001/swagger`
- **Production**: `https://api.241runnersawareness.org/swagger`

### Key Endpoints

#### Health Check
```http
GET /health
```
Returns application health status including database connectivity.

#### Authentication
```http
POST /api/auth/login
POST /api/auth/register
POST /api/auth/google
```

#### Individuals (Missing Persons)
```http
GET    /api/individuals
POST   /api/individuals
GET    /api/individuals/{id}
PUT    /api/individuals/{id}
DELETE /api/individuals/{id}
```

#### DNA Tracking
```http
GET    /api/dna
POST   /api/dna
GET    /api/dna/{id}
PUT    /api/dna/{id}
```

#### Shop/Products
```http
GET    /api/shop/products
POST   /api/shop/products
GET    /api/shop/products/{id}
PUT    /api/shop/products/{id}
DELETE /api/shop/products/{id}
```

#### Notifications
```http
GET    /api/notifications
POST   /api/notifications
GET    /api/notifications/{id}
```

#### Database Cleanup (Admin Only)
```http
POST /api/cleanup/remove-test-data
POST /api/cleanup/remove-duplicates
POST /api/cleanup/full-cleanup
GET  /api/cleanup/stats
```

### Real-time Notifications
Connect to SignalR hub: `/notificationHub`

---

## 🗄️ Database Setup

### Local Development
```bash
# Create database
dotnet ef database update

# Seed initial data
dotnet run --project 241RunnersAwareness.BackendAPI
```

### Database Migrations
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Database Schema
Key tables:
- **Users** - User accounts and authentication
- **Individuals** - Missing persons cases
- **EmergencyContacts** - Emergency contact information
- **Products** - Shop inventory
- **CaseImages** - Images related to cases
- **CaseDocuments** - Documents related to cases

---

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test --project Tests/UnitTests
dotnet test --project Tests/IntegrationTests
```

### Test Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Manual Testing with Postman
1. Import the Postman collection from `/docs/postman/`
2. Set up environment variables
3. Run the collection

---

## 🚀 Deployment

### Local Testing
```bash
# Build and run locally
./deploy.ps1 -Environment local

# Build and run with Docker
./deploy.ps1 -Environment docker
```

### Staging Deployment
```bash
# Deploy to staging
./deploy.ps1 -Environment staging
```

### Production Deployment
```bash
# Deploy to production
./deploy.ps1 -Environment production
```

### Azure DevOps Pipeline
The project includes an Azure DevOps pipeline (`azure-deploy.yml`) for automated deployments.

---

## 🤝 Contributing

### Development Workflow
1. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Changes**
   - Follow coding standards
   - Add tests for new features
   - Update documentation

3. **Test Your Changes**
   ```bash
   dotnet test
   dotnet run
   ```

4. **Create Pull Request**
   - Include description of changes
   - Reference any related issues
   - Ensure CI/CD passes

### Coding Standards
- Use **C# 12** features where appropriate
- Follow **Microsoft C# Coding Conventions**
- Use **async/await** for I/O operations
- Add **XML documentation** for public APIs
- Use **meaningful variable names**

### Commit Message Format
```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

### Code Review Checklist
- [ ] Code follows project standards
- [ ] Tests are included and passing
- [ ] Documentation is updated
- [ ] No security vulnerabilities
- [ ] Performance considerations addressed
- [ ] Error handling is appropriate

---

## 🔍 Troubleshooting

### Common Issues

#### Database Connection Issues
```bash
# Check LocalDB status
sqllocaldb info "MSSQLLocalDB"

# Start LocalDB if stopped
sqllocaldb start "MSSQLLocalDB"
```

#### Port Conflicts
```bash
# Check what's using port 5001
netstat -ano | findstr :5001

# Kill process if needed
taskkill /PID <process-id> /F
```

#### Build Issues
```bash
# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

#### Docker Issues
```bash
# Clean Docker
docker system prune -a

# Rebuild containers
docker-compose up --build --force-recreate
```

### Getting Help
1. Check the [Issues](https://github.com/your-org/241RunnersAwareness/issues) page
2. Search existing discussions
3. Create a new issue with:
   - Description of the problem
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details

---

## 📞 Support

### Team Contacts
- **Technical Lead**: [Your Name] - your.email@example.com
- **Backend Team**: backend-team@241runnersawareness.org
- **DevOps**: devops@241runnersawareness.org

### Resources
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [JWT Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)

---

*Last Updated: January 27, 2025*
*Version: 1.0.0*
