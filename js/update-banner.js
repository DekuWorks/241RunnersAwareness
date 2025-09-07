/**
 * ============================================
 * 241 RUNNERS AWARENESS - UPDATE BANNER
 * ============================================
 * 
 * Service Worker update flow with "Update available" toast
 * P0 Implementation: Service Worker update flow
 */

// ===== CONFIGURATION =====
const UPDATE_CONFIG = {
    checkInterval: 30000, // 30 seconds
    versionUrl: '/version.json',
    bannerDuration: 10000, // 10 seconds
    autoHideDelay: 5000 // 5 seconds
};

// ===== STATE MANAGEMENT =====
let updateState = {
    currentVersion: null,
    newVersion: null,
    isUpdateAvailable: false,
    checkTimer: null,
    bannerElement: null
};

// ===== VERSION CHECKING =====

/**
 * Check for updates
 */
async function checkForUpdates() {
    try {
        console.log('🔄 Checking for updates...');
        
        const response = await fetch(UPDATE_CONFIG.versionUrl, {
            cache: 'no-store',
            headers: {
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache'
            }
        });
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const versionData = await response.json();
        const newVersion = versionData.version;
        
        if (!updateState.currentVersion) {
            updateState.currentVersion = newVersion;
            console.log('📋 Initial version set:', newVersion);
            return;
        }
        
        if (newVersion !== updateState.currentVersion) {
            console.log('🆕 Update available:', newVersion);
            updateState.newVersion = newVersion;
            updateState.isUpdateAvailable = true;
            showUpdateBanner();
        }
        
    } catch (error) {
        console.error('❌ Failed to check for updates:', error);
    }
}

/**
 * Show update banner
 */
function showUpdateBanner() {
    if (updateState.bannerElement) {
        return; // Banner already showing
    }
    
    console.log('📢 Showing update banner...');
    
    // Create banner element
    const banner = document.createElement('div');
    banner.id = 'updateBanner';
    banner.className = 'update-banner';
    banner.innerHTML = `
        <div class="update-banner-content">
            <div class="update-banner-icon">🔄</div>
            <div class="update-banner-text">
                <div class="update-banner-title">Update Available</div>
                <div class="update-banner-message">A new version is ready. Click to update now.</div>
            </div>
            <div class="update-banner-actions">
                <button class="update-banner-button update-banner-button-primary" onclick="applyUpdate()">
                    Update Now
                </button>
                <button class="update-banner-button update-banner-button-secondary" onclick="dismissUpdate()">
                    Later
                </button>
            </div>
        </div>
    `;
    
    // Add styles
    banner.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        background: linear-gradient(135deg, #dc2626, #991b1b);
        color: white;
        padding: 12px 20px;
        z-index: 10000;
        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
        transform: translateY(-100%);
        transition: transform 0.3s ease;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    `;
    
    // Add content styles
    const style = document.createElement('style');
    style.textContent = `
        .update-banner-content {
            display: flex;
            align-items: center;
            justify-content: space-between;
            max-width: 1200px;
            margin: 0 auto;
            gap: 20px;
        }
        
        .update-banner-icon {
            font-size: 24px;
            flex-shrink: 0;
        }
        
        .update-banner-text {
            flex: 1;
        }
        
        .update-banner-title {
            font-weight: 600;
            font-size: 16px;
            margin-bottom: 2px;
        }
        
        .update-banner-message {
            font-size: 14px;
            opacity: 0.9;
        }
        
        .update-banner-actions {
            display: flex;
            gap: 10px;
            flex-shrink: 0;
        }
        
        .update-banner-button {
            padding: 8px 16px;
            border: none;
            border-radius: 6px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s ease;
        }
        
        .update-banner-button-primary {
            background: white;
            color: #dc2626;
        }
        
        .update-banner-button-primary:hover {
            background: #f5f5f5;
            transform: translateY(-1px);
        }
        
        .update-banner-button-secondary {
            background: rgba(255, 255, 255, 0.2);
            color: white;
            border: 1px solid rgba(255, 255, 255, 0.3);
        }
        
        .update-banner-button-secondary:hover {
            background: rgba(255, 255, 255, 0.3);
        }
        
        @media (max-width: 768px) {
            .update-banner-content {
                flex-direction: column;
                text-align: center;
                gap: 15px;
            }
            
            .update-banner-actions {
                width: 100%;
                justify-content: center;
            }
            
            .update-banner-button {
                flex: 1;
                max-width: 120px;
            }
        }
    `;
    
    document.head.appendChild(style);
    document.body.appendChild(banner);
    
    // Show banner with animation
    setTimeout(() => {
        banner.style.transform = 'translateY(0)';
    }, 100);
    
    updateState.bannerElement = banner;
    
    // Auto-hide after delay
    setTimeout(() => {
        if (updateState.bannerElement) {
            dismissUpdate();
        }
    }, UPDATE_CONFIG.autoHideDelay);
}

/**
 * Apply update
 */
async function applyUpdate() {
    console.log('🔄 Applying update...');
    
    try {
        // Show loading state
        if (updateState.bannerElement) {
            updateState.bannerElement.innerHTML = `
                <div class="update-banner-content">
                    <div class="update-banner-icon">⏳</div>
                    <div class="update-banner-text">
                        <div class="update-banner-title">Updating...</div>
                        <div class="update-banner-message">Please wait while we apply the update.</div>
                    </div>
                </div>
            `;
        }
        
        // Check if service worker is available
        if ('serviceWorker' in navigator) {
            const registration = await navigator.serviceWorker.ready;
            
            if (registration.waiting) {
                // Tell the waiting service worker to skip waiting
                registration.waiting.postMessage({ type: 'SKIP_WAITING' });
                
                // Wait for the new service worker to take control
                await new Promise((resolve) => {
                    navigator.serviceWorker.addEventListener('controllerchange', resolve, { once: true });
                });
            }
        }
        
        // Reload the page
        window.location.reload();
        
    } catch (error) {
        console.error('❌ Failed to apply update:', error);
        
        // Show error message
        if (updateState.bannerElement) {
            updateState.bannerElement.innerHTML = `
                <div class="update-banner-content">
                    <div class="update-banner-icon">❌</div>
                    <div class="update-banner-text">
                        <div class="update-banner-title">Update Failed</div>
                        <div class="update-banner-message">Please refresh the page manually.</div>
                    </div>
                    <div class="update-banner-actions">
                        <button class="update-banner-button update-banner-button-primary" onclick="window.location.reload()">
                            Refresh
                        </button>
                    </div>
                </div>
            `;
        }
    }
}

/**
 * Dismiss update banner
 */
function dismissUpdate() {
    if (!updateState.bannerElement) {
        return;
    }
    
    console.log('❌ Dismissing update banner...');
    
    // Hide with animation
    updateState.bannerElement.style.transform = 'translateY(-100%)';
    
    // Remove after animation
    setTimeout(() => {
        if (updateState.bannerElement && updateState.bannerElement.parentElement) {
            updateState.bannerElement.parentElement.removeChild(updateState.bannerElement);
        }
        updateState.bannerElement = null;
    }, 300);
}

// ===== SERVICE WORKER INTEGRATION =====

/**
 * Register service worker
 */
async function registerServiceWorker() {
    if (!('serviceWorker' in navigator)) {
        console.log('⚠️ Service Worker not supported');
        return false;
    }
    
    try {
        console.log('🔄 Registering service worker...');
        
        const registration = await navigator.serviceWorker.register('/sw-optimized.js');
        
        console.log('✅ Service Worker registered:', registration.scope);
        
        // Listen for updates
        registration.addEventListener('updatefound', () => {
            console.log('🆕 Service Worker update found');
            
            const newWorker = registration.installing;
            if (newWorker) {
                newWorker.addEventListener('statechange', () => {
                    if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                        console.log('🆕 New service worker installed, showing update banner');
                        showUpdateBanner();
                    }
                });
            }
        });
        
        return true;
        
    } catch (error) {
        console.error('❌ Service Worker registration failed:', error);
        return false;
    }
}

// ===== INITIALIZATION =====

/**
 * Initialize update system
 */
async function initializeUpdateSystem() {
    console.log('🚀 Initializing update system...');
    
    try {
        // Register service worker
        await registerServiceWorker();
        
        // Start version checking
        startVersionChecking();
        
        console.log('✅ Update system initialized');
        return true;
        
    } catch (error) {
        console.error('❌ Failed to initialize update system:', error);
        return false;
    }
}

/**
 * Start version checking
 */
function startVersionChecking() {
    // Initial check
    checkForUpdates();
    
    // Set up periodic checking
    updateState.checkTimer = setInterval(checkForUpdates, UPDATE_CONFIG.checkInterval);
    
    console.log('🔄 Version checking started');
}

/**
 * Stop version checking
 */
function stopVersionChecking() {
    if (updateState.checkTimer) {
        clearInterval(updateState.checkTimer);
        updateState.checkTimer = null;
    }
    
    console.log('🛑 Version checking stopped');
}

// ===== EXPORTS =====

// Make functions available globally
window.UpdateBanner = {
    checkForUpdates,
    applyUpdate,
    dismissUpdate,
    initializeUpdateSystem,
    startVersionChecking,
    stopVersionChecking,
    getUpdateState: () => ({ ...updateState })
};

// Auto-initialize if this script is loaded
document.addEventListener('DOMContentLoaded', () => {
    initializeUpdateSystem();
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    stopVersionChecking();
});
