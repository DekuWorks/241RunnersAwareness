/**
 * 241 Runners Awareness - Utility Functions
 * 
 * This file contains common utility functions for the application.
 */

/**
 * ============================================
 * DEBOUNCING UTILITIES
 * ============================================
 */

/**
 * Debounce function to limit the rate of function calls
 * @param {Function} func - Function to debounce
 * @param {number} wait - Wait time in milliseconds
 * @param {boolean} immediate - Whether to call immediately
 * @returns {Function} - Debounced function
 */
export function debounce(func, wait, immediate = false) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            timeout = null;
            if (!immediate) func(...args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func(...args);
    };
}

/**
 * Throttle function to limit the rate of function calls
 * @param {Function} func - Function to throttle
 * @param {number} limit - Time limit in milliseconds
 * @returns {Function} - Throttled function
 */
export function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

/**
 * ============================================
 * FORM UTILITIES
 * ============================================
 */

/**
 * Serialize form data to object
 * @param {HTMLFormElement} form - Form element
 * @returns {Object} - Serialized form data
 */
export function serializeForm(form) {
    const formData = new FormData(form);
    const data = {};
    
    for (let [key, value] of formData.entries()) {
        if (data[key]) {
            if (Array.isArray(data[key])) {
                data[key].push(value);
            } else {
                data[key] = [data[key], value];
            }
        } else {
            data[key] = value;
        }
    }
    
    return data;
}

/**
 * Clear form fields
 * @param {HTMLFormElement} form - Form element
 */
export function clearForm(form) {
    const inputs = form.querySelectorAll('input, textarea, select');
    inputs.forEach(input => {
        if (input.type === 'checkbox' || input.type === 'radio') {
            input.checked = false;
        } else {
            input.value = '';
        }
    });
}

/**
 * Disable form inputs
 * @param {HTMLFormElement} form - Form element
 * @param {boolean} disabled - Whether to disable inputs
 */
export function toggleFormInputs(form, disabled) {
    const inputs = form.querySelectorAll('input, textarea, select, button');
    inputs.forEach(input => {
        input.disabled = disabled;
    });
}

/**
 * ============================================
 * STRING UTILITIES
 * ============================================
 */

/**
 * Capitalize first letter of string
 * @param {string} str - String to capitalize
 * @returns {string} - Capitalized string
 */
export function capitalize(str) {
    if (!str) return str;
    return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
}

/**
 * Format name (first name, last name)
 * @param {string} firstName - First name
 * @param {string} lastName - Last name
 * @returns {string} - Formatted name
 */
export function formatName(firstName, lastName) {
    return `${capitalize(firstName)} ${capitalize(lastName)}`;
}

/**
 * Truncate text to specified length
 * @param {string} text - Text to truncate
 * @param {number} length - Maximum length
 * @param {string} suffix - Suffix to add
 * @returns {string} - Truncated text
 */
export function truncateText(text, length = 100, suffix = '...') {
    if (!text || text.length <= length) return text;
    return text.substring(0, length) + suffix;
}

/**
 * ============================================
 * DATE UTILITIES
 * ============================================
 */

/**
 * Format date to readable string
 * @param {Date|string} date - Date to format
 * @param {string} format - Date format
 * @returns {string} - Formatted date
 */
export function formatDate(date, format = 'MM/DD/YYYY') {
    if (!date) return '';
    
    const d = new Date(date);
    if (isNaN(d.getTime())) return '';
    
    const options = {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    };
    
    return d.toLocaleDateString('en-US', options);
}

/**
 * Format date to relative time (e.g., "2 hours ago")
 * @param {Date|string} date - Date to format
 * @returns {string} - Relative time string
 */
export function formatRelativeTime(date) {
    if (!date) return '';
    
    const d = new Date(date);
    if (isNaN(d.getTime())) return '';
    
    const now = new Date();
    const diff = now - d;
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);
    
    if (seconds < 60) return 'Just now';
    if (minutes < 60) return `${minutes} minute${minutes !== 1 ? 's' : ''} ago`;
    if (hours < 24) return `${hours} hour${hours !== 1 ? 's' : ''} ago`;
    if (days < 7) return `${days} day${days !== 1 ? 's' : ''} ago`;
    
    return formatDate(date);
}

/**
 * ============================================
 * FILE UTILITIES
 * ============================================
 */

/**
 * Validate file type
 * @param {File} file - File to validate
 * @param {Array} allowedTypes - Allowed MIME types
 * @returns {boolean} - Whether file type is valid
 */
export function validateFileType(file, allowedTypes = ['image/jpeg', 'image/png', 'image/webp']) {
    return allowedTypes.includes(file.type);
}

/**
 * Validate file size
 * @param {File} file - File to validate
 * @param {number} maxSize - Maximum size in bytes
 * @returns {boolean} - Whether file size is valid
 */
export function validateFileSize(file, maxSize = 5 * 1024 * 1024) { // 5MB default
    return file.size <= maxSize;
}

/**
 * Compress image file
 * @param {File} file - Image file to compress
 * @param {number} quality - Compression quality (0-1)
 * @param {number} maxWidth - Maximum width
 * @param {number} maxHeight - Maximum height
 * @returns {Promise<Blob>} - Compressed image blob
 */
export function compressImage(file, quality = 0.8, maxWidth = 1920, maxHeight = 1080) {
    return new Promise((resolve, reject) => {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const img = new Image();
        
        img.onload = () => {
            // Calculate new dimensions
            let { width, height } = img;
            
            if (width > maxWidth || height > maxHeight) {
                const ratio = Math.min(maxWidth / width, maxHeight / height);
                width *= ratio;
                height *= ratio;
            }
            
            canvas.width = width;
            canvas.height = height;
            
            // Draw and compress
            ctx.drawImage(img, 0, 0, width, height);
            canvas.toBlob(resolve, file.type, quality);
        };
        
        img.onerror = reject;
        img.src = URL.createObjectURL(file);
    });
}

/**
 * ============================================
 * STORAGE UTILITIES
 * ============================================
 */

/**
 * Safe localStorage get
 * @param {string} key - Storage key
 * @param {*} defaultValue - Default value
 * @returns {*} - Stored value or default
 */
export function safeGetStorage(key, defaultValue = null) {
    try {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : defaultValue;
    } catch (error) {
        console.warn(`Failed to get storage item: ${key}`, error);
        return defaultValue;
    }
}

/**
 * Safe localStorage set
 * @param {string} key - Storage key
 * @param {*} value - Value to store
 * @returns {boolean} - Whether storage was successful
 */
export function safeSetStorage(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
        return true;
    } catch (error) {
        console.warn(`Failed to set storage item: ${key}`, error);
        return false;
    }
}

/**
 * Safe localStorage remove
 * @param {string} key - Storage key
 * @returns {boolean} - Whether removal was successful
 */
export function safeRemoveStorage(key) {
    try {
        localStorage.removeItem(key);
        return true;
    } catch (error) {
        console.warn(`Failed to remove storage item: ${key}`, error);
        return false;
    }
}

/**
 * ============================================
 * NETWORK UTILITIES
 * ============================================
 */

/**
 * Check if online
 * @returns {boolean} - Whether user is online
 */
export function isOnline() {
    return navigator.onLine;
}

/**
 * Make API request with error handling
 * @param {string} url - API endpoint
 * @param {Object} options - Fetch options
 * @returns {Promise<Object>} - API response
 */
export async function apiRequest(url, options = {}) {
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
        },
        timeout: 30000, // 30 seconds
    };
    
    const mergedOptions = { ...defaultOptions, ...options };
    
    // Add timeout
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), mergedOptions.timeout);
    mergedOptions.signal = controller.signal;
    
    try {
        const response = await fetch(url, mergedOptions);
        clearTimeout(timeoutId);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        return await response.json();
    } catch (error) {
        clearTimeout(timeoutId);
        throw error;
    }
}

/**
 * ============================================
 * DOM UTILITIES
 * ============================================
 */

/**
 * Wait for element to exist
 * @param {string} selector - CSS selector
 * @param {number} timeout - Timeout in milliseconds
 * @returns {Promise<Element>} - Element when found
 */
export function waitForElement(selector, timeout = 5000) {
    return new Promise((resolve, reject) => {
        const element = document.querySelector(selector);
        if (element) {
            resolve(element);
            return;
        }
        
        const observer = new MutationObserver(() => {
            const element = document.querySelector(selector);
            if (element) {
                observer.disconnect();
                resolve(element);
            }
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
        
        setTimeout(() => {
            observer.disconnect();
            reject(new Error(`Element ${selector} not found within ${timeout}ms`));
        }, timeout);
    });
}

/**
 * Add event listener with automatic cleanup
 * @param {Element} element - Element to add listener to
 * @param {string} event - Event type
 * @param {Function} handler - Event handler
 * @param {Object} options - Event options
 * @returns {Function} - Cleanup function
 */
export function addEventListenerWithCleanup(element, event, handler, options = {}) {
    element.addEventListener(event, handler, options);
    
    return () => {
        element.removeEventListener(event, handler, options);
    };
}

/**
 * ============================================
 * VALIDATION UTILITIES
 * ============================================
 */

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} - Whether email is valid
 */
export function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Validate phone number
 * @param {string} phone - Phone number to validate
 * @returns {boolean} - Whether phone is valid
 */
export function isValidPhone(phone) {
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    return phoneRegex.test(phone.replace(/\s/g, ''));
}

/**
 * ============================================
 * EXPORT ALL UTILITIES
 * ============================================
 */

// Make utilities globally available
if (typeof window !== 'undefined') {
    window.utils = {
        debounce,
        throttle,
        serializeForm,
        clearForm,
        toggleFormInputs,
        capitalize,
        formatName,
        truncateText,
        formatDate,
        formatRelativeTime,
        validateFileType,
        validateFileSize,
        compressImage,
        safeGetStorage,
        safeSetStorage,
        safeRemoveStorage,
        isOnline,
        apiRequest,
        waitForElement,
        addEventListenerWithCleanup,
        isValidEmail,
        isValidPhone
    };
}
