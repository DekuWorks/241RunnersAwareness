/**
 * Public Map API — no auth required
 * GET /api/public/map/missing — missing cases with coordinates.
 * GET /api/public/map/runners — community runners who opted in (ShowOnMap), privacy-safe.
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
     * GET /api/public/map/runners
     * Community runners who opted in to show on map (minimal info only).
     * @returns {Promise<Array>} Array of { id, publicDisplayName, status, latitude, longitude, photoUrl, lastSeenCity, lastSeenState, updatedAt, type: 'community' }
     */
    async function getCommunityRunners() {
        const res = await fetch(`${API_BASE}/public/map/runners`, { credentials: 'omit' });
        if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
        const data = await res.json();
        return Array.isArray(data) ? data : [];
    }

    /**
     * Normalize map item for UI: lat/lng, displayName
     * @param {Object} item - from missing or runners API
     * @param {string} [defaultType] - 'missing' | 'community'
     */
    function normalizeMapItem(item, defaultType) {
        const lat = item.latitude ?? item.lat;
        const lng = item.longitude ?? item.lng;
        const displayName = item.publicDisplayName ?? item.displayName ?? item.fullName ?? (item.type === 'community' ? `Runner #${item.id}` : `Case #${item.id}`);
        return {
            id: item.id,
            displayName,
            status: item.status || (item.type === 'community' ? 'Active' : 'Missing'),
            latitude: lat,
            longitude: lng,
            photoUrl: item.photoUrl || null,
            lastSeenCity: item.lastSeenCity || '',
            lastSeenState: item.lastSeenState || '',
            lastSeenCityState: [item.lastSeenCity, item.lastSeenState].filter(Boolean).join(', ') || 'Unknown',
            updatedAt: item.updatedAt || null,
            type: item.type || defaultType || 'missing'
        };
    }

    window.publicMapApi = {
        getMissingMapCases,
        getCommunityRunners,
        normalizeMapItem,
        API_BASE
    };
})();
