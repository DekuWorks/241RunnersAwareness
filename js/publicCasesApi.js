/**
 * Public Cases API — no auth required.
 * Primary: 241RunnersAPI /api/public/cases (PublicCaseDto)
 * Fallback: legacy /api/v1.0/cases/publiccases while older endpoints exist.
 */

(function () {
    'use strict';

    function getApiBase() {
        return window.APP_CONFIG?.API_BASE_URL || 'https://241runners-api-v2.azurewebsites.net/api';
    }

    /**
     * GET /api/public/cases
     * @param {Object} opts - { status, q, page, pageSize, sort }
     * @returns {Promise<{ items: Array, totalCount?: number }|Array>}
     */
    async function getPublicCases(opts = {}) {
        const API_BASE = getApiBase();
        const params = new URLSearchParams();
        if (opts.status) params.set('status', opts.status);
        if (opts.q) params.set('search', opts.q);
        if (opts.page != null) params.set('page', opts.page);
        if (opts.pageSize != null) params.set('pageSize', opts.pageSize);
        if (opts.region) params.set('region', opts.region);

        const publicUrl = `${API_BASE}/public/cases?${params.toString()}`;

        // Try new public API first
        try {
            const res = await fetch(publicUrl, { credentials: 'omit' });
            if (res.ok) {
                const data = await res.json();
                if (Array.isArray(data)) return { items: data, totalCount: data.length };
                if (data.items) return { items: data.items, totalCount: data.totalCount ?? data.items.length };
                if (data.cases) return { items: data.cases, totalCount: data.pagination?.totalCount ?? data.cases.length };
                return { items: [], totalCount: 0 };
            }
            // If the new endpoint exists but returns an error, fall through to legacy
            console.warn('Public cases API returned non-OK status, falling back to legacy:', res.status);
        } catch (err) {
            console.warn('Public cases API failed, falling back to legacy publiccases endpoint:', err);
        }

        // Fallback: legacy /v1.0/cases/publiccases
        return getLegacyPublicCases(opts);
    }

    /**
     * GET /api/public/cases/{id}
     * @param {string|number} id - Case id
     * @returns {Promise<Object>} PublicCaseDto
     */
    async function getPublicCase(id) {
        const API_BASE = getApiBase();

        // Try new public case endpoint first
        const publicUrl = `${API_BASE}/public/cases/${encodeURIComponent(id)}`;
        try {
            const res = await fetch(publicUrl, { credentials: 'omit' });
            if (res.status === 404) return null;
            if (res.ok) return res.json();
            console.warn('Public case API returned non-OK status, falling back to legacy:', res.status);
        } catch (err) {
            console.warn('Public case API failed, falling back to legacy case endpoint:', err);
        }

        // Fallback: legacy case endpoint used by older APIs
        const legacyUrl = `${API_BASE}/v1.0/cases/${encodeURIComponent(id)}`;
        const legacyRes = await fetch(legacyUrl, { credentials: 'omit' });
        if (legacyRes.status === 404) return null;
        if (!legacyRes.ok) throw new Error(`HTTP ${legacyRes.status}: ${legacyRes.statusText}`);
        return legacyRes.json();
    }

    /** Single entry: 241RunnersAPI public cases (with built-in fallback). */
    async function getPublicCasesWithFallback(opts = {}) {
        return getPublicCases(opts);
    }

    function parseCasesResponse(data) {
        if (data && Array.isArray(data.cases)) return data.cases;
        if (Array.isArray(data)) return data;
        if (data && Array.isArray(data.items)) return data.items;
        if (data && Array.isArray(data.data)) return data.data;
        return [];
    }

    function getLegacyPublicCases(opts) {
        const API_BASE = getApiBase();
        const params = new URLSearchParams();
        if (opts.page != null) params.set('page', opts.page);
        if (opts.pageSize != null) params.set('pageSize', opts.pageSize);
        const url = `${API_BASE}/v1.0/cases/publiccases?${params.toString()}`;
        return fetch(url, { credentials: 'omit' })
            .then(r => { if (!r.ok) throw new Error('HTTP ' + r.status); return r.json(); })
            .then(data => {
                const items = parseCasesResponse(data);
                return { items, totalCount: data.pagination?.totalCount ?? items.length };
            });
    }

    window.publicCasesApi = {
        getPublicCases,
        getPublicCase,
        getPublicCasesWithFallback,
        get API_BASE() { return getApiBase(); }
    };
})();
