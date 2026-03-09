/**
 * Public Cases Page JavaScript
 * Uses /api/public/cases (PublicCaseDto). Works signed-in or signed-out.
 * Fallback: legacy publiccases until backend deploys new API.
 */

class PublicCasesManager {
    constructor() {
        this.currentCases = [];
        this.filteredCases = [];
        this.totalCount = 0;
        this.currentPage = 1;
        this.pageSize = 20;
        this.currentFilter = 'all';
        this.currentSearch = '';
        this.currentCity = '';
        this.currentCounty = '';
        this.init();
    }

    /** Normalize case item for PublicCaseDto or legacy API shape */
    normalizeCase(item) {
        const status = (item.status || item.Status || '').toString().toLowerCase();
        return {
            id: item.id,
            displayName: item.publicDisplayName || item.fullName || item.FullName || item.NamusCaseNumber || `Case #${item.id}`,
            status,
            lastSeenCity: item.lastSeenCity || item.City || '',
            lastSeenState: item.lastSeenState || item.State || item.County || '',
            lastSeenAt: item.lastSeenAt || item.DateMissing || item.dateMissing || null,
            updatedAt: item.updatedAt || item.UpdatedAt || null,
            photoUrl: item.photoUrl || item.PhotoUrl || null,
            descriptionShort: item.descriptionShort || item.description || '',
            ageRange: item.ageRange || (item.age != null ? String(item.age) : null) || item.AgeAtMissing,
            namusCaseNumber: item.NamusCaseNumber || item.namusCaseNumber || null
        };
    }

    /**
     * Initialize the page
     */
    init() {
        this.bindEvents();
        this.loadStatistics();
        this.loadCases();
    }

    /**
     * Bind event listeners
     */
    bindEvents() {
        // Filter tabs
        document.querySelectorAll('.filter-tab').forEach(tab => {
            tab.addEventListener('click', (e) => {
                this.setActiveFilter(e.target.dataset.filter);
            });
        });

        // Search button
        document.getElementById('searchBtn').addEventListener('click', () => {
            this.performSearch();
        });

        // Search input (with debouncing) — reload from API when using new public/cases
        let searchTimeout;
        const nameSearch = document.getElementById('nameSearch');
        if (nameSearch) {
            nameSearch.addEventListener('input', (e) => {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    this.currentSearch = e.target.value.trim();
                    this.currentPage = 1;
                    this.loadCases();
                }, 400);
            });
        }

        // City / County filters (client-side when list already loaded)
        const cityFilter = document.getElementById('cityFilter');
        const countyFilter = document.getElementById('countyFilter');
        if (cityFilter) cityFilter.addEventListener('change', (e) => { this.currentCity = e.target.value; this.applyFilters(); });
        if (countyFilter) countyFilter.addEventListener('change', (e) => { this.currentCounty = e.target.value; this.applyFilters(); });

        // Pagination
        document.getElementById('prevBtn').addEventListener('click', () => {
            this.previousPage();
        });

        document.getElementById('nextBtn').addEventListener('click', () => {
            this.nextPage();
        });
    }

    /**
     * Set active filter tab
     */
    setActiveFilter(filter) {
        const tab = document.querySelector(`.filter-tab[data-filter="${filter}"]`);
        if (tab) {
            document.querySelectorAll('.filter-tab').forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
        }
        this.currentFilter = filter;
        this.currentPage = 1;
        this.loadCases();
    }

    /**
     * Perform search with current criteria (reload from API)
     */
    performSearch() {
        this.currentPage = 1;
        this.loadCases();
    }

    /**
     * Apply client-side filters (search, city, county) to current list
     */
    applyFilters() {
        const statusKey = (s) => (s || '').toString().toLowerCase();
        this.filteredCases = this.currentCases.filter(item => {
            const c = this.normalizeCase(item);
            if (this.currentFilter !== 'all') {
                if (this.currentFilter === 'missing' && statusKey(c.status) !== 'missing') return false;
                if (this.currentFilter === 'found' && statusKey(c.status) !== 'found') return false;
                if (this.currentFilter === 'safe' && statusKey(c.status) !== 'safe') return false;
                if (this.currentFilter === 'urgent' && statusKey(c.status) !== 'urgent') return false;
                if (this.currentFilter === 'resolved' && !['found', 'safe', 'deceased', 'resolved'].includes(statusKey(c.status))) return false;
                if (this.currentFilter === 'pending' && statusKey(c.status) !== 'resolved_pending_verify') return false;
            }
            if (this.currentSearch && !c.displayName.toLowerCase().includes(this.currentSearch.toLowerCase())) return false;
            if (this.currentCity && c.lastSeenCity !== this.currentCity) return false;
            if (this.currentCounty && c.lastSeenState !== this.currentCounty) return false;
            return true;
        });
        this.renderCases();
        this.updatePagination();
    }

    /**
     * Load case statistics (optional stats endpoint or derive from list)
     */
    async loadStatistics() {
        try {
            const api = window.publicCasesApi;
            if (!api) return;
            const base = api.API_BASE || '';
            const response = await fetch(`${base}/publiccases/stats/houston`, { credentials: 'omit' });
            if (response.ok) {
                const stats = await response.json();
                this.updateStatistics(stats);
            }
        } catch (e) {
            console.warn('Stats endpoint not available, using counts from list.');
        }
    }

    /**
     * Update statistics display (from stats API or from current list)
     */
    updateStatistics(stats) {
        const total = stats?.totalCases ?? this.totalCount ?? this.currentCases.length;
        const missing = stats?.missingCases ?? this.currentCases.filter(c => this.normalizeCase(c).status === 'missing').length;
        const resolved = stats?.resolvedCases ?? this.currentCases.filter(c => ['found', 'safe', 'deceased', 'resolved'].includes(this.normalizeCase(c).status)).length;
        const pending = stats?.breakdown?.find(s => (s.Status || s.status || '').toString().toLowerCase() === 'resolved_pending_verify')?.Count ?? this.currentCases.filter(c => this.normalizeCase(c).status === 'resolved_pending_verify').length ?? 0;
        const el = (id) => document.getElementById(id);
        if (el('totalCases')) el('totalCases').textContent = total;
        if (el('missingCases')) el('missingCases').textContent = missing;
        if (el('resolvedCases')) el('resolvedCases').textContent = resolved;
        if (el('pendingCases')) el('pendingCases').textContent = pending;
    }

    /**
     * Load cases from API: GET /api/public/cases (with fallback to legacy publiccases)
     */
    async loadCases() {
        this.showLoading();
        try {
            const api = window.publicCasesApi;
            if (!api) throw new Error('publicCasesApi not loaded');
            const statusParam = this.currentFilter !== 'all' ? this.currentFilter : undefined;
            const result = await api.getPublicCasesWithFallback({
                status: statusParam,
                q: this.currentSearch || undefined,
                page: this.currentPage,
                pageSize: this.pageSize,
                sort: 'updated'
            });
            this.currentCases = result.items || [];
            this.totalCount = result.totalCount ?? this.currentCases.length;
            this.updateStatistics(null);
            this.applyFilters();
        } catch (error) {
            console.error('Error loading cases:', error);
            this.showError('Failed to load cases. Please try again later.');
        } finally {
            this.hideLoading();
        }
    }

    /**
     * Render cases grid
     */
    renderCases() {
        const grid = document.getElementById('casesGrid');
        
        if (this.filteredCases.length === 0) {
            this.showNoCases();
            return;
        }

        this.hideNoCases();

        const startIndex = (this.currentPage - 1) * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        const casesToShow = this.filteredCases.slice(startIndex, endIndex);

        grid.innerHTML = casesToShow.map(caseItem => this.createCaseCard(caseItem)).join('');
    }

    /**
     * Create case card HTML (PublicCaseDto-safe fields only)
     */
    createCaseCard(caseItem) {
        const c = this.normalizeCase(caseItem);
        const statusClass = this.getStatusClass(c.status);
        const statusText = this.getStatusText(c.status);
        const lastSeenDate = c.lastSeenAt ? new Date(c.lastSeenAt).toLocaleDateString() : (c.updatedAt ? new Date(c.updatedAt).toLocaleDateString() : '—');
        const ageText = c.ageRange != null && c.ageRange !== '' ? (typeof c.ageRange === 'number' ? c.ageRange + ' years' : String(c.ageRange)) : '—';
        const location = [c.lastSeenCity, c.lastSeenState].filter(Boolean).join(', ') || '—';
        const detailUrl = `case-detail.html?id=${encodeURIComponent(c.id)}`;

        return `
            <div class="case-card">
                <div class="case-header">
                    <div class="case-name">${this.escapeHtml(c.displayName)}</div>
                    ${c.namusCaseNumber ? `<div class="case-id">${this.escapeHtml(c.namusCaseNumber)}</div>` : ''}
                    <div class="status-badge ${statusClass}">${statusText}</div>
                </div>

                <div class="case-photo">
                    ${c.photoUrl ?
                        `<img src="${this.escapeHtml(c.photoUrl)}" alt="">` :
                        '<span aria-hidden="true">📷</span>'
                    }
                </div>

                <div class="case-details">
                    <div class="detail-row">
                        <span class="detail-label">Last seen:</span>
                        <span class="detail-value">${lastSeenDate}</span>
                    </div>
                    <div class="detail-row">
                        <span class="detail-label">Age / range:</span>
                        <span class="detail-value">${this.escapeHtml(ageText)}</span>
                    </div>
                    <div class="detail-row">
                        <span class="detail-label">Location:</span>
                        <span class="detail-value">${this.escapeHtml(location)}</span>
                    </div>
                    ${c.updatedAt ? `
                    <div class="detail-row">
                        <span class="detail-label">Updated:</span>
                        <span class="detail-value">${new Date(c.updatedAt).toLocaleDateString()}</span>
                    </div>
                    ` : ''}
                </div>

                <div class="case-actions">
                    <a href="${this.escapeHtml(detailUrl)}" class="btn btn-primary">View details</a>
                </div>
            </div>
        `;
    }

    /**
     * Get CSS class for status badge
     */
    getStatusClass(status) {
        const s = (status || '').toString().toLowerCase();
        switch (s) {
            case 'missing': return 'status-missing';
            case 'found': case 'safe': case 'deceased': case 'resolved': return 'status-resolved';
            case 'urgent': return 'status-urgent';
            case 'resolved_pending_verify': return 'status-pending';
            default: return 'status-missing';
        }
    }

    /**
     * Get display text for status
     */
    getStatusText(status) {
        const s = (status || '').toString().toLowerCase();
        switch (s) {
            case 'missing': return 'Missing';
            case 'found': return 'Found';
            case 'safe': return 'Safe';
            case 'urgent': return 'Urgent';
            case 'deceased': case 'resolved': return s === 'deceased' ? 'Deceased' : 'Resolved';
            case 'resolved_pending_verify': return 'Pending';
            default: return status ? String(status) : 'Unknown';
        }
    }

    /**
     * Update pagination controls
     */
    updatePagination() {
        const totalPages = Math.ceil(this.filteredCases.length / this.pageSize);
        const pagination = document.getElementById('pagination');
        const pageInfo = document.getElementById('pageInfo');
        const prevBtn = document.getElementById('prevBtn');
        const nextBtn = document.getElementById('nextBtn');

        if (totalPages <= 1) {
            pagination.style.display = 'none';
            return;
        }

        pagination.style.display = 'flex';
        pageInfo.textContent = `Page ${this.currentPage} of ${totalPages}`;
        
        prevBtn.disabled = this.currentPage <= 1;
        nextBtn.disabled = this.currentPage >= totalPages;
    }

    /**
     * Go to previous page
     */
    previousPage() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.renderCases();
            this.updatePagination();
        }
    }

    /**
     * Go to next page
     */
    nextPage() {
        const totalPages = Math.ceil(this.filteredCases.length / this.pageSize);
        if (this.currentPage < totalPages) {
            this.currentPage++;
            this.renderCases();
            this.updatePagination();
        }
    }

    /**
     * Show loading state
     */
    showLoading() {
        document.getElementById('loadingState').style.display = 'block';
        document.getElementById('casesGrid').style.display = 'none';
        document.getElementById('noCasesState').style.display = 'none';
        document.getElementById('pagination').style.display = 'none';
    }

    /**
     * Hide loading state
     */
    hideLoading() {
        document.getElementById('loadingState').style.display = 'none';
        document.getElementById('casesGrid').style.display = 'grid';
    }

    /**
     * Show no cases state
     */
    showNoCases() {
        document.getElementById('noCasesState').style.display = 'block';
        document.getElementById('casesGrid').style.display = 'none';
        document.getElementById('pagination').style.display = 'none';
    }

    /**
     * Hide no cases state
     */
    hideNoCases() {
        document.getElementById('noCasesState').style.display = 'none';
        document.getElementById('casesGrid').style.display = 'grid';
    }

    /**
     * Show error message
     */
    showError(message) {
        const grid = document.getElementById('casesGrid');
        grid.innerHTML = `
            <div style="grid-column: 1 / -1; text-align: center; padding: 40px; color: #dc2626;">
                <h3>Error Loading Cases</h3>
                <p>${this.escapeHtml(message)}</p>
                <button onclick="location.reload()" class="btn btn-primary">Retry</button>
            </div>
        `;
    }

    /**
     * Escape HTML to prevent XSS
     */
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new PublicCasesManager();
}); 