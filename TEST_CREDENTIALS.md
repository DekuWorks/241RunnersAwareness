# Test User Credentials

## 🔑 **Available Test Users**

### **Regular User**
- **Email**: `test@example.com`
- **Password**: `TestPassword123!`
- **Role**: `user`
- **Status**: Active

### **Admin Users**
- **Email**: `danielcarey9770@yahoo.com`
- **Role**: `admin`
- **Status**: Active

- **Email**: `lthomas3350@gmail.com`
- **Role**: `admin`
- **Status**: Active

- **Email**: `tinaleggins@yahoo.com`
- **Role**: `admin`
- **Status**: Active

## 🌐 **Testing the Web Application**

### **Login Process**
1. Visit: `http://localhost:8080/login.html`
2. Enter credentials:
   - Email: `test@example.com`
   - Password: `TestPassword123!`
3. Click "Sign In"
4. You should be redirected to the dashboard/profile

### **Profile Access**
1. After login, visit: `http://localhost:8080/profile.html`
2. You should see the user's profile information
3. Test profile editing functionality

## 📱 **Testing the Mobile Application**

### **Login Process**
1. Open the mobile app
2. Enter the same credentials
3. Should get the same JWT token
4. Access profile and other features

## 🔧 **API Testing**

### **Direct API Login**
```bash
curl -X POST "https://241runners-api-v2.azurewebsites.net/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!"
  }'
```

### **Profile Access with Token**
```bash
curl -X GET "https://241runners-api-v2.azurewebsites.net/api/auth/me" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📊 **Cross-Platform Testing**

1. **Create account on web** → Login on mobile
2. **Create account on mobile** → Login on web
3. **Update profile on web** → View changes on mobile
4. **Update profile on mobile** → View changes on web

All data should be synchronized in real-time!
