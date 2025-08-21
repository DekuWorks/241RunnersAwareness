# 241 Runners Awareness - Complete Platform

A comprehensive development platform for 241 Runners Awareness, featuring a modern ASP.NET Core backend API, React frontend applications, and a complete database schema for missing persons tracking and management.

## 🏗️ Project Structure

```
241RunnersAwareness/
├── backend/                    # ASP.NET Core 8.0 Backend API
│   ├── Controllers/           # API endpoints
│   ├── DBContext/            # Entity Framework models
│   ├── Services/             # Business logic services
│   ├── Migrations/           # Database migrations
│   └── Program.cs            # Application entry point
├── frontend/                  # React frontend application
│   ├── src/
│   │   ├── components/       # React components
│   │   ├── pages/           # Page components
│   │   ├── features/        # Redux slices
│   │   └── utils/           # Utility functions
│   └── package.json
├── apps/
│   ├── admin/               # React admin dashboard
│   ├── api/                 # Additional API services
│   └── static/              # Static site generator
├── packages/ui/             # Shared design tokens
├── docs/                    # Documentation and assets
└── *.html                   # Static HTML pages
```

## 🚀 Quick Start

### Backend API Setup

1. **Prerequisites**
   - .NET 8.0 SDK
   - SQL Server (LocalDB or full instance)
   - PowerShell (for scripts)

2. **Database Setup**
   ```powershell
   # Run the database fix script
   .\fix-database.ps1
   ```

3. **Start Backend**
   ```powershell
   cd backend
   dotnet run
   ```
   - API runs on: `http://localhost:5113`
   - Swagger UI: `http://localhost:5113/swagger`

### Frontend Setup

1. **Install Dependencies**
   ```bash
   cd frontend
   npm install
   ```

2. **Start Development Server**
   ```bash
   npm run dev
   ```
   - Frontend runs on: `http://localhost:5173`

### Testing Authentication

Use the provided test scripts:
```powershell
# Test API connectivity and authentication
.\test-auth.ps1

# Simple login test
.\test-login.ps1
```

## 📊 Database Schema

### Users Table
Complete user management with authentication and role-based access:

```sql
Users (
    UserId (Guid, PK),
    Username (string),
    Email (string, unique),
    FullName (string),
    PhoneNumber (string),
    PasswordHash (string),
    Role (string), -- admin, user, therapist, caregiver, parent, adoptive_parent
    EmailVerified (bool),
    PhoneVerified (bool),
    Organization (string),
    Credentials (string),
    Specialization (string),
    YearsOfExperience (string),
    CreatedAt (DateTime),
    IsActive (bool),
    
    -- Password Reset Fields
    PasswordResetCount (int),
    LastPasswordResetAt (DateTime),
    PasswordResetToken (string),
    PasswordResetTokenExpiry (DateTime),
    PasswordResetYear (int),
    
    -- Two-Factor Authentication
    TwoFactorEnabled (bool),
    TwoFactorSecret (string),
    TwoFactorBackupCodes (string),
    TwoFactorSetupDate (DateTime),
    
    -- Refresh Tokens
    RefreshToken (string),
    RefreshTokenExpiry (DateTime),
    
    -- Email/Phone Verification
    EmailVerificationToken (string),
    EmailVerificationExpiry (DateTime),
    PhoneVerificationCode (string),
    PhoneVerificationExpiry (DateTime),
    
    -- Additional Fields
    Address, City, State, ZipCode,
    EmergencyContactName, EmergencyContactPhone, EmergencyContactRelationship,
    RelationshipToRunner, LicenseNumber, IndividualId,
    LastLoginAt
)
```

### Other Tables
- **Individuals**: Missing persons data
- **DNAReports**: DNA analysis results
- **Products**: E-commerce items
- **EmergencyContacts**: Emergency contact information

## 🔐 Authentication System

### JWT-Based Authentication
- **Access Tokens**: 60-minute expiry
- **Refresh Tokens**: 30-day expiry
- **Role-Based Authorization**: Multiple user roles supported

### Available Endpoints

#### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register-simple` - Simple user registration
- `POST /api/auth/register` - Full user registration with role-specific fields
- `POST /api/auth/google-login` - Google OAuth integration
- `GET /api/auth/test` - API connectivity test

#### User Management
- `GET /api/usermanagement/users` - List all users
- `GET /api/usermanagement/users/{id}` - Get specific user
- `POST /api/usermanagement/users` - Create new user
- `PUT /api/usermanagement/users/{id}` - Update user

#### Other Services
- `GET /api/map` - Map data endpoints
- `GET /api/dna` - DNA tracking endpoints
- `GET /api/notifications` - Real-time notifications

## 🛠️ Development Status

### ✅ Completed Features
- **Backend API**: Fully functional ASP.NET Core 8.0 API
- **Database Schema**: Complete with all required tables and relationships
- **Authentication**: JWT-based auth with role management
- **Swagger Documentation**: Interactive API documentation
- **Database Migrations**: Entity Framework migrations
- **Error Handling**: Comprehensive error handling and logging
- **CORS Configuration**: Cross-origin resource sharing setup
- **SignalR Integration**: Real-time notifications
- **Email/SMS Services**: Integration with SendGrid and Twilio

### 🔧 Current Issues
- **Database Schema Sync**: Some password reset columns need final synchronization
- **Authentication Testing**: Login/registration endpoints returning 500 errors (schema-related)

### 🚧 In Progress
- **Frontend Integration**: React app connecting to backend
- **Admin Dashboard**: User management interface
- **Testing Suite**: Comprehensive API testing

## 🧪 Testing

### API Testing Scripts
- `test-auth.ps1`: Comprehensive authentication testing
- `test-login.ps1`: Simple login verification
- `fix-database.ps1`: Database schema repair

### Manual Testing
1. **Swagger UI**: Visit `http://localhost:5113/swagger`
2. **API Endpoints**: Test all endpoints through Swagger
3. **Database**: Use SQL Server Management Studio or sqlcmd

## 🔧 Configuration

### Backend Configuration (`backend/appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RunnersDb;Trusted_Connection=true;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  },
  "Jwt": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "241RunnersAwareness",
    "Audience": "241RunnersAwareness",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 30
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "http://localhost:5113"
    ]
  }
}
```

### Environment Variables
- `JWT_SECRET_KEY`: Strong secret for JWT signing
- `SENDGRID_API_KEY`: Email service API key
- `TWILIO_ACCOUNT_SID`: SMS service credentials
- `GOOGLE_CLIENT_ID`: Google OAuth credentials

## 🚀 Deployment

### Backend Deployment
1. **Azure**: Use provided Azure deployment scripts
2. **Docker**: Dockerfile included for containerization
3. **Local**: Direct deployment with `dotnet publish`

### Frontend Deployment
1. **Build**: `npm run build`
2. **Deploy**: Static hosting (Netlify, Vercel, etc.)

### Database Deployment
1. **Local**: SQL Server LocalDB
2. **Production**: Azure SQL Database or SQL Server instance
3. **Migrations**: Use Entity Framework migrations

## 📚 Documentation

### API Documentation
- **Swagger UI**: `http://localhost:5113/swagger`
- **OpenAPI Spec**: Available through Swagger

### Code Documentation
- **Controllers**: Well-documented API endpoints
- **Services**: Business logic with XML comments
- **Models**: Entity Framework models with annotations

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch
3. **Make** your changes
4. **Test** thoroughly
5. **Submit** a pull request

## 📞 Support

For technical support or questions:
- **Email**: Contact the development team
- **Issues**: Use GitHub issues for bug reports
- **Documentation**: Check the docs folder for detailed guides

## 📄 License

This project is proprietary software for 241 Runners Awareness.

---

**Last Updated**: August 17, 2025  
**Version**: 1.0.0  
**Status**: Development Phase


