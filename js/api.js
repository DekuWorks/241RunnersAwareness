/**
 * Centralized API client for 241 Runners Awareness
 * Handles all API communication with proper error handling
 */

// Global config object
window.__CONFIG = window.__CONFIG || {};

// Load configuration
async function loadConfig() {
    try {
        const response = await fetch('/config.json');
        if (response.ok) {
            const config = await response.json();
            window.__CONFIG = { ...window.__CONFIG, ...config };
            console.log('🔧 Configuration loaded:', window.__CONFIG);
        } else {
            console.warn('⚠️ Could not load config.json, using defaults');
            window.__CONFIG.API_BASE_URL = window.__CONFIG.API_BASE_URL || 'https://241runners-api-v2.azurewebsites.net';
        }
    } catch (error) {
        console.error('❌ Error loading configuration:', error);
        window.__CONFIG.API_BASE_URL = window.__CONFIG.API_BASE_URL || 'https://241runners-api-v2.azurewebsites.net';
    }
}

// Initialize config on load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadConfig);
} else {
    loadConfig();
}

/**
 * Main API function
 * @param {string} path - API endpoint path (e.g., '/api/admin/stats')
 * @param {object} options - Fetch options
 * @returns {Promise<any>} API response data
 */
async function api(path, options = {}) {
    const base = window.__CONFIG.API_BASE_URL;
    
    // Add JWT token if available
    const token = localStorage.getItem('access_token') || localStorage.getItem('ra_admin_token');
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    const finalOptions = {
        credentials: 'include',
        headers,
        ...options
    };
    
    const url = `${base}${path}`;
    
    try {
        console.log(`🌐 API Request: ${finalOptions.method || 'GET'} ${url}`);
        const res = await fetch(url, finalOptions);
        
        if (!res.ok) {
            const text = await res.text().catch(() => '');
            console.error(`❌ API Error ${res.status}:`, text);
            
            // Handle authentication errors
            if (res.status === 401) {
                // Clear invalid tokens
                localStorage.removeItem('access_token');
                localStorage.removeItem('ra_admin_token');
                localStorage.removeItem('ra_admin_role');
                localStorage.removeItem('ra_admin_user');
                
                // Redirect to login if on admin pages
                if (window.location.pathname.includes('/admin/')) {
                    window.location.href = '/admin/login.html';
                    return;
                }
            }
            
            throw new Error(`API ${res.status}: ${text}`);
        }
        
        const data = await res.json();
        console.log(`✅ API Response: ${finalOptions.method || 'GET'} ${url}`, data);
        return data;
    } catch (error) {
        console.error(`❌ API Request failed: ${finalOptions.method || 'GET'} ${url}`, error);
        throw error;
    }
}

/**
 * Admin API methods
 */
const adminApi = {
    // Get dashboard statistics
    async getStats() {
        return api('/api/admin/stats');
    },

    // Get admin activity
    async getActivity(page = 1, pageSize = 25) {
        return api(`/api/admin/activity?page=${page}&pageSize=${pageSize}`);
    },

    // Get all users
    async getUsers(role = null, search = '', page = 1, pageSize = 25) {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: pageSize.toString()
        });
        if (role) params.append('role', role);
        if (search) params.append('q', search);
        return api(`/api/users?${params}`);
    },

    // Create user
    async createUser(userData) {
        return api('/api/users', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    },

    // Update user
    async updateUser(userId, userData) {
        return api(`/api/users/${userId}`, {
            method: 'PUT',
            body: JSON.stringify(userData)
        });
    },

    // Delete user
    async deleteUser(userId) {
        return api(`/api/users/${userId}`, { method: 'DELETE' });
    }
};

/**
 * Auth API methods
 */
const authApi = {
    // Login
    async login(email, password) {
        return api('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
    },

    // Register
    async register(userData) {
        return api('/api/auth/register', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    },

    // Logout
    async logout() {
        return api('/api/auth/logout', { method: 'POST' });
    }
};

/**
 * Cases API methods
 */
const casesApi = {
    // Get cases with pagination and filtering
    async getCases(status = null, search = '', page = 1, pageSize = 25) {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: pageSize.toString()
        });
        if (status) params.append('status', status);
        if (search) params.append('q', search);
        return api(`/api/cases?${params}`);
    },

    // Get case by ID
    async getCase(caseId) {
        return api(`/api/cases/${caseId}`);
    },

    // Create case
    async createCase(caseData) {
        return api('/api/cases', {
            method: 'POST',
            body: JSON.stringify(caseData)
        });
    },

    // Update case
    async updateCase(caseId, caseData) {
        return api(`/api/cases/${caseId}`, {
            method: 'PUT',
            body: JSON.stringify(caseData)
        });
    },

    // Delete case
    async deleteCase(caseId) {
        return api(`/api/cases/${caseId}`, { method: 'DELETE' });
    }
};

/**
 * Individuals API methods
 */
const individualsApi = {
    // Get individuals with pagination and filtering
    async getIndividuals(status = null, search = '', mine = false, page = 1, pageSize = 25) {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: pageSize.toString()
        });
        if (status) params.append('status', status);
        if (search) params.append('q', search);
        if (mine) params.append('mine', 'true');
        return api(`/api/individuals?${params}`);
    },

    // Get individual by ID
    async getIndividual(individualId) {
        return api(`/api/individuals/${individualId}`);
    },

    // Create individual
    async createIndividual(individualData) {
        return api('/api/individuals', {
            method: 'POST',
            body: JSON.stringify(individualData)
        });
    },

    // Update individual
    async updateIndividual(individualId, individualData) {
        return api(`/api/individuals/${individualId}`, {
            method: 'PUT',
            body: JSON.stringify(individualData)
        });
    },

    // Delete individual
    async deleteIndividual(individualId) {
        return api(`/api/individuals/${individualId}`, { method: 'DELETE' });
    }
};

/**
 * Map API methods
 */
const mapApi = {
    // Get map points
    async getMapPoints(status = null, cluster = true) {
        const params = new URLSearchParams();
        if (status) params.append('status', status);
        if (cluster) params.append('cluster', 'true');
        return api(`/api/map/points?${params}`);
    },

    // Get raw map points (admin only)
    async getRawMapPoints() {
        return api('/api/map/points/raw');
    }
};

/**
 * Notifications API methods
 */
const notificationsApi = {
    // Get notifications
    async getNotifications(read = null, page = 1, pageSize = 25) {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: pageSize.toString()
        });
        if (read !== null) params.append('read', read.toString());
        return api(`/api/notifications?${params}`);
    },

    // Create notification (admin only)
    async createNotification(notificationData) {
        return api('/api/notifications', {
            method: 'POST',
            body: JSON.stringify(notificationData)
        });
    },

    // Mark notification as read
    async markAsRead(notificationId) {
        return api(`/api/notifications/${notificationId}/read`, { method: 'POST' });
    }
};

/**
 * Utility functions
 */
const apiUtils = {
    // Check if user is authenticated
    isAuthenticated() {
        const token = localStorage.getItem('access_token') || localStorage.getItem('ra_admin_token');
        if (!token) return false;

        try {
            // Basic JWT validation
            const parts = token.split('.');
            if (parts.length !== 3) return false;

            const payload = JSON.parse(atob(parts[1]));
            const now = Math.floor(Date.now() / 1000);
            
            if (payload.exp && payload.exp < now) {
                // Token expired
                localStorage.removeItem('access_token');
                localStorage.removeItem('ra_admin_token');
                localStorage.removeItem('ra_admin_role');
                localStorage.removeItem('ra_admin_user');
                return false;
            }

            return true;
        } catch (error) {
            console.error('Token validation error:', error);
            return false;
        }
    },

    // Get user role
    getUserRole() {
        return localStorage.getItem('ra_admin_role') || 'user';
    },

    // Clear authentication data
    clearAuth() {
        localStorage.removeItem('access_token');
        localStorage.removeItem('ra_admin_token');
        localStorage.removeItem('ra_admin_role');
        localStorage.removeItem('ra_admin_user');
    }
};

// Make API functions globally available
window.api = api;
window.adminApi = adminApi;
window.authApi = authApi;
window.casesApi = casesApi;
window.individualsApi = individualsApi;
window.mapApi = mapApi;
window.notificationsApi = notificationsApi;
window.apiUtils = apiUtils;

console.log('🔧 API client initialized');
