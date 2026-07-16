/**
 * ============================================
 * 241 RUNNERS AWARENESS - CONFIGURATION
 * ============================================
 * 
 * Centralized configuration for the static site
 */

// Single source of truth: all APIs point to 241RunnersAPI (this repo).
window.APP_CONFIG = {
    API_BASE_URL: "https://241runners-api-v2.azurewebsites.net/api",
    APP_NAME: "241 Runners Awareness",
    APP_VERSION: "1.0.0",
    ENVIRONMENT: "production",
    DEBUG: false,
    HEALTH_CHECK_ENDPOINT: "/v1/auth/health",
    API_TIMEOUT: 10000,
    RETRY_ATTEMPTS: 3,
    // Google Maps API key — create at https://console.cloud.google.com/
    // Same value as mobile EXPO_PUBLIC_GOOGLE_MAPS_API_KEY; overridden by config.json when present
    GOOGLE_MAPS_API_KEY: "YOUR_GOOGLE_MAPS_API_KEY"
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

    // Local override (gitignored) — keeps real Maps keys out of tracked config.json
    try {
        const localResponse = await fetch('/config.local.json');
        if (localResponse.ok) {
            const localConfig = await localResponse.json();
            window.APP_CONFIG = {
                ...window.APP_CONFIG,
                ...localConfig
            };
        }
    } catch (error) {
        // config.local.json is optional
    }
}

// Initialize config on load
loadConfig(); 