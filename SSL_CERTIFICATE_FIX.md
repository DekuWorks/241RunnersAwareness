# SSL Certificate "Connection is Private" Error - Troubleshooting Guide

## Problem
Users are reporting they cannot access the static site and are seeing a "Your connection is not private" or "NET::ERR_CERT_*" error in their browsers.

## Root Cause
This error occurs when GitHub Pages hasn't properly provisioned an SSL certificate for your custom domain(s), or the certificate has expired/needs renewal. Both `www.241runnersawareness.org` and `241runnersawareness.org` (apex domain) need to be configured separately.

## Solution Steps

### Step 1: Configure Both Domains in GitHub Pages

**IMPORTANT:** Both `www.241runnersawareness.org` and `241runnersawareness.org` need to be configured separately.

1. Go to your GitHub repository: `https://github.com/[your-username]/241RunnersAwareness`
2. Navigate to **Settings** → **Pages**
3. Under **Custom domain**:
   - **Primary domain:** Set to `www.241runnersawareness.org` (this should match your CNAME file)
   - **Additional domains:** Click "Add domain" and add `241runnersawareness.org`
   - **Enforce HTTPS** checkbox should be checked ✅ for both domains
   - Both domains should show as "Verified" (green checkmark)
   
**Note:** GitHub Pages will automatically provision SSL certificates for both domains once they are verified.

### Step 2: Re-provision SSL Certificates (if needed)

If the domains are not verified or SSL is not working:

1. In GitHub Pages settings, **remove** both custom domains temporarily
2. Wait 5-10 minutes
3. **Re-add** the domains:
   - First add: `www.241runnersawareness.org` (primary)
   - Then add: `241runnersawareness.org` (additional)
4. Wait up to 24 hours for GitHub to automatically provision SSL certificates for both domains

**For apex domain specifically:** If `241runnersawareness.org` doesn't verify, ensure A records are configured in DNS (see Step 3).

### Step 3: Configure DNS Records

**CRITICAL:** Both domains require different DNS record types. Configure these at your domain registrar:

**For `www.241runnersawareness.org` (subdomain):**
- **Type:** CNAME
- **Name:** www
- **Value:** `[your-username].github.io` (replace with your GitHub username)
- **TTL:** 3600 (or auto)

**For `241runnersawareness.org` (apex domain - REQUIRED):**
- **Type:** A records (create 4 separate A records)
- **Name:** @ (or leave blank, depending on registrar)
- **Value:** Create 4 A records with these IPs:
  1. `185.199.108.153`
  2. `185.199.109.153`
  3. `185.199.110.153`
  4. `185.199.111.153`
- **TTL:** 3600 (or auto)

**Why both are needed:**
- The CNAME file in the repo handles `www.241runnersawareness.org`
- The A records in DNS allow `241runnersawareness.org` (apex) to work
- GitHub Pages will provision SSL for both once DNS is correct

### Step 4: Check SSL Certificate Status for Both Domains

After configuring both domains, verify SSL certificates:

**For www subdomain:**
- Visit: `https://www.sslshopper.com/ssl-checker.html#hostname=www.241runnersawareness.org`
- Or: `https://www.ssllabs.com/ssltest/analyze.html?d=www.241runnersawareness.org`

**For apex domain:**
- Visit: `https://www.sslshopper.com/ssl-checker.html#hostname=241runnersawareness.org`
- Or: `https://www.ssllabs.com/ssltest/analyze.html?d=241runnersawareness.org`

Both certificates should show:
- ✅ Valid (not expired)
- ✅ Issued by GitHub or Let's Encrypt
- ✅ Trusted by major browsers
- ✅ Covers both www and apex domains

### Step 5: Verify CNAME File

Ensure the `CNAME` file in the repository root contains:
```
www.241runnersawareness.org
```

**Important:** The CNAME file should:
- Be in the root directory
- Contain only the domain name (no `https://` or trailing slashes)
- Match exactly what's configured in GitHub Pages settings

### Step 6: Test Both Domains

After SSL is provisioned for both domains:
1. Visit: `https://www.241runnersawareness.org` - should load with SSL
2. Visit: `https://241runnersawareness.org` - should load with SSL (no redirect)
3. Check browser console for any mixed content warnings
4. Verify the padlock icon appears in the address bar for both URLs
5. Test that both domains work without "connection is private" errors

## Common Issues & Fixes

### Issue: Certificate still not working after 24 hours
**Fix:** 
- Verify DNS propagation with `dig www.241runnersawareness.org` or `nslookup www.241runnersawareness.org`
- Ensure CNAME points to `[username].github.io`
- Contact GitHub Support if issue persists

### Issue: Certificate works but shows "Not Secure"
**Fix:**
- Check for mixed content (HTTP resources on HTTPS page)
- Ensure all external resources use HTTPS
- Review browser console for security warnings

### Issue: Certificate works for www but not apex domain (`241runnersawareness.org`)
**Fix:**
- **REQUIRED:** Configure 4 A records for apex domain pointing to GitHub Pages IPs (see Step 3)
- Add `241runnersawareness.org` as an additional custom domain in GitHub Pages settings
- Verify DNS propagation: `dig 241runnersawareness.org` or `nslookup 241runnersawareness.org`
- Wait up to 24 hours for GitHub to provision SSL certificate for apex domain
- Do NOT use a redirect - both domains should work independently

### Issue: Apex domain shows "connection is private" but www works
**Fix:**
- This means A records are missing or incorrect
- Verify all 4 A records are configured at your DNS provider
- Check that A records point to GitHub Pages IPs (185.199.108.153, 185.199.109.153, 185.199.110.153, 185.199.111.153)
- Ensure apex domain is added in GitHub Pages settings as an additional domain

## Additional Notes

### .htaccess File
The `.htaccess` file in this repository **will not work** on GitHub Pages, as GitHub Pages doesn't support Apache configuration files. The HTTPS redirect and security headers are handled automatically by GitHub Pages when SSL is properly configured.

### Mixed Content
All resources should use HTTPS. Check for:
- `http://` URLs in HTML files
- External scripts loaded over HTTP
- Images or fonts loaded over HTTP

## Verification Checklist

**For www subdomain:**
- [ ] `www.241runnersawareness.org` configured in GitHub Pages settings (primary)
- [ ] "Enforce HTTPS" checkbox is checked
- [ ] Domain shows as "Verified" in GitHub
- [ ] CNAME file exists in repository root with `www.241runnersawareness.org`
- [ ] CNAME DNS record configured at registrar pointing to `[username].github.io`
- [ ] SSL certificate is valid for www subdomain

**For apex domain:**
- [ ] `241runnersawareness.org` added as additional domain in GitHub Pages settings
- [ ] "Enforce HTTPS" checkbox is checked for apex domain
- [ ] Apex domain shows as "Verified" in GitHub
- [ ] All 4 A records configured at DNS registrar with GitHub Pages IPs
- [ ] SSL certificate is valid for apex domain

**General:**
- [ ] Both `https://www.241runnersawareness.org` and `https://241runnersawareness.org` load without errors
- [ ] Browser shows padlock icon for both domains
- [ ] No "connection is private" errors for either domain
- [ ] No mixed content warnings in browser console

## Timeline

- **DNS Propagation:** 5 minutes to 48 hours (usually under 1 hour)
- **SSL Certificate Provision:** 5 minutes to 24 hours (usually under 1 hour)
- **Full Resolution:** Up to 48 hours in worst case

## If Issues Persist

1. **Contact GitHub Support:** https://support.github.com
2. **Check GitHub Status:** https://www.githubstatus.com
3. **Verify with SSL Labs:** https://www.ssllabs.com/ssltest/

## Related Files

- `CNAME` - Custom domain configuration
- `.htaccess` - Apache config (not used by GitHub Pages)
- `.github/workflows/pages.yml` - GitHub Pages deployment workflow

---

**Last Updated:** January 2025
**Primary Domain:** www.241runnersawareness.org
**Apex Domain:** 241runnersawareness.org
**Hosting:** GitHub Pages

## Quick Reference: DNS Records Needed

```
# For www subdomain (CNAME record)
www.241runnersawareness.org  CNAME  [username].github.io

# For apex domain (4 A records required)
241runnersawareness.org  A  185.199.108.153
241runnersawareness.org  A  185.199.109.153
241runnersawareness.org  A  185.199.110.153
241runnersawareness.org  A  185.199.111.153
```

**Note:** Replace `[username]` with your actual GitHub username in the CNAME record.

