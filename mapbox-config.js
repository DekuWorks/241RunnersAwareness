/**
 * ============================================
 * MAPBOX CONFIGURATION
 * ============================================
 *
 * Shared map settings for map.html (aligned with mobile EXPO_PUBLIC_MAPBOX_ACCESS_TOKEN).
 * Set MAPBOX_ACCESS_TOKEN in config.json, or replace YOUR_MAPBOX_ACCESS_TOKEN below.
 * Create tokens at https://account.mapbox.com/ — restrict by URL in production.
 */

window.MAPBOX_CONFIG = {
  // Public token (pk.*) — override via config.json MAPBOX_ACCESS_TOKEN
  ACCESS_TOKEN: 'YOUR_MAPBOX_ACCESS_TOKEN',

  // Default map center (Houston, TX) — matches mobile default
  DEFAULT_CENTER: {
    lat: 29.7604,
    lng: -95.3698,
  },

  DEFAULT_ZOOM: 10,

  // Mapbox style URL (streets-v12 matches Mapbox.StyleURL.Street on mobile)
  STYLE_URL: 'mapbox://styles/mapbox/streets-v12',

  HOUSTON_RADIUS_MILES: 64.4,

  CLUSTER_MAX_ZOOM: 14,
  CLUSTER_RADIUS: 50,

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
  },
};

// Merge token from APP_CONFIG when config.json loads
(function applyMapboxTokenFromAppConfig() {
  function merge() {
    const fromApp = window.APP_CONFIG && window.APP_CONFIG.MAPBOX_ACCESS_TOKEN;
    if (fromApp && fromApp !== 'YOUR_MAPBOX_ACCESS_TOKEN') {
      window.MAPBOX_CONFIG.ACCESS_TOKEN = fromApp;
    }
    if (window.APP_CONFIG && window.APP_CONFIG.MAPBOX_STYLE_URL) {
      window.MAPBOX_CONFIG.STYLE_URL = window.APP_CONFIG.MAPBOX_STYLE_URL;
    }
  }
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', merge);
  } else {
    setTimeout(merge, 100);
  }
})();

console.log(
  '🗺️ Mapbox configuration loaded; token set:',
  window.MAPBOX_CONFIG.ACCESS_TOKEN !== 'YOUR_MAPBOX_ACCESS_TOKEN'
);
