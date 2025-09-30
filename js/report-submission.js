/**
 * 241 Runners Awareness - Enhanced Report Submission
 * 
 * This file handles report submission with validation, error handling,
 * loading states, and security measures.
 */

// Import utilities
import { debounce, apiRequest, sanitizeInput } from './utils.js';

/**
 * ============================================
 * REPORT SUBMISSION CLASS
 * ============================================
 */

class ReportSubmission {
    constructor() {
        this.isSubmitting = false;
        this.uploadedImages = [];
        this.setupSubmissionHandlers();
        this.setupImageHandling();
    }

    /**
     * Setup form submission handlers
     */
    setupSubmissionHandlers() {
        // Quick report form
        this.setupFormHandler('quickReportForm', this.handleQuickReport.bind(this));
        
        // Detailed report form
        this.setupFormHandler('detailedReportForm', this.handleDetailedReport.bind(this));
        
        // Update form
        this.setupFormHandler('updateRunnerForm', this.handleUpdateReport.bind(this));
    }

    /**
     * Setup form handler
     * @param {string} formId - Form ID
     * @param {Function} handler - Submission handler
     */
    setupFormHandler(formId, handler) {
        const form = document.getElementById(formId);
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            if (this.isSubmitting) {
                return;
            }

            // Validate form before submission
            if (window.reportValidator && !window.reportValidator.validateForm(form, formId)) {
                return;
            }

            await handler(form);
        });
    }

    /**
     * Setup image handling
     */
    setupImageHandling() {
        // File input handlers
        const fileInputs = document.querySelectorAll('input[type="file"]');
        fileInputs.forEach(input => {
            input.addEventListener('change', (e) => {
                this.handleFileSelection(e.target.files);
            });
        });

        // Drag and drop handlers
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
                this.handleFileSelection(e.dataTransfer.files);
            });
        });
    }

    /**
     * Handle quick report submission
     * @param {HTMLFormElement} form - Form element
     */
    async handleQuickReport(form) {
        try {
            this.setSubmissionState(form, true);
            
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());
            
            // Sanitize data
            const sanitizedData = this.sanitizeFormData(data);
            
            // Add emergency flags
            sanitizedData.isEmergency = true;
            sanitizedData.reportType = 'quick';
            sanitizedData.isUrgent = true;
            
            // Add uploaded images
            if (this.uploadedImages.length > 0) {
                sanitizedData.additionalImageUrls = JSON.stringify(this.uploadedImages.map(img => img.url));
            }

            // Submit to API
            const response = await this.submitReport(sanitizedData);
            
            if (response.success) {
                this.showSuccessMessage('🚨 Emergency report submitted successfully! Case ID: ' + (response.id || 'N/A'));
                this.resetForm(form);
            } else {
                throw new Error(response.message || 'Failed to submit emergency report');
            }
            
        } catch (error) {
            console.error('Quick report error:', error);
            this.handleSubmissionError(error, 'emergency report');
        } finally {
            this.setSubmissionState(form, false);
        }
    }

    /**
     * Handle detailed report submission
     * @param {HTMLFormElement} form - Form element
     */
    async handleDetailedReport(form) {
        try {
            this.setSubmissionState(form, true);
            
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());
            
            // Sanitize data
            const sanitizedData = this.sanitizeFormData(data);
            
            // Add report type
            sanitizedData.reportType = 'detailed';
            
            // Add uploaded images
            if (this.uploadedImages.length > 0) {
                sanitizedData.additionalImageUrls = JSON.stringify(this.uploadedImages.map(img => img.url));
            }

            // Submit to API
            const response = await this.submitReport(sanitizedData);
            
            if (response.success) {
                this.showSuccessMessage('📋 Detailed report submitted successfully! Case ID: ' + (response.id || 'N/A'));
                this.resetForm(form);
            } else {
                throw new Error(response.message || 'Failed to submit detailed report');
            }
            
        } catch (error) {
            console.error('Detailed report error:', error);
            this.handleSubmissionError(error, 'detailed report');
        } finally {
            this.setSubmissionState(form, false);
        }
    }

    /**
     * Handle update report submission
     * @param {HTMLFormElement} form - Form element
     */
    async handleUpdateReport(form) {
        try {
            this.setSubmissionState(form, true);
            
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());
            
            // Sanitize data
            const sanitizedData = this.sanitizeFormData(data);
            
            // Validate runner ID
            if (!sanitizedData.runnerId) {
                throw new Error('Runner ID is required for updates');
            }

            // Submit update to API
            const response = await this.updateReport(sanitizedData.runnerId, sanitizedData);
            
            if (response.success) {
                this.showSuccessMessage('🔄 Update submitted successfully!');
                this.resetForm(form);
            } else {
                throw new Error(response.message || 'Failed to submit update');
            }
            
        } catch (error) {
            console.error('Update report error:', error);
            this.handleSubmissionError(error, 'update');
        } finally {
            this.setSubmissionState(form, false);
        }
    }

    /**
     * Submit report to API
     * @param {Object} data - Report data
     * @returns {Promise<Object>} - API response
     */
    async submitReport(data) {
        const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api';
        
        try {
            const response = await fetch(`${API_BASE_URL}/v1/runners`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.getAuthToken()}`
                },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                throw new Error('Network error. Please check your connection and try again.');
            }
            throw error;
        }
    }

    /**
     * Update report via API
     * @param {string} runnerId - Runner ID
     * @param {Object} data - Update data
     * @returns {Promise<Object>} - API response
     */
    async updateReport(runnerId, data) {
        const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api';
        
        try {
            const response = await fetch(`${API_BASE_URL}/v1/runners/${runnerId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.getAuthToken()}`
                },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                throw new Error('Network error. Please check your connection and try again.');
            }
            throw error;
        }
    }

    /**
     * Handle file selection
     * @param {FileList} files - Selected files
     */
    async handleFileSelection(files) {
        if (!files || files.length === 0) return;

        // Validate files
        if (window.reportValidator && !await window.reportValidator.validateFiles(files, document.querySelector('.file-drop-zone'))) {
            return;
        }

        // Show upload progress
        this.showUploadProgress();

        try {
            for (let file of files) {
                await this.uploadImage(file);
            }
            
            this.hideUploadProgress();
            this.showSuccessMessage(`${files.length} image(s) uploaded successfully!`);
        } catch (error) {
            console.error('File upload error:', error);
            this.hideUploadProgress();
            this.showErrorMessage('Failed to upload images. Please try again.');
        }
    }

    /**
     * Upload single image
     * @param {File} file - File to upload
     */
    async uploadImage(file) {
        const formData = new FormData();
        formData.append('image', file);

        const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api';
        
        const response = await fetch(`${API_BASE_URL}/v1/image-upload`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${this.getAuthToken()}`
            },
            body: formData
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Upload failed');
        }

        const result = await response.json();
        
        if (result.success) {
            this.uploadedImages.push({
                url: result.imageUrl,
                filename: file.name,
                size: file.size
            });
            this.displayUploadedImage(result.imageUrl, file.name);
        } else {
            throw new Error(result.message || 'Upload failed');
        }
    }

    /**
     * Display uploaded image
     * @param {string} imageUrl - Image URL
     * @param {string} filename - Filename
     */
    displayUploadedImage(imageUrl, filename) {
        const container = document.getElementById('uploadedImages');
        if (!container) return;

        const imageDiv = document.createElement('div');
        imageDiv.className = 'uploaded-image';
        imageDiv.innerHTML = `
            <img src="${imageUrl}" alt="${filename}" style="max-width: 100px; max-height: 100px; object-fit: cover;">
            <button class="image-remove-btn" onclick="window.reportSubmission.removeUploadedImage(this, '${imageUrl}')">×</button>
        `;
        
        container.appendChild(imageDiv);
    }

    /**
     * Remove uploaded image
     * @param {HTMLElement} button - Remove button
     * @param {string} imageUrl - Image URL to remove
     */
    removeUploadedImage(button, imageUrl) {
        if (confirm('Are you sure you want to remove this image?')) {
            // Remove from array
            this.uploadedImages = this.uploadedImages.filter(img => img.url !== imageUrl);
            
            // Remove from DOM
            button.parentElement.remove();
        }
    }

    /**
     * Set submission state
     * @param {HTMLFormElement} form - Form element
     * @param {boolean} isSubmitting - Whether submitting
     */
    setSubmissionState(form, isSubmitting) {
        this.isSubmitting = isSubmitting;
        
        const submitBtn = form.querySelector('button[type="submit"]');
        const submitText = form.querySelector('.submit-text');
        const submitLoading = form.querySelector('.submit-loading');
        
        if (submitBtn) {
            submitBtn.disabled = isSubmitting;
            if (isSubmitting) {
                submitBtn.classList.add('btn-loading');
            } else {
                submitBtn.classList.remove('btn-loading');
            }
        }
        
        if (submitText) {
            submitText.style.display = isSubmitting ? 'none' : 'inline';
        }
        
        if (submitLoading) {
            submitLoading.style.display = isSubmitting ? 'flex' : 'none';
        }

        // Disable all form inputs
        const inputs = form.querySelectorAll('input, textarea, select, button');
        inputs.forEach(input => {
            input.disabled = isSubmitting;
        });
    }

    /**
     * Reset form
     * @param {HTMLFormElement} form - Form to reset
     */
    resetForm(form) {
        form.reset();
        this.uploadedImages = [];
        
        // Clear uploaded images display
        const container = document.getElementById('uploadedImages');
        if (container) {
            container.innerHTML = '';
        }
        
        // Clear any errors
        if (window.reportValidator) {
            window.reportValidator.clearAllErrors(form);
        }
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

    /**
     * Handle submission error
     * @param {Error} error - Error object
     * @param {string} reportType - Type of report
     */
    handleSubmissionError(error, reportType) {
        let errorMessage = `Failed to submit ${reportType}. Please try again.`;
        
        if (error.message.includes('Network error')) {
            errorMessage = 'Network error. Please check your connection and try again.';
        } else if (error.message.includes('401')) {
            errorMessage = 'Your session has expired. Please sign in again.';
            setTimeout(() => {
                window.location.href = '/login.html';
            }, 2000);
        } else if (error.message.includes('429')) {
            errorMessage = 'Too many requests. Please wait a moment and try again.';
        } else if (error.message.includes('500')) {
            errorMessage = 'Server error. Please try again later.';
        } else if (error.message) {
            errorMessage = error.message;
        }
        
        this.showErrorMessage(errorMessage);
        
        // Log error for debugging
        if (window.errorHandler) {
            window.errorHandler.handleError('Report Submission Error', error, { reportType });
        }
    }

    /**
     * Show success message
     * @param {string} message - Success message
     */
    showSuccessMessage(message) {
        if (window.errorHandler) {
            window.errorHandler.showToast(message, 'success');
        } else {
            alert(message);
        }
    }

    /**
     * Show error message
     * @param {string} message - Error message
     */
    showErrorMessage(message) {
        if (window.errorHandler) {
            window.errorHandler.showToast(message, 'error');
        } else {
            alert(message);
        }
    }

    /**
     * Show upload progress
     */
    showUploadProgress() {
        const progressContainer = document.getElementById('uploadProgress');
        if (progressContainer) {
            progressContainer.style.display = 'block';
        }
    }

    /**
     * Hide upload progress
     */
    hideUploadProgress() {
        const progressContainer = document.getElementById('uploadProgress');
        if (progressContainer) {
            progressContainer.style.display = 'none';
        }
    }

    /**
     * Get authentication token
     * @returns {string|null} - Auth token
     */
    getAuthToken() {
        return localStorage.getItem('userToken') || localStorage.getItem('jwtToken');
    }
}

// Initialize report submission when DOM is loaded
let reportSubmission;
document.addEventListener('DOMContentLoaded', () => {
    reportSubmission = new ReportSubmission();
    
    // Make it globally available
    window.reportSubmission = reportSubmission;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ReportSubmission;
}
