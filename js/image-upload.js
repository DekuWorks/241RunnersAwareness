/**
 * Image Upload Component for 241 Runners Awareness
 * Provides comprehensive image upload functionality with validation
 */

class ImageUploadManager {
    constructor(options = {}) {
        this.options = {
            apiBaseUrl: 'https://241runners-api.azurewebsites.net/api',
            maxFileSize: 10 * 1024 * 1024, // 10MB
            maxImages: 10,
            allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/bmp', 'image/webp'],
            allowedExtensions: ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'],
            maxImageDimension: 4096,
            enableDragAndDrop: true,
            enablePreview: true,
            enableProgress: true,
            ...options
        };
        
        this.uploadQueue = [];
        this.isUploading = false;
        this.uploadProgress = 0;
        
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.setupDragAndDrop();
        this.setupValidation();
    }

    setupEventListeners() {
        // File input change event
        document.addEventListener('change', (e) => {
            if (e.target.matches('input[type="file"]')) {
                this.handleFileSelection(e.target.files);
            }
        });

        // Drag and drop events
        if (this.options.enableDragAndDrop) {
            this.setupDragAndDrop();
        }
    }

    setupDragAndDrop() {
        const dropZones = document.querySelectorAll('.image-drop-zone');
        
        dropZones.forEach(zone => {
            zone.addEventListener('dragover', (e) => {
                e.preventDefault();
                zone.classList.add('drag-over');
            });

            zone.addEventListener('dragleave', (e) => {
                e.preventDefault();
                zone.classList.remove('drag-over');
            });

            zone.addEventListener('drop', (e) => {
                e.preventDefault();
                zone.classList.remove('drag-over');
                const files = Array.from(e.dataTransfer.files);
                this.handleFileSelection(files);
            });
        });
    }

    setupValidation() {
        // Add custom validation attributes to file inputs
        const fileInputs = document.querySelectorAll('input[type="file"]');
        fileInputs.forEach(input => {
            input.setAttribute('accept', this.options.allowedTypes.join(','));
            input.setAttribute('data-max-size', this.options.maxFileSize);
            input.setAttribute('data-max-images', this.options.maxImages);
        });
    }

    handleFileSelection(files) {
        const fileArray = Array.from(files);
        
        // Validate files
        const validationResult = this.validateFiles(fileArray);
        if (!validationResult.isValid) {
            this.showError(validationResult.errors.join(', '));
            return;
        }

        // Add files to upload queue
        this.addToUploadQueue(fileArray);
        
        // Show preview if enabled
        if (this.options.enablePreview) {
            this.showPreview(fileArray);
        }
    }

    validateFiles(files) {
        const errors = [];
        
        if (files.length === 0) {
            errors.push('No files selected');
            return { isValid: false, errors };
        }

        if (files.length > this.options.maxImages) {
            errors.push(`Maximum ${this.options.maxImages} images allowed`);
        }

        files.forEach((file, index) => {
            // Check file type
            if (!this.options.allowedTypes.includes(file.type)) {
                errors.push(`${file.name}: Invalid file type`);
            }

            // Check file extension
            const extension = '.' + file.name.split('.').pop().toLowerCase();
            if (!this.options.allowedExtensions.includes(extension)) {
                errors.push(`${file.name}: Invalid file extension`);
            }

            // Check file size
            if (file.size > this.options.maxFileSize) {
                errors.push(`${file.name}: File too large (max ${this.formatFileSize(this.options.maxFileSize)})`);
            }

            // Check if it's actually an image
            if (!file.type.startsWith('image/')) {
                errors.push(`${file.name}: Not an image file`);
            }
        });

        return {
            isValid: errors.length === 0,
            errors
        };
    }

    addToUploadQueue(files) {
        files.forEach(file => {
            const uploadItem = {
                id: this.generateId(),
                file: file,
                status: 'pending', // pending, uploading, success, error
                progress: 0,
                error: null
            };
            
            this.uploadQueue.push(uploadItem);
        });

        this.updateUploadQueueDisplay();
    }

    async uploadImages(category, relatedId = null, additionalData = {}) {
        if (this.uploadQueue.length === 0) {
            this.showError('No images to upload');
            return;
        }

        if (this.isUploading) {
            this.showError('Upload already in progress');
            return;
        }

        this.isUploading = true;
        this.updateUploadStatus('Uploading...');

        try {
            const token = this.getAuthToken();
            if (!token) {
                throw new Error('Authentication required');
            }

            const formData = new FormData();
            
            // Add images
            this.uploadQueue.forEach(item => {
                formData.append('Images', item.file);
            });

            // Add metadata
            formData.append('Category', category);
            if (relatedId) {
                formData.append('RelatedId', relatedId);
            }
            
            // Add additional data
            Object.keys(additionalData).forEach(key => {
                if (additionalData[key] !== null && additionalData[key] !== undefined) {
                    formData.append(key, additionalData[key]);
                }
            });

            // Upload to API
            const response = await fetch(`${this.options.apiBaseUrl}/ImageUpload/upload-multiple`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                },
                body: formData
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.error || `Upload failed: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.handleUploadSuccess(result);
            } else {
                throw new Error(result.error || 'Upload failed');
            }

        } catch (error) {
            this.handleUploadError(error);
        } finally {
            this.isUploading = false;
            this.updateUploadStatus('Upload completed');
        }
    }

    async uploadSingleImage(file, category, relatedId = null, additionalData = {}) {
        try {
            const token = this.getAuthToken();
            if (!token) {
                throw new Error('Authentication required');
            }

            const formData = new FormData();
            formData.append('Image', file);
            formData.append('Category', category);
            
            if (relatedId) {
                formData.append('RelatedId', relatedId);
            }
            
            Object.keys(additionalData).forEach(key => {
                if (additionalData[key] !== null && additionalData[key] !== undefined) {
                    formData.append(key, additionalData[key]);
                }
            });

            const response = await fetch(`${this.options.apiBaseUrl}/ImageUpload/upload`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                },
                body: formData
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.error || `Upload failed: ${response.status}`);
            }

            const result = await response.json();
            return result;

        } catch (error) {
            this.showError(`Upload failed: ${error.message}`);
            throw error;
        }
    }

    handleUploadSuccess(result) {
        this.showSuccess(`Successfully uploaded ${result.totalUploaded} images`);
        
        // Clear upload queue
        this.uploadQueue = [];
        this.updateUploadQueueDisplay();
        
        // Trigger success callback
        if (this.options.onUploadSuccess) {
            this.options.onUploadSuccess(result);
        }
    }

    handleUploadError(error) {
        this.showError(`Upload failed: ${error.message}`);
        
        // Mark failed items
        this.uploadQueue.forEach(item => {
            if (item.status === 'uploading') {
                item.status = 'error';
                item.error = error.message;
            }
        });
        
        this.updateUploadQueueDisplay();
        
        // Trigger error callback
        if (this.options.onUploadError) {
            this.options.onUploadError(error);
        }
    }

    showPreview(files) {
        const previewContainer = document.querySelector('.image-preview-container');
        if (!previewContainer) return;

        previewContainer.innerHTML = '';
        
        files.forEach(file => {
            const reader = new FileReader();
            reader.onload = (e) => {
                const preview = document.createElement('div');
                preview.className = 'image-preview-item';
                preview.innerHTML = `
                    <img src="${e.target.result}" alt="${file.name}" />
                    <div class="image-preview-info">
                        <span class="image-name">${file.name}</span>
                        <span class="image-size">${this.formatFileSize(file.size)}</span>
                    </div>
                    <button class="remove-image" onclick="imageUploadManager.removeFromQueue('${file.name}')">×</button>
                `;
                previewContainer.appendChild(preview);
            };
            reader.readAsDataURL(file);
        });
    }

    removeFromQueue(fileName) {
        this.uploadQueue = this.uploadQueue.filter(item => item.file.name !== fileName);
        this.updateUploadQueueDisplay();
        this.updatePreview();
    }

    updateUploadQueueDisplay() {
        const queueContainer = document.querySelector('.upload-queue');
        if (!queueContainer) return;

        queueContainer.innerHTML = '';
        
        this.uploadQueue.forEach(item => {
            const queueItem = document.createElement('div');
            queueItem.className = `upload-queue-item ${item.status}`;
            queueItem.innerHTML = `
                <div class="upload-item-info">
                    <span class="file-name">${item.file.name}</span>
                    <span class="file-size">${this.formatFileSize(item.file.size)}</span>
                </div>
                <div class="upload-item-status">
                    <span class="status-text">${item.status}</span>
                    ${item.status === 'uploading' ? `<div class="progress-bar"><div class="progress-fill" style="width: ${item.progress}%"></div></div>` : ''}
                </div>
                ${item.error ? `<div class="upload-error">${item.error}</div>` : ''}
            `;
            queueContainer.appendChild(queueItem);
        });
    }

    updatePreview() {
        const previewContainer = document.querySelector('.image-preview-container');
        if (!previewContainer) return;

        previewContainer.innerHTML = '';
        
        this.uploadQueue.forEach(item => {
            const reader = new FileReader();
            reader.onload = (e) => {
                const preview = document.createElement('div');
                preview.className = 'image-preview-item';
                preview.innerHTML = `
                    <img src="${e.target.result}" alt="${item.file.name}" />
                    <div class="image-preview-info">
                        <span class="image-name">${item.file.name}</span>
                        <span class="image-size">${this.formatFileSize(item.file.size)}</span>
                    </div>
                    <button class="remove-image" onclick="imageUploadManager.removeFromQueue('${item.file.name}')">×</button>
                `;
                previewContainer.appendChild(preview);
            };
            reader.readAsDataURL(item.file);
        });
    }

    updateUploadStatus(message) {
        const statusElement = document.querySelector('.upload-status');
        if (statusElement) {
            statusElement.textContent = message;
        }
    }

    showSuccess(message) {
        if (this.options.onShowSuccess) {
            this.options.onShowSuccess(message);
        } else {
            // Default success display
            const successDiv = document.createElement('div');
            successDiv.className = 'upload-success';
            successDiv.textContent = message;
            document.body.appendChild(successDiv);
            
            setTimeout(() => {
                successDiv.remove();
            }, 5000);
        }
    }

    showError(message) {
        if (this.options.onShowError) {
            this.options.onShowError(message);
        } else {
            // Default error display
            const errorDiv = document.createElement('div');
            errorDiv.className = 'upload-error';
            errorDiv.textContent = message;
            document.body.appendChild(errorDiv);
            
            setTimeout(() => {
                errorDiv.remove();
            }, 5000);
        }
    }

    getAuthToken() {
        return localStorage.getItem('ra_token') || localStorage.getItem('ra_admin_token');
    }

    generateId() {
        return 'upload_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Public methods for external use
    addFiles(files) {
        this.handleFileSelection(files);
    }

    clearQueue() {
        this.uploadQueue = [];
        this.updateUploadQueueDisplay();
        this.updatePreview();
    }

    getQueueStatus() {
        return {
            total: this.uploadQueue.length,
            pending: this.uploadQueue.filter(item => item.status === 'pending').length,
            uploading: this.uploadQueue.filter(item => item.status === 'uploading').length,
            success: this.uploadQueue.filter(item => item.status === 'success').length,
            error: this.uploadQueue.filter(item => item.status === 'error').length
        };
    }
}

// Utility functions for standalone use
const ImageUploadUtils = {
    validateFile(file, options = {}) {
        const defaults = {
            maxSize: 10 * 1024 * 1024,
            allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/bmp', 'image/webp'],
            allowedExtensions: ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
        };
        
        const config = { ...defaults, ...options };
        
        const errors = [];
        
        if (!file) {
            errors.push('No file provided');
            return { isValid: false, errors };
        }

        if (!file.type.startsWith('image/')) {
            errors.push('File must be an image');
        }

        if (!config.allowedTypes.includes(file.type)) {
            errors.push('File type not allowed');
        }

        const extension = '.' + file.name.split('.').pop().toLowerCase();
        if (!config.allowedExtensions.includes(extension)) {
            errors.push('File extension not allowed');
        }

        if (file.size > config.maxSize) {
            errors.push(`File too large (max ${ImageUploadUtils.formatFileSize(config.maxSize)})`);
        }

        return {
            isValid: errors.length === 0,
            errors
        };
    },

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },

    createImagePreview(file, container, options = {}) {
        const reader = new FileReader();
        reader.onload = (e) => {
            const img = document.createElement('img');
            img.src = e.target.result;
            img.alt = file.name;
            
            if (options.maxWidth) img.style.maxWidth = options.maxWidth + 'px';
            if (options.maxHeight) img.style.maxHeight = options.maxHeight + 'px';
            
            container.innerHTML = '';
            container.appendChild(img);
        };
        reader.readAsDataURL(file);
    }
};

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ImageUploadManager, ImageUploadUtils };
} 