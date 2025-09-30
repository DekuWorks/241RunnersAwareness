/**
 * 241 Runners Awareness - Centralized Error Handler
 * 
 * This file provides comprehensive error handling with toast notifications,
 * error logging, and user-friendly error messages.
 */

/**
 * ============================================
 * ERROR HANDLER CLASS
 * ============================================
 */

class ErrorHandler {
    constructor() {
        this.errorCount = 0;
        this.maxErrors = 5;
        this.errorTimeout = 30000; // 30 seconds
        this.sessionTimeout = 15 * 60 * 1000; // 15 minutes
        this.lastActivity = Date.now();
        this.sessionTimer = null;
        this.retryAttempts = new Map();
        this.maxRetries = 3;
        this.isOnline = navigator.onLine;
        
        this.init();
    }

    /**
     * Initialize error handler
     */
    init() {
        this.setupGlobalErrorHandlers();
        this.setupSessionManagement();
        this.setupActivityTracking();
        this.setupNetworkMonitoring();
        this.setupToastContainer();
    }

    /**
     * Setup global error handlers
     */
    setupGlobalErrorHandlers() {
        // Handle unhandled promise rejections
        window.addEventListener('unhandledrejection', (event) => {
            this.handleError('Unhandled Promise Rejection', event.reason);
            event.preventDefault();
        });

        // Handle JavaScript errors
        window.addEventListener('error', (event) => {
            this.handleError('JavaScript Error', {
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                error: event.error
            });
        });

        // Handle resource loading errors
        window.addEventListener('error', (event) => {
            if (event.target !== window) {
                this.handleError('Resource Loading Error', {
                    type: event.target.tagName,
                    src: event.target.src || event.target.href,
                    error: event.error
                });
            }
        }, true);
    }

    /**
     * Setup session management
     */
    setupSessionManagement() {
        // Check session on page load
        this.checkSession();
        
        // Start session timer
        this.startSessionTimer();
        
        // Listen for storage changes (logout from other tabs)
        window.addEventListener('storage', (event) => {
            if (event.key === 'ra_auth' && !event.newValue) {
                this.handleSessionExpired();
            }
        });
    }

    /**
     * Setup activity tracking
     */
    setupActivityTracking() {
        const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'];
        
        events.forEach(event => {
            document.addEventListener(event, () => {
                this.updateLastActivity();
            }, true);
        });
    }

    /**
     * Setup network monitoring
     */
    setupNetworkMonitoring() {
        // Monitor online/offline status
        window.addEventListener('online', () => {
            this.handleNetworkStatusChange(true);
        });

        window.addEventListener('offline', () => {
            this.handleNetworkStatusChange(false);
        });

        // Monitor API connectivity
        this.startConnectivityMonitoring();
    }

    /**
     * Setup toast container
     */
    setupToastContainer() {
        if (!document.getElementById('toast-container')) {
            const container = document.createElement('div');
            container.id = 'toast-container';
            container.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 10000;
                max-width: 400px;
            `;
            document.body.appendChild(container);
        }
    }

    /**
     * Handle errors with retry logic
     */
    async handleError(type, error, context = {}) {
        this.errorCount++;
        
        const errorId = this.generateErrorId();
        const errorInfo = {
            id: errorId,
            type,
            error,
            context,
            timestamp: new Date().toISOString(),
            userAgent: navigator.userAgent,
            url: window.location.href,
            userId: this.getCurrentUserId()
        };

        // Log error
        console.error(`[${type}]`, errorInfo);

        // Store error for reporting
        this.storeError(errorInfo);

        // Handle different error types
        switch (type) {
            case 'API Error':
                await this.handleApiError(error, context);
                break;
            case 'Network Error':
                await this.handleNetworkError(error, context);
                break;
            case 'Authentication Error':
                this.handleAuthError(error, context);
                break;
            case 'Session Error':
                this.handleSessionError(error, context);
                break;
            case 'Validation Error':
                this.handleValidationError(error, context);
                break;
            default:
                this.handleGenericError(error, context);
        }

        // Check if we've exceeded max errors
        if (this.errorCount >= this.maxErrors) {
            this.handleMaxErrorsReached();
        }
    }

    /**
     * Handle API errors with retry logic
     */
    async handleApiError(error, context) {
        const { url, method = 'GET' } = context;
        
        if (!url) return;

        const retryKey = `${method}:${url}`;
        const retryCount = this.retryAttempts.get(retryKey) || 0;

        if (retryCount < this.maxRetries && this.shouldRetry(error)) {
            this.retryAttempts.set(retryKey, retryCount + 1);
            
            // Exponential backoff
            const delay = Math.pow(2, retryCount) * 1000;
            
            this.showToast(`Retrying request... (${retryCount + 1}/${this.maxRetries})`, 'warning');
            
            setTimeout(async () => {
                try {
                    await this.retryRequest(url, method, context);
                    this.retryAttempts.delete(retryKey);
                } catch (retryError) {
                    this.handleError('API Error', retryError, { ...context, retry: true });
                }
            }, delay);
        } else {
            this.showToast('Request failed after multiple attempts. Please try again later.', 'error');
            this.retryAttempts.delete(retryKey);
        }
    }

    /**
     * Handle network errors
     */
    async handleNetworkError(error, context) {
        if (!navigator.onLine) {
            this.showToast('You are currently offline. Please check your internet connection.', 'error');
        } else {
            this.showToast('Network error. Please check your connection and try again.', 'error');
        }
    }

    /**
     * Handle authentication errors
     */
    handleAuthError(error, context) {
        this.clearAuth();
        this.showToast('Your session has expired. Please sign in again.', 'error');
        
        // Redirect to login after a short delay
        setTimeout(() => {
            window.location.href = '/login.html';
        }, 2000);
    }

    /**
     * Handle session errors
     */
    handleSessionError(error, context) {
        this.handleSessionExpired();
    }

    /**
     * Handle validation errors
     */
    handleValidationError(error, context) {
        if (error.errors && typeof error.errors === 'object') {
            // Display validation errors
            Object.keys(error.errors).forEach(field => {
                const fieldElement = document.querySelector(`[name="${field}"]`);
                if (fieldElement) {
                    this.showFieldError(fieldElement, error.errors[field]);
                }
            });
        } else {
            this.showToast(error.message || 'Please check your input and try again.', 'error');
        }
    }

    /**
     * Handle generic errors
     */
    handleGenericError(error, context) {
        this.showToast('An unexpected error occurred. Please refresh the page.', 'error');
    }

    /**
     * Handle maximum errors reached
     */
    handleMaxErrorsReached() {
        this.showToast('Too many errors occurred. Please refresh the page.', 'error');
        
        // Reset error count after timeout
        setTimeout(() => {
            this.errorCount = 0;
        }, this.errorTimeout);
    }

    /**
     * Check if error should be retried
     */
    shouldRetry(error) {
        // Don't retry client errors (4xx) except 408, 429
        if (error.status >= 400 && error.status < 500) {
            return error.status === 408 || error.status === 429;
        }
        
        // Retry server errors (5xx) and network errors
        return error.status >= 500 || !error.status;
    }

    /**
     * Retry a failed request
     */
    async retryRequest(url, method, context) {
        const options = {
            method,
            headers: context.headers || {},
            body: context.body
        };

        // Add auth header if available
        const auth = this.getAuth();
        if (auth && auth.token) {
            options.headers['Authorization'] = `Bearer ${auth.token}`;
        }

        const response = await fetch(url, options);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        return response;
    }

    /**
     * Session management methods
     */
    checkSession() {
        const auth = this.getAuth();
        
        if (!auth) {
            return;
        }

        // Check if session is expired
        if (auth.expiresAt && new Date(auth.expiresAt) < new Date()) {
            this.handleSessionExpired();
            return;
        }

        // Update last activity
        this.updateLastActivity();
    }

    startSessionTimer() {
        this.sessionTimer = setInterval(() => {
            const now = Date.now();
            const timeSinceActivity = now - this.lastActivity;

            if (timeSinceActivity > this.sessionTimeout) {
                this.handleSessionExpired();
            }
        }, 60000); // Check every minute
    }

    updateLastActivity() {
        this.lastActivity = Date.now();
    }

    handleSessionExpired() {
        this.clearAuth();
        this.showToast('Your session has expired due to inactivity. Please sign in again.', 'warning');
        
        // Redirect to login
        setTimeout(() => {
            window.location.href = '/login.html';
        }, 3000);
    }

    /**
     * Network status handling
     */
    handleNetworkStatusChange(isOnline) {
        this.isOnline = isOnline;
        
        if (isOnline) {
            this.showToast('Connection restored', 'success');
            // Retry any pending requests
            this.retryPendingRequests();
        } else {
            this.showToast('Connection lost. Some features may not work.', 'warning');
        }
    }

    startConnectivityMonitoring() {
        setInterval(async () => {
            if (navigator.onLine) {
                try {
                    const response = await fetch('https://241runners-api-v2.azurewebsites.net/api/v1/auth/health', { 
                        method: 'HEAD',
                        cache: 'no-cache'
                    });
                    
                    if (!response.ok) {
                        this.handleError('Connectivity Error', new Error('API not responding'));
                    }
                } catch (error) {
                    this.handleError('Connectivity Error', error);
                }
            }
        }, 30000); // Check every 30 seconds
    }

    retryPendingRequests() {
        // Implementation would depend on your request queue system
        console.log('Retrying pending requests...');
    }

    /**
     * Toast notification system
     */
    showToast(message, type = 'info', duration = 5000) {
        const container = document.getElementById('toast-container');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <span class="toast-message">${message}</span>
                <button class="toast-close" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;
        
        toast.style.cssText = `
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            padding: 16px;
            margin-bottom: 10px;
            min-width: 300px;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            border-left: 4px solid ${this.getToastColor(type)};
        `;
        
        container.appendChild(toast);
        
        setTimeout(() => toast.style.transform = 'translateX(0)', 100);
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => toast.remove(), 300);
        }, duration);
    }

    getToastColor(type) {
        switch (type) {
            case 'success': return '#28a745';
            case 'error': return '#dc3545';
            case 'warning': return '#ffc107';
            case 'info': return '#17a2b8';
            default: return '#6c757d';
        }
    }

    /**
     * Field error display
     */
    showFieldError(field, message) {
        this.clearFieldError(field);
        
        const errorElement = document.createElement('div');
        errorElement.className = 'field-error';
        errorElement.textContent = message;
        errorElement.style.cssText = `
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        `;
        
        field.parentNode.insertBefore(errorElement, field.nextSibling);
        field.classList.add('error');
    }

    clearFieldError(field) {
        const errorElement = field.parentNode.querySelector('.field-error');
        if (errorElement) {
            errorElement.remove();
        }
        field.classList.remove('error');
    }

    /**
     * Utility methods
     */
    generateErrorId() {
        return `error_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    storeError(errorInfo) {
        try {
            const errors = JSON.parse(localStorage.getItem('app_errors') || '[]');
            errors.push(errorInfo);
            
            // Keep only last 50 errors
            if (errors.length > 50) {
                errors.splice(0, errors.length - 50);
            }
            
            localStorage.setItem('app_errors', JSON.stringify(errors));
        } catch (error) {
            console.error('Failed to store error:', error);
        }
    }

    getCurrentUserId() {
        const auth = this.getAuth();
        return auth ? auth.user?.id : null;
    }

    getAuth() {
        try {
            const authData = localStorage.getItem('ra_auth');
            return authData ? JSON.parse(authData) : null;
        } catch (error) {
            return null;
        }
    }

    clearAuth() {
        localStorage.removeItem('ra_auth');
        localStorage.removeItem('google_access_token');
        localStorage.removeItem('userToken');
    }

    /**
     * Get error report for debugging
     */
    getErrorReport() {
        const errors = JSON.parse(localStorage.getItem('app_errors') || '[]');
        return {
            errors,
            errorCount: this.errorCount,
            lastActivity: new Date(this.lastActivity).toISOString(),
            sessionTimeout: this.sessionTimeout,
            isOnline: navigator.onLine,
            userAgent: navigator.userAgent,
            url: window.location.href
        };
    }

    /**
     * Clear all stored errors
     */
    clearErrorHistory() {
        localStorage.removeItem('app_errors');
        this.errorCount = 0;
        this.retryAttempts.clear();
    }

    /**
     * Cleanup resources
     */
    destroy() {
        if (this.sessionTimer) {
            clearInterval(this.sessionTimer);
            this.sessionTimer = null;
        }
    }
}

// Initialize error handler when DOM is loaded
let errorHandler;
document.addEventListener('DOMContentLoaded', () => {
    errorHandler = new ErrorHandler();
    
    // Make it globally available
    window.errorHandler = errorHandler;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ErrorHandler;
}