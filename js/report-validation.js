/**
 * 241 Runners Awareness - Report Form Validation
 * 
 * This file handles validation for all report submission forms
 * including quick reports, detailed reports, and updates.
 */

// Import validation schemas and utilities
import { validateFormData, runnerReportSchema, sanitizeInput } from './validation.js';
import { debounce, validateFileType, validateFileSize, compressImage } from './utils.js';

/**
 * ============================================
 * REPORT VALIDATION CLASS
 * ============================================
 */

class ReportValidator {
    constructor() {
        this.validationErrors = new Map();
        this.setupRealTimeValidation();
        this.setupFileValidation();
    }

    /**
     * Setup real-time validation for all forms
     */
    setupRealTimeValidation() {
        // Quick report form validation
        this.setupFormValidation('quickReportForm');
        
        // Detailed report form validation
        this.setupFormValidation('detailedReportForm');
        
        // Update form validation
        this.setupFormValidation('updateRunnerForm');
    }

    /**
     * Setup validation for a specific form
     * @param {string} formId - Form ID to validate
     */
    setupFormValidation(formId) {
        const form = document.getElementById(formId);
        if (!form) return;

        // Add real-time validation to all inputs
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            // Validate on blur
            input.addEventListener('blur', () => {
                this.validateField(input, formId);
            });

            // Clear errors on input
            input.addEventListener('input', () => {
                this.clearFieldError(input);
            });

            // Special handling for specific field types
            if (input.type === 'email') {
                input.addEventListener('input', debounce(() => {
                    this.validateEmailField(input);
                }, 300));
            }

            if (input.name === 'phone' || input.name === 'contactPhone') {
                input.addEventListener('input', debounce(() => {
                    this.validatePhoneField(input);
                }, 300));
            }
        });

        // Add form submission validation
        form.addEventListener('submit', (e) => {
            if (!this.validateForm(form, formId)) {
                e.preventDefault();
                return false;
            }
        });
    }

    /**
     * Setup file validation
     */
    setupFileValidation() {
        const fileInputs = document.querySelectorAll('input[type="file"]');
        fileInputs.forEach(input => {
            input.addEventListener('change', (e) => {
                this.validateFiles(e.target.files, e.target);
            });
        });

        // Setup drag and drop validation
        const dropZones = document.querySelectorAll('.file-drop-zone');
        dropZones.forEach(zone => {
            zone.addEventListener('dragover', (e) => {
                e.preventDefault();
                zone.classList.add('drag-over');
            });

            zone.addEventListener('dragleave', () => {
                zone.classList.remove('drag-over');
            });

            zone.addEventListener('drop', (e) => {
                e.preventDefault();
                zone.classList.remove('drag-over');
                this.validateFiles(e.dataTransfer.files, zone);
            });
        });
    }

    /**
     * Validate individual field
     * @param {HTMLElement} field - Field to validate
     * @param {string} formId - Form ID
     */
    validateField(field, formId) {
        const fieldName = field.name;
        if (!fieldName) return;

        let isValid = true;
        let errorMessage = '';

        // Basic required field validation
        if (field.hasAttribute('required') && !field.value.trim()) {
            isValid = false;
            errorMessage = `${this.getFieldLabel(field)} is required`;
        }

        // Email validation
        if (field.type === 'email' && field.value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(field.value)) {
                isValid = false;
                errorMessage = 'Please enter a valid email address';
            }
        }

        // Phone validation
        if ((field.name === 'phone' || field.name === 'contactPhone') && field.value) {
            const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
            if (!phoneRegex.test(field.value.replace(/\s/g, ''))) {
                isValid = false;
                errorMessage = 'Please enter a valid phone number';
            }
        }

        // Name validation
        if ((field.name === 'firstName' || field.name === 'lastName') && field.value) {
            if (field.value.length > 100) {
                isValid = false;
                errorMessage = 'Name cannot exceed 100 characters';
            } else if (!/^[a-zA-Z\s\-'\.]+$/.test(field.value)) {
                isValid = false;
                errorMessage = 'Name can only contain letters, spaces, hyphens, apostrophes, and periods';
            }
        }

        // Description validation
        if (field.name === 'description' && field.value) {
            if (field.value.length > 1000) {
                isValid = false;
                errorMessage = 'Description cannot exceed 1000 characters';
            }
        }

        // Age validation
        if (field.name === 'age' && field.value) {
            const age = parseInt(field.value);
            if (isNaN(age) || age < 1 || age > 120) {
                isValid = false;
                errorMessage = 'Age must be between 1 and 120';
            }
        }

        // Date validation
        if (field.type === 'date' && field.value) {
            const date = new Date(field.value);
            const today = new Date();
            if (date > today) {
                isValid = false;
                errorMessage = 'Date cannot be in the future';
            }
        }

        // Show or clear error
        if (!isValid) {
            this.showFieldError(field, errorMessage);
        } else {
            this.clearFieldError(field);
        }

        return isValid;
    }

    /**
     * Validate email field specifically
     * @param {HTMLElement} field - Email field
     */
    validateEmailField(field) {
        if (!field.value) return;

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(field.value)) {
            this.showFieldError(field, 'Please enter a valid email address');
        } else {
            this.clearFieldError(field);
        }
    }

    /**
     * Validate phone field specifically
     * @param {HTMLElement} field - Phone field
     */
    validatePhoneField(field) {
        if (!field.value) return;

        const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
        const cleanPhone = field.value.replace(/\s/g, '');
        if (!phoneRegex.test(cleanPhone)) {
            this.showFieldError(field, 'Please enter a valid phone number');
        } else {
            this.clearFieldError(field);
        }
    }

    /**
     * Validate files
     * @param {FileList} files - Files to validate
     * @param {HTMLElement} target - Target element
     */
    async validateFiles(files, target) {
        const maxFiles = 5;
        const maxSize = 5 * 1024 * 1024; // 5MB
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];

        if (files.length > maxFiles) {
            this.showFileError(target, `Maximum ${maxFiles} files allowed`);
            return false;
        }

        for (let file of files) {
            // Validate file type
            if (!validateFileType(file, allowedTypes)) {
                this.showFileError(target, `File "${file.name}" is not a valid image type. Only JPEG, PNG, and WebP files are allowed.`);
                return false;
            }

            // Validate file size
            if (!validateFileSize(file, maxSize)) {
                this.showFileError(target, `File "${file.name}" is too large. Maximum size is 5MB.`);
                return false;
            }

            // Compress image if needed
            try {
                if (file.size > 2 * 1024 * 1024) { // Compress if > 2MB
                    const compressedFile = await compressImage(file, 0.8, 1920, 1080);
                    // Replace original file with compressed version
                    Object.defineProperty(file, 'size', { value: compressedFile.size });
                }
            } catch (error) {
                console.warn('Image compression failed:', error);
            }
        }

        this.clearFileError(target);
        return true;
    }

    /**
     * Validate entire form
     * @param {HTMLFormElement} form - Form to validate
     * @param {string} formId - Form ID
     * @returns {boolean} - Whether form is valid
     */
    validateForm(form, formId) {
        let isValid = true;
        const formData = new FormData(form);
        const data = Object.fromEntries(formData.entries());

        // Clear previous errors
        this.clearAllErrors(form);

        // Validate each field
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            if (!this.validateField(input, formId)) {
                isValid = false;
            }
        });

        // Validate required fields
        const requiredFields = form.querySelectorAll('[required]');
        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                this.showFieldError(field, `${this.getFieldLabel(field)} is required`);
                isValid = false;
            }
        });

        // Validate file uploads
        const fileInputs = form.querySelectorAll('input[type="file"]');
        for (let fileInput of fileInputs) {
            if (fileInput.files.length > 0) {
                if (!this.validateFiles(fileInput.files, fileInput)) {
                    isValid = false;
                }
            }
        }

        // Form-specific validation
        if (formId === 'quickReportForm') {
            isValid = this.validateQuickReport(data) && isValid;
        } else if (formId === 'detailedReportForm') {
            isValid = this.validateDetailedReport(data) && isValid;
        } else if (formId === 'updateRunnerForm') {
            isValid = this.validateUpdateReport(data) && isValid;
        }

        // Show general error if form is invalid
        if (!isValid) {
            this.showFormError(form, 'Please correct the errors below and try again.');
        }

        return isValid;
    }

    /**
     * Validate quick report data
     * @param {Object} data - Form data
     * @returns {boolean} - Whether data is valid
     */
    validateQuickReport(data) {
        let isValid = true;

        // Check required fields
        const requiredFields = ['firstName', 'lastName', 'age', 'gender', 'lastSeenLocation', 'lastSeenDate', 'description'];
        requiredFields.forEach(field => {
            if (!data[field] || !data[field].trim()) {
                this.showFieldError(document.querySelector(`[name="${field}"]`), `${this.capitalize(field)} is required`);
                isValid = false;
            }
        });

        // Validate age
        if (data.age) {
            const age = parseInt(data.age);
            if (isNaN(age) || age < 1 || age > 120) {
                this.showFieldError(document.querySelector('[name="age"]'), 'Age must be between 1 and 120');
                isValid = false;
            }
        }

        // Validate date
        if (data.lastSeenDate) {
            const date = new Date(data.lastSeenDate);
            const today = new Date();
            if (date > today) {
                this.showFieldError(document.querySelector('[name="lastSeenDate"]'), 'Date cannot be in the future');
                isValid = false;
            }
        }

        return isValid;
    }

    /**
     * Validate detailed report data
     * @param {Object} data - Form data
     * @returns {boolean} - Whether data is valid
     */
    validateDetailedReport(data) {
        let isValid = true;

        // Check required fields
        const requiredFields = ['firstName', 'lastName', 'age', 'gender', 'height', 'weight', 'hairColor', 'eyeColor', 'lastSeenLocation', 'lastSeenDate', 'description'];
        requiredFields.forEach(field => {
            if (!data[field] || !data[field].trim()) {
                this.showFieldError(document.querySelector(`[name="${field}"]`), `${this.capitalize(field)} is required`);
                isValid = false;
            }
        });

        // Validate physical characteristics
        if (data.height && !/^[0-9]+'[0-9]+"$|^[0-9]+cm$|^[0-9]+\.[0-9]+m$/.test(data.height)) {
            this.showFieldError(document.querySelector('[name="height"]'), 'Please enter height in format: 5\'10" or 180cm or 1.8m');
            isValid = false;
        }

        if (data.weight && !/^[0-9]+lbs?$|^[0-9]+kg$|^[0-9]+\.[0-9]+kg$/.test(data.weight)) {
            this.showFieldError(document.querySelector('[name="weight"]'), 'Please enter weight in format: 150lbs or 68kg');
            isValid = false;
        }

        return isValid;
    }

    /**
     * Validate update report data
     * @param {Object} data - Form data
     * @returns {boolean} - Whether data is valid
     */
    validateUpdateReport(data) {
        let isValid = true;

        // Check if runnerId exists
        if (!data.runnerId) {
            this.showFormError(document.getElementById('updateRunnerForm'), 'Runner ID is required for updates');
            isValid = false;
        }

        return isValid;
    }

    /**
     * Show field error
     * @param {HTMLElement} field - Field to show error for
     * @param {string} message - Error message
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

    /**
     * Clear field error
     * @param {HTMLElement} field - Field to clear error for
     */
    clearFieldError(field) {
        const errorElement = field.parentNode.querySelector('.field-error');
        if (errorElement) {
            errorElement.remove();
        }
        field.classList.remove('error');
    }

    /**
     * Show file error
     * @param {HTMLElement} target - Target element
     * @param {string} message - Error message
     */
    showFileError(target, message) {
        this.clearFileError(target);
        
        const errorElement = document.createElement('div');
        errorElement.className = 'file-error';
        errorElement.textContent = message;
        errorElement.style.cssText = `
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        `;
        
        target.parentNode.insertBefore(errorElement, target.nextSibling);
    }

    /**
     * Clear file error
     * @param {HTMLElement} target - Target element
     */
    clearFileError(target) {
        const errorElement = target.parentNode.querySelector('.file-error');
        if (errorElement) {
            errorElement.remove();
        }
    }

    /**
     * Show form error
     * @param {HTMLFormElement} form - Form element
     * @param {string} message - Error message
     */
    showFormError(form, message) {
        this.clearFormError(form);
        
        const errorElement = document.createElement('div');
        errorElement.className = 'form-error';
        errorElement.textContent = message;
        errorElement.style.cssText = `
            color: #dc3545;
            font-size: 1rem;
            margin-bottom: 1rem;
            padding: 0.75rem;
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            border-radius: 0.375rem;
        `;
        
        form.insertBefore(errorElement, form.firstChild);
    }

    /**
     * Clear form error
     * @param {HTMLFormElement} form - Form element
     */
    clearFormError(form) {
        const errorElement = form.querySelector('.form-error');
        if (errorElement) {
            errorElement.remove();
        }
    }

    /**
     * Clear all errors in form
     * @param {HTMLFormElement} form - Form element
     */
    clearAllErrors(form) {
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            this.clearFieldError(input);
        });
        this.clearFormError(form);
    }

    /**
     * Get field label
     * @param {HTMLElement} field - Field element
     * @returns {string} - Field label
     */
    getFieldLabel(field) {
        const label = field.parentNode.querySelector('label');
        if (label) {
            return label.textContent.replace('*', '').trim();
        }
        return field.name || field.id;
    }

    /**
     * Capitalize first letter
     * @param {string} str - String to capitalize
     * @returns {string} - Capitalized string
     */
    capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    /**
     * Sanitize form data
     * @param {Object} data - Form data
     * @returns {Object} - Sanitized data
     */
    sanitizeFormData(data) {
        const sanitized = {};
        for (const [key, value] of Object.entries(data)) {
            if (typeof value === 'string') {
                sanitized[key] = sanitizeInput(value);
            } else {
                sanitized[key] = value;
            }
        }
        return sanitized;
    }
}

// Initialize report validator when DOM is loaded
let reportValidator;
document.addEventListener('DOMContentLoaded', () => {
    reportValidator = new ReportValidator();
    
    // Make it globally available
    window.reportValidator = reportValidator;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ReportValidator;
}
