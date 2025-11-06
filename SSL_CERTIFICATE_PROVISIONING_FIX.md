# SSL Certificate Provisioning Fix for Apex Domain

## Problem Identified

The apex domain `241runnersawareness.org` is using GitHub's wildcard certificate (`*.github.io`) instead of a dedicated certificate. This causes Chrome and Edge to show "connection isn't private" errors because the certificate doesn't match the domain name.

**Current Status:**
- ❌ `241runnersawareness.org` → Certificate: `CN=*.github.io` (WRONG)
- ✅ `www.241runnersawareness.org` → Certificate: `CN=www.241runnersawareness.org` (CORRECT)

## Root Cause

GitHub Pages hasn't provisioned a dedicated SSL certificate for the apex domain. This happens when:
1. The domain isn't fully verified in GitHub Pages settings
2. DNS records aren't correctly configured (we fixed this)
3. GitHub's certificate provisioning hasn't completed yet
4. The domain needs to be re-verified to trigger certificate provisioning

## Permanent Solution Steps

### Step 1: Verify DNS is Correct (CRITICAL)

**Check that you have EXACTLY 4 A records (no more, no less):**
```bash
dig 241runnersawareness.org +short
```

Should show ONLY these 4 IPs:
- 185.199.108.153
- 185.199.109.153
- 185.199.110.153
- 185.199.111.153

**If you see `20.40.202.22` or any other IP, DELETE IT immediately.**

### Step 2: Force GitHub to Re-Provision Certificate

1. **Go to GitHub Pages Settings:**
   - Navigate to: https://github.com/DekuWorks/241RunnersAwareness/settings/pages

2. **Remove the apex domain:**
   - Find `241runnersawareness.org` in the custom domains list
   - Click "Remove" next to it
   - Wait 5 minutes

3. **Re-add the apex domain:**
   - Click "Add domain"
   - Enter: `241runnersawareness.org`
   - Click "Save"
   - Wait for verification (green checkmark)

4. **Verify "Enforce HTTPS" is checked:**
   - Ensure the checkbox is checked for BOTH domains
   - If it's grayed out, wait for verification to complete first

### Step 3: Wait for Certificate Provisioning

GitHub will automatically provision a dedicated SSL certificate for the apex domain. This can take:
- **Minimum:** 5 minutes
- **Typical:** 15-60 minutes
- **Maximum:** 24 hours

**How to check if certificate is provisioned:**
```bash
echo | openssl s_client -connect 241runnersawareness.org:443 -servername 241runnersawareness.org 2>&1 | openssl x509 -noout -subject
```

**When correct, it should show:**
```
subject=CN = 241runnersawareness.org
```

**NOT:**
```
subject=CN = *.github.io
```

### Step 4: Verify Certificate in Browser

1. Open Chrome or Edge
2. Navigate to: `https://241runnersawareness.org`
3. Click the padlock icon (or warning icon) in the address bar
4. Click "Certificate" or "Connection is secure"
5. Verify:
   - **Issued to:** `241runnersawareness.org` (NOT `*.github.io`)
   - **Issued by:** Let's Encrypt or Sectigo
   - **Valid from:** Current date
   - **Valid to:** Future date (usually 90 days)

## Alternative: Use www as Primary Domain

If GitHub continues to have issues provisioning a certificate for the apex domain, you can:

1. **Set www as the primary domain** (already done)
2. **Use DNS redirect** at your registrar to redirect apex to www
3. **Or use a meta redirect** in your HTML (less ideal)

However, the proper solution is to get GitHub to provision the certificate for the apex domain.

## Troubleshooting

### Issue: Domain shows as "Verified" but certificate still wrong

**Solution:**
1. Remove domain from GitHub Pages
2. Wait 10 minutes
3. Verify DNS is correct (only 4 A records)
4. Re-add domain
5. Wait up to 24 hours for certificate provisioning

### Issue: Certificate still shows `*.github.io` after 24 hours

**Possible causes:**
1. DNS still has extra A records (check with `dig`)
2. Domain not properly verified in GitHub
3. GitHub Pages certificate provisioning is delayed

**Solution:**
1. Double-check DNS records
2. Contact GitHub Support: https://support.github.com
3. Consider using www subdomain as primary (it works correctly)

### Issue: Works in Safari but not Chrome/Edge

**This is the exact problem we're fixing!**

Safari is more lenient with SSL certificates, while Chrome and Edge strictly require the certificate to match the domain name. Once GitHub provisions the correct certificate, all browsers will work.

## Verification Commands

```bash
# Check DNS (should show only 4 IPs)
dig 241runnersawareness.org +short

# Check certificate subject (should show 241runnersawareness.org)
echo | openssl s_client -connect 241runnersawareness.org:443 -servername 241runnersawareness.org 2>&1 | openssl x509 -noout -subject

# Check certificate validity
echo | openssl s_client -connect 241runnersawareness.org:443 -servername 241runnersawareness.org 2>&1 | openssl x509 -noout -dates

# Test SSL connection
curl -vI https://241runnersawareness.org 2>&1 | grep -i "certificate\|ssl"
```

## Expected Timeline

1. **DNS Propagation:** 5 minutes - 1 hour (usually immediate if already configured)
2. **Domain Verification:** 5-15 minutes after re-adding
3. **Certificate Provisioning:** 15 minutes - 24 hours (usually 30-60 minutes)
4. **Full Resolution:** Once certificate is provisioned, works immediately in all browsers

## Why This Happens

GitHub Pages uses Let's Encrypt to automatically provision SSL certificates for custom domains. The provisioning process:
1. Verifies domain ownership via DNS
2. Requests certificate from Let's Encrypt
3. Installs certificate on GitHub's servers
4. Updates DNS to use the new certificate

If any step fails or is delayed, the site falls back to the default `*.github.io` certificate, which doesn't match custom domains and causes browser warnings.

---

**Last Updated:** January 2025
**Repository:** DekuWorks/241RunnersAwareness
**Issue:** Apex domain using wrong SSL certificate

