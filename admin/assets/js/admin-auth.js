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
const userKey = "ra_admin_user";

// API Configuration - Fixed URL without trailing slash
let API_BASE_URL = 'https://241runners-api.azurewebsites.net/api';

// Load API configuration from config.json
async function loadConfig() {
    try {
        const response = await fetch('/config.json', {
            cache: 'no-store'
        });
        const config = await response.json();
        API_BASE_URL = config.API_BASE_URL;
        console.log('API Base URL loaded:', API_BASE_URL);
    } catch (error) {
        console.warn('Failed to load config.json, using default API URL:', error);
    }
}

// Initialize config on load
loadConfig();

/**
 * Save authentication tokens and user data
 * @param {string} token - JWT access token
 * @param {Object} user - User data including role
 */
function saveAuthData(token, user) {
    try {
        localStorage.setItem(tokenKey, token);
        localStorage.setItem(roleKey, user.role);
        localStorage.setItem(userKey, JSON.stringify(user));
        console.log('Auth data saved successfully');
    } catch (error) {
        console.error('Failed to save auth data:', error);
    }
}

/**
 * Get stored authentication data
 * @returns {Object} Stored authentication data
 */
function getAuthData() {
    return {
        token: localStorage.getItem(tokenKey),
        role: localStorage.getItem(roleKey),
        user: JSON.parse(localStorage.getItem(userKey) || 'null')
    };
}

/**
 * Clear all authentication data
 */
function clearAuthData() {
    try {
        localStorage.removeItem(tokenKey);
        localStorage.removeItem(roleKey);
        localStorage.removeItem(userKey);
        console.log('Auth data cleared successfully');
    } catch (error) {
        console.error('Failed to clear auth data:', error);
    }
}

/**
 * Check if user is authenticated as admin
 * @returns {boolean} True if authenticated as admin
 */
function isAuthenticated() {
    const { token, role } = getAuthData();
    return !!(token && role && role.toLowerCase() === 'admin' && token.length > 10);
}

/**
 * Require admin role - redirect to login if not admin
 */
function requireAdmin() {
    if (!isAuthenticated()) {
        console.log('Admin authentication required, redirecting to login');
        window.location.href = "/admin/login.html";
        return false;
    }
    return true;
}

/**
 * Get authorization header for API requests
 * @returns {Object} Authorization header object
 */
function authHeader() {
    const token = localStorage.getItem(tokenKey);
    return token ? { "Authorization": `Bearer ${token}` } : {};
}

/**
 * Enhanced fetch wrapper with proper headers and error handling
 * @param {string} url - Full API URL
 * @param {Object} options - Fetch options
 * @returns {Promise} Parsed JSON response
 */
async function apiRequest(url, options = {}) {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 10000); // 10 second timeout
    
    try {
        const response = await fetch(url, {
            ...options,
            signal: controller.signal,
            credentials: 'include', // Include credentials for CORS with JWT tokens
            headers: {
                'Content-Type': 'application/json',
                'X-Client': '241RA-Admin/1.0',
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Expires': '0',
                ...options.headers
            },
            cache: 'no-store'
        });
        
        clearTimeout(timeout);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        return await response.json();
    } catch (error) {
        clearTimeout(timeout);
        if (error.name === 'AbortError') {
            throw new Error('Request timeout');
        }
        throw error;
    }
}

/**
 * Enhanced fetch wrapper with auth headers
 * @param {string} path - API endpoint path
 * @param {Object} options - Fetch options
 * @returns {Promise} Parsed JSON response
 */
async function fetchWithAuth(path, options = {}) {
    const url = path.startsWith('http') ? path : `${API_BASE_URL}${path}`;
    
    const headers = {
        ...authHeader(),
        ...options.headers
    };
    
    try {
        const data = await apiRequest(url, {
            ...options,
            headers
        });
        
        return data;
    } catch (error) {
        // Handle 401 Unauthorized
        if (error.message.includes('401')) {
            clearAuthData();
            window.location.href = '/admin/login.html';
            throw new Error('Session expired. Please sign in again.');
        }
        
        // Re-throw other errors
        throw error;
    }
}

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} True if valid email
 */
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Show notification/toast message
 * @param {string} message - Message to display
 * @param {string} type - Message type (success, error, warning, info)
 */
function showNotification(message, type = 'info') {
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
        setTimeout(() => {
            if (document.body.contains(toast)) {
                document.body.removeChild(toast);
            }
        }, 300);
    }, 5000);
}

/**
 * Show error message in form
 * @param {string} elementId - Error element ID
 * @param {string} message - Error message
 */
function showError(elementId, message) {
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
function clearError(elementId) {
    const errorElement = document.getElementById(elementId);
    if (errorElement) {
        errorElement.textContent = '';
        errorElement.hidden = true;
    }
}

/**
 * Clear all error messages
 */
function clearAllErrors() {
    const errorElements = document.querySelectorAll('.error-text, [role="alert"]');
    errorElements.forEach(element => {
        element.textContent = '';
        element.hidden = true;
    });
}

// Export functions for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        saveAuthData,
        getAuthData,
        clearAuthData,
        isAuthenticated,
        requireAdmin,
        authHeader,
        fetchWithAuth,
        apiRequest,
        isValidEmail,
        showNotification,
        showError,
        clearError,
        clearAllErrors
    };
}
