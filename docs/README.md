# 📚 241RunnersAwareness Project Documentation

## Overview
This folder contains comprehensive documentation for the 241RunnersAwareness project, including deployment guides, bug reports, cleanup summaries, and next steps planning.

---

## 📋 **Documentation Index**

### 🚀 **Deployment & Operations**
- **[AZURE_DEPLOYMENT_STATUS.md](./AZURE_DEPLOYMENT_STATUS.md)** - Complete Azure backend deployment status and testing results
- **[DEPLOYMENT.md](./DEPLOYMENT.md)** - Step-by-step deployment guide for Azure and frontend
- **[DATABASE_SETUP.md](./DATABASE_SETUP.md)** - Database setup and migration guide

### 🧹 **Project Management**
- **[CLEANUP_SUMMARY.md](./CLEANUP_SUMMARY.md)** - Project cleanup summary and file optimization results
- **[NEXT_STEPS_SUMMARY.md](./NEXT_STEPS_SUMMARY.md)** - Comprehensive roadmap and action items for future development

### 🐛 **Quality Assurance**
- **[BUG_REPORT.md](./BUG_REPORT.md)** - Bug analysis, fixes applied, and code quality assessment

### 📖 **Legal & Policy**
- **[privacy-policy.pdf](./privacy-policy.pdf)** - Privacy policy document
- **[terms-of-use.pdf](./terms-of-use.pdf)** - Terms of use document

### 📝 **Project History**
- **[CHANGELOG.md](./CHANGELOG.md)** - Project change log and version history

---

## 🎯 **Quick Reference**

### **Current Status**
- **Backend**: ✅ Deployed to Azure and fully operational
- **Frontend**: ✅ Deployed to GitHub Pages and functional
- **Database**: ✅ Connected and healthy
- **Security**: ✅ Enterprise-grade implementation
- **Performance**: ✅ Sub-second response times

### **Key Metrics**
- **API Endpoints**: 17 tested and working
- **Users**: 6 total (4 admin, 2 regular)
- **Runners**: 3 active cases
- **Uptime**: 100% since deployment
- **Response Time**: < 400ms average

---

## 🔧 **Development Tools**

### **Admin Setup Script**
- **File**: `../setup-azure-admin-users.js`
- **Purpose**: Create and configure admin users in Azure backend
- **Usage**: `node setup-azure-admin-users.js`

### **API Testing**
- **Health Check**: `https://241runners-api.azurewebsites.net/api/auth/health`
- **Test Endpoint**: `https://241runners-api.azurewebsites.net/api/auth/test`
- **Admin Dashboard**: `https://241runnersawareness.org/admin/`

---

## 📁 **Repository Structure**

```
241RunnersAwareness/
├── docs/                           # 📚 This documentation folder
│   ├── README.md                   # 📖 This file
│   ├── AZURE_DEPLOYMENT_STATUS.md  # 🚀 Azure deployment status
│   ├── CLEANUP_SUMMARY.md          # 🧹 Project cleanup summary
│   ├── NEXT_STEPS_SUMMARY.md       # 🎯 Future roadmap
│   ├── BUG_REPORT.md               # 🐛 Bug analysis and fixes
│   ├── DEPLOYMENT.md                # 🚀 Deployment guide
│   ├── DATABASE_SETUP.md            # 🗄️ Database setup
│   ├── CHANGELOG.md                 # 📝 Change log
│   ├── privacy-policy.pdf           # 📄 Privacy policy
│   └── terms-of-use.pdf             # 📄 Terms of use
├── 241RunnersAwarenessAPI/         # 🔧 .NET Core Backend
├── admin/                          # 👥 Admin Dashboard
├── assets/                         # 🎨 Shared Assets
├── js/                             # 📜 Frontend JavaScript
├── partials/                       # 🧩 HTML Partials
├── setup-azure-admin-users.js      # ⚙️ Admin Setup Script
├── DEPLOYMENT.md                   # 🚀 Deployment Guide
├── README.md                       # 📖 Main Project README
└── *.html                          # 🌐 Website Pages
```

---

## 🚀 **Getting Started**

### **For Developers**
1. **Setup**: Follow [DEPLOYMENT.md](./DEPLOYMENT.md) for local development
2. **Backend**: Use `dotnet run` in `241RunnersAwarenessAPI/` folder
3. **Frontend**: Open HTML files in browser or use local server
4. **Admin**: Run `node setup-azure-admin-users.js` after backend deployment

### **For Administrators**
1. **Access**: Login at `https://241runnersawareness.org/admin/`
2. **Credentials**: Use admin accounts from [AZURE_DEPLOYMENT_STATUS.md](./AZURE_DEPLOYMENT_STATUS.md)
3. **Monitoring**: Check health endpoint for system status

### **For Users**
1. **Main Site**: Visit `https://241runnersawareness.org`
2. **Report Cases**: Use the report case form
3. **Search Cases**: Browse active missing persons cases

---

## 📊 **Documentation Status**

| Document | Status | Last Updated | Next Review |
|----------|--------|--------------|-------------|
| **AZURE_DEPLOYMENT_STATUS.md** | ✅ Complete | Sep 2, 2025 | Sep 9, 2025 |
| **CLEANUP_SUMMARY.md** | ✅ Complete | Sep 2, 2025 | N/A |
| **NEXT_STEPS_SUMMARY.md** | ✅ Complete | Sep 2, 2025 | Sep 9, 2025 |
| **BUG_REPORT.md** | ✅ Complete | Sep 2, 2025 | Sep 9, 2025 |
| **README.md** | ✅ Complete | Sep 2, 2025 | Ongoing |

---

## 🔄 **Maintenance Schedule**

### **Daily**
- Monitor API health endpoint
- Check error logs
- Verify admin dashboard functionality

### **Weekly**
- Review performance metrics
- Update documentation as needed
- Test critical user flows

### **Monthly**
- Security review and updates
- Performance optimization
- Documentation review and updates

---

## 📞 **Support & Contact**

### **Technical Issues**
- **Backend**: Check Azure App Service logs
- **Frontend**: Check browser console and network tab
- **Database**: Verify Azure SQL connection

### **Documentation Updates**
- **Process**: Update relevant markdown files
- **Review**: Update this README index
- **Commit**: Include documentation updates in feature commits

---

## 📝 **Documentation Standards**

### **File Naming**
- Use descriptive names with underscores
- Include date in filename if time-sensitive
- Use consistent capitalization

### **Content Structure**
- Include clear headers and sections
- Use emojis for visual organization
- Include status indicators (✅, ⚠️, ❌)
- Provide actionable next steps

### **Maintenance**
- Update documents when features change
- Review and refresh quarterly
- Archive outdated documents
- Keep this README current

---

**Last Updated**: September 2, 2025  
**Maintained By**: Development Team  
**Version**: 1.0.0 