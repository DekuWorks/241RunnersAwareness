# SSL Certificate Fix Guide

## 🚨 **Current Issue**
- Custom domain `https://241runnersawareness.org` shows SSL certificate error
- Error: `NET::ERR_CERT_COMMON_NAME_INVALID`
- Site is accessible via GitHub Pages default URL

## ✅ **Immediate Solutions**

### **Option 1: Use GitHub Pages Default URL (Working Now)**
```
https://dekuworks.github.io/241RunnersAwareness/
```

### **Option 2: Force SSL Certificate Renewal**

#### **Step 1: GitHub Repository Settings**
1. Go to: `https://github.com/DekuWorks/241RunnersAwareness/settings/pages`
2. In **Custom domain** section:
   - Clear the domain field
   - Click **Save**
   - Wait 30 seconds
   - Re-enter: `241runnersawareness.org`
   - Click **Save**
3. **Uncheck "Enforce HTTPS"** temporarily
4. Wait 5 minutes
5. **Re-enable "Enforce HTTPS"**

#### **Step 2: DNS Verification**
Ensure your DNS records point to GitHub Pages:
```
Type: CNAME
Name: www
Value: dekuworks.github.io

Type: A
Name: @
Value: 185.199.108.153
Value: 185.199.109.153
Value: 185.199.110.153
Value: 185.199.111.153
```

#### **Step 3: Browser Cache Clear**
1. **Chrome/Edge**: Ctrl+Shift+Delete → Clear browsing data
2. **Firefox**: Ctrl+Shift+Delete → Clear recent history
3. **Safari**: Cmd+Option+E → Empty caches

## 🔧 **Alternative Access Methods**

### **Temporary Workarounds**
1. **HTTP (temporary)**: `http://241runnersawareness.org`
2. **GitHub Pages**: `https://dekuworks.github.io/241RunnersAwareness/`
3. **Admin Dashboard**: `https://dekuworks.github.io/241RunnersAwareness/admin/`

### **Browser-Specific Fixes**

#### **Chrome/Edge**
1. Click **Advanced** on the error page
2. Click **Proceed to 241runnersawareness.org (unsafe)**
3. This is temporary until SSL renews

#### **Firefox**
1. Click **Advanced** on the error page
2. Click **Accept the Risk and Continue**

## ⏰ **Timeline**
- **Immediate**: Use GitHub Pages URL
- **1-2 hours**: DNS propagation complete
- **24-48 hours**: SSL certificate fully renewed
- **Permanent**: Custom domain with valid SSL

## 🧪 **Testing Commands**

### **Check Site Status**
```bash
# Test GitHub Pages default
curl -I https://dekuworks.github.io/241RunnersAwareness/

# Test custom domain
curl -I https://241runnersawareness.org/

# Test admin dashboard
curl -I https://dekuworks.github.io/241RunnersAwareness/admin/
```

### **Check SSL Certificate**
```bash
# Check SSL certificate details
openssl s_client -connect 241runnersawareness.org:443 -servername 241runnersawareness.org
```

## 🎯 **Success Criteria**
- ✅ Site loads without SSL errors
- ✅ Custom domain works with HTTPS
- ✅ Admin dashboard accessible
- ✅ All pages load correctly

## 🚨 **If Issues Persist**
1. Check DNS propagation: `https://dnschecker.org`
2. Verify GitHub Pages settings
3. Contact domain registrar for DNS issues
4. Use GitHub Pages default URL as backup
