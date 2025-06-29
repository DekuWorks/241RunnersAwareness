# Google SSO Integration Guide

## Overview

This document describes the unified Google Single Sign-On (SSO) implementation for the 241 Runners Awareness platform. The implementation supports both the React application and public website with consistent authentication flow.

## 🏗️ Architecture

### Backend (ASP.NET Core)
- **Google OAuth Endpoint**: `/api/auth/google-login`
- **CORS Configuration**: Supports multiple domains
- **JWT Token Generation**: Secure authentication tokens
- **User Management**: Automatic user creation and role assignment

### Frontend (React)
- **Google OAuth Provider**: `@react-oauth/google`
- **Redux State Management**: Centralized auth state
- **Unified Logout**: Consistent logout across all components
- **Shared Styles**: Consistent UI/UX

### Public Website
- **Standalone Auth Page**: `auth.html` for non-React environments
- **Google Sign-In Button**: Native Google OAuth integration
- **Responsive Design**: Mobile-friendly authentication

## 🚀 Quick Start

### 1. Backend Setup

```bash
# Navigate to backend directory
cd backend

# Build the project
dotnet build

# Run the application
dotnet run
```

### 2. Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

### 3. Configuration

Update the configuration files with your actual URLs:

#### Backend Configuration (`backend/appsettings.json`)
```json
{
  "App": {
    "BaseUrl": "https://241runnersawareness.org",
    "ApiUrl": "https://api.241runnersawareness.org"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://241runnersawareness.org",
      "https://www.241runnersawareness.org",
      "https://app.241runnersawareness.org"
    ]
  }
}
```

#### Frontend Configuration (`frontend/src/config/api.js`)
```javascript
const config = {
  production: {
    API_BASE_URL: 'https://api.241runnersawareness.org/api',
    GOOGLE_CLIENT_ID: 'your-google-client-id',
    APP_URL: 'https://241runnersawareness.org'
  }
};
```

## 🔧 Configuration Details

### Google OAuth Setup

1. **Google Cloud Console**
   - Create a new project or use existing
   - Enable Google+ API
   - Create OAuth 2.0 credentials
   - Add authorized origins:
     - `https://241runnersawareness.org`
     - `https://www.241runnersawareness.org`
     - `http://localhost:5173` (development)

2. **Client ID Configuration**
   - Update `frontend/src/config/api.js`
   - Update `frontend/public/auth.html`
   - Update `frontend/src/main.jsx`

### CORS Configuration

The backend is configured to allow requests from:
- Main website: `https://241runnersawareness.org`
- www subdomain: `https://www.241runnersawareness.org`
- App subdomain: `https://app.241runnersawareness.org`
- Development: `http://localhost:3000`, `http://localhost:5173`

## 📱 Usage

### React Application

#### Login Page
```jsx
import { GoogleLogin } from '@react-oauth/google';
import { loginWithGoogle } from '../features/auth/authSlice';

const handleGoogleSuccess = (credentialResponse) => {
  dispatch(loginWithGoogle({ token: credentialResponse.credential }));
};
```

#### Logout
```jsx
import { unifiedLogout } from '../utils/authUtils';

const handleLogout = async () => {
  await unifiedLogout(navigate, '/');
};
```

### Public Website

#### Standalone Auth Page
Access the authentication page at `/auth.html` for non-React environments.

#### Google Sign-In Button
```html
<div id="g_id_onload"
     data-client_id="your-google-client-id"
     data-callback="handleGoogleSignIn"
     data-auto_prompt="false">
</div>
<div class="g_id_signin"
     data-type="standard"
     data-size="large"
     data-theme="outline"
     data-text="sign_in_with"
     data-shape="rectangular"
     data-logo_alignment="left">
</div>
```

## 🧪 Testing

### Automated Tests
Run the test script to verify integration:

```bash
# Development testing
NODE_ENV=development node test-google-sso.js

# Production testing
NODE_ENV=production node test-google-sso.js
```

### Manual Testing

1. **Backend Health Check**
   - Visit: `https://api.241runnersawareness.org`
   - Expected: 200 OK response

2. **Swagger UI**
   - Visit: `https://api.241runnersawareness.org/swagger`
   - Expected: API documentation interface

3. **Google OAuth Flow**
   - Click Google Sign-In button
   - Complete Google authentication
   - Verify JWT token generation
   - Check user creation/update

4. **CORS Testing**
   - Test from different domains
   - Verify preflight requests work
   - Check authentication headers

## 🔒 Security Considerations

### JWT Tokens
- Tokens expire after 7 days
- Stored securely in localStorage
- Automatically refreshed on API calls

### Google OAuth
- ID tokens validated server-side
- Access tokens revoked on logout
- Secure token transmission

### CORS
- Specific origins allowed only
- Credentials supported
- Preflight requests handled

## 🐛 Troubleshooting

### Common Issues

1. **CORS Errors**
   - Check backend CORS configuration
   - Verify frontend URL is in allowed origins
   - Ensure credentials are included in requests

2. **Google OAuth Errors**
   - Verify Google Client ID is correct
   - Check authorized origins in Google Console
   - Ensure HTTPS is used in production

3. **JWT Token Issues**
   - Check token expiration
   - Verify JWT secret key configuration
   - Ensure proper token storage

### Debug Mode

Enable debug logging in development:

```javascript
// Frontend
console.log('Google OAuth Response:', credentialResponse);

// Backend
app.UseDeveloperExceptionPage();
```

## 📚 API Reference

### Authentication Endpoints

#### POST `/api/auth/google-login`
Google OAuth authentication endpoint.

**Request:**
```json
{
  "IdToken": "google-id-token"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Google login successful.",
  "token": "jwt-token",
  "user": {
    "userId": "guid",
    "email": "user@example.com",
    "fullName": "User Name",
    "role": "user"
  },
  "requiresVerification": false
}
```

#### POST `/api/auth/login`
Traditional email/password authentication.

#### POST `/api/auth/register`
User registration endpoint.

## 🔄 Deployment

### Backend Deployment
1. Build the application: `dotnet publish -c Release`
2. Deploy to your hosting provider
3. Update environment variables
4. Configure SSL certificates

### Frontend Deployment
1. Build the application: `npm run build`
2. Deploy to Netlify/Vercel
3. Update environment variables
4. Configure custom domain

### Environment Variables
```bash
# Backend
JWT_SECRET_KEY=your-super-secret-key
GOOGLE_CLIENT_ID=your-google-client-id

# Frontend
VITE_API_BASE_URL=https://api.241runnersawareness.org/api
VITE_GOOGLE_CLIENT_ID=your-google-client-id
```

## 📞 Support

For issues or questions:
1. Check the troubleshooting section
2. Review the test script output
3. Check browser console for errors
4. Verify network requests in DevTools

## 🔄 Updates and Maintenance

### Regular Maintenance
- Monitor Google OAuth quotas
- Update dependencies regularly
- Review security configurations
- Test authentication flows

### Version Updates
- Keep Google OAuth library updated
- Monitor for breaking changes
- Test thoroughly before deployment
- Maintain backward compatibility

---

**Last Updated**: January 2025
**Version**: 1.0.0 