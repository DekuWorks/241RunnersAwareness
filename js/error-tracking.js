/**
 * ============================================
 * 241 RUNNERS AWARENESS - ERROR TRACKING
 * ============================================
 * 
 * P2 Implementation: Error telemetry and client error capture
 * Simple error tracking for frontend issues
 */

// ===== CONFIGURATION =====
const ERROR_CONFIG = {
    apiEndpoint: 'https://241runners-api.azurewebsites.net/api/admin/errors',
    maxRetries: 3,
    batchSize: 10,
    flushInterval: 30000, // 30 seconds
    maxStorageSize: 100 // Maximum errors to store locally
};

// ===== STATE MANAGEMENT =====
let errorQueue = [];
let isOnline = true;

// ===== ERROR TRACKING =====

/**
 * Track an error
 * @param {Error|string} error - Error object or error message
 * @param {Object} context - Additional context information
 * @param {string} severity - Error severity (low, medium, high, critical)
 */
function trackError(error, context = {}, severity = 'medium') {
    try {
        const errorData = {
            id: generateErrorId(),
            timestamp: new Date().toISOString(),
            message: error instanceof Error ? error.message : error,
            stack: error instanceof Error ? error.stack : null,
            url: window.location.href,
            userAgent: navigator.userAgent,
            severity: severity,
            context: {
                ...context,
                userId: getCurrentUserId(),
                sessionId: getSessionId(),
                pageLoadTime: getPageLoadTime(),
                memoryUsage: getMemoryUsage()
            }
        };

        // Add to queue
        errorQueue.push(errorData);

        // Trim queue if too large
        if (errorQueue.length > ERROR_CONFIG.maxStorageSize) {
            errorQueue = errorQueue.slice(-ERROR_CONFIG.maxStorageSize);
        }

        // Try to send immediately if online
        if (isOnline) {
            sendErrorToServer(errorData);
        }

        // Store in localStorage as backup
        storeErrorLocally(errorData);

        console.error('Error tracked:', errorData);

    } catch (trackingError) {
        console.error('Failed to track error:', trackingError);
    }
}

/**
 * Generate unique error ID
 * @returns {string} Unique error ID
 */
function generateErrorId() {
    return `err_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
}

/**
 * Get current user ID
 * @returns {string|null} User ID or null
 */
function getCurrentUserId() {
    try {
        const userData = localStorage.getItem('ra_admin_user') || localStorage.getItem('user');
        if (userData) {
            const user = JSON.parse(userData);
            return user.id || user.email || null;
        }
    } catch (e) {
        // Ignore parsing errors
    }
    return null;
}

/**
 * Get session ID
 * @returns {string} Session ID
 */
function getSessionId() {
    let sessionId = sessionStorage.getItem('sessionId');
    if (!sessionId) {
        sessionId = `sess_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
        sessionStorage.setItem('sessionId', sessionId);
    }
    return sessionId;
}

/**
 * Get page load time
 * @returns {number|null} Page load time in milliseconds
 */
function getPageLoadTime() {
    if (performance && performance.timing) {
        return performance.timing.loadEventEnd - performance.timing.navigationStart;
    }
    return null;
}

/**
 * Get memory usage (if available)
 * @returns {Object|null} Memory usage information
 */
function getMemoryUsage() {
    if (performance && performance.memory) {
        return {
            used: Math.round(performance.memory.usedJSHeapSize / 1024 / 1024),
            total: Math.round(performance.memory.totalJSHeapSize / 1024 / 1024),
            limit: Math.round(performance.memory.jsHeapSizeLimit / 1024 / 1024)
        };
    }
    return null;
}

// ===== SERVER COMMUNICATION =====

/**
 * Send error to server
 * @param {Object} errorData - Error data to send
 */
async function sendErrorToServer(errorData) {
    try {
        const response = await fetch(ERROR_CONFIG.apiEndpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Client': '241RA-ErrorTracker/1.0'
            },
            body: JSON.stringify(errorData)
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        // Remove from local storage if successfully sent
        removeErrorLocally(errorData.id);

    } catch (error) {
        console.warn('Failed to send error to server:', error);
        // Error will remain in queue for retry
    }
}

/**
 * Flush error queue
 */
async function flushErrorQueue() {
    if (errorQueue.length === 0 || !isOnline) {
        return;
    }

    const errorsToSend = errorQueue.splice(0, ERROR_CONFIG.batchSize);
    
    for (const errorData of errorsToSend) {
        await sendErrorToServer(errorData);
    }
}

// ===== LOCAL STORAGE =====

/**
 * Store error locally
 * @param {Object} errorData - Error data to store
 */
function storeErrorLocally(errorData) {
    try {
        const storedErrors = getStoredErrors();
        storedErrors.push(errorData);
        
        // Keep only recent errors
        if (storedErrors.length > ERROR_CONFIG.maxStorageSize) {
            storedErrors.splice(0, storedErrors.length - ERROR_CONFIG.maxStorageSize);
        }
        
        localStorage.setItem('errorQueue', JSON.stringify(storedErrors));
    } catch (error) {
        console.warn('Failed to store error locally:', error);
    }
}

/**
 * Get stored errors
 * @returns {Array} Array of stored errors
 */
function getStoredErrors() {
    try {
        const stored = localStorage.getItem('errorQueue');
        return stored ? JSON.parse(stored) : [];
    } catch (error) {
        return [];
    }
}

/**
 * Remove error from local storage
 * @param {string} errorId - Error ID to remove
 */
function removeErrorLocally(errorId) {
    try {
        const storedErrors = getStoredErrors();
        const filteredErrors = storedErrors.filter(error => error.id !== errorId);
        localStorage.setItem('errorQueue', JSON.stringify(filteredErrors));
    } catch (error) {
        console.warn('Failed to remove error from local storage:', error);
    }
}

// ===== GLOBAL ERROR HANDLERS =====

/**
 * Handle uncaught errors
 * @param {Event} event - Error event
 */
function handleUncaughtError(event) {
    trackError(event.error || event.message || 'Unknown error', {
        type: 'uncaught',
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno
    }, 'high');
}

/**
 * Handle unhandled promise rejections
 * @param {Event} event - Promise rejection event
 */
function handleUnhandledRejection(event) {
    trackError(event.reason || 'Unhandled promise rejection', {
        type: 'unhandledRejection',
        promise: event.promise
    }, 'medium');
}

/**
 * Handle fetch errors
 * @param {Error} error - Fetch error
 * @param {string} url - Request URL
 * @param {Object} options - Request options
 */
function handleFetchError(error, url, options = {}) {
    trackError(error, {
        type: 'fetch',
        url: url,
        method: options.method || 'GET',
        status: error.status || null
    }, 'medium');
}

// ===== NETWORK STATUS =====

/**
 * Handle online status change
 */
function handleOnlineStatusChange() {
    isOnline = navigator.onLine;
    
    if (isOnline) {
        // Try to flush queued errors when coming back online
        flushErrorQueue();
    }
}

// ===== INITIALIZATION =====

/**
 * Initialize error tracking
 */
function initializeErrorTracking() {
    console.log('🔍 Initializing error tracking...');
    
    // Set up global error handlers
    window.addEventListener('error', handleUncaughtError);
    window.addEventListener('unhandledrejection', handleUnhandledRejection);
    window.addEventListener('online', handleOnlineStatusChange);
    window.addEventListener('offline', handleOnlineStatusChange);
    
    // Set up periodic flush
    setInterval(flushErrorQueue, ERROR_CONFIG.flushInterval);
    
    // Load and send stored errors
    const storedErrors = getStoredErrors();
    if (storedErrors.length > 0) {
        console.log(`📦 Found ${storedErrors.length} stored errors, adding to queue`);
        errorQueue.push(...storedErrors);
        flushErrorQueue();
    }
    
    console.log('✅ Error tracking initialized');
}

// ===== EXPORTS =====

// Make functions available globally
window.ErrorTracker = {
    track: trackError,
    flush: flushErrorQueue,
    getQueue: () => [...errorQueue],
    getStored: getStoredErrors,
    clear: () => {
        errorQueue = [];
        localStorage.removeItem('errorQueue');
    }
};

// Auto-initialize
document.addEventListener('DOMContentLoaded', initializeErrorTracking);

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        trackError,
        initializeErrorTracking,
        ErrorTracker: window.ErrorTracker
    };
}
