# 🚀 Quick Start Guide - 241 Runners Awareness Backend

## ⚡ **For New Developers - Get Running in 5 Minutes**

### **Step 1: Install Prerequisites**
```bash
# 1. Install Docker Desktop
# Download from: https://www.docker.com/products/docker-desktop
# Start Docker Desktop

# 2. Install Git
# Download from: https://git-scm.com/

# 3. Install VS Code (optional but recommended)
# Download from: https://code.visualstudio.com/
```

### **Step 2: Clone the Repository**
```bash
# Clone the repository
git clone https://github.com/your-org/241RunnersAwareness.git

# Navigate to backend directory
cd 241RunnersAwareness/backend
```

### **Step 3: Start the Application**
```bash
# Start all services (API + Database + Redis)
docker-compose up --build
```

### **Step 4: Verify It's Working**
```bash
# Open your browser and go to:
# Health Check: http://localhost:8080/health
# API Documentation: http://localhost:8080/swagger
```

### **Step 5: Test the API**
```bash
# Test health endpoint
curl http://localhost:8080/health

# Expected response:
# {"status":"Healthy","checks":[...]}
```

---

## 🛠️ **Development Commands**

### **Start Services**
```bash
# Start all services in background
docker-compose up -d

# Start with logs visible
docker-compose up
```

### **Stop Services**
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (clears database)
docker-compose down -v
```

### **View Logs**
```bash
# View API logs
docker-compose logs -f api

# View database logs
docker-compose logs -f sqlserver

# View all logs
docker-compose logs -f
```

### **Restart Services**
```bash
# Restart just the API
docker-compose restart api

# Restart all services
docker-compose restart
```

---

## 📚 **What's Running**

| Service | URL | Description |
|---------|-----|-------------|
| **API** | http://localhost:8080 | Main backend API |
| **Health** | http://localhost:8080/health | Health check endpoint |
| **Swagger** | http://localhost:8080/swagger | API documentation |
| **Database** | localhost:1433 | SQL Server database |
| **Redis** | localhost:6379 | Cache/real-time messaging |

---

## 🔍 **Testing the API**

### **1. Health Check**
```bash
curl http://localhost:8080/health
```

### **2. View API Documentation**
- Open: http://localhost:8080/swagger
- Explore available endpoints
- Test endpoints directly from the UI

### **3. Test Authentication**
```bash
# Register a new user
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'

# Login
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

### **4. Test Individuals Endpoint**
```bash
# Get all individuals (requires authentication)
curl -X GET http://localhost:8080/api/individuals \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🗄️ **Database Access**

### **Using Azure Data Studio**
1. Install Azure Data Studio
2. Connect to: `localhost:1433`
3. Username: `sa`
4. Password: `YourStrongPassword123!`
5. Database: `RunnersDb`

### **Using SQL Server Management Studio**
1. Connect to: `localhost:1433`
2. Authentication: SQL Server Authentication
3. Username: `sa`
4. Password: `YourStrongPassword123!`

### **Using Command Line**
```bash
# Connect to database
docker exec -it runners-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrongPassword123! -d RunnersDb

# Run a query
SELECT * FROM Users;
GO
```

---

## 🔧 **Troubleshooting**

### **Port Already in Use**
```bash
# Check what's using port 8080
netstat -ano | findstr :8080

# Kill the process
taskkill /PID <process-id> /F
```

### **Docker Issues**
```bash
# Clean Docker
docker system prune -a

# Rebuild containers
docker-compose up --build --force-recreate
```

### **Database Connection Issues**
```bash
# Check if database is running
docker-compose ps

# Restart database
docker-compose restart sqlserver

# Check database logs
docker-compose logs sqlserver
```

### **API Not Starting**
```bash
# Check API logs
docker-compose logs api

# Check if all services are healthy
docker-compose ps
```

---

## 📝 **Making Changes**

### **Code Changes**
1. Edit files in your IDE
2. Save changes
3. Docker will auto-reload (if using `docker-compose up`)
4. Or restart: `docker-compose restart api`

### **Database Changes**
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

### **Adding New Dependencies**
1. Edit `241RunnersAwareness.BackendAPI.csproj`
2. Rebuild: `docker-compose up --build`

---

## 🎯 **Next Steps**

### **For Frontend Developers**
- API is ready at: http://localhost:8080
- Update your frontend API base URL
- Test authentication flow
- Test all CRUD operations

### **For Backend Developers**
- Explore the codebase structure
- Check out the controllers in `/Controllers`
- Review the data models in `/DBContext/Models`
- Look at services in `/Services`

### **For DevOps**
- Review deployment files
- Check Docker configuration
- Review environment variables
- Test production deployment

---

## 📞 **Getting Help**

### **Common Issues**
1. **Docker not running** - Start Docker Desktop
2. **Port conflicts** - Check if ports 8080, 1433, 6379 are free
3. **Database connection** - Wait for SQL Server to fully start
4. **API not responding** - Check logs with `docker-compose logs api`

### **Team Support**
- **Technical Lead**: [Your Name] - your.email@example.com
- **Backend Team**: backend-team@241runnersawareness.org
- **Documentation**: Check `/DEVELOPER_SETUP.md` for detailed guide

---

## ✅ **Success Checklist**

- [ ] Docker Desktop installed and running
- [ ] Repository cloned successfully
- [ ] `docker-compose up --build` completed without errors
- [ ] Health check returns: `{"status":"Healthy"}`
- [ ] Swagger UI accessible at: http://localhost:8080/swagger
- [ ] Can connect to database
- [ ] Can test at least one API endpoint

---

*Last Updated: January 27, 2025*
*Version: 1.0.0*
