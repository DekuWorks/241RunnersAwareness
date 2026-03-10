/**
 * Public Map API — no auth required
 * GET /api/public/map/missing — missing cases with coordinates.
 */

(function () {
    'use strict';

    const API_BASE = window.APP_CONFIG?.API_BASE_URL || 'https://241runners-api-v2.azurewebsites.net/api';

    /**
     * GET /api/public/map/missing
     * @returns {Promise<Array>} Array of { id, displayName|publicDisplayName, status, lat|latitude, lng|longitude, photoUrl, lastSeenCity, lastSeenState, updatedAt }
     */
    async function getMissingMapCases() {
        const res = await fetch(`${API_BASE}/public/map/missing`, { credentials: 'omit' });
        if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
        const data = await res.json();
        return Array.isArray(data) ? data : [];
    }

    /**
     * Normalize map item for UI: lat/lng, displayName
     */
    function normalizeMapItem(item) {
        const lat = item.latitude ?? item.lat;
        const lng = item.longitude ?? item.lng;
        const displayName = item.publicDisplayName ?? item.displayName ?? item.fullName ?? `Case #${item.id}`;
        return {
            id: item.id,
            displayName,
            status: item.status || 'Missing',
            latitude: lat,
            longitude: lng,
            photoUrl: item.photoUrl || null,
            lastSeenCity: item.lastSeenCity || '',
            lastSeenState: item.lastSeenState || '',
            lastSeenCityState: [item.lastSeenCity, item.lastSeenState].filter(Boolean).join(', ') || 'Unknown',
            updatedAt: item.updatedAt || null
        };
    }

    window.publicMapApi = {
        getMissingMapCases,
        normalizeMapItem,
        API_BASE
    };
})();
