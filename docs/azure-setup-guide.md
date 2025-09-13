# Azure Portal Setup Guide

## 🎯 **Step 1: Enable Application Insights**

### In Azure Portal:
1. **Navigate to App Service**:
   - Go to Azure Portal → App Services → `241runners-api`
   
2. **Enable Application Insights**:
   - In the left menu, click **"Application Insights"**
   - Click **"Turn On Application Insights"**
   - Select **"Autodetect"** for the Application Insights resource
   - Click **"Apply"** and wait for the configuration to complete

3. **Verify Configuration**:
   - Check that the Application Insights resource is created and linked
   - Note the **Instrumentation Key** and **Connection String**

### Expected Result:
- Application Insights resource created (e.g., `241runners-api-ai`)
- Telemetry collection enabled for requests, dependencies, and exceptions
- Live metrics available in Azure Portal

---

## 🎯 **Step 2: Create Staging Slot**

### In Azure Portal:
1. **Navigate to Deployment Slots**:
   - Go to App Service → `241runners-api` → **"Deployment slots"**
   - Click **"Add slot"**

2. **Configure Staging Slot**:
   - **Name**: `staging`
   - **Configuration source**: Clone settings from production
   - Click **"Add"**

3. **Configure Staging Environment Variables**:
   - Go to the new `staging` slot
   - Navigate to **"Configuration"** → **"Application settings"**
   - Update the following settings:
     ```
     ASPNETCORE_ENVIRONMENT = Staging
     DefaultConnection = [Staging SQL Connection String]
     JWT_ISSUER = 241RunnersAwareness-Staging
     JWT_AUDIENCE = 241RunnersAwareness-Staging
     JWT_KEY = [Staging JWT Key - different from production]
     ```

4. **Create Staging Database** (if needed):
   - Create a separate Azure SQL Database for staging
   - Update the `DefaultConnection` string in staging slot configuration

### Expected Result:
- Staging slot available at `https://241runners-api-staging.azurewebsites.net`
- Separate configuration from production
- Ready for safe deployment testing

---

## 🎯 **Step 3: Set Up Alerts**

### Alert 1: Health Check Failures
1. **Navigate to Alerts**:
   - Go to App Service → `241runners-api` → **"Alerts"**
   - Click **"New alert rule"**

2. **Configure Signal**:
   - **Signal type**: Custom log search
   - **Query**: 
     ```kql
     requests
     | where url contains "/healthz"
     | where resultCode >= 400
     | where timestamp > ago(5m)
     ```
   - **Measurement**: Count of results
   - **Aggregation type**: Count
   - **Aggregation granularity**: 5 minutes

3. **Set Threshold**:
   - **Threshold value**: 0
   - **Operator**: Greater than
   - **Frequency**: Every 5 minutes

4. **Configure Actions**:
   - Create action group with email and SMS notifications
   - Add team members to receive alerts

### Alert 2: Database Latency Spike
1. **Create New Alert Rule**:
   - **Signal type**: Custom log search
   - **Query**:
     ```kql
     dependencies
     | where target contains "sql"
     | where duration > 500
     | where timestamp > ago(5m)
     ```
   - **Threshold**: > 0 for 5 minutes

### Alert 3: Exception Rate
1. **Create New Alert Rule**:
   - **Signal type**: Custom log search
   - **Query**:
     ```kql
     exceptions
     | where timestamp > ago(5m)
     | summarize count() by bin(timestamp, 5m)
     ```
   - **Threshold**: > baseline (e.g., 10 exceptions in 5 minutes)

---

## 🎯 **Step 4: Remove SEED_ADMIN_PWD**

### After First Admin Login:
1. **Navigate to Configuration**:
   - Go to App Service → `241runners-api` → **"Configuration"**
   - Click **"Application settings"**

2. **Remove SEED_ADMIN_PWD**:
   - Find `SEED_ADMIN_PWD` in the list
   - Click the **"Delete"** button (trash icon)
   - Click **"Save"** to apply changes

3. **Verify Removal**:
   - Check that `SEED_ADMIN_PWD` is no longer in the configuration
   - Restart the App Service to ensure changes take effect

### Security Best Practice:
- Store admin credentials in a secure password manager
- Rotate admin passwords regularly
- Use strong, unique passwords

---

## 🎯 **Step 5: Test Staging Deployment**

### Manual Testing:
1. **Access Staging Environment**:
   - Go to `https://241runners-api-staging.azurewebsites.net/healthz`
   - Verify it returns 200 OK

2. **Test Database Connectivity**:
   - Go to `https://241runners-api-staging.azurewebsites.net/readyz`
   - Verify it shows `"db": "connected"`

3. **Test Admin Flow**:
   - Access staging admin dashboard
   - Test login with staging credentials
   - Verify all admin functions work

### Automated Testing:
- GitHub Actions will automatically deploy to staging slot
- Smoke tests will run automatically
- Monitor the Actions tab for deployment status

---

## 🎯 **Step 6: Production Swap Procedure**

### When Ready for Production:
1. **Final Staging Validation**:
   - Run comprehensive tests on staging
   - Verify all functionality works correctly
   - Check performance and response times

2. **Execute Swap**:
   - Go to App Service → `241runners-api` → **"Deployment slots"**
   - Click **"Swap"** button
   - Select **"staging"** → **"production"**
   - Click **"Swap"** to confirm

3. **Post-Swap Validation**:
   - Test production endpoints immediately
   - Monitor for 10 minutes for any issues
   - Verify all functionality works correctly

4. **Rollback (if needed)**:
   - If issues detected, swap back immediately
   - Investigate and fix issues in staging
   - Re-attempt swap when ready

---

## 📊 **Verification Checklist**

### Application Insights:
- [ ] Application Insights resource created
- [ ] Telemetry collection enabled
- [ ] Live metrics showing data
- [ ] Custom queries working

### Staging Slot:
- [ ] Staging slot created and accessible
- [ ] Separate configuration from production
- [ ] Staging database connected
- [ ] GitHub Actions deploying to staging

### Alerts:
- [ ] Health check failure alert configured
- [ ] Database latency alert configured
- [ ] Exception rate alert configured
- [ ] Action groups with notifications set up

### Security:
- [ ] SEED_ADMIN_PWD removed from production
- [ ] Admin credentials stored securely
- [ ] JWT keys rotated if needed

### Deployment:
- [ ] Staging deployment tested
- [ ] Production swap procedure documented
- [ ] Rollback procedure tested
- [ ] Monitoring and alerting active

---

## 🚨 **Emergency Contacts**

- **Primary**: Development Team
- **Azure Support**: Available through Azure Portal
- **Escalation**: On-call rotation

---

**Last Updated**: 2025-01-13
**Version**: 1.0
