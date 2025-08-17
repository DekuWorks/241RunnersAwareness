# 241RA Monorepo

A comprehensive development platform for 241 Runners Awareness, built as a monorepo with shared design tokens and modern web technologies.

## Structure

```
241ra/
├── packages/ui/           # Shared design tokens (CSS variables)
├── apps/
│   ├── static/           # Monorepo landing page (HTML + CSS)
│   ├── admin/            # React admin dashboard (Vite + TS + Tailwind)
│   └── api/              # .NET 8 API with Identity + JWT
├── index.html            # Main 241RA static site
├── login.html            # Authentication pages
├── aboutus.html          # Public pages
├── cases.html            # Missing persons cases
├── shop.html             # E-commerce integration
└── dna-tracking.html     # DNA technology showcase
```

## Features

- **Shared Design System**: All apps use the same design tokens from `packages/ui/tokens.css`
- **JWT Authentication**: Secure login with access + refresh tokens
- **Role-Based Access**: Admin/Manager/Staff/User roles with policy-based guards
- **Modern Stack**: React 18, TypeScript, Tailwind CSS, .NET 8, Entity Framework
- **Monorepo**: Single repository for all related applications
- **Static Site**: Main 241RA website with public pages and information
- **E-commerce**: Shop integration with Varlo partnership
- **DNA Technology**: Advanced DNA tracking and identification features

## Quick Start

### 1. Main Static Site

The main 241RA website is served from the root directory:
- Open `index.html` in a browser
- Features: Cases, Shop, DNA tracking, About Us, Authentication
- No build process required

### 2. Monorepo Landing

Access the monorepo overview at `apps/static/index.html`:
- Overview of all applications
- Links to admin dashboard and API
- Development status indicators

### 3. API Setup

```bash
cd apps/api/Api
dotnet run
```

The API will:
- Create the database automatically
- Seed an admin user: `admin@example.com` / `ChangeMe123!`
- Run on `https://localhost:5001`

### 4. Admin Dashboard

```bash
cd apps/admin
npm install
npm run dev
```

The admin app will run on `http://localhost:5173`

## Authentication Flow

1. **Login**: POST `/api/auth/login` with email/password
2. **JWT Token**: Returns access token (15min) + refresh token (14 days)
3. **Auto Refresh**: Axios interceptors handle token refresh automatically
4. **Role Guards**: React Router guards protect routes based on user roles

## API Endpoints

### Auth
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/refresh` - Refresh access token
- `GET /api/auth/me` - Get current user info
- `POST /api/auth/register` - Register new user

### Admin (Admin/Manager roles)
- `GET /api/admin/users` - List users with search/pagination
- `POST /api/admin/users/{id}/disable` - Disable user
- `POST /api/admin/users/{id}/roles` - Update user roles

## Environment Variables

### API (.NET)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AdminDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "Api",
    "Audience": "Admin", 
    "Key": "REPLACE_WITH_LONG_RANDOM_SECRET",
    "AccessMinutes": "15",
    "RefreshDays": "14"
  }
}
```

### Admin (React)
```env
VITE_API_URL=https://localhost:5001/api
```

## Navigation

### Main Site Navigation
- **Home**: Main landing page
- **About Us**: Organization information
- **Cases**: Missing persons cases
- **Shop**: E-commerce with Varlo partnership
- **DNA**: DNA tracking technology
- **Dev**: Link to monorepo overview
- **Donate**: External donation link
- **Follow Us**: Social media links

### Authentication
- **Sign Up**: User registration
- **Login**: User authentication
- **Logout**: Session termination

## Deployment

### Static Site
- Deploy all HTML/CSS/JS files to any static hosting
- No build process required
- Works with Netlify, Vercel, GitHub Pages, etc.

### API (Azure/Render/Fly)
- Set connection string, JWT key, and admin credentials
- Configure CORS for your admin domain

### Admin Dashboard
- Build with `npm run build`
- Deploy to any static hosting service
- Configure API URL for production

## Security Best Practices

- **JWT Keys**: Use strong, unique keys in production
- **HTTPS**: Always use HTTPS in production
- **CORS**: Configure CORS properly for your domains
- **Environment Variables**: Never commit secrets to version control
- **Database**: Use connection strings with proper authentication


