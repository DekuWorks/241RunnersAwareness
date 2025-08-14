# 241 Runners Awareness - Authentication Testing Guide

## 🚀 Quick Start

The authentication system is now working with a **mock authentication system** that allows you to test login and signup functionality immediately, even when the backend is not running.

## 📋 Test Credentials

### Pre-configured Test Users

**Regular User:**
- Email: `test@example.com`
- Password: `password123`

**Admin User:**
- Email: `admin@241runners.org`
- Password: `admin123`

## 🧪 How to Test

### 1. Test Authentication System
Open `test-auth.html` in your browser to run comprehensive tests:
- Check authentication status
- Test login/logout functionality
- View mock user database
- Test navigation state changes
- Test notification system

### 2. Test Login Page
1. Go to `login.html`
2. Use the test credentials above
3. Click "Sign In"
4. You should see a success message and be redirected to the dashboard

### 3. Test Signup Page
1. Go to `signup.html`
2. Fill out the registration form
3. Click "Create Account"
4. You should see a success message and be redirected to the dashboard

### 4. Test Logout
1. After logging in, you'll see a "Logout" button in the navigation
2. Click "Logout" from any page
3. You should be logged out and redirected to the home page

## 🔧 How It Works

### Mock Authentication System
- **Fallback Mode**: When the backend is not available, the system automatically switches to mock authentication
- **Real Backend Priority**: The system first tries to connect to the real backend at `http://localhost:5113`
- **Seamless Experience**: Users get the same experience whether using mock or real authentication

### Features
- ✅ Login with email/password
- ✅ User registration with role selection
- ✅ Session management (localStorage)
- ✅ Navigation state management
- ✅ Logout functionality
- ✅ User feedback notifications
- ✅ Role-based access control

## 🛠️ Backend Integration

When the backend is running:
1. The system will automatically use the real API endpoints
2. All authentication data will be stored in the database
3. Google OAuth will work (when configured)

### To Start the Backend:
```bash
cd backend
dotnet run
```

The backend will run on `http://localhost:5113`

## 📁 Files Modified

- `auth-utils.js` - Enhanced with mock authentication system
- `login.html` - Updated to use universal authentication handlers
- `signup.html` - Updated to use universal authentication handlers
- `test-auth.html` - New testing page for authentication system

## 🎯 Next Steps

1. **Test the system** using the provided test page
2. **Try logging in** with the test credentials
3. **Test registration** with new user accounts
4. **Verify logout** functionality works from any page
5. **Check navigation** state changes when logged in/out

## 🔍 Troubleshooting

### If login/signup doesn't work:
1. Check the browser console for errors
2. Verify that `auth-utils.js` is loaded
3. Try refreshing the page
4. Clear browser cache and try again

### If backend integration fails:
1. The system will automatically fall back to mock authentication
2. Check that the backend is running on port 5113
3. Verify CORS settings in the backend
4. Check network connectivity

## 📞 Support

If you encounter any issues:
1. Check the browser console for error messages
2. Use the test page (`test-auth.html`) to diagnose problems
3. Verify all files are in the correct locations
4. Ensure JavaScript is enabled in your browser

---

**Note**: This mock authentication system is for development and testing purposes. In production, always use the real backend with proper security measures.
