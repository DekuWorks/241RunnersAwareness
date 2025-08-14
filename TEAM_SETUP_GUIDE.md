# 🚀 241 Runners Awareness - Team Setup Guide

## 🔧 Quick Start for Developers

### Prerequisites
- .NET 8.0 SDK installed
- SQL Server LocalDB (usually comes with Visual Studio)
- Git

### 🚀 Starting the Backend Server

**Option 1: Using the Batch File (Easiest)**
```bash
# Double-click this file in Windows Explorer:
start-backend.bat
```

**Option 2: Using PowerShell Script**
```powershell
# Run this in PowerShell:
.\start-backend.ps1
```

**Option 3: Manual Commands**
```bash
# Navigate to backend directory
cd backend

# Start the server
dotnet run
```

### ✅ Verify Backend is Running

Once started, you should see:
```
Database seeded successfully!
Now listening on: http://localhost:5113
Application started. Press Ctrl+C to shut down.
```

### 🧪 Test Authentication

Use these test accounts to verify everything works:

| Email | Password | Role |
|-------|----------|------|
| `admin@241runners.org` | `admin123` | Admin |
| `test@example.com` | `password123` | GeneralUser |
| `lisa@241runners.org` | `lisa2025` | Admin |

### 🌐 Access Points

- **Health Check**: http://localhost:5113/health
- **API Documentation**: http://localhost:5113/swagger
- **Frontend**: Open `index.html` in your browser

## 🔍 Troubleshooting

### Backend Won't Start?

1. **Check .NET Installation**:
   ```bash
   dotnet --version
   ```
   Should show 8.0.x

2. **Check Database**:
   ```bash
   cd backend
   dotnet ef database update
   ```

3. **Port Already in Use**:
   ```bash
   # Find what's using port 5113
   netstat -ano | findstr :5113
   
   # Kill the process if needed
   taskkill /PID <process_id> /F
   ```

### Can't Sign In?

1. **Backend Not Running**: Make sure you see "Now listening on: http://localhost:5113"
2. **Wrong Credentials**: Use the exact test credentials above
3. **Browser Cache**: Clear browser cache or try incognito mode
4. **CORS Issues**: Make sure you're accessing from localhost

### Database Issues?

```bash
cd backend
dotnet ef database drop --force
dotnet ef database update
dotnet run
```

## 📁 Project Structure

```
241RunnersAwareness/
├── backend/                 # ASP.NET Core API
│   ├── Controllers/         # API endpoints
│   ├── Services/           # Business logic
│   ├── DBContext/          # Database models
│   └── Program.cs          # Main entry point
├── frontend/               # React application
├── index.html              # Main static page
├── login.html              # Login page
├── signup.html             # Registration page
├── start-backend.bat       # Windows startup script
├── start-backend.ps1       # PowerShell startup script
└── BACKEND_SETUP_COMPLETE.md  # Detailed documentation
```

## 🔄 Development Workflow

1. **Start Backend**: Run `start-backend.bat` or `dotnet run` in backend folder
2. **Test API**: Visit http://localhost:5113/swagger
3. **Test Frontend**: Open `index.html` in browser
4. **Sign In**: Use test credentials above
5. **Make Changes**: Edit code as needed
6. **Restart if Needed**: Ctrl+C to stop, then restart

## 🆘 Getting Help

1. **Check Logs**: Look at the console output when starting backend
2. **API Testing**: Use Swagger at http://localhost:5113/swagger
3. **Database**: Check if LocalDB is running
4. **Network**: Ensure port 5113 is not blocked by firewall

## 🎯 Success Checklist

- [ ] Backend starts without errors
- [ ] "Database seeded successfully!" appears
- [ ] "Now listening on: http://localhost:5113" appears
- [ ] Can access http://localhost:5113/health
- [ ] Can access http://localhost:5113/swagger
- [ ] Can sign in with test credentials
- [ ] Can see dashboard after login
- [ ] Logout button works

---

**🎉 Once all checkboxes are green, you're ready to develop!**
