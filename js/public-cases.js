/**
 * Public Cases Page JavaScript
 * Handles loading and displaying NamUs missing persons cases for the Houston area
 */

class PublicCasesManager {
    constructor() {
        this.apiBaseUrl = 'https://241runners-api.azurewebsites.net/api';
        this.currentCases = [];
        this.filteredCases = [];
        this.currentPage = 1;
        this.pageSize = 20;
        this.currentFilter = 'all';
        this.currentSearch = '';
        this.currentCity = '';
        this.currentCounty = '';
        
        this.init();
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

        // Search input (with debouncing)
        let searchTimeout;
        document.getElementById('nameSearch').addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                this.currentSearch = e.target.value.trim();
                this.applyFilters();
            }, 300);
        });

        // City filter
        document.getElementById('cityFilter').addEventListener('change', (e) => {
            this.currentCity = e.target.value;
            this.applyFilters();
        });

        // County filter
        document.getElementById('countyFilter').addEventListener('change', (e) => {
            this.currentCounty = e.target.value;
            this.applyFilters();
        });

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
        // Update active tab
        document.querySelectorAll('.filter-tab').forEach(tab => {
            tab.classList.remove('active');
        });
        document.querySelector(`[data-filter="${filter}"]`).classList.add('active');

        this.currentFilter = filter;
        this.currentPage = 1;
        this.applyFilters();
    }

    /**
     * Perform search with current criteria
     */
    performSearch() {
        this.currentPage = 1;
        this.applyFilters();
    }

    /**
     * Apply all current filters
     */
    applyFilters() {
        this.filteredCases = this.currentCases.filter(caseItem => {
            // Status filter
            if (this.currentFilter !== 'all') {
                if (this.currentFilter === 'missing' && caseItem.Status !== 'missing') return false;
                if (this.currentFilter === 'resolved' && 
                    !['found', 'safe', 'deceased'].includes(caseItem.Status)) return false;
                if (this.currentFilter === 'pending' && caseItem.Status !== 'resolved_pending_verify') return false;
            }

            // Name search
            if (this.currentSearch && !caseItem.FullName.toLowerCase().includes(this.currentSearch.toLowerCase())) {
                return false;
            }

            // City filter
            if (this.currentCity && caseItem.City !== this.currentCity) {
                return false;
            }

            // County filter
            if (this.currentCounty && caseItem.County !== this.currentCounty) {
                return false;
            }

            return true;
        });

        this.renderCases();
        this.updatePagination();
    }

    /**
     * Load case statistics
     */
    async loadStatistics() {
        try {
            const response = await fetch(`${this.apiBaseUrl}/publiccases/stats/houston`);
            if (response.ok) {
                const stats = await response.json();
                this.updateStatistics(stats);
            }
        } catch (error) {
            console.error('Error loading statistics:', error);
        }
    }

    /**
     * Update statistics display
     */
    updateStatistics(stats) {
        document.getElementById('totalCases').textContent = stats.totalCases || 0;
        document.getElementById('missingCases').textContent = stats.missingCases || 0;
        document.getElementById('resolvedCases').textContent = stats.resolvedCases || 0;
        
        const pendingCases = stats.breakdown?.find(s => s.Status === 'resolved_pending_verify')?.Count || 0;
        document.getElementById('pendingCases').textContent = pendingCases;
    }

    /**
     * Load cases from API
     */
    async loadCases() {
        this.showLoading();

        try {
            const params = new URLSearchParams({
                region: 'houston',
                page: this.currentPage,
                pageSize: this.pageSize
            });

            const response = await fetch(`${this.apiBaseUrl}/publiccases?${params}`);
            
            if (response.ok) {
                const cases = await response.json();
                this.currentCases = cases;
                this.applyFilters();
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
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
     * Create case card HTML
     */
    createCaseCard(caseItem) {
        const statusClass = this.getStatusClass(caseItem.Status);
        const statusText = this.getStatusText(caseItem.Status);
        const dateMissing = caseItem.DateMissing ? new Date(caseItem.DateMissing).toLocaleDateString() : 'Unknown';
        const ageText = caseItem.AgeAtMissing ? `${caseItem.AgeAtMissing} years` : 'Unknown';
        const location = [caseItem.City, caseItem.County, caseItem.State].filter(Boolean).join(', ');

        return `
            <div class="case-card">
                <div class="case-header">
                    <div class="case-name">${this.escapeHtml(caseItem.FullName)}</div>
                    <div class="case-id">${this.escapeHtml(caseItem.NamusCaseNumber)}</div>
                    <div class="status-badge ${statusClass}">${statusText}</div>
                </div>

                <div class="case-photo">
                    ${caseItem.PhotoUrl ? 
                        `<img src="${this.escapeHtml(caseItem.PhotoUrl)}" alt="Photo of ${this.escapeHtml(caseItem.FullName)}">` : 
                        '📷'
                    }
                </div>

                <div class="case-details">
                    <div class="detail-row">
                        <span class="detail-label">Date Missing:</span>
                        <span class="detail-value">${dateMissing}</span>
                    </div>
                    <div class="detail-row">
                        <span class="detail-label">Age at Missing:</span>
                        <span class="detail-value">${ageText}</span>
                    </div>
                    <div class="detail-row">
                        <span class="detail-label">Sex:</span>
                        <span class="detail-value">${this.escapeHtml(caseItem.Sex || 'Unknown')}</span>
                    </div>
                    <div class="detail-row">
                        <span class="detail-label">Location:</span>
                        <span class="detail-value">${this.escapeHtml(location)}</span>
                    </div>
                    <div class="detail-row">
                        <span class="detail-label">Agency:</span>
                        <span class="detail-value">${this.escapeHtml(caseItem.Agency || 'Unknown')}</span>
                    </div>
                    ${caseItem.StatusNote ? `
                        <div class="detail-row">
                            <span class="detail-label">Status Note:</span>
                            <span class="detail-value">${this.escapeHtml(caseItem.StatusNote)}</span>
                        </div>
                    ` : ''}
                    <div class="detail-row">
                        <span class="detail-label">Source:</span>
                        <span class="detail-value">
                            <a href="https://namus.nij.ojp.gov/" target="_blank" class="source-link">NamUs</a>
                        </span>
                    </div>
                </div>

                <div class="case-actions">
                    ${caseItem.Status === 'resolved_pending_verify' ? `
                        <div class="tooltip">
                            <button class="btn btn-secondary" style="cursor: help;">
                                ⚠️ Pending Verification
                            </button>
                            <span class="tooltiptext">
                                This case is likely resolved but awaiting verification from state/local sources. 
                                Please check TxDPS bulletins or contact local law enforcement for updates.
                            </span>
                        </div>
                    ` : ''}
                    <button class="btn btn-primary" onclick="window.open('https://namus.nij.ojp.gov/', '_blank')">
                        View on NamUs
                    </button>
                </div>
            </div>
        `;
    }

    /**
     * Get CSS class for status badge
     */
    getStatusClass(status) {
        switch (status) {
            case 'missing':
                return 'status-missing';
            case 'found':
            case 'safe':
            case 'deceased':
                return 'status-resolved';
            case 'resolved_pending_verify':
                return 'status-pending';
            default:
                return 'status-missing';
        }
    }

    /**
     * Get display text for status
     */
    getStatusText(status) {
        switch (status) {
            case 'missing':
                return 'Missing';
            case 'found':
                return 'Found';
            case 'safe':
                return 'Safe';
            case 'deceased':
                return 'Deceased';
            case 'resolved_pending_verify':
                return 'Pending';
            default:
                return 'Unknown';
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