# 241 Runners Awareness - Testing Checklist

## 🧪 Frontend Testing

### ✅ Build Tests
- [x] Frontend builds successfully (`npm run build`)
- [x] No compilation errors
- [x] All dependencies resolved

### 🌐 UI/UX Tests
- [ ] **Homepage Navigation**
  - [ ] Home page loads correctly
  - [ ] Navigation bar displays properly
  - [ ] All navigation links work
  - [ ] Mobile responsive design

- [ ] **Registration Form**
  - [ ] Form loads without errors
  - [ ] Role selection dropdown works
  - [ ] Conditional fields appear/disappear based on role
  - [ ] Form validation works
  - [ ] All field types render correctly (text, email, tel, date, select)
  - [ ] Google OAuth button displays
  - [ ] Form submission works

- [ ] **Login Form**
  - [ ] Form loads without errors
  - [ ] Email and password fields work
  - [ ] Google OAuth button displays
  - [ ] Form validation works
  - [ ] Login submission works

### 🎨 Styling Tests
- [ ] **Form Styling**
  - [ ] Form sections are properly styled
  - [ ] Individual section has distinct styling
  - [ ] Responsive design works on mobile
  - [ ] Focus states work correctly
  - [ ] Error messages display properly

## 🔧 Backend Testing

### ✅ Build Tests
- [x] Backend builds successfully (`dotnet build`)
- [x] No critical compilation errors
- [x] All dependencies resolved

### 🚀 API Tests
- [ ] **Server Startup**
  - [ ] Backend starts without errors
  - [ ] Swagger UI is accessible
  - [ ] API endpoints are available

- [ ] **Registration Endpoint** (`POST /api/auth/register`)
  - [ ] Accepts basic user registration
  - [ ] Accepts parent registration with individual info
  - [ ] Accepts caregiver registration with individual info
  - [ ] Accepts ABA therapist registration with credentials
  - [ ] Accepts adoptive parent registration
  - [ ] Stores all role-specific fields correctly
  - [ ] Creates individual records when provided
  - [ ] Creates emergency contacts when provided
  - [ ] Returns proper success/error responses

- [ ] **Login Endpoint** (`POST /api/auth/login`)
  - [ ] Accepts valid credentials
  - [ ] Rejects invalid credentials
  - [ ] Returns JWT token on success
  - [ ] Returns user data with role information

- [ ] **Google OAuth Endpoint** (`POST /api/auth/google-login`)
  - [ ] Accepts Google ID token
  - [ ] Creates new user from Google data
  - [ ] Logs in existing Google user
  - [ ] Returns proper authentication response

## 🔄 Integration Tests

### 📝 Registration Flow
- [ ] **Basic User Registration**
  1. Navigate to registration page
  2. Select "General User" role
  3. Fill in basic information (name, email, phone, password)
  4. Submit form
  5. Verify success message
  6. Check database for user record

- [ ] **Parent Registration**
  1. Navigate to registration page
  2. Select "Parent" role
  3. Fill in all required fields including runner information
  4. Submit form
  5. Verify success message
  6. Check database for user and individual records

- [ ] **ABA Therapist Registration**
  1. Navigate to registration page
  2. Select "ABA Therapist" role
  3. Fill in all required fields including credentials
  4. Submit form
  5. Verify success message
  6. Check database for user record with professional info

### 🔐 Login Flow
- [ ] **Traditional Login**
  1. Navigate to login page
  2. Enter valid credentials
  3. Submit form
  4. Verify successful login
  5. Check user state in Redux store

- [ ] **Google OAuth Login**
  1. Navigate to login page
  2. Click Google OAuth button
  3. Complete Google authentication
  4. Verify successful login
  5. Check user state in Redux store

## 🗄️ Database Tests

### 📊 Data Storage
- [ ] **User Table**
  - [ ] All role-specific fields are stored correctly
  - [ ] Role field is properly set
  - [ ] Address information is stored
  - [ ] Emergency contact information is stored

- [ ] **Individual Table** (when applicable)
  - [ ] Individual records are created for non-user roles
  - [ ] Runner information is stored correctly
  - [ ] Emergency contacts are linked properly

### 🔗 Relationships
- [ ] **User-Individual Relationship**
  - [ ] Users are properly linked to individuals
  - [ ] Foreign key constraints work correctly

## 🎯 User Experience Tests

### 📱 Responsive Design
- [ ] **Desktop View**
  - [ ] All forms display properly
  - [ ] Navigation works correctly
  - [ ] Styling is consistent

- [ ] **Mobile View**
  - [ ] Forms are mobile-friendly
  - [ ] Navigation menu works on mobile
  - [ ] Touch interactions work properly

### ⚡ Performance
- [ ] **Page Load Times**
  - [ ] Homepage loads quickly
  - [ ] Registration form loads quickly
  - [ ] Login form loads quickly

- [ ] **Form Responsiveness**
  - [ ] Form submissions are responsive
  - [ ] Loading states display correctly
  - [ ] Error handling is user-friendly

## 🚨 Error Handling

### ❌ Form Validation
- [ ] **Required Fields**
  - [ ] Required field validation works
  - [ ] Error messages display correctly
  - [ ] Form won't submit with missing required fields

- [ ] **Data Validation**
  - [ ] Email format validation
  - [ ] Phone number format validation
  - [ ] Password strength validation

### 🔄 API Error Handling
- [ ] **Network Errors**
  - [ ] Connection errors are handled gracefully
  - [ ] User sees appropriate error messages
  - [ ] App doesn't crash on network issues

- [ ] **Server Errors**
  - [ ] 500 errors are handled properly
  - [ ] User sees appropriate error messages
  - [ ] App remains functional

## 📋 Test Results Summary

### ✅ Passed Tests
- Frontend build
- Backend build
- Form structure and styling

### 🔄 Tests to Run
- [ ] Complete all UI/UX tests
- [ ] Complete all API tests
- [ ] Complete all integration tests
- [ ] Complete all database tests
- [ ] Complete all error handling tests

### 🎯 Ready for Production
- [ ] All tests pass
- [ ] No critical errors
- [ ] User experience is smooth
- [ ] Data is stored correctly
- [ ] Authentication works properly

---

## 🚀 Quick Test Commands

```bash
# Frontend
cd frontend
npm run build
npm run dev

# Backend
cd backend
dotnet build
dotnet run

# Database (if needed)
dotnet ef database update
```

## 📞 Support
If any tests fail, check:
1. Console errors in browser
2. Backend logs
3. Database connection
4. Environment variables
5. API endpoints accessibility 