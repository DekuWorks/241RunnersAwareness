/**
 * 241 Runners Awareness - Loading States & UX Utilities
 * 
 * This file provides loading spinners, progress bars, and skeleton loaders
 * for improved user experience during data loading and form submissions.
 */

/**
 * ============================================
 * LOADING SPINNER UTILITIES
 * ============================================
 */

class LoadingManager {
    constructor() {
        this.activeLoaders = new Set();
        this.setupStyles();
    }

    /**
     * Setup loading styles
     */
    setupStyles() {
        if (document.getElementById('loading-styles')) return;

        const style = document.createElement('style');
        style.id = 'loading-styles';
        style.textContent = `
            /* Loading Spinner Styles */
            .loading-spinner {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.5);
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
                z-index: 9999;
            }

            .spinner {
                position: relative;
                width: 60px;
                height: 60px;
            }

            .spinner-ring {
                position: absolute;
                width: 100%;
                height: 100%;
                border: 3px solid transparent;
                border-top: 3px solid #007bff;
                border-radius: 50%;
                animation: spin 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
            }

            .spinner-ring:nth-child(1) {
                animation-delay: -0.45s;
            }

            .spinner-ring:nth-child(2) {
                animation-delay: -0.3s;
            }

            .spinner-ring:nth-child(3) {
                animation-delay: -0.15s;
            }

            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }

            .loading-text {
                color: white;
                margin-top: 20px;
                font-size: 16px;
                font-weight: 500;
            }

            /* Skeleton Loader Styles */
            .skeleton-loader {
                padding: 20px;
            }

            .skeleton-item {
                display: flex;
                align-items: center;
                padding: 20px;
                border-bottom: 1px solid #eee;
                animation: skeleton-pulse 1.5s ease-in-out infinite;
            }

            .skeleton-avatar {
                width: 50px;
                height: 50px;
                border-radius: 50%;
                background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
                background-size: 200% 100%;
                animation: skeleton-shimmer 1.5s infinite;
                margin-right: 15px;
            }

            .skeleton-content {
                flex: 1;
            }

            .skeleton-line {
                height: 12px;
                background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
                background-size: 200% 100%;
                animation: skeleton-shimmer 1.5s infinite;
                margin-bottom: 8px;
                border-radius: 4px;
            }

            .skeleton-line--title {
                width: 60%;
                height: 16px;
            }

            .skeleton-line--text {
                width: 100%;
            }

            .skeleton-line--text:last-child {
                width: 80%;
            }

            @keyframes skeleton-pulse {
                0%, 100% { opacity: 1; }
                50% { opacity: 0.5; }
            }

            @keyframes skeleton-shimmer {
                0% { background-position: -200% 0; }
                100% { background-position: 200% 0; }
            }

            /* Progress Bar Styles */
            .progress-bar {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 4px;
                background: rgba(0, 0, 0, 0.1);
                z-index: 10000;
            }

            .progress-bar-container {
                position: relative;
                width: 100%;
                height: 100%;
            }

            .progress-bar-fill {
                height: 100%;
                background: linear-gradient(90deg, #007bff, #0056b3);
                width: 0%;
                transition: width 0.3s ease;
            }

            .progress-bar-text {
                position: absolute;
                top: 10px;
                right: 20px;
                font-size: 12px;
                color: #666;
                font-weight: 500;
            }

            /* Button Loading States */
            .btn-loading {
                position: relative;
                pointer-events: none;
                opacity: 0.7;
            }

            .btn-loading::after {
                content: '';
                position: absolute;
                top: 50%;
                left: 50%;
                width: 16px;
                height: 16px;
                margin: -8px 0 0 -8px;
                border: 2px solid transparent;
                border-top: 2px solid currentColor;
                border-radius: 50%;
                animation: spin 1s linear infinite;
            }

            /* Fade In Animation */
            .fade-in {
                animation: fadeIn 0.5s ease-in-out;
            }

            @keyframes fadeIn {
                from { opacity: 0; transform: translateY(20px); }
                to { opacity: 1; transform: translateY(0); }
            }
        `;
        document.head.appendChild(style);
    }

    /**
     * Show loading spinner
     * @param {string} message - Loading message
     * @param {string} id - Unique identifier for the loader
     */
    showSpinner(message = 'Loading...', id = 'default') {
        const spinner = document.getElementById('loading-spinner');
        if (!spinner) {
            this.createSpinner();
        }

        const loadingText = document.querySelector('.loading-text');
        if (loadingText) {
            loadingText.textContent = message;
        }

        document.getElementById('loading-spinner').style.display = 'flex';
        this.activeLoaders.add(id);
    }

    /**
     * Hide loading spinner
     * @param {string} id - Unique identifier for the loader
     */
    hideSpinner(id = 'default') {
        this.activeLoaders.delete(id);
        
        if (this.activeLoaders.size === 0) {
            const spinner = document.getElementById('loading-spinner');
            if (spinner) {
                spinner.style.display = 'none';
            }
        }
    }

    /**
     * Show skeleton loader
     * @param {string} containerId - Container to show skeleton in
     * @param {number} count - Number of skeleton items
     */
    showSkeleton(containerId, count = 3) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const skeleton = document.getElementById('skeleton-loader');
        if (!skeleton) {
            this.createSkeleton();
        }

        // Clone skeleton items
        const skeletonItem = document.querySelector('.skeleton-item');
        if (skeletonItem) {
            container.innerHTML = '';
            for (let i = 0; i < count; i++) {
                container.appendChild(skeletonItem.cloneNode(true));
            }
        }

        container.style.display = 'block';
    }

    /**
     * Hide skeleton loader
     * @param {string} containerId - Container to hide skeleton in
     */
    hideSkeleton(containerId) {
        const container = document.getElementById(containerId);
        if (container) {
            container.style.display = 'none';
        }
    }

    /**
     * Show progress bar
     * @param {string} id - Unique identifier for the progress bar
     */
    showProgress(id = 'default') {
        const progressBar = document.getElementById('progress-bar');
        if (!progressBar) {
            this.createProgressBar();
        }

        document.getElementById('progress-bar').style.display = 'block';
        this.activeLoaders.add(`progress-${id}`);
    }

    /**
     * Update progress bar
     * @param {number} percentage - Progress percentage (0-100)
     * @param {string} id - Unique identifier for the progress bar
     */
    updateProgress(percentage, id = 'default') {
        const fill = document.querySelector('.progress-bar-fill');
        const text = document.querySelector('.progress-bar-text');
        
        if (fill) {
            fill.style.width = `${Math.min(100, Math.max(0, percentage))}%`;
        }
        
        if (text) {
            text.textContent = `${Math.round(percentage)}%`;
        }
    }

    /**
     * Hide progress bar
     * @param {string} id - Unique identifier for the progress bar
     */
    hideProgress(id = 'default') {
        this.activeLoaders.delete(`progress-${id}`);
        
        if (!this.activeLoaders.has(`progress-${id}`)) {
            const progressBar = document.getElementById('progress-bar');
            if (progressBar) {
                progressBar.style.display = 'none';
            }
        }
    }

    /**
     * Set button loading state
     * @param {HTMLElement} button - Button element
     * @param {boolean} loading - Loading state
     * @param {string} text - Loading text
     */
    setButtonLoading(button, loading, text = 'Loading...') {
        if (loading) {
            button.classList.add('btn-loading');
            button.disabled = true;
            button.dataset.originalText = button.textContent;
            button.textContent = text;
        } else {
            button.classList.remove('btn-loading');
            button.disabled = false;
            if (button.dataset.originalText) {
                button.textContent = button.dataset.originalText;
                delete button.dataset.originalText;
            }
        }
    }

    /**
     * Create spinner element
     */
    createSpinner() {
        const spinner = document.createElement('div');
        spinner.id = 'loading-spinner';
        spinner.className = 'loading-spinner';
        spinner.innerHTML = `
            <div class="spinner">
                <div class="spinner-ring"></div>
                <div class="spinner-ring"></div>
                <div class="spinner-ring"></div>
                <div class="spinner-ring"></div>
            </div>
            <div class="loading-text">Loading...</div>
        `;
        document.body.appendChild(spinner);
    }

    /**
     * Create skeleton element
     */
    createSkeleton() {
        const skeleton = document.createElement('div');
        skeleton.id = 'skeleton-loader';
        skeleton.className = 'skeleton-loader';
        skeleton.innerHTML = `
            <div class="skeleton-item">
                <div class="skeleton-avatar"></div>
                <div class="skeleton-content">
                    <div class="skeleton-line skeleton-line--title"></div>
                    <div class="skeleton-line skeleton-line--text"></div>
                    <div class="skeleton-line skeleton-line--text"></div>
                </div>
            </div>
        `;
        document.body.appendChild(skeleton);
    }

    /**
     * Create progress bar element
     */
    createProgressBar() {
        const progressBar = document.createElement('div');
        progressBar.id = 'progress-bar';
        progressBar.className = 'progress-bar';
        progressBar.innerHTML = `
            <div class="progress-bar-container">
                <div class="progress-bar-fill"></div>
                <div class="progress-bar-text">0%</div>
            </div>
        `;
        document.body.appendChild(progressBar);
    }

    /**
     * Add fade-in animation to element
     * @param {HTMLElement} element - Element to animate
     */
    fadeIn(element) {
        element.classList.add('fade-in');
    }

    /**
     * Show loading state for form submission
     * @param {HTMLFormElement} form - Form element
     * @param {string} message - Loading message
     */
    showFormLoading(form, message = 'Submitting...') {
        const submitButton = form.querySelector('button[type="submit"]');
        if (submitButton) {
            this.setButtonLoading(submitButton, true, message);
        }
        
        // Disable all form inputs
        const inputs = form.querySelectorAll('input, textarea, select, button');
        inputs.forEach(input => {
            input.disabled = true;
        });
    }

    /**
     * Hide loading state for form submission
     * @param {HTMLFormElement} form - Form element
     */
    hideFormLoading(form) {
        const submitButton = form.querySelector('button[type="submit"]');
        if (submitButton) {
            this.setButtonLoading(submitButton, false);
        }
        
        // Re-enable all form inputs
        const inputs = form.querySelectorAll('input, textarea, select, button');
        inputs.forEach(input => {
            input.disabled = false;
        });
    }

    /**
     * Show loading state for API request
     * @param {string} endpoint - API endpoint
     * @param {string} method - HTTP method
     * @param {string} message - Loading message
     */
    showApiLoading(endpoint, method = 'GET', message = 'Loading...') {
        const id = `${method}-${endpoint}`;
        this.showSpinner(message, id);
    }

    /**
     * Hide loading state for API request
     * @param {string} endpoint - API endpoint
     * @param {string} method - HTTP method
     */
    hideApiLoading(endpoint, method = 'GET') {
        const id = `${method}-${endpoint}`;
        this.hideSpinner(id);
    }
}

// Initialize loading manager
let loadingManager;
document.addEventListener('DOMContentLoaded', () => {
    loadingManager = new LoadingManager();
    
    // Make it globally available
    window.loadingManager = loadingManager;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = LoadingManager;
}
