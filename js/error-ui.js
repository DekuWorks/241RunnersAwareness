/**
 * ============================================
 * 241 RUNNERS AWARENESS - ERROR UI COMPONENTS
 * ============================================
 * 
 * Centralized error handling and UI components for better user experience
 */

// Error UI Configuration
const ERROR_UI_CONFIG = {
    retryAttempts: 3,
    retryDelay: 2000,
    showRetryButton: true,
    showContactSupport: true
};

/**
 * Show error message with retry functionality
 * @param {string} message - Error message to display
 * @param {Object} options - Error display options
 */
function showErrorUI(message, options = {}) {
    const {
        title = 'Error',
        retryFunction = null,
        showRetry = ERROR_UI_CONFIG.showRetryButton,
        showContact = ERROR_UI_CONFIG.showContactSupport,
        containerId = 'errorContainer'
    } = options;

    const container = document.getElementById(containerId) || createErrorContainer();
    
    const errorHTML = `
        <div class="error-ui" role="alert">
            <div class="error-header">
                <div class="error-icon">⚠️</div>
                <h3 class="error-title">${title}</h3>
            </div>
            <div class="error-message">${message}</div>
            <div class="error-actions">
                ${showRetry && retryFunction ? `
                    <button class="btn btn-retry" onclick="retryAction('${containerId}')">
                        <span class="retry-icon">🔄</span>
                        Try Again
                    </button>
                ` : ''}
                ${showContact ? `
                    <button class="btn btn-contact" onclick="contactSupport()">
                        <span class="contact-icon">📧</span>
                        Contact Support
                    </button>
                ` : ''}
                <button class="btn btn-dismiss" onclick="dismissError('${containerId}')">
                    <span class="dismiss-icon">✕</span>
                    Dismiss
                </button>
            </div>
        </div>
    `;
    
    container.innerHTML = errorHTML;
    container.style.display = 'block';
    
    // Store retry function for later use
    if (retryFunction) {
        window.retryFunctions = window.retryFunctions || {};
        window.retryFunctions[containerId] = retryFunction;
    }
    
    // Add error tracking
    trackError(message, options);
}

/**
 * Show loading state with progress indicator
 * @param {string} message - Loading message
 * @param {string} containerId - Container ID
 */
function showLoadingUI(message = 'Loading...', containerId = 'loadingContainer') {
    const container = document.getElementById(containerId) || createLoadingContainer();
    
    const loadingHTML = `
        <div class="loading-ui">
            <div class="loading-spinner"></div>
            <div class="loading-message">${message}</div>
            <div class="loading-progress">
                <div class="progress-bar">
                    <div class="progress-fill"></div>
                </div>
            </div>
        </div>
    `;
    
    container.innerHTML = loadingHTML;
    container.style.display = 'block';
}

/**
 * Hide loading state
 * @param {string} containerId - Container ID
 */
function hideLoadingUI(containerId = 'loadingContainer') {
    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = 'none';
    }
}

/**
 * Show success message
 * @param {string} message - Success message
 * @param {number} duration - Auto-hide duration in ms
 */
function showSuccessUI(message, duration = 3000) {
    const container = document.getElementById('successContainer') || createSuccessContainer();
    
    const successHTML = `
        <div class="success-ui" role="status">
            <div class="success-icon">✅</div>
            <div class="success-message">${message}</div>
        </div>
    `;
    
    container.innerHTML = successHTML;
    container.style.display = 'block';
    
    // Auto-hide after duration
    if (duration > 0) {
        setTimeout(() => {
            container.style.display = 'none';
        }, duration);
    }
}

/**
 * Create error container if it doesn't exist
 * @returns {HTMLElement} Error container element
 */
function createErrorContainer() {
    const container = document.createElement('div');
    container.id = 'errorContainer';
    container.className = 'error-container';
    document.body.appendChild(container);
    return container;
}

/**
 * Create loading container if it doesn't exist
 * @returns {HTMLElement} Loading container element
 */
function createLoadingContainer() {
    const container = document.createElement('div');
    container.id = 'loadingContainer';
    container.className = 'loading-container';
    document.body.appendChild(container);
    return container;
}

/**
 * Create success container if it doesn't exist
 * @returns {HTMLElement} Success container element
 */
function createSuccessContainer() {
    const container = document.createElement('div');
    container.id = 'successContainer';
    container.className = 'success-container';
    document.body.appendChild(container);
    return container;
}

/**
 * Retry action function
 * @param {string} containerId - Container ID
 */
function retryAction(containerId) {
    const retryFunction = window.retryFunctions?.[containerId];
    if (retryFunction) {
        hideErrorUI(containerId);
        retryFunction();
    }
}

/**
 * Dismiss error
 * @param {string} containerId - Container ID
 */
function dismissError(containerId) {
    hideErrorUI(containerId);
}

/**
 * Hide error UI
 * @param {string} containerId - Container ID
 */
function hideErrorUI(containerId = 'errorContainer') {
    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = 'none';
        container.innerHTML = '';
    }
}

/**
 * Contact support function
 */
function contactSupport() {
    const supportEmail = 'support@241runnersawareness.org';
    const subject = 'Support Request - 241 Runners Awareness';
    const body = `Please describe the issue you're experiencing:\n\n`;
    
    const mailtoLink = `mailto:${supportEmail}?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
    window.open(mailtoLink);
}

/**
 * Track error for analytics
 * @param {string} message - Error message
 * @param {Object} options - Error options
 */
function trackError(message, options = {}) {
    // Basic error tracking
    console.error('Error tracked:', {
        message,
        timestamp: new Date().toISOString(),
        url: window.location.href,
        userAgent: navigator.userAgent,
        ...options
    });
    
    // Send to analytics if available
    if (window.gtag) {
        window.gtag('event', 'error', {
            error_message: message,
            error_category: options.category || 'general',
            error_action: options.action || 'unknown'
        });
    }
}

/**
 * Handle API errors with retry logic
 * @param {Error} error - Error object
 * @param {Function} retryFunction - Function to retry
 * @param {Object} options - Error handling options
 */
function handleAPIError(error, retryFunction, options = {}) {
    const {
        maxRetries = ERROR_UI_CONFIG.retryAttempts,
        retryDelay = ERROR_UI_CONFIG.retryDelay,
        showUI = true
    } = options;
    
    let errorMessage = 'An error occurred while loading data.';
    
    if (error.message.includes('fetch')) {
        errorMessage = 'Network error: Unable to connect to the server. Please check your internet connection.';
    } else if (error.message.includes('401')) {
        errorMessage = 'Authentication required. Please sign in again.';
    } else if (error.message.includes('403')) {
        errorMessage = 'Access denied. You do not have permission to perform this action.';
    } else if (error.message.includes('404')) {
        errorMessage = 'The requested resource was not found.';
    } else if (error.message.includes('500')) {
        errorMessage = 'Server error. Please try again later.';
    } else if (error.message) {
        errorMessage = error.message;
    }
    
    if (showUI) {
        showErrorUI(errorMessage, {
            title: 'API Error',
            retryFunction,
            showRetry: !!retryFunction
        });
    }
    
    return errorMessage;
}

// Add CSS styles for error UI
const errorUIStyles = `
    <style>
        .error-container, .loading-container, .success-container {
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 10000;
            max-width: 400px;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        }
        
        .error-ui, .loading-ui, .success-ui {
            background: white;
            border-radius: 8px;
            padding: 20px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
            border-left: 4px solid;
            animation: slideIn 0.3s ease;
        }
        
        .error-ui {
            border-left-color: #ef4444;
        }
        
        .success-ui {
            border-left-color: #10b981;
        }
        
        .loading-ui {
            border-left-color: #3b82f6;
        }
        
        .error-header {
            display: flex;
            align-items: center;
            margin-bottom: 10px;
        }
        
        .error-icon, .success-icon {
            font-size: 20px;
            margin-right: 10px;
        }
        
        .error-title {
            margin: 0;
            color: #dc2626;
            font-size: 16px;
            font-weight: 600;
        }
        
        .error-message, .success-message, .loading-message {
            color: #374151;
            margin-bottom: 15px;
            line-height: 1.5;
        }
        
        .error-actions {
            display: flex;
            gap: 10px;
            flex-wrap: wrap;
        }
        
        .btn {
            padding: 8px 16px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 5px;
            transition: all 0.2s ease;
        }
        
        .btn-retry {
            background: #3b82f6;
            color: white;
        }
        
        .btn-retry:hover {
            background: #2563eb;
        }
        
        .btn-contact {
            background: #6b7280;
            color: white;
        }
        
        .btn-contact:hover {
            background: #4b5563;
        }
        
        .btn-dismiss {
            background: #e5e7eb;
            color: #374151;
        }
        
        .btn-dismiss:hover {
            background: #d1d5db;
        }
        
        .loading-spinner {
            width: 20px;
            height: 20px;
            border: 2px solid #e5e7eb;
            border-top: 2px solid #3b82f6;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 10px;
        }
        
        .loading-progress {
            margin-top: 10px;
        }
        
        .progress-bar {
            width: 100%;
            height: 4px;
            background: #e5e7eb;
            border-radius: 2px;
            overflow: hidden;
        }
        
        .progress-fill {
            height: 100%;
            background: #3b82f6;
            border-radius: 2px;
            animation: progress 2s ease-in-out infinite;
        }
        
        @keyframes slideIn {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        @keyframes progress {
            0% { width: 0%; }
            50% { width: 70%; }
            100% { width: 100%; }
        }
        
        .retry-icon, .contact-icon, .dismiss-icon {
            font-size: 12px;
        }
    </style>
`;

// Inject styles into the page
if (!document.getElementById('error-ui-styles')) {
    const styleElement = document.createElement('div');
    styleElement.id = 'error-ui-styles';
    styleElement.innerHTML = errorUIStyles;
    document.head.appendChild(styleElement);
}

// Export functions for use in other scripts
window.errorUI = {
    showErrorUI,
    showLoadingUI,
    hideLoadingUI,
    showSuccessUI,
    hideErrorUI,
    handleAPIError,
    trackError,
    contactSupport
};
