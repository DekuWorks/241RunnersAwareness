/**
 * ============================================
 * 241 RUNNERS AWARENESS - CONFIGURATION
 * ============================================
 * 
 * Centralized configuration for the static site
 */

window.APP_CONFIG = {
    API_BASE_URL: "https://241runners-api.azurewebsites.net/api",
    APP_NAME: "241 Runners Awareness",
    APP_VERSION: "1.0.0",
    ENVIRONMENT: "production",
    DEBUG: false
};

// Load configuration from config.json if available
async function loadConfig() {
    try {
        const response = await fetch('/config.json');
        const config = await response.json();
        
        // Merge with default config
        window.APP_CONFIG = {
            ...window.APP_CONFIG,
            ...config
        };
        
        console.log('Configuration loaded:', window.APP_CONFIG);
    } catch (error) {
        console.warn('Failed to load config.json, using default configuration');
    }
}

// Initialize config on load
loadConfig(); 