/**
 * 241 Runners Awareness - Admin Authentication Module
 * Provides route guards, token refresh, and authentication utilities
 */

class AdminAuth {
    constructor() {
        this.API_BASE_URL = 'https://241runners-api.azurewebsites.net/api';
        this.TOKEN_KEY = 'ra_admin_token';
        this.ROLE_KEY = 'ra_admin_role';
        this.USER_KEY = 'ra_admin_user';
        this.REFRESH_THRESHOLD = 5 * 60 * 1000; // 5 minutes before expiry
        this.refreshTimer = null;
        
        this.init();
    }

    init() {
        console.log('🔐 AdminAuth initialized');
        this.setupTokenRefresh();
        this.setupStorageListener();
    }

    // Storage utilities with error handling
    storage = {
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
        }
    };

    // Check if user is authenticated
    isAuthenticated() {
        const token = this.storage.get(this.TOKEN_KEY);
        const role = this.storage.get(this.ROLE_KEY);
        
        if (!token || !role) {
            return false;
        }

        // Check if token is expired
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const now = Date.now() / 1000;
            if (payload.exp < now) {
                console.log('🔐 Token expired');
                this.clearAuthData();
                return false;
            }
            return true;
        } catch (e) {
            console.warn('🔐 Invalid token format');
            this.clearAuthData();
            return false;
        }
    }

    // Check if user has admin role
    isAdmin() {
        const role = this.storage.get(this.ROLE_KEY);
        return role && role.toLowerCase() === 'admin';
    }

    // Get authentication headers
    getAuthHeaders() {
        const token = this.storage.get(this.TOKEN_KEY);
        return token ? { "Authorization": `Bearer ${token}` } : {};
    }

    // Clear all authentication data
    clearAuthData() {
        console.log('🧹 Clearing authentication data...');
        const authKeys = [
            this.TOKEN_KEY, this.ROLE_KEY, this.USER_KEY,
            'ra_user_token', 'ra_user_role', 'ra_user_data',
            'admin_token', 'admin_role', 'admin_user'
        ];
        
        authKeys.forEach(key => {
            this.storage.remove(key);
        });

        // Clear sessionStorage as well
        if (typeof sessionStorage !== 'undefined') {
            authKeys.forEach(key => {
                try {
                    sessionStorage.removeItem(key);
                } catch (e) {
                    console.warn('sessionStorage remove error:', e);
                }
            });
        }

        if (this.refreshTimer) {
            clearTimeout(this.refreshTimer);
            this.refreshTimer = null;
        }
    }

    // Setup automatic token refresh
    setupTokenRefresh() {
        if (!this.isAuthenticated()) {
            return;
        }

        try {
            const token = this.storage.get(this.TOKEN_KEY);
            const payload = JSON.parse(atob(token.split('.')[1]));
            const expiryTime = payload.exp * 1000;
            const now = Date.now();
            const timeUntilExpiry = expiryTime - now;
            const refreshTime = timeUntilExpiry - this.REFRESH_THRESHOLD;

            if (refreshTime > 0) {
                console.log(`🔄 Token refresh scheduled in ${Math.round(refreshTime / 1000)}s`);
                this.refreshTimer = setTimeout(() => {
                    this.refreshToken();
                }, refreshTime);
            } else {
                console.log('🔄 Token needs immediate refresh');
                this.refreshToken();
            }
        } catch (e) {
            console.warn('🔐 Error setting up token refresh:', e);
        }
    }

    // Refresh authentication token
    async refreshToken() {
        try {
            console.log('🔄 Refreshing authentication token...');
            
            const response = await fetch(`${this.API_BASE_URL}/Auth/verify`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Client': '241RA-Admin/1.0',
                    ...this.getAuthHeaders()
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success && data.user) {
                    // Update stored user data
                    this.storage.set(this.USER_KEY, data.user);
                    console.log('✅ Token refresh successful');
                    
                    // Schedule next refresh
                    this.setupTokenRefresh();
                    return true;
                }
            }

            console.log('❌ Token refresh failed, redirecting to login');
            this.redirectToLogin('Session expired. Please log in again.');
            return false;

        } catch (error) {
            console.error('❌ Token refresh error:', error);
            this.redirectToLogin('Network error. Please check your connection.');
            return false;
        }
    }

    // Setup storage listener for cross-tab synchronization
    setupStorageListener() {
        window.addEventListener('storage', (e) => {
            if (e.key === this.TOKEN_KEY && !e.newValue) {
                // Token was cleared in another tab
                console.log('🔐 Token cleared in another tab');
                this.redirectToLogin('Logged out in another tab.');
            }
        });
    }

    // Redirect to login with optional message
    redirectToLogin(message = 'Please log in to continue.') {
        const loginUrl = '/admin/login.html';
        if (message) {
            const params = new URLSearchParams({ message: encodeURIComponent(message) });
            window.location.href = `${loginUrl}?${params.toString()}`;
        } else {
            window.location.href = loginUrl;
        }
    }

    // Route guard - call this on admin pages
    requireAdmin() {
        console.log('🔐 Checking admin authentication...');
        
        if (!this.isAuthenticated()) {
            console.log('❌ Not authenticated, redirecting to login');
            this.redirectToLogin('Please log in to access this page.');
            return false;
        }

        if (!this.isAdmin()) {
            console.log('❌ Not an admin, redirecting to login');
            this.redirectToLogin('Admin access required.');
            return false;
        }

        console.log('✅ Admin authentication successful');
        return true;
    }

    // Get current user data
    getCurrentUser() {
        return this.storage.get(this.USER_KEY);
    }

    // Logout function
    logout() {
        console.log('🚪 Admin logout initiated...');
        this.clearAuthData();
        this.redirectToLogin('Logged out successfully.');
    }

    // API request helper with automatic auth headers
    async apiRequest(url, options = {}) {
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 10000);

        try {
            const response = await fetch(url, {
                ...options,
                signal: controller.signal,
                headers: {
                    'Content-Type': 'application/json',
                    'X-Client': '241RA-Admin/1.0',
                    ...this.getAuthHeaders(),
                    ...options.headers
                }
            });

            clearTimeout(timeout);

            if (response.status === 401) {
                console.log('🔐 Unauthorized response, redirecting to login');
                this.redirectToLogin('Session expired. Please log in again.');
                return null;
            }

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
}

// Initialize global admin auth instance
window.adminAuth = new AdminAuth();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminAuth;
}
