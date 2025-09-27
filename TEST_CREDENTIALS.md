# Test User Credentials

## 🔑 **Available Test Users**

### **Regular User**
- **Email**: `test@example.com`
- **Password**: `TestPassword123!`
- **Role**: `user`
- **Status**: Active

### **Admin Users (All 6 Admin Accounts)**

#### **1. Marcus Brown (System Administrator)**
- **Email**: `dekuworks1@gmail.com`
- **Password**: `marcus2025`
- **Role**: `admin`
- **Organization**: 241 Runners Awareness
- **Title**: System Administrator
- **Status**: Active

#### **2. Daniel Carey (Administrator)**
- **Email**: `danielcarey9770@yahoo.com`
- **Password**: `Daniel2025!`
- **Role**: `admin`
- **Organization**: 241 Runners Awareness
- **Title**: Administrator
- **Status**: Active

#### **3. Lisa Thomas (Administrator)**
- **Email**: `lthomas3350@gmail.com`
- **Password**: `Lisa2025!`
- **Role**: `admin`
- **Organization**: 241 Runners Awareness
- **Title**: Administrator
- **Status**: Active

#### **4. Tina Matthews (Administrator)**
- **Email**: `tinaleggins@yahoo.com`
- **Password**: `Tina2025!`
- **Role**: `admin`
- **Organization**: 241 Runners Awareness
- **Title**: Administrator
- **Status**: Active

#### **5. Mark Melasky (Legal Administrator)**
- **Email**: `mmelasky@iplawconsulting.com`
- **Password**: `Mark2025!`
- **Role**: `admin`
- **Organization**: IP Law Consulting
- **Title**: Legal Administrator
- **Status**: Active

#### **6. Ralph Frank (Administrator)**
- **Email**: `ralphfrank900@gmail.com`
- **Password**: `Ralph2025!`
- **Role**: `admin`
- **Organization**: 241 Runners Awareness
- **Title**: Administrator
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
