/**
 * ============================================
 * 241 RUNNERS AWARENESS - ADMIN AUTHENTICATION
 * ============================================
 * 
 * Admin-specific authentication utilities with role-based access control
 */

// Token storage keys
const tokenKey = "ra_admin_token";
const roleKey = "ra_admin_role";
const refreshKey = "ra_admin_refresh";

// API Configuration
let API_BASE_URL = 'https://241runners-api.azurewebsites.net/api';

// Load API configuration from config.json
async function loadConfig() {
    try {
        const response = await fetch('/config.json');
        const config = await response.json();
        API_BASE_URL = config.API_BASE_URL;
    } catch (error) {
        console.warn('Failed to load config.json, using default API URL');
    }
}

// Initialize config on load
loadConfig();

/**
 * Save authentication tokens and role
 * @param {Object} tokens - Authentication data
 * @param {string} tokens.accessToken - JWT access token
 * @param {string} tokens.refreshToken - Refresh token
 * @param {string} tokens.role - User role
 */
export function saveTokens({ accessToken, refreshToken, role }) {
    localStorage.setItem(tokenKey, accessToken);
    localStorage.setItem(refreshKey, refreshToken);
    localStorage.setItem(roleKey, role);
}

/**
 * Get stored tokens
 * @returns {Object} Stored authentication data
 */
export function getTokens() {
    return {
        accessToken: localStorage.getItem(tokenKey),
        refreshToken: localStorage.getItem(refreshKey),
        role: localStorage.getItem(roleKey)
    };
}

/**
 * Clear all authentication data
 */
export function clearTokens() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(refreshKey);
    localStorage.removeItem(roleKey);
}

/**
 * Check if user is authenticated as admin
 * @returns {boolean} True if authenticated as admin
 */
export function isAuthenticated() {
    const tokens = getTokens();
    return !!(tokens.accessToken && tokens.role === 'admin');
}

/**
 * Require admin role - redirect to login if not admin
 */
export function requireAdmin() {
    if (!isAuthenticated()) {
        window.location.href = "/admin/login.html";
        return false;
    }
    return true;
}

/**
 * Get authorization header for API requests
 * @returns {Object} Authorization header object
 */
export function authHeader() {
    const token = localStorage.getItem(tokenKey);
    return token ? { "Authorization": `Bearer ${token}` } : {};
}

/**
 * Enhanced fetch wrapper with auth headers and error handling
 * @param {string} path - API endpoint path
 * @param {Object} options - Fetch options
 * @returns {Promise} Parsed JSON response
 */
export async function fetchWithAuth(path, options = {}) {
    const url = path.startsWith('http') ? path : `${API_BASE_URL}${path}`;
    
    const headers = {
        'Content-Type': 'application/json',
        'X-Client': '241RA-Admin/1.0',
        ...authHeader(),
        ...options.headers
    };
    
    try {
        const response = await fetch(url, {
            ...options,
            headers
        });
        
        // Handle 401 Unauthorized
        if (response.status === 401) {
            clearTokens();
            window.location.href = '/admin/login.html';
            throw new Error('Session expired. Please sign in again.');
        }
        
        // Parse JSON response
        const data = await response.json();
        
        // Handle error responses
        if (!response.ok) {
            const error = new Error(data.message || `HTTP ${response.status}`);
            error.status = response.status;
            error.errors = data.errors;
            throw error;
        }
        
        return data;
    } catch (error) {
        // Re-throw if it's already our custom error
        if (error.status) {
            throw error;
        }
        
        // Handle network errors
        console.error('API request failed:', error);
        throw new Error('Network error. Please check your connection.');
    }
}

/**
 * Refresh access token using refresh token
 * @returns {Promise<boolean>} True if refresh successful
 */
export async function refreshToken() {
    try {
        const refreshToken = localStorage.getItem(refreshKey);
        if (!refreshToken) {
            throw new Error('No refresh token available');
        }
        
        const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ refreshToken })
        });
        
        if (response.ok) {
            const data = await response.json();
            saveTokens({
                accessToken: data.accessToken,
                refreshToken: data.refreshToken,
                role: data.role
            });
            return true;
        } else {
            throw new Error('Token refresh failed');
        }
    } catch (error) {
        console.error('Token refresh failed:', error);
        clearTokens();
        return false;
    }
}

/**
 * Logout admin user
 */
export function logout() {
    clearTokens();
    window.location.href = '/admin/login.html';
}

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} True if valid email
 */
export function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Show notification/toast message
 * @param {string} message - Message to display
 * @param {string} type - Message type (success, error, warning, info)
 */
export function showNotification(message, type = 'info') {
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    
    // Add to page
    document.body.appendChild(toast);
    
    // Show toast
    setTimeout(() => toast.classList.add('show'), 100);
    
    // Remove after 5 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => document.body.removeChild(toast), 300);
    }, 5000);
}

/**
 * Show error message in form
 * @param {string} elementId - Error element ID
 * @param {string} message - Error message
 */
export function showError(elementId, message) {
    const errorElement = document.getElementById(elementId);
    if (errorElement) {
        errorElement.textContent = message;
        errorElement.hidden = false;
    }
}

/**
 * Clear error message in form
 * @param {string} elementId - Error element ID
 */
export function clearError(elementId) {
    const errorElement = document.getElementById(elementId);
    if (errorElement) {
        errorElement.textContent = '';
        errorElement.hidden = true;
    }
}

/**
 * Clear all error messages
 */
export function clearAllErrors() {
    const errorElements = document.querySelectorAll('.error-text, [role="alert"]');
    errorElements.forEach(element => {
        element.textContent = '';
        element.hidden = true;
    });
} 