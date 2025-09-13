# 241 Runners Awareness - Operations Runbook

## 🚨 Emergency Procedures

### Health Check Commands
```bash
# Basic health checks
curl -sS -I https://241runners-api.azurewebsites.net/healthz
curl -sS https://241runners-api.azurewebsites.net/readyz | jq .

# API health
curl -sS https://241runners-api.azurewebsites.net/api/health | jq .
```

### Authentication Testing
```bash
# Login test (replace credentials)
curl -sS -X POST https://241runners-api.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@241runnersawareness.org","password":"<REDACTED>"}' | jq .

# Admin stats (requires valid token)
curl -sS -H "Authorization: Bearer <ACCESS_TOKEN>" \
  https://241runners-api.azurewebsites.net/api/admin/stats | jq .
```

## 🔄 Deployment Procedures

### Staging Slot Deployment
1. **Deploy to Staging Slot**:
   ```bash
   # GitHub Actions will deploy to staging slot
   # Manual verification:
   curl -sS https://241runners-api-staging.azurewebsites.net/healthz
   curl -sS https://241runners-api-staging.azurewebsites.net/readyz | jq .
   ```

2. **Smoke Test on Staging**:
   - Test admin login flow
   - Verify database connectivity
   - Check all critical endpoints

3. **Swap to Production**:
   - Azure Portal → App Service → Swap (Staging → Production)
   - Validate `/healthz` and key endpoints post-swap
   - Monitor for 10 minutes

4. **Rollback Procedure**:
   - If health fails for >10 minutes: Use Swap back
   - Keep last successful package for quick redeploy

## 📊 Monitoring & Alerts

### Application Insights Queries

#### Request Analysis
```kql
requests
| where timestamp > ago(24h)
| summarize count() by resultCode
| order by resultCode
```

#### Database Performance
```kql
dependencies
| where timestamp > ago(24h)
| where target contains "sql"
| summarize avg(duration) by target
```

#### Exception Tracking
```kql
exceptions
| where timestamp > ago(24h)
| summarize count() by type
| order by count_ desc
```

### Alert Rules
1. **Health Check Failures**: `/healthz` returns non-200 status
2. **Database Latency**: SQL queries > 500ms for 5 minutes
3. **Exception Rate**: Exception count > baseline

## 🔧 Common Issues & Solutions

### Database Connection Issues
**Symptoms**: `/readyz` returns 503, "db": "disconnected"
**Solutions**:
1. Check Azure SQL firewall rules
2. Verify connection string in App Service Configuration
3. Check SQL Server status in Azure Portal

### Authentication Failures
**Symptoms**: 401/403 errors, JWT validation failures
**Solutions**:
1. Verify JWT_ISSUER, JWT_AUDIENCE, JWT_KEY in App Service Configuration
2. Check token expiry and refresh logic
3. Validate CORS configuration

### Migration Failures
**Symptoms**: App startup errors, "EF migrations applied" not in logs
**Solutions**:
1. Check database permissions
2. Verify connection string
3. Review migration files for conflicts

## 🚀 Performance Optimization

### Database Optimization
- Monitor slow queries in Application Insights
- Use connection pooling
- Implement proper indexing

### API Optimization
- Monitor response times
- Implement caching where appropriate
- Use async/await patterns

## 🔒 Security Checklist

### Regular Security Tasks
- [ ] Rotate JWT keys quarterly
- [ ] Review and update CORS origins
- [ ] Monitor for suspicious login attempts
- [ ] Keep dependencies updated
- [ ] Review access logs

### Incident Response
1. **Identify**: Check Application Insights for anomalies
2. **Contain**: Disable affected endpoints if necessary
3. **Eradicate**: Fix root cause
4. **Recover**: Deploy fix and monitor
5. **Lessons Learned**: Document and improve procedures

## 📞 Escalation Contacts

- **Primary**: Development Team
- **Secondary**: Azure Support (if infrastructure issues)
- **Emergency**: On-call rotation

## 📋 Maintenance Windows

- **Planned**: Sundays 2-4 AM UTC
- **Emergency**: As needed with stakeholder notification
- **Database**: Coordinate with Azure SQL maintenance windows

---

**Last Updated**: 2025-01-13
**Version**: 1.0
