// Shared utility functions for 241 Runners Awareness
// This file contains common functions used across multiple HTML pages

/**
 * Show error message for a specific element
 * @param {string} elementId - The ID of the element to show error for
 * @param {string} message - The error message to display
 */
function showError(elementId, message) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.style.display = 'block';
        element.className = 'error-text';
    }
}

/**
 * Clear all error messages
 */
function clearAllErrors() {
    const errorElements = document.querySelectorAll('.error-text, .status-error');
    errorElements.forEach(element => {
        element.textContent = '';
        element.style.display = 'none';
    });
}

/**
 * Show success message
 * @param {string} elementId - The ID of the element to show success for
 * @param {string} message - The success message to display
 */
function showSuccess(elementId, message) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.style.display = 'block';
        element.className = 'success-text';
    }
}

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} - True if valid, false otherwise
 */
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Validate phone number format
 * @param {string} phone - Phone number to validate
 * @returns {boolean} - True if valid, false otherwise
 */
function isValidPhone(phone) {
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    return phoneRegex.test(phone);
}

/**
 * Validate name format (letters, spaces, hyphens, apostrophes)
 * @param {string} name - Name to validate
 * @returns {boolean} - True if valid, false otherwise
 */
function isValidName(name) {
    const nameRegex = /^[a-zA-Z\s\-']+$/;
    return nameRegex.test(name);
}

/**
 * Validate zip code format
 * @param {string} zipCode - Zip code to validate
 * @returns {boolean} - True if valid, false otherwise
 */
function isValidZipCode(zipCode) {
    const zipRegex = /^[\d\-]+$/;
    return zipRegex.test(zipCode);
}

/**
 * Show toast notification
 * @param {string} message - Message to display
 * @param {string} type - Type of toast (success, error, info, warning)
 * @param {number} duration - Duration in milliseconds (default: 5000)
 */
function showToast(message, type = 'info', duration = 5000) {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        // Create toast container if it doesn't exist
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }
    
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    
    toastContainer.appendChild(toast);
    
    // Remove toast after specified duration
    setTimeout(() => {
        toast.remove();
    }, duration);
}

/**
 * Set loading state for a button
 * @param {string} buttonId - The ID of the button
 * @param {boolean} loading - Whether to show loading state
 * @param {string} loadingText - Text to show while loading
 * @param {string} originalText - Original button text
 */
function setButtonLoading(buttonId, loading, loadingText = 'Loading...', originalText = 'Submit') {
    const button = document.getElementById(buttonId);
    if (!button) return;
    
    if (loading) {
        button.disabled = true;
        button.dataset.originalText = button.textContent;
        button.textContent = loadingText;
    } else {
        button.disabled = false;
        button.textContent = button.dataset.originalText || originalText;
    }
}

/**
 * Format date for display
 * @param {string|Date} date - Date to format
 * @returns {string} - Formatted date string
 */
function formatDate(date) {
    if (!date) return 'N/A';
    
    const d = new Date(date);
    if (isNaN(d.getTime())) return 'Invalid Date';
    
    return d.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

/**
 * Sanitize input to prevent XSS
 * @param {string} input - Input to sanitize
 * @returns {string} - Sanitized input
 */
function sanitizeInput(input) {
    if (typeof input !== 'string') return input;
    
    const div = document.createElement('div');
    div.textContent = input;
    return div.innerHTML;
}

/**
 * Debounce function to limit function calls
 * @param {Function} func - Function to debounce
 * @param {number} wait - Wait time in milliseconds
 * @returns {Function} - Debounced function
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Check if user is authenticated
 * @returns {boolean} - True if authenticated, false otherwise
 */
function isAuthenticated() {
    const token = localStorage.getItem('ra_token') || localStorage.getItem('ra_admin_token');
    return !!token;
}

/**
 * Get authentication headers for API calls
 * @returns {Object} - Headers object with authorization
 */
function getAuthHeaders() {
    const token = localStorage.getItem('ra_token') || localStorage.getItem('ra_admin_token');
    return {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };
}

/**
 * Logout user and clear all data
 */
function logout() {
    // Clear all authentication data
    localStorage.removeItem('ra_token');
    localStorage.removeItem('ra_admin_token');
    localStorage.removeItem('ra_user');
    localStorage.removeItem('ra_admin_user');
    localStorage.removeItem('ra_role');
    
    // Redirect to login page
    window.location.href = '/login.html';
}

// Export functions for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        showError,
        clearAllErrors,
        showSuccess,
        isValidEmail,
        isValidPhone,
        isValidName,
        isValidZipCode,
        showToast,
        setButtonLoading,
        formatDate,
        sanitizeInput,
        debounce,
        isAuthenticated,
        getAuthHeaders,
        logout
    };
} 