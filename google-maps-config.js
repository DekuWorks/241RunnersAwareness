/**
 * ============================================
 * GOOGLE MAPS CONFIGURATION
 * ============================================
 *
 * Shared map settings for map.html (aligned with mobile EXPO_PUBLIC_GOOGLE_MAPS_API_KEY).
 * Set GOOGLE_MAPS_API_KEY in config.json, or replace YOUR_GOOGLE_MAPS_API_KEY below.
 * Enable Maps JavaScript API in Google Cloud Console — restrict by URL in production.
 */

window.GOOGLE_MAPS_CONFIG = {
  API_KEY: 'YOUR_GOOGLE_MAPS_API_KEY',

  DEFAULT_CENTER: {
    lat: 29.7604,
    lng: -95.3698,
  },

  DEFAULT_ZOOM: 10,

  HOUSTON_RADIUS_MILES: 64.4,

  MAP_STYLES: [
    {
      featureType: 'poi',
      elementType: 'all',
      stylers: [{ visibility: 'off' }],
    },
    {
      featureType: 'transit',
      elementType: 'all',
      stylers: [{ visibility: 'off' }],
    },
  ],

  HEATMAP_CONFIG: {
    radius: 20,
    opacity: 0.6,
  },

  STATUS_COLORS: {
    missing: '#f59e0b',
    found: '#dc2626',
    safe: '#10b981',
    urgent: '#ef4444',
    resolved: '#33cc33',
    deceased: '#6b7280',
    resolved_pending_verify: '#8b5cf6',
  },
};

(function applyGoogleMapsKeyFromAppConfig() {
  function merge() {
    const fromApp = window.APP_CONFIG && window.APP_CONFIG.GOOGLE_MAPS_API_KEY;
    if (fromApp && fromApp !== 'YOUR_GOOGLE_MAPS_API_KEY') {
      window.GOOGLE_MAPS_CONFIG.API_KEY = fromApp;
    }
  }
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', merge);
  } else {
    setTimeout(merge, 100);
  }
})();

console.log(
  '🗺️ Google Maps configuration loaded; API key set:',
  window.GOOGLE_MAPS_CONFIG.API_KEY !== 'YOUR_GOOGLE_MAPS_API_KEY'
);
