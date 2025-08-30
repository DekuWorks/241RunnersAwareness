/**
 * ============================================
 * 241 RUNNERS AWARENESS - API UTILITIES
 * ============================================
 * 
 * Universal fetch wrapper with enhanced error handling, logging, and timeout
 */

// API Configuration
const API_TIMEOUT = 10000; // 10 seconds
const API_RETRY_ATTEMPTS = 2;

/**
 * Universal fetch wrapper with enhanced error handling
 * @param {string} endpoint - API endpoint path
 * @param {Object} options - Fetch options
 * @param {number} timeout - Request timeout in milliseconds
 * @returns {Promise} Parsed JSON response
 */
async function apiFetch(endpoint, options = {}, timeout = API_TIMEOUT) {
    const startTime = Date.now();
    const requestId = generateRequestId();
    
    // Ensure we have the API base URL
    if (!window.APP_CONFIG?.API_BASE_URL) {
        throw new Error('API configuration not loaded');
    }
    
    const url = endpoint.startsWith('http') ? endpoint : `${window.APP_CONFIG.API_BASE_URL}${endpoint}`;
    
    // Default headers
    const headers = {
        'Content-Type': 'application/json',
        'X-Request-ID': requestId,
        'X-Client': '241RA-Web/1.0',
        ...options.headers
    };
    
    // Add auth token if available
    const auth = getAuth();
    if (auth?.token) {
        headers['Authorization'] = `Bearer ${auth.token}`;
    }
    
    // Create abort controller for timeout
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeout);
    
    try {
        console.group(`🌐 API Request [${requestId}]`);
        console.log('URL:', url);
        console.log('Method:', options.method || 'GET');
        console.log('Headers:', headers);
        if (options.body) {
            console.log('Body:', options.body);
        }
        
        const response = await fetch(url, {
            ...options,
            headers,
            signal: controller.signal
        });
        
        const responseTime = Date.now() - startTime;
        console.log('Response Status:', response.status);
        console.log('Response Time:', `${responseTime}ms`);
        
        // Clear timeout
        clearTimeout(timeoutId);
        
        // Handle different response types
        const contentType = response.headers.get('content-type');
        let data;
        
        if (contentType && contentType.includes('application/json')) {
            data = await response.json();
            console.log('Response Data:', data);
        } else {
            data = await response.text();
            console.log('Response Text:', data);
        }
        
        // Handle error responses
        if (!response.ok) {
            const error = new Error(data.message || `HTTP ${response.status}: ${response.statusText}`);
            error.status = response.status;
            error.statusText = response.statusText;
            error.data = data;
            error.requestId = requestId;
            error.responseTime = responseTime;
            
            // Handle specific error cases
            if (response.status === 401) {
                clearAuth();
                window.location.href = '/login.html';
                error.message = 'Session expired. Please sign in again.';
            } else if (response.status === 403) {
                error.message = 'Access denied. You do not have permission to perform this action.';
            } else if (response.status === 404) {
                error.message = 'Resource not found.';
            } else if (response.status >= 500) {
                error.message = 'Server error. Please try again later.';
            }
            
            throw error;
        }
        
        console.groupEnd();
        return data;
        
    } catch (error) {
        clearTimeout(timeoutId);
        const responseTime = Date.now() - startTime;
        
        // Handle specific error types
        if (error.name === 'AbortError') {
            error.message = 'Request timed out. Please check your connection and try again.';
        } else if (error.name === 'TypeError' && error.message.includes('fetch')) {
            error.message = 'Network error. Please check your connection.';
        }
        
        console.error('❌ API Error:', {
            requestId,
            url,
            error: error.message,
            status: error.status,
            responseTime: `${responseTime}ms`
        });
        console.groupEnd();
        
        throw error;
    }
}

/**
 * Retry wrapper for API calls
 * @param {Function} apiCall - API call function
 * @param {number} maxAttempts - Maximum retry attempts
 * @returns {Promise} API response
 */
async function apiRetry(apiCall, maxAttempts = API_RETRY_ATTEMPTS) {
    let lastError;
    
    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        try {
            return await apiCall();
        } catch (error) {
            lastError = error;
            
            // Don't retry on client errors (4xx)
            if (error.status && error.status >= 400 && error.status < 500) {
                throw error;
            }
            
            // Don't retry on auth errors
            if (error.status === 401) {
                throw error;
            }
            
            if (attempt < maxAttempts) {
                console.warn(`🔄 Retrying API call (attempt ${attempt + 1}/${maxAttempts})`);
                await new Promise(resolve => setTimeout(resolve, 1000 * attempt)); // Exponential backoff
            }
        }
    }
    
    throw lastError;
}

/**
 * Generate unique request ID
 * @returns {string} Request ID
 */
function generateRequestId() {
    return `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
}

/**
 * Get authentication data from localStorage
 * @returns {Object|null} Auth data
 */
function getAuth() {
    try {
        const authData = localStorage.getItem('ra_auth');
        return authData ? JSON.parse(authData) : null;
    } catch (error) {
        console.error('Error parsing auth data:', error);
        return null;
    }
}

/**
 * Clear authentication data
 */
function clearAuth() {
    localStorage.removeItem('ra_auth');
    localStorage.removeItem('ra_user');
}

/**
 * Show user-friendly error message
 * @param {Error} error - Error object
 * @param {string} context - Context where error occurred
 */
function showApiError(error, context = 'API call') {
    const message = error.message || 'An unexpected error occurred';
    
    // Log error for debugging
    console.error(`❌ ${context} failed:`, error);
    
    // Show user-friendly message
    if (typeof showToast === 'function') {
        showError(message, context);
    } else if (typeof alert === 'function') {
        alert(`${context}: ${message}`);
    }
}

/**
 * API helper functions for common operations
 */
const apiHelpers = {
    // GET request
    get: (endpoint, options = {}) => apiFetch(endpoint, { ...options, method: 'GET' }),
    
    // POST request
    post: (endpoint, data, options = {}) => apiFetch(endpoint, {
        ...options,
        method: 'POST',
        body: JSON.stringify(data)
    }),
    
    // PUT request
    put: (endpoint, data, options = {}) => apiFetch(endpoint, {
        ...options,
        method: 'PUT',
        body: JSON.stringify(data)
    }),
    
    // DELETE request
    delete: (endpoint, options = {}) => apiFetch(endpoint, { ...options, method: 'DELETE' }),
    
    // GET with retry
    getWithRetry: (endpoint, options = {}) => apiRetry(() => apiFetch(endpoint, { ...options, method: 'GET' })),
    
    // POST with retry
    postWithRetry: (endpoint, data, options = {}) => apiRetry(() => apiFetch(endpoint, {
        ...options,
        method: 'POST',
        body: JSON.stringify(data)
    }))
};

// Export for use in other scripts
window.apiFetch = apiFetch;
window.apiRetry = apiRetry;
window.showApiError = showApiError;
window.apiHelpers = apiHelpers; 