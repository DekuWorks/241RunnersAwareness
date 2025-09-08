/**
 * ============================================
 * 241 RUNNERS AWARENESS - API UTILITIES
 * ============================================
 * 
 * Centralized API communication utilities with health checks and fallback handling
 */

class APIUtils {
    constructor() {
        this.config = window.APP_CONFIG || {};
        this.isApiAvailable = true;
        this.healthCheckInterval = null;
        this.retryCount = 0;
        this.maxRetries = this.config.RETRY_ATTEMPTS || 3;
        this.timeout = this.config.API_TIMEOUT || 10000;
        
        this.init();
    }

    /**
     * Initialize API utilities
     */
    async init() {
        await this.loadConfig();
        await this.checkApiHealth();
        this.startHealthCheck();
        this.createApiBanner();
    }

    /**
     * Load configuration from config.json
     */
    async loadConfig() {
        try {
            const response = await fetch('/config.json');
            const config = await response.json();
            this.config = { ...this.config, ...config };
        } catch (error) {
            console.warn('Failed to load config.json, using default configuration');
        }
    }

    /**
     * Check API health status
     */
    async checkApiHealth() {
        try {
            const healthUrl = `${this.config.API_BASE_URL}${this.config.HEALTH_CHECK_ENDPOINT}`;
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), this.timeout);

            const response = await fetch(healthUrl, {
                method: 'GET',
                signal: controller.signal,
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            clearTimeout(timeoutId);

            if (response.ok) {
                this.setApiAvailable(true);
                this.retryCount = 0;
                return true;
            } else {
                this.setApiAvailable(false);
                return false;
            }
        } catch (error) {
            console.error('API health check failed:', error);
            this.setApiAvailable(false);
            return false;
        }
    }

    /**
     * Start periodic health checks
     */
    startHealthCheck() {
        // Check every 30 seconds
        this.healthCheckInterval = setInterval(async () => {
            await this.checkApiHealth();
        }, 30000);
    }

    /**
     * Stop health checks
     */
    stopHealthCheck() {
        if (this.healthCheckInterval) {
            clearInterval(this.healthCheckInterval);
            this.healthCheckInterval = null;
        }
    }

    /**
     * Set API availability status
     */
    setApiAvailable(available) {
        const wasAvailable = this.isApiAvailable;
        this.isApiAvailable = available;
        
        if (wasAvailable !== available) {
            this.updateApiBanner();
            this.notifyApiStatusChange(available);
        }
    }

    /**
     * Create API status banner
     */
    createApiBanner() {
        // Remove existing banner if any
        const existingBanner = document.getElementById('api-status-banner');
        if (existingBanner) {
            existingBanner.remove();
        }

        const banner = document.createElement('div');
        banner.id = 'api-status-banner';
        banner.className = 'api-status-banner';
        banner.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            z-index: 10000;
            padding: 12px 20px;
            text-align: center;
            font-weight: 600;
            font-size: 14px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            transition: all 0.3s ease;
            transform: translateY(-100%);
        `;

        document.body.appendChild(banner);
        this.updateApiBanner();
    }

    /**
     * Update API banner based on status
     */
    updateApiBanner() {
        const banner = document.getElementById('api-status-banner');
        if (!banner) return;

        if (this.isApiAvailable) {
            banner.style.display = 'none';
            banner.style.transform = 'translateY(-100%)';
            banner.style.backgroundColor = '';
            banner.style.color = '';
            banner.textContent = '';
        } else {
            banner.style.display = 'block';
            banner.style.transform = 'translateY(0)';
            banner.style.backgroundColor = '#dc2626';
            banner.style.color = 'white';
            banner.innerHTML = `
                <span>⚠️ API Unavailable</span>
                <span style="margin-left: 10px; font-weight: normal;">
                    Some features may not work properly. We're working to restore service.
                </span>
                <button onclick="apiUtils.retryConnection()" 
                        style="margin-left: 15px; background: rgba(255,255,255,0.2); 
                               border: 1px solid rgba(255,255,255,0.3); 
                               color: white; padding: 4px 8px; border-radius: 4px; 
                               cursor: pointer; font-size: 12px;">
                    Retry
                </button>
            `;
        }
    }

    /**
     * Retry API connection
     */
    async retryConnection() {
        if (this.retryCount >= this.maxRetries) {
            this.showToast('Maximum retry attempts reached. Please refresh the page.', 'error');
            return;
        }

        this.retryCount++;
        this.showToast(`Retrying connection... (${this.retryCount}/${this.maxRetries})`, 'info');
        
        const isHealthy = await this.checkApiHealth();
        if (isHealthy) {
            this.showToast('API connection restored!', 'success');
        } else {
            this.showToast('Connection failed. Please try again later.', 'error');
        }
    }

    /**
     * Notify about API status changes
     */
    notifyApiStatusChange(available) {
        const event = new CustomEvent('apiStatusChange', {
            detail: { available }
        });
        document.dispatchEvent(event);
    }

    /**
     * Enhanced fetch with retry logic and error handling
     */
    async fetchWithRetry(url, options = {}) {
        if (!this.isApiAvailable && !options.ignoreHealthCheck) {
            throw new Error('API is currently unavailable');
        }

        const fullUrl = url.startsWith('http') ? url : `${this.config.API_BASE_URL}${url}`;
        
        for (let attempt = 1; attempt <= this.maxRetries; attempt++) {
            try {
                const controller = new AbortController();
                const timeoutId = setTimeout(() => controller.abort(), this.timeout);

                const response = await fetch(fullUrl, {
                    ...options,
                    signal: controller.signal,
                    headers: {
                        'Content-Type': 'application/json',
                        ...options.headers
                    }
                });

                clearTimeout(timeoutId);

                if (response.ok) {
                    return response;
                } else if (response.status === 401) {
                    // Handle authentication errors
                    this.handleAuthError();
                    throw new Error('Authentication required');
                } else {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
            } catch (error) {
                if (attempt === this.maxRetries) {
                    console.error(`API request failed after ${this.maxRetries} attempts:`, error);
                    throw error;
                }
                
                // Wait before retry (exponential backoff)
                await new Promise(resolve => setTimeout(resolve, Math.pow(2, attempt) * 1000));
            }
        }
    }

    /**
     * Handle authentication errors
     */
    handleAuthError() {
        // Clear auth data
        localStorage.removeItem('ra_auth');
        localStorage.removeItem('google_access_token');
        localStorage.removeItem('userToken');
        
        // Redirect to login
        if (window.location.pathname !== '/login.html') {
            window.location.href = '/login.html';
        }
    }

    /**
     * Show toast notification
     */
    showToast(message, type = 'info', duration = 5000) {
        // Use existing toast function if available
        if (window.authUtils && window.authUtils.showToast) {
            window.authUtils.showToast(message, type, duration);
            return;
        }

        // Fallback toast implementation
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <span class="toast-message">${message}</span>
                <button class="toast-close" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;
        
        toast.style.cssText = `
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
        `;
        
        document.body.appendChild(toast);
        
        setTimeout(() => toast.style.transform = 'translateX(0)', 100);
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => toast.remove(), 300);
        }, duration);
    }

    /**
     * Get API base URL
     */
    getApiBaseUrl() {
        return this.config.API_BASE_URL;
    }

    /**
     * Check if API is available
     */
    isApiHealthy() {
        return this.isApiAvailable;
    }

    /**
     * Cleanup resources
     */
    destroy() {
        this.stopHealthCheck();
        const banner = document.getElementById('api-status-banner');
        if (banner) {
            banner.remove();
        }
    }
}

// Initialize API utilities
let apiUtils;
document.addEventListener('DOMContentLoaded', () => {
    apiUtils = new APIUtils();
    
    // Make it globally available
    window.apiUtils = apiUtils;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = APIUtils;
}