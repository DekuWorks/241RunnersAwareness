/**
 * Image Upload Utility for 241 Runners Awareness
 * Provides reusable image upload functionality across the site
 */

class ImageUploader {
    constructor(options = {}) {
        this.apiBaseUrl = options.apiBaseUrl || 'https://241runners-api.azurewebsites.net/api';
        this.maxFileSize = options.maxFileSize || 5 * 1024 * 1024; // 5MB
        this.allowedTypes = options.allowedTypes || ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        this.maxFiles = options.maxFiles || 10;
        this.uploadedImages = [];
        this.onUploadComplete = options.onUploadComplete || null;
        this.onUploadError = options.onUploadError || null;
        this.onProgress = options.onProgress || null;
    }

    /**
     * Initialize image upload functionality
     * @param {string} containerId - ID of the container element
     * @param {string} inputId - ID of the file input element
     */
    init(containerId, inputId) {
        const container = document.getElementById(containerId);
        const fileInput = document.getElementById(inputId);

        if (!container || !fileInput) {
            console.error('Image upload container or input not found');
            return;
        }

        // Click to upload
        container.addEventListener('click', () => {
            fileInput.click();
        });

        // File input change
        fileInput.addEventListener('change', (e) => {
            this.handleFiles(e.target.files);
        });

        // Drag and drop functionality
        container.addEventListener('dragover', (e) => {
            e.preventDefault();
            container.classList.add('dragover');
        });

        container.addEventListener('dragleave', (e) => {
            e.preventDefault();
            container.classList.remove('dragover');
        });

        container.addEventListener('drop', (e) => {
            e.preventDefault();
            container.classList.remove('dragover');
            this.handleFiles(e.dataTransfer.files);
        });
    }

    /**
     * Handle multiple files
     * @param {FileList} files - Files to process
     */
    handleFiles(files) {
        const validFiles = [];

        for (let file of files) {
            if (this.validateFile(file)) {
                validFiles.push(file);
            }
        }

        if (validFiles.length > 0) {
            this.uploadFiles(validFiles);
        }
    }

    /**
     * Validate a single file
     * @param {File} file - File to validate
     * @returns {boolean} - Whether file is valid
     */
    validateFile(file) {
        // Check file type
        if (!this.allowedTypes.includes(file.type)) {
            this.showError(`File "${file.name}" is not a valid image type. Please select JPEG, PNG, GIF, or WebP files.`);
            return false;
        }

        // Check file size
        if (file.size > this.maxFileSize) {
            this.showError(`File "${file.name}" is too large. Maximum size is ${this.formatFileSize(this.maxFileSize)}.`);
            return false;
        }

        // Check total file count
        if (this.uploadedImages.length >= this.maxFiles) {
            this.showError(`Maximum ${this.maxFiles} images allowed.`);
            return false;
        }

        return true;
    }

    /**
     * Upload multiple files
     * @param {File[]} files - Files to upload
     */
    async uploadFiles(files) {
        for (let file of files) {
            await this.uploadFile(file);
        }
    }

    /**
     * Upload a single file
     * @param {File} file - File to upload
     */
    async uploadFile(file) {
        const formData = new FormData();
        formData.append('image', file);

        try {
            // Show progress
            if (this.onProgress) {
                this.onProgress(0, `Uploading ${file.name}...`);
            }

            const response = await fetch(`${this.apiBaseUrl}/ImageUpload/upload`, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    const imageData = {
                        url: result.imageUrl,
                        filename: file.name,
                        size: file.size,
                        uploadedAt: new Date().toISOString()
                    };

                    this.uploadedImages.push(imageData);

                    if (this.onProgress) {
                        this.onProgress(100, `Uploaded ${file.name}`);
                    }

                    if (this.onUploadComplete) {
                        this.onUploadComplete(imageData);
                    }
                } else {
                    throw new Error(result.message || 'Upload failed');
                }
            } else {
                throw new Error(`Upload failed: ${response.statusText}`);
            }
        } catch (error) {
            console.error('Upload error:', error);
            if (this.onUploadError) {
                this.onUploadError(error, file);
            } else {
                this.showError(`Failed to upload ${file.name}: ${error.message}`);
            }
        }
    }

    /**
     * Remove an uploaded image
     * @param {string} imageUrl - URL of image to remove
     */
    removeImage(imageUrl) {
        this.uploadedImages = this.uploadedImages.filter(img => img.url !== imageUrl);
    }

    /**
     * Get all uploaded images
     * @returns {Array} - Array of uploaded image data
     */
    getUploadedImages() {
        return [...this.uploadedImages];
    }

    /**
     * Get uploaded image URLs
     * @returns {Array} - Array of image URLs
     */
    getUploadedImageUrls() {
        return this.uploadedImages.map(img => img.url);
    }

    /**
     * Clear all uploaded images
     */
    clearImages() {
        this.uploadedImages = [];
    }

    /**
     * Format file size for display
     * @param {number} bytes - File size in bytes
     * @returns {string} - Formatted file size
     */
    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    /**
     * Show error message
     * @param {string} message - Error message
     */
    showError(message) {
        // Try to use existing toast system, fallback to alert
        if (window.showToast) {
            window.showToast('error', 'Upload Error', message);
        } else {
            alert(message);
        }
    }
}

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ImageUploader;
}

// Make available globally
window.ImageUploader = ImageUploader;
