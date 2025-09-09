/**
 * ============================================
 * 241 RUNNERS AWARENESS - ENHANCED ADMIN AUTH
 * ============================================
 * 
 * Enhanced admin authentication with route guards, silent refresh, and real-time updates
 * P0 Implementation: Multi-Admin Login Reliability
 */

// ===== CONFIGURATION =====
const AUTH_CONFIG = {
    tokenKey: "ra_admin_token",
    roleKey: "ra_admin_role", 
    userKey: "ra_admin_user",
    refreshKey: "ra_admin_refresh_token",
    apiBaseUrl: 'https://241runners-api.azurewebsites.net/api',
    tokenRefreshThreshold: 5 * 60 * 1000, // 5 minutes before expiry
    maxRetryAttempts: 3,
    retryDelay: 1000
};

// ===== STATE MANAGEMENT =====
let authState = {
    isAuthenticated: false,
    currentUser: null,
    tokenExpiry: null,
    refreshTimer: null,
    isRefreshing: false
};

// ===== UTILITY FUNCTIONS =====

/**
 * Safe localStorage operations with error handling
 */
const storage = {
    get: (key, defaultValue = null) => {
        try {
            const value = localStorage.getItem(key);
            return value || defaultValue;
        } catch (e) {
            console.warn('localStorage get error:', e);
            return defaultValue;
        }
    },
    set: (key, value) => {
        try {
            localStorage.setItem(key, typeof value === 'string' ? value : JSON.stringify(value));
            return true;
        } catch (e) {
            console.warn('localStorage set error:', e);
            return false;
        }
    },
    remove: (key) => {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (e) {
            console.warn('localStorage remove error:', e);
            return false;
        }
    },
    clear: () => {
        try {
            const keys = Object.values(AUTH_CONFIG).filter(key => key.includes('ra_admin_'));
            keys.forEach(key => {
                localStorage.removeItem(key);
                sessionStorage.removeItem(key);
            });
            return true;
        } catch (e) {
            console.warn('localStorage clear error:', e);
            return false;
        }
    }
};

/**
 * Enhanced fetch with timeout, retry, and proper error handling
 */
async function apiRequest(url, options = {}, retryCount = 0) {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 15000); // 15 second timeout
    
    try {
        const response = await fetch(url, {
            ...options,
            signal: controller.signal,
            headers: {
                'Content-Type': 'application/json',
                'X-Client': '241RA-Admin/2.0',
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Expires': '0',
                ...options.headers
            },
            cache: 'no-store'
        });
        
        clearTimeout(timeout);
        
        // Handle 401 Unauthorized with retry logic
        if (response.status === 401 && retryCount < AUTH_CONFIG.maxRetryAttempts) {
            console.log(`🔄 401 detected, attempting token refresh (attempt ${retryCount + 1})`);
            
            const refreshSuccess = await attemptTokenRefresh();
            if (refreshSuccess) {
                // Retry the original request with new token
                const newHeaders = {
                    ...options.headers,
                    ...authHeader()
                };
                return apiRequest(url, { ...options, headers: newHeaders }, retryCount + 1);
            }
        }
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        return await response.json();
    } catch (error) {
        clearTimeout(timeout);
        
        if (error.name === 'AbortError') {
            throw new Error('Request timeout - please check your connection');
        }
        
        // Handle network errors with retry
        if (retryCount < AUTH_CONFIG.maxRetryAttempts && 
            (error.message.includes('Failed to fetch') || error.message.includes('NetworkError'))) {
            console.log(`🔄 Network error, retrying (attempt ${retryCount + 1})`);
            await new Promise(resolve => setTimeout(resolve, AUTH_CONFIG.retryDelay * (retryCount + 1)));
            return apiRequest(url, options, retryCount + 1);
        }
        
        throw error;
    }
}

// ===== AUTHENTICATION FUNCTIONS =====

/**
 * Save authentication data with enhanced validation
 */
function saveAuthData(token, user, refreshToken = null) {
    try {
        console.log('💾 Saving authentication data...');
        
        // Validate token format
        if (!token || typeof token !== 'string' || token.length < 10) {
            throw new Error('Invalid token format');
        }
        
        // Validate user object
        if (!user || !user.email || !user.role) {
            throw new Error('Invalid user data');
        }
        
        // Validate admin role
        if (user.role.toLowerCase() !== 'admin') {
            throw new Error('Admin role required');
        }
        
        // Calculate token expiry (JWT tokens typically last 1 hour)
        const tokenExpiry = Date.now() + (60 * 60 * 1000); // 1 hour from now
        
        // Save to storage
        storage.set(AUTH_CONFIG.tokenKey, token);
        storage.set(AUTH_CONFIG.roleKey, user.role);
        storage.set(AUTH_CONFIG.userKey, user);
        if (refreshToken) {
            storage.set(AUTH_CONFIG.refreshKey, refreshToken);
        }
        
        // Update auth state
        authState.isAuthenticated = true;
        authState.currentUser = user;
        authState.tokenExpiry = tokenExpiry;
        
        // Start token refresh timer
        startTokenRefreshTimer();
        
        console.log('✅ Authentication data saved successfully');
        return true;
    } catch (error) {
        console.error('❌ Failed to save auth data:', error);
        return false;
    }
}

/**
 * Clear all authentication data
 */
function clearAuthData() {
    try {
        console.log('🧹 Clearing authentication data...');
        
        // Clear storage
        storage.clear();
        
        // Clear auth state
        authState.isAuthenticated = false;
        authState.currentUser = null;
        authState.tokenExpiry = null;
        
        // Clear refresh timer
        if (authState.refreshTimer) {
            clearTimeout(authState.refreshTimer);
            authState.refreshTimer = null;
        }
        
        console.log('✅ Authentication data cleared successfully');
        return true;
    } catch (error) {
        console.error('❌ Error clearing auth data:', error);
        return false;
    }
}

/**
 * Check if user is authenticated with enhanced validation
 */
function isAuthenticated() {
    try {
        const token = storage.get(AUTH_CONFIG.tokenKey);
        const role = storage.get(AUTH_CONFIG.roleKey);
        const user = storage.get(AUTH_CONFIG.userKey);
        
        // Basic validation
        if (!token || !role || !user) {
            return false;
        }
        
        // Role validation
        if (role.toLowerCase() !== 'admin') {
            return false;
        }
        
        // Token format validation
        if (typeof token !== 'string' || token.length < 10) {
            return false;
        }
        
        // Check token expiry
        if (authState.tokenExpiry && Date.now() > authState.tokenExpiry) {
            console.log('⏰ Token expired, clearing auth data');
            clearAuthData();
            return false;
        }
        
        // Update auth state
        authState.isAuthenticated = true;
        authState.currentUser = user;
        
        return true;
    } catch (error) {
        console.error('❌ Error checking authentication:', error);
        return false;
    }
}

/**
 * Get authorization header for API requests
 */
function authHeader() {
    const token = storage.get(AUTH_CONFIG.tokenKey);
    return token ? { "Authorization": `Bearer ${token}` } : {};
}

/**
 * Attempt to refresh the authentication token
 */
async function attemptTokenRefresh() {
    if (authState.isRefreshing) {
        console.log('🔄 Token refresh already in progress, waiting...');
        // Wait for current refresh to complete
        while (authState.isRefreshing) {
            await new Promise(resolve => setTimeout(resolve, 100));
        }
        return authState.isAuthenticated;
    }
    
    try {
        authState.isRefreshing = true;
        console.log('🔄 Attempting token refresh...');
        
        // Try to verify current token first
        const response = await apiRequest(`${AUTH_CONFIG.apiBaseUrl}/Auth/verify`, {
            method: 'GET',
            headers: authHeader()
        });
        
        if (response.success && response.user) {
            console.log('✅ Token is still valid');
            authState.isRefreshing = false;
            return true;
        }
        
        // If verification fails, try to refresh
        const refreshToken = storage.get(AUTH_CONFIG.refreshKey);
        if (refreshToken) {
            const refreshResponse = await apiRequest(`${AUTH_CONFIG.apiBaseUrl}/Auth/refresh`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${refreshToken}` }
            });
            
            if (refreshResponse.success && refreshResponse.token) {
                saveAuthData(refreshResponse.token, refreshResponse.user, refreshResponse.refreshToken);
                authState.isRefreshing = false;
                return true;
            }
        }
        
        // If all refresh attempts fail, clear auth data
        console.log('❌ Token refresh failed, clearing auth data');
        clearAuthData();
        authState.isRefreshing = false;
        return false;
        
    } catch (error) {
        console.error('❌ Token refresh error:', error);
        clearAuthData();
        authState.isRefreshing = false;
        return false;
    }
}

/**
 * Start token refresh timer
 */
function startTokenRefreshTimer() {
    if (authState.refreshTimer) {
        clearTimeout(authState.refreshTimer);
    }
    
    // Refresh token 5 minutes before expiry
    const refreshTime = AUTH_CONFIG.tokenRefreshThreshold;
    
    authState.refreshTimer = setTimeout(async () => {
        console.log('⏰ Token refresh timer triggered');
        await attemptTokenRefresh();
    }, refreshTime);
}

// ===== ROUTE GUARD FUNCTIONS =====

/**
 * Require admin authentication - redirect to login if not authenticated
 */
function requireAdmin() {
    console.log('🔐 Checking admin authentication...');
    
    if (!isAuthenticated()) {
        console.log('❌ Authentication failed, redirecting to login');
        const currentPath = window.location.pathname;
        const redirectUrl = `/admin/login.html?redirect=${encodeURIComponent(currentPath)}`;
        window.location.href = redirectUrl;
        return false;
    }
    
    console.log('✅ Admin authentication successful');
    return true;
}

/**
 * Initialize route guard for admin pages
 */
function initializeRouteGuard() {
    console.log('��️ Initializing route guard...');
    
    // Check authentication on page load
    if (!requireAdmin()) {
        return false;
    }
    
    // Set up periodic authentication checks
    setInterval(() => {
        if (!isAuthenticated()) {
            console.log('🔄 Periodic auth check failed, redirecting to login');
            window.location.href = '/admin/login.html';
        }
    }, 30000); // Check every 30 seconds
    
    return true;
}

// ===== API FUNCTIONS =====

/**
 * Enhanced fetch with authentication
 */
async function fetchWithAuth(path, options = {}) {
    const url = path.startsWith('http') ? path : `${AUTH_CONFIG.apiBaseUrl}${path}`;
    
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
        // Handle authentication errors
        if (error.message.includes('401') || error.message.includes('Unauthorized')) {
            console.log('🔐 Authentication error, attempting refresh...');
            const refreshSuccess = await attemptTokenRefresh();
            if (!refreshSuccess) {
                window.location.href = '/admin/login.html';
                throw new Error('Session expired. Please sign in again.');
            }
        }
        
        throw error;
    }
}

// ===== NOTIFICATION FUNCTIONS =====

/**
 * Show toast notification
 */
function showToast(message, type = 'info', duration = 5000) {
    const container = document.getElementById('toastContainer') || createToastContainer();
    
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <span class="toast-message">${message}</span>
            <button class="toast-close" onclick="this.parentElement.parentElement.remove()">×</button>
        </div>
    `;
    
    container.appendChild(toast);
    
    // Show animation
    setTimeout(() => toast.classList.add('show'), 100);
    
    // Auto remove
    setTimeout(() => {
        if (toast.parentElement) {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }
    }, duration);
}

/**
 * Create toast container if it doesn't exist
 */
function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toastContainer';
    container.className = 'toast-container';
    container.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 10000;
        pointer-events: none;
    `;
    document.body.appendChild(container);
    return container;
}

// ===== INITIALIZATION =====

/**
 * Initialize admin authentication system
 */
async function initializeAdminAuth() {
    console.log('🚀 Initializing admin authentication system...');
    
    try {
        // Load configuration
        await loadConfig();
        
        // Initialize route guard
        if (!initializeRouteGuard()) {
            return false;
        }
        
        // Load current user data
        await loadCurrentUser();
        
        console.log('✅ Admin authentication system initialized successfully');
        return true;
        
    } catch (error) {
        console.error('❌ Failed to initialize admin auth:', error);
        return false;
    }
}

/**
 * Load configuration from config.json
 */
async function loadConfig() {
    try {
        const response = await fetch('/config.json', {
            cache: 'no-store'
        });
        const config = await response.json();
        
        if (config.API_BASE_URL) {
            AUTH_CONFIG.apiBaseUrl = config.API_BASE_URL;
        }
        
        console.log('📋 Configuration loaded:', AUTH_CONFIG.apiBaseUrl);
    } catch (error) {
        console.warn('⚠️ Failed to load config.json, using default API URL:', error);
    }
}

/**
 * Load current user data from API
 */
async function loadCurrentUser() {
    try {
        const data = await fetchWithAuth('/Auth/verify');
        
        if (data.success && data.user) {
            authState.currentUser = data.user;
            console.log('👤 Current user loaded:', data.user.email);
            return data.user;
        }
        
        throw new Error('Failed to load user data');
    } catch (error) {
        console.error('❌ Error loading current user:', error);
        throw error;
    }
}

// ===== EXPORTS =====

// Make functions available globally for non-module usage
window.AdminAuth = {
    // Core functions
    saveAuthData,
    clearAuthData,
    isAuthenticated,
    requireAdmin,
    authHeader,
    fetchWithAuth,
    apiRequest,
    
    // Advanced functions
    attemptTokenRefresh,
    initializeAdminAuth,
    loadCurrentUser,
    
    // Utilities
    showToast,
    storage,
    
    // State
    getAuthState: () => ({ ...authState }),
    getCurrentUser: () => authState.currentUser
};

// Auto-initialize if this script is loaded on an admin page
if (window.location.pathname.includes('/admin/') && window.location.pathname !== '/admin/login.html') {
    document.addEventListener('DOMContentLoaded', () => {
        initializeAdminAuth().catch(error => {
            console.error('Failed to initialize admin auth:', error);
            window.location.href = '/admin/login.html';
        });
    });
}
