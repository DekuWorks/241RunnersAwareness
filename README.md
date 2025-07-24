# 241RunnersAwareness.org

**Secure full-stack platform for reporting and managing missing individuals.**  
Built with React, .NET 8, and modern deployment tools, this system empowers users, caregivers, therapists, and law enforcement with secure tools to support recovery efforts.

🌐 [Visit Live Site](https://www.241runnersawareness.org)

---

## 👥 Our Team

### **Lisa Thomas** - Founder
*Leading the mission to honor Israel Thomas and support missing persons cases*

### **Marcus Brown** - Lead Front End Developer
*Expert in React, modern web technologies, and user experience design*

### **Daniel Carey** - Full Stack Developer
*Specialized in .NET Core, database design, and API development*

### **Tina Matthews** - Program Director
*Overseeing community outreach and program coordination*

### **Ralph Frank** - Event Coordinator
*Managing community events and awareness campaigns*

### **Arquelle Gilder** - Real Estate Broker / Sponsor
*Supporting our mission through strategic partnerships and sponsorship*

---

*In memory of Israel Thomas, who passed away at 2:41 AM. Our organization works tirelessly to prevent similar tragedies and support families affected by missing persons cases.*

---

## 🚀 Features

### 🔐 Authentication
- Google SSO Login
- Email/Password Signup + 2FA
- Role-based access control (User, Parent, Caregiver, ABA Therapist, Admin)
- Email verification via SendGrid
- SMS verification via Twilio

### 👥 User & Runner Management
- Custom registration forms based on user roles
- Emergency contact management
- Runner profiles with photo and biometric support
- Track adoption status and special needs context
- Photo upload/update features for case records

### 🧑‍💻 Admin Dashboard
- Full CRUD for users and case records
- Case tagging with status indicators: Missing, Found, Urgent, Resolved
- Real-time search, filter, and pagination
- Role-based permissions for sensitive actions
- Audit logging

### 🛍️ 241RA x Varlo Shop
- **Premium Athletic Wear**: Collaboration with Varlo for high-quality running and triathlon apparel
- **Mission-Driven Merchandise**: Every purchase supports DNA collection and identification technology
- **Campaign Support**: Active fundraising campaigns for specific initiatives
- **Shopping Cart**: Full e-commerce functionality with cart management
- **Donation Tracking**: Automatic donation calculations with each purchase
- **Responsive Design**: Mobile-friendly interface for all devices

### 🧬 DNA Tracking & Identification
- **Secure Sample Collection**: Buccal swabs, blood samples, hair follicles
- **Advanced Analysis**: STR markers, SNP analysis, haplotype determination
- **Database Integration**: NAMUS, CODIS, and local law enforcement databases
- **Real-Time Alerts**: Instant notifications for DNA matches
- **Comprehensive Reports**: Detailed analysis with recommendations
- **Lab Partnerships**: Integration with leading forensic laboratories

### 🧰 Tech Stack

**Frontend:**
- React (Vite)
- Redux Toolkit
- TailwindCSS
- React Hook Form
- Radix UI
- React Router

**Backend:**
- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL (with migrations)
- JWT Authentication
- Google OAuth

**DevOps:**
- CI/CD via Netlify
- Backend on Render
- Monorepo structure
- Swagger API documentation
- .env and CORS secured

---

## �� Project Structure
```
241RunnersAwareness-2/
├── frontend/          # React app
├── backend/           # ASP.NET Core Web API
├── index.html         # Static homepage
├── login.html         # Authentication page
├── signup.html        # Registration page
├── dashboard.html     # User dashboard
├── cases.html         # Case management
├── shop.html          # 241RA x Varlo Shop
├── dna-tracking.html  # DNA Tracking & Identification
├── privacy.html       # Privacy policy
├── terms.html         # Terms of use
├── manifest.json      # PWA manifest
├── styles.css         # Global styles
└── *.md              # Documentation
```

---

## 🛠️ Setup Instructions

### Prerequisites
- Node.js 18+ and npm
- .NET 8 SDK
- Git
- SQL Server or SQLite

### Frontend Setup (React)
```bash
cd frontend
npm install
npm run dev          # Start development server
npm run build        # Build for production
npm run test         # Run tests
```

### Backend Setup (.NET Core)
```bash
cd backend
dotnet restore       # Restore packages
dotnet build         # Build project
dotnet run           # Start development server
dotnet test          # Run tests
```

### Database Setup
```bash
cd backend
dotnet ef database update    # Apply migrations
dotnet ef migrations add     # Create new migration
```

### Environment Variables
Create `.env` files in both frontend and backend directories:

**Frontend (.env)**
```env
VITE_API_BASE_URL=http://localhost:5113/api
VITE_GOOGLE_CLIENT_ID=your_google_client_id
```

**Backend (.env)**
```env
ConnectionStrings__DefaultConnection=your_connection_string
JWT__SecretKey=your_jwt_secret
SendGrid__ApiKey=your_sendgrid_key
Twilio__AccountSid=your_twilio_sid
Twilio__AuthToken=your_twilio_token
Google__ClientId=your_google_client_id
```

---

## 🚀 Deployment

### Frontend (Netlify)
1. Connect GitHub repository to Netlify
2. Set build command: `cd frontend && npm run build`
3. Set publish directory: `frontend/dist`
4. Configure environment variables in Netlify dashboard

### Backend (Render)
1. Connect GitHub repository to Render
2. Set build command: `cd backend && dotnet publish -c Release`
3. Set start command: `cd backend && dotnet run`
4. Configure environment variables in Render dashboard

### Domain Configuration
- Custom domain: `241runnersawareness.org`
- SSL certificate (automatic via Netlify/Render)
- CNAME configuration for subdomains

---

## 🧪 Testing

### Frontend Testing
```bash
cd frontend
npm run test         # Unit tests
npm run test:coverage # Coverage report
npm run test:e2e     # End-to-end tests
```

### Backend Testing
```bash
cd backend
dotnet test          # Unit tests
dotnet test --collect:"XPlat Code Coverage" # Coverage
```

### Cross-Browser Testing
- Chrome, Firefox, Safari, Edge (desktop)
- iOS Safari, Android Chrome (mobile)
- Test responsive design and PWA functionality

---

## 📱 PWA Features

### Installability
- Manifest.json with app metadata
- Service worker for offline support
- App icons and splash screens
- Mobile install prompts

### Offline Support
- Cached static assets
- Offline fallback pages
- Background sync for data updates

---

## 🔒 Security Features

### Authentication
- JWT tokens with refresh mechanism
- Multi-factor authentication (2FA)
- Google OAuth integration
- Role-based access control

### Data Protection
- HTTPS encryption
- Input validation and sanitization
- SQL injection prevention
- XSS protection

### Privacy Compliance
- GDPR compliance
- HIPAA considerations
- Data retention policies
- User consent management

---

## 📊 API Documentation

### Authentication Endpoints
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/google-login` - Google OAuth
- `POST /api/auth/verify-email` - Email verification
- `POST /api/auth/verify-sms` - SMS verification
- `POST /api/auth/setup-2fa` - 2FA setup
- `POST /api/auth/verify-2fa` - 2FA verification

### User Management
- `GET /api/users` - Get all users (admin)
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (admin)

### Case Management
- `GET /api/individuals` - Get all cases
- `POST /api/individuals` - Create new case
- `PUT /api/individuals/{id}` - Update case
- `DELETE /api/individuals/{id}` - Delete case

### Shop & E-commerce
- `GET /api/shop/products` - Get all products
- `GET /api/shop/products/{id}` - Get product details
- `POST /api/shop/orders` - Create new order
- `GET /api/shop/partners` - Get partner information

### DNA Tracking
- `POST /api/dna/samples` - Submit DNA sample
- `GET /api/dna/samples/{id}` - Get DNA sample details
- `POST /api/dna/compare` - Compare DNA sequences
- `GET /api/dna/reports/{id}` - Generate DNA report
- `POST /api/dna/export/namus` - Export to NAMUS database
- `POST /api/dna/export/codis` - Export to CODIS database

### Export Features
- `GET /api/export/csv` - Export cases to CSV
- `GET /api/export/pdf` - Export cases to PDF

---

## 🐛 Troubleshooting

### Common Issues

**Backend won't start:**
```bash
cd backend
dotnet clean
dotnet restore
dotnet build
dotnet run
```

**Frontend build fails:**
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
npm run build
```

**Database connection issues:**
```bash
cd backend
dotnet ef database drop
dotnet ef database update
```

**PWA not installing:**
- Check manifest.json syntax
- Verify HTTPS is enabled
- Test on mobile device
- Check browser console for errors

---

## 📞 Support

### Contact Information
- **Email:** support@241runnersawareness.org
- **Emergency:** Call 911 for immediate assistance
- **Missing Persons:** 1-800-THE-LOST

### Documentation
- [Privacy Policy](privacy.html)
- [Terms of Use](terms.html)
- [API Documentation](https://api.241runnersawareness.org/swagger)

### Development
- **GitHub:** [Repository](https://github.com/DekuWorks/241RunnersAwareness)
- **Issues:** [GitHub Issues](https://github.com/DekuWorks/241RunnersAwareness/issues)
- **Discussions:** [GitHub Discussions](https://github.com/DekuWorks/241RunnersAwareness/discussions)

---

## 📈 Roadmap

### ✅ Completed
- Multi-role authentication system
- Admin dashboard with CRUD operations
- Case management and tracking
- PWA support and installability
- Legal compliance (Privacy Policy, Terms of Use)
- Mobile-responsive design
- **241RA x Varlo Shop**: Complete e-commerce platform with donation tracking
- **DNA Tracking & Identification**: Advanced DNA technology for missing persons cases
- **Static Site Integration**: Full functionality on both static site and React app

### 🔄 In Progress
- Cross-browser testing
- Service worker implementation
- App store preparation
- Enhanced documentation
- Payment processing integration
- Real DNA database connections

### 📋 Planned
- Notification system
- Analytics integration
- Advanced search features
- Mobile app development
- Advanced DNA analysis with machine learning
- International expansion

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

### Code Standards
- Follow existing code style
- Add comments for complex logic
- Write unit tests for new features
- Update documentation as needed

---

*Last updated: January 2025 - Added 241RA x Varlo Shop and DNA Tracking features*


