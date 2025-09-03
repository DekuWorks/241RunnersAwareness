/**
 * ============================================
 * 241 RUNNERS AWARENESS - AUTHENTICATION UTILITIES
 * ============================================
 * 
 * This file provides authentication utilities for the static HTML site.
 * It includes API communication, token management, and error handling.
 */

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
 * ============================================
 * API COMMUNICATION
 * ============================================
 */

/**
 * Enhanced fetch wrapper with JSON handling and auth headers
 * @param {string} path - API endpoint path
 * @param {Object} options - Fetch options
 * @returns {Promise} Parsed JSON response
 */
async function fetchJson(path, options = {}) {
    const url = path.startsWith('http') ? path : `${API_BASE_URL}${path}`;
    
    // Get auth token
    const auth = getAuth();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    
    // Add auth header if token exists
    if (auth && auth.token) {
        headers['Authorization'] = `Bearer ${auth.token}`;
    }
    
    try {
        const response = await fetch(url, {
            ...options,
            headers
        });
        
        // Handle 401 Unauthorized
        if (response.status === 401) {
            clearAuth();
            window.location.href = '/login.html';
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
 * ============================================
 * AUTHENTICATION MANAGEMENT
 * ============================================
 */

const AUTH_KEY = 'ra_auth';

/**
 * Save authentication data to localStorage
 * @param {Object} authData - Authentication data with token and user info
 */
function setAuth(authData) {
    try {
        localStorage.setItem(AUTH_KEY, JSON.stringify(authData));
    } catch (error) {
        console.error('Failed to save auth data:', error);
    }
}

/**
 * Get authentication data from localStorage
 * @returns {Object|null} Authentication data or null if not found
 */
function getAuth() {
    try {
        const authData = localStorage.getItem(AUTH_KEY);
        return authData ? JSON.parse(authData) : null;
    } catch (error) {
        console.error('Failed to get auth data:', error);
        return null;
    }
}

/**
 * Clear authentication data from localStorage
 */
function clearAuth() {
    try {
        localStorage.removeItem(AUTH_KEY);
        localStorage.removeItem('google_access_token');
        localStorage.removeItem('userToken');
    } catch (error) {
        console.error('Failed to clear auth data:', error);
    }
}

/**
 * Check if user is authenticated
 * @returns {boolean} True if user has valid auth data
 */
function isAuthenticated() {
    const auth = getAuth();
    return !!(auth && auth.token);
}

/**
 * Get current user data
 * @returns {Object|null} User data or null if not authenticated
 */
function getCurrentUser() {
    const auth = getAuth();
    return auth ? auth.user : null;
}

/**
 * Check if user has specific role
 * @param {string} role - Role to check
 * @returns {boolean} True if user has the role
 */
function hasRole(role) {
    const user = getCurrentUser();
    return user && user.role === role;
}

/**
 * Check if user is admin
 * @returns {boolean} True if user is admin
 */
function isAdmin() {
    return hasRole('Admin');
}

/**
 * ============================================
 * NAVIGATION HELPERS
 * ============================================
 */

/**
 * Update navigation based on authentication status
 */
function updateNavigation() {
    const isAuth = isAuthenticated();
    const user = getCurrentUser();
    
    // Update auth links
    const authLinks = document.querySelectorAll('[data-auth="guest"]');
    const userLinks = document.querySelectorAll('[data-auth="user"]');
    const adminLinks = document.querySelectorAll('[data-auth="admin"]');
    
    authLinks.forEach(link => {
        link.style.display = isAuth ? 'none' : 'inline-block';
    });
    
    userLinks.forEach(link => {
        link.style.display = isAuth ? 'inline-block' : 'none';
    });
    
    adminLinks.forEach(link => {
        link.style.display = (isAuth && isAdmin()) ? 'inline-block' : 'none';
    });
    
    // Update user name display
    const userNameElements = document.querySelectorAll('[data-user-name]');
    userNameElements.forEach(element => {
        if (user) {
            element.textContent = `${user.firstName} ${user.lastName}`;
        }
    });
}

/**
 * Handle logout
 */
function handleLogout() {
    // Clear auth data
    clearAuth();
    
    // Update navigation
    updateNavigation();
    
    // Redirect to login
    window.location.href = '/login.html';
}

/**
 * ============================================
 * FORM VALIDATION HELPERS
 * ============================================
 */

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
 * Validate password strength
 * @param {string} password - Password to validate
 * @returns {Object} Validation result with isValid and message
 */
function validatePassword(password) {
    if (password.length < 8) {
        return {
            isValid: false,
            message: 'Password must be at least 8 characters long'
        };
    }
    
    return {
        isValid: true,
        message: ''
    };
}

/**
 * ============================================
 * TOAST NOTIFICATIONS
 * ============================================
 */

/**
 * Show toast notification
 * @param {string} message - Message to display
 * @param {string} type - Type of toast (success, error, warning, info)
 * @param {number} duration - Duration in milliseconds
 */
function showToast(message, type = 'info', duration = 5000) {
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <span class="toast-message">${message}</span>
            <button class="toast-close" onclick="this.parentElement.parentElement.remove()">×</button>
        </div>
    `;
    
    // Add styles if not already present
    if (!document.getElementById('toast-styles')) {
        const styles = document.createElement('style');
        styles.id = 'toast-styles';
        styles.textContent = `
            .toast {
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                background: white;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
                padding: 16px;
                min-width: 300px;
                transform: translateX(100%);
                transition: transform 0.3s ease;
            }
            
            .toast.show {
                transform: translateX(0);
            }
            
            .toast-content {
                display: flex;
                align-items: center;
                justify-content: space-between;
            }
            
            .toast-message {
                flex: 1;
                margin-right: 12px;
            }
            
            .toast-close {
                background: none;
                border: none;
                font-size: 18px;
                cursor: pointer;
                color: #666;
            }
            
            .toast-success {
                border-left: 4px solid #10b981;
            }
            
            .toast-error {
                border-left: 4px solid #ef4444;
            }
            
            .toast-warning {
                border-left: 4px solid #f59e0b;
            }
            
            .toast-info {
                border-left: 4px solid #3b82f6;
            }
        `;
        document.head.appendChild(styles);
    }
    
    // Add to page
    document.body.appendChild(toast);
    
    // Show toast
    setTimeout(() => toast.classList.add('show'), 100);
    
    // Auto remove
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

/**
 * ============================================
 * INITIALIZATION
 * ============================================
 */

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Update navigation
    updateNavigation();
    
    // Add logout handlers
    const logoutButtons = document.querySelectorAll('[data-action="logout"]');
    logoutButtons.forEach(button => {
        button.addEventListener('click', handleLogout);
    });
});

// Export functions for use in other scripts
window.authUtils = {
    fetchJson,
    setAuth,
    getAuth,
    clearAuth,
    isAuthenticated,
    getCurrentUser,
    hasRole,
    isAdmin,
    updateNavigation,
    handleLogout,
    isValidEmail,
    validatePassword,
    showToast
};
