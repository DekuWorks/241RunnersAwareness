/**
 * ============================================
 * GOOGLE MAPS CONFIGURATION
 * ============================================
 * 
 * Configuration for Google Maps API integration
 * Replace YOUR_GOOGLE_MAPS_API_KEY with your actual API key
 */

window.GOOGLE_MAPS_CONFIG = {
    // Replace this with your actual Google Maps API key from your mobile repo
    API_KEY: 'YOUR_GOOGLE_MAPS_API_KEY',
    
    // Map configuration
    DEFAULT_CENTER: {
        lat: 29.7604,
        lng: -95.3698
    },
    DEFAULT_ZOOM: 10,
    HOUSTON_RADIUS_MILES: 64.4,
    
    // Map styles
    MAP_STYLES: [
        {
            featureType: "poi",
            elementType: "labels",
            stylers: [{ visibility: "off" }]
        }
    ],
    
    // Marker cluster configuration
    CLUSTER_CONFIG: {
        maxZoom: 15,
        gridSize: 50
    },
    
    // Heatmap configuration
    HEATMAP_CONFIG: {
        radius: 50,
        opacity: 0.6
    }
};

// Helper function to get the API key
function getGoogleMapsApiKey() {
    return window.GOOGLE_MAPS_CONFIG.API_KEY;
}

// Helper function to check if API key is configured
function isGoogleMapsConfigured() {
    const apiKey = getGoogleMapsApiKey();
    return apiKey && apiKey !== 'YOUR_GOOGLE_MAPS_API_KEY';
}
