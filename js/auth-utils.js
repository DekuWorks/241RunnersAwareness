/**
 * 241 Runners Awareness - Authentication Utilities
 * 
 * Unified authentication system for token storage, retrieval, and session management
 */

// API Configuration
let API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api';

// Load API configuration from config.json
async function loadConfig() {
    // Configuration is already set in API_BASE_URL
    // No need to fetch config.json to prevent 404 errors
    return;
}

// Initialize config on load (disabled to prevent 404 errors)
// loadConfig();

/**
 * ============================================
 * AUTHENTICATION STORAGE
 * ============================================
 */

/**
 * Store authentication data
 * @param {Object} authData - Authentication data object
 */
function setAuth(authData) {
    if (!authData || !authData.token) {
        console.error('Invalid auth data provided');
        return;
    }

    const authStorage = {
        token: authData.token,
        refreshToken: authData.refreshToken,
        expiresIn: authData.expiresIn,
        user: authData.user,
        timestamp: Date.now()
    };

    localStorage.setItem('ra_auth', JSON.stringify(authStorage));
    console.log('Auth data stored successfully');
}

/**
 * Get authentication data
 * @returns {Object|null} Authentication data or null if not found
 */
function getAuth() {
    try {
        const authData = localStorage.getItem('ra_auth');
        return authData ? JSON.parse(authData) : null;
    } catch (error) {
        console.error('Error retrieving auth data:', error);
        return null;
    }
}

/**
 * Get authentication token
 * @returns {string|null} Authentication token or null if not found
 */
function getAuthToken() {
    const authData = getAuth();
    return authData ? authData.token : null;
}

/**
 * Get current user data
 * @returns {Object|null} Current user data or null if not found
 */
function getCurrentUser() {
    const authData = getAuth();
    return authData ? authData.user : null;
}

/**
 * Check if user is authenticated
 * @returns {boolean} True if user is authenticated
 */
function isAuthenticated() {
    const authData = getAuth();
    if (!authData || !authData.token) {
        return false;
    }

    // Check if token is expired (if expiresIn is provided)
    if (authData.expiresIn && authData.timestamp) {
        const now = Date.now();
        const expirationTime = authData.timestamp + (authData.expiresIn * 1000);
        if (now > expirationTime) {
            console.log('Token expired, clearing auth data');
            clearAuth();
            return false;
        }
    }

    return true;
}

/**
 * Clear authentication data
 */
function clearAuth() {
    localStorage.removeItem('ra_auth');
    console.log('Auth data cleared');
}

/**
 * ============================================
 * API COMMUNICATION
 * ============================================
 */

/**
 * Make authenticated API request
 * @param {string} endpoint - API endpoint path
 * @param {Object} options - Fetch options
 * @returns {Promise} Fetch response
 */
async function apiRequest(endpoint, options = {}) {
    const token = getAuthToken();
    if (!token) {
        throw new Error('No authentication token found');
    }

    const url = endpoint.startsWith('http') ? endpoint : `${API_BASE_URL}${endpoint}`;
    
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            ...options.headers
        }
    };

    const response = await fetch(url, { ...defaultOptions, ...options });
    
    // Handle 401 Unauthorized
    if (response.status === 401) {
        console.log('Unauthorized request, clearing auth data');
        clearAuth();
        window.location.href = '/login.html';
        throw new Error('Unauthorized - please log in again');
    }

    return response;
}

/**
 * Get current user data from API
 * @returns {Promise<Object>} User data
 */
async function getCurrentUserFromAPI() {
    const response = await apiRequest('/v1.0/users/me');
    if (!response.ok) {
        throw new Error(`Failed to get user data: ${response.statusText}`);
    }
    return await response.json();
}

/**
 * ============================================
 * SESSION MANAGEMENT
 * ============================================
 */

/**
 * Check authentication status and redirect if needed
 * @param {string} redirectTo - Where to redirect if not authenticated
 */
function requireAuth(redirectTo = '/login.html') {
    if (!isAuthenticated()) {
        window.location.href = redirectTo;
        return false;
    }
    return true;
}

/**
 * Handle logout
 */
function logout() {
    clearAuth();
    window.location.href = '/login.html';
}

/**
 * ============================================
 * TOAST NOTIFICATIONS
 * ============================================
 */

/**
 * Show toast notification
 * @param {string} message - Message to display
 * @param {string} type - Type of toast (success, error, info, warning)
 */
function showToast(message, type = 'info') {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 10000;
            max-width: 400px;
        `;
        document.body.appendChild(toastContainer);
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.style.cssText = `
        background: #fff;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        margin-bottom: 10px;
        padding: 16px 20px;
        display: flex;
        align-items: center;
        gap: 12px;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        border-left: 4px solid #3b82f6;
    `;

    // Set border color based on type
    const colors = {
        success: '#10b981',
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    };
    toast.style.borderLeftColor = colors[type] || colors.info;

    // Add icon based on type
    const icons = {
        success: '✅',
        error: '❌',
        warning: '⚠️',
        info: 'ℹ️'
    };
    toast.innerHTML = `
        <span style="font-size: 18px;">${icons[type] || icons.info}</span>
        <span>${message}</span>
    `;

    toastContainer.appendChild(toast);

    // Show toast
    setTimeout(() => {
        toast.style.transform = 'translateX(0)';
    }, 100);

    // Hide toast after 5 seconds
    setTimeout(() => {
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }, 5000);
}

// Export functions for use in other scripts
window.authUtils = {
    setAuth,
    getAuth,
    getAuthToken,
    getCurrentUser,
    isAuthenticated,
    clearAuth,
    apiRequest,
    getCurrentUserFromAPI,
    requireAuth,
    logout,
    showToast
};
