/**
 * 241 Runners Awareness - Admin Dashboard Validation & Security
 * Enhanced input validation, data sanitization, and security measures
 */

class AdminDashboardSecurity {
    constructor() {
        this.init();
    }

    init() {
        this.setupFormValidation();
        this.setupInputSanitization();
        this.setupSecurityHeaders();
        this.setupCSRFProtection();
        this.setupRateLimiting();
    }

    // Enhanced form validation
    setupFormValidation() {
        // Password reset form validation
        const passwordResetForm = document.getElementById('passwordResetForm');
        if (passwordResetForm) {
            passwordResetForm.addEventListener('submit', (e) => this.validatePasswordResetForm(e));
        }

        // Add admin form validation
        const addAdminForm = document.getElementById('addAdminForm');
        if (addAdminForm) {
            addAdminForm.addEventListener('submit', (e) => this.validateAddAdminForm(e));
        }

        // Edit user form validation
        const editUserForm = document.getElementById('editUserForm');
        if (editUserForm) {
            editUserForm.addEventListener('submit', (e) => this.validateEditUserForm(e));
        }

        // Edit runner form validation
        const editRunnerForm = document.getElementById('editRunnerForm');
        if (editRunnerForm) {
            editRunnerForm.addEventListener('submit', (e) => this.validateEditRunnerForm(e));
        }

        // Real-time validation for all inputs
        this.setupRealTimeValidation();
    }

    // Validate password reset form
    validatePasswordResetForm(e) {
        const currentPassword = document.getElementById('currentPassword').value;
        const newPassword = document.getElementById('newPassword').value;
        const confirmPassword = document.getElementById('confirmNewPassword').value;

        // Enhanced validation
        if (!this.validatePasswordStrength(newPassword)) {
            e.preventDefault();
            showToast('Password does not meet security requirements', 'error');
            return false;
        }

        if (newPassword !== confirmPassword) {
            e.preventDefault();
            showToast('New passwords do not match', 'error');
            return false;
        }

        if (currentPassword === newPassword) {
            e.preventDefault();
            showToast('New password must be different from current password', 'error');
            return false;
        }

        return true;
    }

    // Enhanced password strength validation
    validatePasswordStrength(password) {
        const minLength = 8;
        const maxLength = 128;
        
        if (password.length < minLength || password.length > maxLength) {
            return false;
        }

        // Check for common weak patterns
        const weakPatterns = [
            /^1234567890*$/,
            /^qwertyuiop*$/,
            /^password*$/,
            /^admin*$/,
            /^letmein*$/,
            /^welcome*$/,
            /^monkey*$/,
            /^dragon*$/,
            /^master*$/,
            /^football*$/
        ];

        if (weakPatterns.some(pattern => pattern.test(password.toLowerCase()))) {
            return false;
        }

        // Check for sequential characters
        if (this.hasSequentialChars(password)) {
            return false;
        }

        // Check for repeated characters
        if (this.hasRepeatedChars(password)) {
            return false;
        }

        return true;
    }

    // Check for sequential characters
    hasSequentialChars(password) {
        const sequences = ['abcdefghijklmnopqrstuvwxyz', '1234567890', 'qwertyuiopasdfghjklzxcvbnm'];
        return sequences.some(seq => {
            for (let i = 0; i <= seq.length - 3; i++) {
                const sequence = seq.substring(i, i + 3);
                if (password.toLowerCase().includes(sequence)) {
                    return true;
                }
            }
            return false;
        });
    }

    // Check for repeated characters
    hasRepeatedChars(password) {
        for (let i = 0; i < password.length - 2; i++) {
            if (password[i] === password[i + 1] && password[i] === password[i + 2]) {
                return true;
            }
        }
        return false;
    }

    // Validate add admin form
    validateAddAdminForm(e) {
        const firstName = document.getElementById('newAdminFirstName').value;
        const lastName = document.getElementById('newAdminLastName').value;
        const email = document.getElementById('newAdminEmail').value;
        const password = document.getElementById('newAdminPassword').value;
        const confirmPassword = document.getElementById('newAdminConfirmPassword').value;

        // Name validation
        if (!this.validateName(firstName) || !this.validateName(lastName)) {
            e.preventDefault();
            showToast('Names can only contain letters, spaces, hyphens, and apostrophes', 'error');
            return false;
        }

        // Email validation
        if (!this.validateEmail(email)) {
            e.preventDefault();
            showToast('Please enter a valid email address', 'error');
            return false;
        }

        // Password validation
        if (!this.validatePasswordStrength(password)) {
            e.preventDefault();
            showToast('Password does not meet security requirements', 'error');
            return false;
        }

        if (password !== confirmPassword) {
            e.preventDefault();
            showToast('Passwords do not match', 'error');
            return false;
        }

        return true;
    }

    // Validate edit user form
    validateEditUserForm(e) {
        const firstName = document.getElementById('editUserFirstName').value;
        const lastName = document.getElementById('editUserLastName').value;
        const email = document.getElementById('editUserEmail').value;
        const role = document.getElementById('editUserRole').value;

        // Name validation
        if (!this.validateName(firstName) || !this.validateName(lastName)) {
            e.preventDefault();
            showToast('Names can only contain letters, spaces, hyphens, and apostrophes', 'error');
            return false;
        }

        // Email validation
        if (!this.validateEmail(email)) {
            e.preventDefault();
            showToast('Please enter a valid email address', 'error');
            return false;
        }

        // Role validation
        if (!this.validateRole(role)) {
            e.preventDefault();
            showToast('Please select a valid role', 'error');
            return false;
        }

        return true;
    }

    // Validate edit runner form
    validateEditRunnerForm(e) {
        const firstName = document.getElementById('editRunnerFirstName').value;
        const lastName = document.getElementById('editRunnerLastName').value;
        const age = document.getElementById('editRunnerAge').value;
        const city = document.getElementById('editRunnerCity').value;
        const state = document.getElementById('editRunnerState').value;
        const status = document.getElementById('editRunnerStatus').value;

        // Name validation
        if (!this.validateName(firstName) || !this.validateName(lastName)) {
            e.preventDefault();
            showToast('Names can only contain letters, spaces, hyphens, and apostrophes', 'error');
            return false;
        }

        // Age validation
        if (!this.validateAge(age)) {
            e.preventDefault();
            showToast('Age must be between 0 and 120', 'error');
            return false;
        }

        // City validation
        if (!this.validateCity(city)) {
            e.preventDefault();
            showToast('City can only contain letters, spaces, hyphens, and apostrophes', 'error');
            return false;
        }

        // State validation
        if (!this.validateState(state)) {
            e.preventDefault();
            showToast('State can only contain letters, spaces, hyphens, and apostrophes', 'error');
            return false;
        }

        // Status validation
        if (!this.validateStatus(status)) {
            e.preventDefault();
            showToast('Please select a valid status', 'error');
            return false;
        }

        return true;
    }

    // Validation helper functions
    validateName(name) {
        const nameRegex = /^[a-zA-Z\s\-']+$/;
        return nameRegex.test(name) && name.trim().length >= 2 && name.trim().length <= 50;
    }

    validateEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email) && email.length <= 254;
    }

    validateAge(age) {
        const ageNum = parseInt(age);
        return !isNaN(ageNum) && ageNum >= 0 && ageNum <= 120;
    }

    validateCity(city) {
        const cityRegex = /^[a-zA-Z\s\-']+$/;
        return cityRegex.test(city) && city.trim().length >= 2 && city.trim().length <= 100;
    }

    validateState(state) {
        const stateRegex = /^[a-zA-Z\s\-']+$/;
        return stateRegex.test(state) && state.trim().length >= 2 && state.trim().length <= 50;
    }

    validateRole(role) {
        const validRoles = ['admin', 'user', 'parent', 'runner'];
        return validRoles.includes(role);
    }

    validateStatus(status) {
        const validStatuses = ['active', 'inactive', 'pending', 'suspended'];
        return validStatuses.includes(status);
    }

    // Real-time validation
    setupRealTimeValidation() {
        const inputs = document.querySelectorAll('input, select, textarea');
        
        inputs.forEach(input => {
            input.addEventListener('blur', (e) => this.validateField(e.target));
            input.addEventListener('input', (e) => this.clearFieldError(e.target));
        });
    }

    // Validate individual field
    validateField(field) {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';

        // Required field validation
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'This field is required';
        }

        // Type-specific validation
        if (isValid && value) {
            switch (field.type) {
                case 'email':
                    if (!this.validateEmail(value)) {
                        isValid = false;
                        errorMessage = 'Please enter a valid email address';
                    }
                    break;
                case 'number':
                    if (field.id === 'editRunnerAge') {
                        if (!this.validateAge(value)) {
                            isValid = false;
                            errorMessage = 'Age must be between 0 and 120';
                        }
                    }
                    break;
                case 'text':
                    if (field.id.includes('Name')) {
                        if (!this.validateName(value)) {
                            isValid = false;
                            errorMessage = 'Name can only contain letters, spaces, hyphens, and apostrophes';
                        }
                    } else if (field.id.includes('City')) {
                        if (!this.validateCity(value)) {
                            isValid = false;
                            errorMessage = 'City can only contain letters, spaces, hyphens, and apostrophes';
                        }
                    } else if (field.id.includes('State')) {
                        if (!this.validateState(value)) {
                            isValid = false;
                            errorMessage = 'State can only contain letters, spaces, hyphens, and apostrophes';
                        }
                    }
                    break;
            }
        }

        // Apply validation result
        if (!isValid) {
            this.showFieldError(field, errorMessage);
        } else {
            this.clearFieldError(field);
        }

        return isValid;
    }

    // Show field error
    showFieldError(field, message) {
        this.clearFieldError(field);
        
        field.style.borderColor = '#dc2626';
        field.style.boxShadow = '0 0 0 3px rgba(220, 38, 38, 0.1)';
        
        const errorDiv = document.createElement('div');
        errorDiv.className = 'field-error';
        errorDiv.textContent = message;
        errorDiv.style.color = '#dc2626';
        errorDiv.style.fontSize = '0.8rem';
        errorDiv.style.marginTop = '4px';
        
        field.parentNode.appendChild(errorDiv);
        field.setAttribute('data-error', 'true');
    }

    // Clear field error
    clearFieldError(field) {
        field.style.borderColor = '#e5e7eb';
        field.style.boxShadow = 'none';
        
        const errorDiv = field.parentNode.querySelector('.field-error');
        if (errorDiv) {
            errorDiv.remove();
        }
        
        field.removeAttribute('data-error');
    }

    // Input sanitization
    setupInputSanitization() {
        const inputs = document.querySelectorAll('input, textarea');
        
        inputs.forEach(input => {
            input.addEventListener('input', (e) => {
                e.target.value = this.sanitizeInput(e.target.value);
            });
        });
    }

    // Enhanced input sanitization
    sanitizeInput(input) {
        if (typeof input !== 'string') return input;
        
        return input
            // Remove script tags and content
            .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '')
            // Remove event handlers
            .replace(/\s*on\w+\s*=\s*["'][^"']*["']/gi, '')
            // Remove javascript: protocol
            .replace(/javascript:/gi, '')
            // Remove data: protocol (potential XSS)
            .replace(/data:/gi, '')
            // Remove vbscript: protocol
            .replace(/vbscript:/gi, '')
            // Remove expression() function
            .replace(/expression\s*\(/gi, '')
            // Remove eval() function
            .replace(/eval\s*\(/gi, '')
            // Remove SQL injection patterns
            .replace(/['";]/g, '')
            // Remove HTML entities
            .replace(/&[a-zA-Z0-9#]+;/g, '')
            // Remove HTML tags
            .replace(/<[^>]*>/g, '')
            // Trim whitespace
            .trim();
    }

    // Security headers setup
    setupSecurityHeaders() {
        // Add security-related meta tags
        this.addSecurityMetaTags();
        
        // Add Content Security Policy
        this.addCSP();
    }

    // Add security meta tags
    addSecurityMetaTags() {
        const head = document.head;
        
        // Prevent MIME type sniffing
        if (!head.querySelector('meta[http-equiv="X-Content-Type-Options"]')) {
            const meta = document.createElement('meta');
            meta.setAttribute('http-equiv', 'X-Content-Type-Options');
            meta.setAttribute('content', 'nosniff');
            head.appendChild(meta);
        }
        
        // Prevent clickjacking
        if (!head.querySelector('meta[http-equiv="X-Frame-Options"]')) {
            const meta = document.createElement('meta');
            meta.setAttribute('http-equiv', 'X-Frame-Options');
            meta.setAttribute('content', 'DENY');
            head.appendChild(meta);
        }
        
        // XSS protection
        if (!head.querySelector('meta[http-equiv="X-XSS-Protection"]')) {
            const meta = document.createElement('meta');
            meta.setAttribute('http-equiv', 'X-XSS-Protection');
            meta.setAttribute('content', '1; mode=block');
            head.appendChild(meta);
        }
    }

    // Add Content Security Policy
    addCSP() {
        const head = document.head;
        
        if (!head.querySelector('meta[http-equiv="Content-Security-Policy"]')) {
            const meta = document.createElement('meta');
            meta.setAttribute('http-equiv', 'Content-Security-Policy');
            meta.setAttribute('content', "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https://241runners-api.azurewebsites.net;");
            head.appendChild(meta);
        }
    }

    // CSRF protection
    setupCSRFProtection() {
        // Generate CSRF token
        const csrfToken = this.generateCSRFToken();
        
        // Add token to all forms
        this.addCSRFTokenToForms(csrfToken);
        
        // Validate token on form submission
        this.validateCSRFToken();
    }

    // Generate CSRF token
    generateCSRFToken() {
        const token = Math.random().toString(36).substring(2) + Date.now().toString(36);
        localStorage.setItem('csrf_token', token);
        return token;
    }

    // Add CSRF token to forms
    addCSRFTokenToForms(token) {
        const forms = document.querySelectorAll('form');
        
        forms.forEach(form => {
            if (!form.querySelector('input[name="csrf_token"]')) {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = 'csrf_token';
                input.value = token;
                form.appendChild(input);
            }
        });
    }

    // Validate CSRF token
    validateCSRFToken() {
        const forms = document.querySelectorAll('form');
        
        forms.forEach(form => {
            form.addEventListener('submit', (e) => {
                const token = form.querySelector('input[name="csrf_token"]')?.value;
                const storedToken = localStorage.getItem('csrf_token');
                
                if (!token || token !== storedToken) {
                    e.preventDefault();
                    showToast('Security validation failed. Please refresh the page and try again.', 'error');
                    return false;
                }
            });
        });
    }

    // Rate limiting
    setupRateLimiting() {
        this.rateLimitMap = new Map();
        
        // Rate limit form submissions
        this.rateLimitFormSubmissions();
        
        // Rate limit API calls
        this.rateLimitAPICalls();
    }

    // Rate limit form submissions
    rateLimitFormSubmissions() {
        const forms = document.querySelectorAll('form');
        
        forms.forEach(form => {
            form.addEventListener('submit', (e) => {
                if (this.isRateLimited('form_submit', 5, 60000)) { // 5 submissions per minute
                    e.preventDefault();
                    showToast('Too many form submissions. Please wait before trying again.', 'warning');
                    return false;
                }
            });
        });
    }

    // Rate limit API calls
    rateLimitAPICalls() {
        // Override fetch to add rate limiting
        const originalFetch = window.fetch;
        
        window.fetch = async (...args) => {
            if (this.isRateLimited('api_call', 30, 60000)) { // 30 API calls per minute
                throw new Error('Rate limit exceeded. Please wait before making more requests.');
            }
            
            return originalFetch(...args);
        };
    }

    // Check if action is rate limited
    isRateLimited(action, maxAttempts, timeWindow) {
        const now = Date.now();
        const key = `${action}_${Math.floor(now / timeWindow)}`;
        
        const attempts = this.rateLimitMap.get(key) || 0;
        
        if (attempts >= maxAttempts) {
            return true;
        }
        
        this.rateLimitMap.set(key, attempts + 1);
        
        // Clean up old entries
        setTimeout(() => {
            this.rateLimitMap.delete(key);
        }, timeWindow);
        
        return false;
    }

    // Security audit
    performSecurityAudit() {
        
        const issues = [];
        
        // Check for sensitive data in localStorage
        const sensitiveKeys = ['password', 'token', 'secret', 'key'];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (sensitiveKeys.some(sensitive => key.toLowerCase().includes(sensitive))) {
                issues.push(`Sensitive data found in localStorage: ${key}`);
            }
        }
        
        // Check for console.log statements in production
        if (window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') {
            issues.push('Console logging should be disabled in production');
        }
        
        // Check for missing security headers
        if (!document.querySelector('meta[http-equiv="X-Content-Type-Options"]')) {
            issues.push('Missing X-Content-Type-Options header');
        }
        
        // Report issues
        if (issues.length > 0) {
            console.warn('⚠️ Security issues found:');
            issues.forEach(issue => console.warn(`  - ${issue}`));
        } else {
            console.log('✅ No security issues found');
        }
        
        return issues;
    }
}

// Initialize security when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.adminSecurity = new AdminDashboardSecurity();
    
    // Perform security audit
    setTimeout(() => {
        window.adminSecurity.performSecurityAudit();
    }, 2000);
});

// Export for manual use
window.AdminDashboardSecurity = AdminDashboardSecurity; 