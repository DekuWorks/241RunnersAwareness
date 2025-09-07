/**
 * 241 Runners Awareness - Update Banner
 * Handles service worker updates and prompts user to refresh
 */

class UpdateBanner {
    constructor() {
        this.banner = null;
        this.isVisible = false;
        this.init();
    }

    init() {
        // Listen for service worker updates
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.addEventListener('message', (event) => {
                if (event.data && event.data.type === 'SW_UPDATE_AVAILABLE') {
                    this.showUpdateBanner();
                }
            });

            // Check for updates on page load
            this.checkForUpdates();
        }
    }

    async checkForUpdates() {
        try {
            const response = await fetch('/version.json?t=' + Date.now());
            const currentVersion = await response.json();
            const storedVersion = localStorage.getItem('app_version');
            
            if (storedVersion && storedVersion !== currentVersion.commit) {
                this.showUpdateBanner();
            }
            
            localStorage.setItem('app_version', currentVersion.commit);
        } catch (error) {
            console.warn('Failed to check for updates:', error);
        }
    }

    showUpdateBanner() {
        if (this.isVisible) return;

        this.createBanner();
        this.isVisible = true;
        
        // Auto-hide after 30 seconds if not dismissed
        setTimeout(() => {
            if (this.isVisible) {
                this.hideUpdateBanner();
            }
        }, 30000);
    }

    createBanner() {
        // Remove existing banner if any
        const existingBanner = document.getElementById('update-banner');
        if (existingBanner) {
            existingBanner.remove();
        }

        this.banner = document.createElement('div');
        this.banner.id = 'update-banner';
        this.banner.innerHTML = `
            <div class="update-banner-content">
                <div class="update-banner-icon">🔄</div>
                <div class="update-banner-text">
                    <strong>Update Available</strong>
                    <span>A new version of the application is available.</span>
                </div>
                <div class="update-banner-actions">
                    <button class="update-banner-btn update-banner-btn-primary" onclick="updateBanner.refreshApp()">
                        Update Now
                    </button>
                    <button class="update-banner-btn update-banner-btn-secondary" onclick="updateBanner.hideUpdateBanner()">
                        Later
                    </button>
                </div>
            </div>
        `;

        // Add styles
        this.banner.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            background: linear-gradient(135deg, #dc2626, #991b1b);
            color: white;
            padding: 12px 20px;
            z-index: 10000;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
            animation: slideDown 0.3s ease;
        `;

        // Add CSS for banner content
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideDown {
                from { transform: translateY(-100%); }
                to { transform: translateY(0); }
            }
            
            .update-banner-content {
                display: flex;
                align-items: center;
                justify-content: space-between;
                max-width: 1200px;
                margin: 0 auto;
                gap: 15px;
            }
            
            .update-banner-icon {
                font-size: 1.5rem;
                animation: spin 2s linear infinite;
            }
            
            @keyframes spin {
                from { transform: rotate(0deg); }
                to { transform: rotate(360deg); }
            }
            
            .update-banner-text {
                flex: 1;
                display: flex;
                flex-direction: column;
                gap: 2px;
            }
            
            .update-banner-text strong {
                font-size: 1rem;
                font-weight: 600;
            }
            
            .update-banner-text span {
                font-size: 0.9rem;
                opacity: 0.9;
            }
            
            .update-banner-actions {
                display: flex;
                gap: 10px;
            }
            
            .update-banner-btn {
                padding: 8px 16px;
                border: none;
                border-radius: 6px;
                cursor: pointer;
                font-size: 0.9rem;
                font-weight: 600;
                transition: all 0.2s ease;
            }
            
            .update-banner-btn-primary {
                background: white;
                color: #dc2626;
            }
            
            .update-banner-btn-primary:hover {
                background: #f3f4f6;
                transform: translateY(-1px);
            }
            
            .update-banner-btn-secondary {
                background: rgba(255,255,255,0.2);
                color: white;
                border: 1px solid rgba(255,255,255,0.3);
            }
            
            .update-banner-btn-secondary:hover {
                background: rgba(255,255,255,0.3);
            }
            
            @media (max-width: 768px) {
                .update-banner-content {
                    flex-direction: column;
                    text-align: center;
                    gap: 10px;
                }
                
                .update-banner-actions {
                    width: 100%;
                    justify-content: center;
                }
                
                .update-banner-btn {
                    flex: 1;
                    max-width: 120px;
                }
            }
        `;
        
        document.head.appendChild(style);
        document.body.appendChild(this.banner);

        // Add top padding to body to account for banner
        document.body.style.paddingTop = '60px';
    }

    hideUpdateBanner() {
        if (this.banner) {
            this.banner.style.animation = 'slideUp 0.3s ease';
            setTimeout(() => {
                if (this.banner && this.banner.parentNode) {
                    this.banner.remove();
                }
                document.body.style.paddingTop = '';
                this.isVisible = false;
            }, 300);
        }
    }

    async refreshApp() {
        try {
            // Clear all caches
            if ('caches' in window) {
                const cacheNames = await caches.keys();
                await Promise.all(
                    cacheNames.map(cacheName => caches.delete(cacheName))
                );
            }

            // Clear localStorage version
            localStorage.removeItem('app_version');

            // Reload the page
            window.location.reload(true);
        } catch (error) {
            console.error('Error refreshing app:', error);
            // Fallback to simple reload
            window.location.reload(true);
        }
    }
}

// Initialize update banner
const updateBanner = new UpdateBanner();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UpdateBanner;
}
