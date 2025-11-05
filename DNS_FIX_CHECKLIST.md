# DNS Configuration Fix Checklist

## Current Issues Found:
1. ❌ **CRITICAL:** Extra A record (`20.40.202.22`) - **MUST BE DELETED** (confirmed via DNS lookup)
2. ⚠️ www CNAME exists but may have conflicting A records
3. ⚠️ SSL certificates may not be provisioned yet after DNS changes

## Required DNS Records (GoDaddy):

### A Records (for apex domain - 4 records):
- ✅ `@` → `185.199.108.153` (TTL: 600)
- ✅ `@` → `185.199.109.153` (TTL: 600)
- ✅ `@` → `185.199.110.153` (TTL: 600)
- ✅ `@` → `185.199.111.153` (TTL: 600)
- ❌ `@` → `20.40.202.22` **DELETE THIS ONE**

### CNAME Record (for www subdomain):
- ❌ `www` → `DekuWorks.github.io` **ADD THIS**

## GitHub Pages Settings Verification:

1. Go to: https://github.com/DekuWorks/241RunnersAwareness/settings/pages
2. Verify:
   - Primary domain: `www.241runnersawareness.org`
   - Additional domain: `241runnersawareness.org` (should be added)
   - "Enforce HTTPS" is checked ✅
   - Both domains show as "Verified" (green checkmark)

## After DNS Changes:

1. **Wait for DNS propagation** (5 minutes to 1 hour)
   - Check with: `dig 241runnersawareness.org` or `nslookup 241runnersawareness.org`
   - Check with: `dig www.241runnersawareness.org` or `nslookup www.241runnersawareness.org`

2. **Verify DNS records are correct:**
   ```bash
   # Should show 4 A records (no 20.40.202.22)
   dig 241runnersawareness.org +short
   
   # Should show CNAME pointing to DekuWorks.github.io
   dig www.241runnersawareness.org +short
   ```

3. **Wait for GitHub to provision SSL** (up to 24 hours, usually under 1 hour)
   - Check SSL status: https://www.sslshopper.com/ssl-checker.html#hostname=241runnersawareness.org
   - Check SSL status: https://www.sslshopper.com/ssl-checker.html#hostname=www.241runnersawareness.org

4. **Test both domains:**
   - https://241runnersawareness.org
   - https://www.241runnersawareness.org

## Common "Connection is Private" Causes:

1. **DNS not propagated yet** - Wait longer
2. **Extra A record causing conflicts** - Must be deleted
3. **Missing CNAME for www** - Must be added
4. **SSL certificate not provisioned yet** - Wait up to 24 hours
5. **Domain not verified in GitHub Pages** - Check Settings → Pages
6. **HTTPS not enforced** - Check "Enforce HTTPS" checkbox

## Troubleshooting Commands:

```bash
# Check DNS propagation
dig 241runnersawareness.org
dig www.241runnersawareness.org

# Check SSL certificate
openssl s_client -connect 241runnersawareness.org:443 -servername 241runnersawareness.org
openssl s_client -connect www.241runnersawareness.org:443 -servername www.241runnersawareness.org
```

---

**Last Updated:** January 2025
**Repository:** DekuWorks/241RunnersAwareness
**GitHub Pages URL:** https://DekuWorks.github.io/241RunnersAwareness

