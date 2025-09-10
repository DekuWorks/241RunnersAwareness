/**
 * Centralized API Client for 241 Runners Awareness
 * Handles JWT authentication and provides consistent API interface
 */

// Configure axios defaults
axios.defaults.baseURL = "https://241runners-api.azurewebsites.net/api";
axios.defaults.withCredentials = false; // No cookies - use JWT only

// Request interceptor to add JWT token
axios.interceptors.request.use((config) => {
    const token = localStorage.getItem("jwtToken") || localStorage.getItem("ra_admin_token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Response interceptor for error handling
axios.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            // Token expired or invalid
            console.warn('Authentication failed, redirecting to login...');
            localStorage.removeItem("jwtToken");
            localStorage.removeItem("ra_admin_token");
            localStorage.removeItem("ra_admin_role");
            localStorage.removeItem("ra_admin_user");
            window.location.href = '/admin/login.html';
        }
        return Promise.reject(error);
    }
);

// API Client class
class ApiClient {
    constructor() {
        this.baseURL = "https://241runners-api.azurewebsites.net/api";
    }

    // Generic request method
    async request(method, endpoint, data = null, options = {}) {
        try {
            const config = {
                method,
                url: endpoint,
                ...options
            };

            if (data) {
                if (method.toLowerCase() === 'get') {
                    config.params = data;
                } else {
                    config.data = data;
                }
            }

            const response = await axios(config);
            return response.data;
        } catch (error) {
            console.error(`API ${method.toUpperCase()} ${endpoint} failed:`, error);
            throw error;
        }
    }

    // Convenience methods
    async get(endpoint, params = null) {
        return this.request('GET', endpoint, params);
    }

    async post(endpoint, data) {
        return this.request('POST', endpoint, data);
    }

    async put(endpoint, data) {
        return this.request('PUT', endpoint, data);
    }

    async delete(endpoint) {
        return this.request('DELETE', endpoint);
    }

    // Authentication methods
    async login(email, password) {
        const response = await this.post('/auth/login', { email, password });
        if (response.success && response.token) {
            localStorage.setItem("jwtToken", response.token);
            localStorage.setItem("ra_admin_token", response.token); // Backward compatibility
            localStorage.setItem("ra_admin_role", response.user?.role || 'admin');
            localStorage.setItem("ra_admin_user", JSON.stringify(response.user));
        }
        return response;
    }

    async logout() {
        localStorage.removeItem("jwtToken");
        localStorage.removeItem("ra_admin_token");
        localStorage.removeItem("ra_admin_role");
        localStorage.removeItem("ra_admin_user");
        return this.post('/auth/logout');
    }

    // Admin methods
    async getUsers() {
        return this.get('/admin/users');
    }

    async getAdmins() {
        return this.get('/admin/admins');
    }

    async deleteUser(userId) {
        return this.delete(`/admin/users/${userId}`);
    }

    async deleteAdmin(adminId) {
        return this.delete(`/admin/admins/${adminId}`);
    }

    async updateUser(userId, userData) {
        return this.put(`/admin/users/${userId}`, userData);
    }

    async getDashboardStats() {
        return this.get('/admin/dashboard-stats');
    }

    // Cases methods
    async getPublicCases() {
        return this.get('/cases');
    }

    async getCases() {
        return this.get('/admin/cases');
    }

    // Health check
    async healthCheck() {
        return this.get('/health');
    }
}

// Create global instance
window.apiClient = new ApiClient();

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApiClient;
}
