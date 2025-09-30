/**
 * 241 Runners Awareness - Optimized Cases Loader
 * 
 * This file provides optimized case loading with caching,
 * performance monitoring, and intelligent data management.
 */

/**
 * ============================================
 * OPTIMIZED CASES CLASS
 * ============================================
 */

class OptimizedCasesPage {
    constructor() {
        this.cases = [];
        this.filteredCases = [];
        this.currentPage = 1;
        this.itemsPerPage = 12;
        this.filters = {
            status: '',
            location: '',
            urgency: '',
            search: '',
            ageRange: '',
            gender: '',
            dateRange: ''
        };
        this.searchHistory = this.loadSearchHistory();
        this.savedSearches = this.loadSavedSearches();
        this.cacheKey = 'cases_data';
        this.cacheTTL = 5 * 60 * 1000; // 5 minutes
        
        this.init();
    }
    
    init() {
        this.bindEvents();
        this.loadCases();
        this.updateStats();
        this.renderSearchHistory();
        this.renderSavedSearches();
    }
    
    bindEvents() {
        // Filter events with debouncing
        this.setupDebouncedFilters();
        
        // Search with debouncing
        const searchInput = document.getElementById('searchFilter');
        if (searchInput) {
            searchInput.addEventListener('input', debounce((e) => {
                this.filters.search = e.target.value;
                this.applyFilters();
                this.addToSearchHistory(e.target.value);
            }, 300));
        }
        
        // Load more button
        document.getElementById('loadMoreButton')?.addEventListener('click', () => {
            this.loadMore();
        });
        
        // Retry button
        document.getElementById('retryButton')?.addEventListener('click', () => {
            this.loadCases(true); // Force refresh
        });
        
        // Search suggestions
        this.setupSearchSuggestions();
    }

    /**
     * Setup debounced filters
     */
    setupDebouncedFilters() {
        const filterElements = [
            'statusFilter',
            'locationFilter', 
            'urgencyFilter',
            'ageRangeFilter',
            'genderFilter',
            'dateRangeFilter'
        ];

        filterElements.forEach(filterId => {
            const element = document.getElementById(filterId);
            if (element) {
                element.addEventListener('change', debounce((e) => {
                    this.filters[filterId.replace('Filter', '')] = e.target.value;
                    this.applyFilters();
                }, 200));
            }
        });
    }

    /**
     * Setup search suggestions
     */
    setupSearchSuggestions() {
        const searchInput = document.getElementById('searchFilter');
        if (!searchInput) return;

        searchInput.addEventListener('focus', () => {
            this.showSearchSuggestions();
        });
        
        searchInput.addEventListener('blur', () => {
            setTimeout(() => this.hideSearchSuggestions(), 200);
        });
    }
    
    /**
     * Load cases with optimization
     * @param {boolean} forceRefresh - Force refresh from API
     */
    async loadCases(forceRefresh = false) {
        try {
            this.showLoading();
            
            // Check cache first
            if (!forceRefresh && this.isCacheValid()) {
                const cachedData = this.getCachedData();
                if (cachedData) {
                    this.cases = cachedData;
                    this.filteredCases = [...this.cases];
                    this.hideLoading();
                    this.renderCases();
                    this.updateStats();
                    return;
                }
            }
            
            // Load from API with optimization
            const data = await this.loadCasesFromAPI();
            this.cases = data;
            
            // Cache the data
            this.cacheData(data);
            
            this.filteredCases = [...this.cases];
            
            this.hideLoading();
            this.renderCases();
            this.updateStats();
            
        } catch (error) {
            console.error('Error loading cases:', error);
            this.showError();
        }
    }

    /**
     * Load cases from API with optimization
     * @returns {Promise<Array>} - Cases data
     */
    async loadCasesFromAPI() {
        const apiUrl = 'https://241runners-api-v2.azurewebsites.net/api/v1/cases';
        
        // Use optimized API request if available
        if (window.apiOptimizer) {
            return await window.apiOptimizer.makeRequest(apiUrl, {
                headers: {
                    'Authorization': `Bearer ${this.getAuthToken()}`
                }
            }, {
                cache: true,
                retry: true,
                priority: 'high',
                batch: false
            });
        }
        
        // Use cached API request if available
        if (window.apiCache) {
            return await window.apiCache.request(apiUrl, {
                headers: {
                    'Authorization': `Bearer ${this.getAuthToken()}`
                }
            }, {
                cache: true,
                ttl: this.cacheTTL
            });
        }
        
        // Fallback to direct fetch
        const response = await fetch(apiUrl, {
            headers: {
                'Authorization': `Bearer ${this.getAuthToken()}`
            }
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return await response.json();
    }

    /**
     * Check if cache is valid
     * @returns {boolean} - Whether cache is valid
     */
    isCacheValid() {
        const cacheData = localStorage.getItem(this.cacheKey);
        if (!cacheData) return false;
        
        try {
            const parsed = JSON.parse(cacheData);
            const now = Date.now();
            return (now - parsed.timestamp) < this.cacheTTL;
        } catch {
            return false;
        }
    }

    /**
     * Get cached data
     * @returns {Array|null} - Cached cases data
     */
    getCachedData() {
        try {
            const cacheData = localStorage.getItem(this.cacheKey);
            if (!cacheData) return null;
            
            const parsed = JSON.parse(cacheData);
            return parsed.data;
        } catch {
            return null;
        }
    }

    /**
     * Cache data
     * @param {Array} data - Cases data to cache
     */
    cacheData(data) {
        try {
            const cacheData = {
                data: data,
                timestamp: Date.now()
            };
            localStorage.setItem(this.cacheKey, JSON.stringify(cacheData));
        } catch (error) {
            console.warn('Failed to cache data:', error);
        }
    }

    /**
     * Apply filters with optimization
     */
    applyFilters() {
        // Use requestAnimationFrame for smooth filtering
        requestAnimationFrame(() => {
            this.filteredCases = this.cases.filter(caseItem => {
                return this.matchesFilters(caseItem);
            });
            
            this.currentPage = 1;
            this.renderCases();
            this.updateStats();
        });
    }

    /**
     * Check if case matches filters
     * @param {Object} caseItem - Case item to check
     * @returns {boolean} - Whether case matches filters
     */
    matchesFilters(caseItem) {
        // Status filter
        if (this.filters.status && caseItem.status !== this.filters.status) {
            return false;
        }
        
        // Location filter
        if (this.filters.location && !this.matchesLocation(caseItem, this.filters.location)) {
            return false;
        }
        
        // Urgency filter
        if (this.filters.urgency === 'urgent' && !caseItem.isUrgent) {
            return false;
        }
        
        if (this.filters.urgency === 'recent') {
            const sevenDaysAgo = new Date();
            sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
            const caseDate = new Date(caseItem.dateReported);
            if (caseDate < sevenDaysAgo) {
                return false;
            }
        }
        
        // Age range filter
        if (this.filters.ageRange && !this.matchesAgeRange(caseItem, this.filters.ageRange)) {
            return false;
        }
        
        // Gender filter
        if (this.filters.gender && caseItem.gender !== this.filters.gender) {
            return false;
        }
        
        // Date range filter
        if (this.filters.dateRange && !this.matchesDateRange(caseItem, this.filters.dateRange)) {
            return false;
        }
        
        // Search filter
        if (this.filters.search) {
            const searchTerm = this.filters.search.toLowerCase();
            const searchableText = [
                caseItem.firstName,
                caseItem.lastName,
                caseItem.description,
                caseItem.city,
                caseItem.state
            ].join(' ').toLowerCase();
            
            if (!searchableText.includes(searchTerm)) {
                return false;
            }
        }
        
        return true;
    }

    /**
     * Check if case matches location filter
     * @param {Object} caseItem - Case item
     * @param {string} location - Location filter
     * @returns {boolean} - Whether case matches location
     */
    matchesLocation(caseItem, location) {
        const caseLocation = `${caseItem.city} ${caseItem.state}`.toLowerCase();
        return caseLocation.includes(location.toLowerCase());
    }

    /**
     * Check if case matches age range filter
     * @param {Object} caseItem - Case item
     * @param {string} ageRange - Age range filter
     * @returns {boolean} - Whether case matches age range
     */
    matchesAgeRange(caseItem, ageRange) {
        const age = caseItem.age;
        switch (ageRange) {
            case 'child': return age < 18;
            case 'adult': return age >= 18 && age < 65;
            case 'senior': return age >= 65;
            default: return true;
        }
    }

    /**
     * Check if case matches date range filter
     * @param {Object} caseItem - Case item
     * @param {string} dateRange - Date range filter
     * @returns {boolean} - Whether case matches date range
     */
    matchesDateRange(caseItem, dateRange) {
        const caseDate = new Date(caseItem.dateReported);
        const now = new Date();
        
        switch (dateRange) {
            case 'today': 
                return caseDate.toDateString() === now.toDateString();
            case 'week': 
                const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
                return caseDate >= weekAgo;
            case 'month': 
                const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
                return caseDate >= monthAgo;
            default: 
                return true;
        }
    }
    
    /**
     * Render cases with virtualization for large lists
     */
    renderCases() {
        const grid = document.getElementById('casesGrid');
        if (!grid) return;
        
        const startIndex = 0;
        const endIndex = this.currentPage * this.itemsPerPage;
        const casesToShow = this.filteredCases.slice(startIndex, endIndex);
        
        if (this.filteredCases.length === 0) {
            this.showEmpty();
            return;
        }
        
        this.hideEmpty();
        
        // Use document fragment for better performance
        const fragment = document.createDocumentFragment();
        
        casesToShow.forEach(caseItem => {
            const caseElement = this.createCaseCard(caseItem);
            fragment.appendChild(caseElement);
        });
        
        grid.innerHTML = '';
        grid.appendChild(fragment);
        
        // Show/hide load more button
        this.updateLoadMoreButton(endIndex);
        
        // Update count
        this.updateCaseCount(endIndex);
    }

    /**
     * Create case card element
     * @param {Object} caseItem - Case item data
     * @returns {HTMLElement} - Case card element
     */
    createCaseCard(caseItem) {
        const urgencyClass = caseItem.isUrgent ? 'urgent' : '';
        const statusClass = caseItem.status.toLowerCase().replace(' ', '-');
        const dateReported = new Date(caseItem.dateReported).toLocaleDateString();
        
        const card = document.createElement('div');
        card.className = `case-card ${urgencyClass}`;
        card.innerHTML = `
            <div class="case-header">
                <div class="case-status ${statusClass}">${caseItem.status}</div>
                ${caseItem.isUrgent ? '<div class="urgent-badge">URGENT</div>' : ''}
            </div>
            
            <div class="case-content">
                <h3 class="case-name">${caseItem.firstName} ${caseItem.lastName}</h3>
                <div class="case-id">ID: ${caseItem.runnerId}</div>
                
                <div class="case-details">
                    <div class="detail-item">
                        <span class="detail-label">Age:</span>
                        <span class="detail-value">${caseItem.age} years</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-label">Gender:</span>
                        <span class="detail-value">${caseItem.gender}</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-label">Location:</span>
                        <span class="detail-value">${caseItem.city}, ${caseItem.state}</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-label">Reported:</span>
                        <span class="detail-value">${dateReported}</span>
                    </div>
                </div>
                
                <div class="case-description">
                    <p>${caseItem.description}</p>
                </div>
                
                <div class="case-physical">
                    <div class="physical-item">
                        <span class="physical-label">Height:</span>
                        <span class="physical-value">${caseItem.height}</span>
                    </div>
                    <div class="physical-item">
                        <span class="physical-label">Weight:</span>
                        <span class="physical-value">${caseItem.weight}</span>
                    </div>
                    <div class="physical-item">
                        <span class="physical-label">Hair:</span>
                        <span class="physical-value">${caseItem.hairColor}</span>
                    </div>
                    <div class="physical-item">
                        <span class="physical-label">Eyes:</span>
                        <span class="physical-value">${caseItem.eyeColor}</span>
                    </div>
                </div>
                
                ${caseItem.identifyingMarks ? `
                    <div class="case-marks">
                        <strong>Identifying Marks:</strong>
                        <p>${caseItem.identifyingMarks}</p>
                    </div>
                ` : ''}
                
                ${caseItem.medicalConditions ? `
                    <div class="case-medical">
                        <strong>Medical Conditions:</strong>
                        <p>${caseItem.medicalConditions}</p>
                    </div>
                ` : ''}
            </div>
            
            <div class="case-actions">
                <a href="runner.html?id=${caseItem.id}" class="btn-primary">View Details</a>
                <a href="report-case.html?edit=${caseItem.id}" class="btn-secondary">Edit Case</a>
                <a href="map.html?case=${caseItem.id}" class="btn-secondary">View on Map</a>
            </div>
        `;
        
        return card;
    }

    /**
     * Update load more button
     * @param {number} endIndex - End index of displayed cases
     */
    updateLoadMoreButton(endIndex) {
        const loadMoreContainer = document.getElementById('loadMoreContainer');
        if (loadMoreContainer) {
            if (endIndex < this.filteredCases.length) {
                loadMoreContainer.style.display = 'block';
            } else {
                loadMoreContainer.style.display = 'none';
            }
        }
    }

    /**
     * Update case count display
     * @param {number} endIndex - End index of displayed cases
     */
    updateCaseCount(endIndex) {
        const showingCount = document.getElementById('showingCount');
        const totalCount = document.getElementById('totalCount');
        
        if (showingCount) {
            showingCount.textContent = Math.min(endIndex, this.filteredCases.length);
        }
        if (totalCount) {
            totalCount.textContent = this.filteredCases.length;
        }
    }
    
    loadMore() {
        this.currentPage++;
        this.renderCases();
    }
    
    updateStats() {
        const totalCases = this.cases.length;
        const activeCases = this.cases.filter(c => c.status === 'Missing').length;
        const resolvedCases = this.cases.filter(c => ['Found', 'Safe', 'Deceased'].includes(c.status)).length;
        
        const totalElement = document.getElementById('totalCases');
        const activeElement = document.getElementById('activeCases');
        const resolvedElement = document.getElementById('resolvedCases');
        
        if (totalElement) totalElement.textContent = totalCases;
        if (activeElement) activeElement.textContent = activeCases;
        if (resolvedElement) resolvedElement.textContent = resolvedCases;
    }
    
    showLoading() {
        const loadingElement = document.getElementById('loadingState');
        const errorElement = document.getElementById('errorState');
        const emptyElement = document.getElementById('emptyState');
        const gridElement = document.getElementById('casesGrid');
        
        if (loadingElement) loadingElement.style.display = 'flex';
        if (errorElement) errorElement.style.display = 'none';
        if (emptyElement) emptyElement.style.display = 'none';
        if (gridElement) gridElement.style.display = 'none';
    }
    
    hideLoading() {
        const loadingElement = document.getElementById('loadingState');
        const gridElement = document.getElementById('casesGrid');
        
        if (loadingElement) loadingElement.style.display = 'none';
        if (gridElement) gridElement.style.display = 'grid';
    }
    
    showError() {
        const loadingElement = document.getElementById('loadingState');
        const errorElement = document.getElementById('errorState');
        const emptyElement = document.getElementById('emptyState');
        const gridElement = document.getElementById('casesGrid');
        
        if (loadingElement) loadingElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'flex';
        if (emptyElement) emptyElement.style.display = 'none';
        if (gridElement) gridElement.style.display = 'none';
    }
    
    showEmpty() {
        const loadingElement = document.getElementById('loadingState');
        const errorElement = document.getElementById('errorState');
        const emptyElement = document.getElementById('emptyState');
        const gridElement = document.getElementById('casesGrid');
        const loadMoreContainer = document.getElementById('loadMoreContainer');
        
        if (loadingElement) loadingElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'none';
        if (emptyElement) emptyElement.style.display = 'flex';
        if (gridElement) gridElement.style.display = 'none';
        if (loadMoreContainer) loadMoreContainer.style.display = 'none';
    }
    
    hideEmpty() {
        const emptyElement = document.getElementById('emptyState');
        const gridElement = document.getElementById('casesGrid');
        
        if (emptyElement) emptyElement.style.display = 'none';
        if (gridElement) gridElement.style.display = 'grid';
    }

    /**
     * Get authentication token
     * @returns {string|null} - Auth token
     */
    getAuthToken() {
        return localStorage.getItem('userToken') || localStorage.getItem('jwtToken') || localStorage.getItem('token');
    }

    /**
     * Load search history
     * @returns {Array} - Search history
     */
    loadSearchHistory() {
        try {
            return JSON.parse(localStorage.getItem('searchHistory') || '[]');
        } catch {
            return [];
        }
    }

    /**
     * Save search history
     * @param {Array} history - Search history
     */
    saveSearchHistory(history) {
        try {
            localStorage.setItem('searchHistory', JSON.stringify(history));
        } catch (error) {
            console.warn('Failed to save search history:', error);
        }
    }

    /**
     * Add to search history
     * @param {string} searchTerm - Search term
     */
    addToSearchHistory(searchTerm) {
        if (!searchTerm.trim()) return;
        
        const history = this.searchHistory.filter(term => term !== searchTerm);
        history.unshift(searchTerm);
        
        // Keep only last 10 searches
        this.searchHistory = history.slice(0, 10);
        this.saveSearchHistory(this.searchHistory);
        this.renderSearchHistory();
    }

    /**
     * Render search history
     */
    renderSearchHistory() {
        const container = document.getElementById('searchHistory');
        if (!container || this.searchHistory.length === 0) return;
        
        container.innerHTML = this.searchHistory.map(term => 
            `<div class="search-history-item" onclick="this.searchTerm('${term}')">${term}</div>`
        ).join('');
    }

    /**
     * Load saved searches
     * @returns {Array} - Saved searches
     */
    loadSavedSearches() {
        try {
            return JSON.parse(localStorage.getItem('savedSearches') || '[]');
        } catch {
            return [];
        }
    }

    /**
     * Save searches
     * @param {Array} searches - Saved searches
     */
    saveSearches(searches) {
        try {
            localStorage.setItem('savedSearches', JSON.stringify(searches));
        } catch (error) {
            console.warn('Failed to save searches:', error);
        }
    }

    /**
     * Render saved searches
     */
    renderSavedSearches() {
        const container = document.getElementById('savedSearches');
        if (!container || this.savedSearches.length === 0) return;
        
        container.innerHTML = this.savedSearches.map(search => 
            `<div class="saved-search-item" onclick="this.loadSearch('${search.name}')">${search.name}</div>`
        ).join('');
    }

    /**
     * Show search suggestions
     */
    showSearchSuggestions() {
        const container = document.getElementById('searchSuggestions');
        if (!container) return;
        
        const suggestions = this.getSearchSuggestions();
        if (suggestions.length === 0) return;
        
        container.innerHTML = suggestions.map(suggestion => 
            `<div class="search-suggestion" onclick="this.selectSuggestion('${suggestion}')">${suggestion}</div>`
        ).join('');
        
        container.style.display = 'block';
    }

    /**
     * Hide search suggestions
     */
    hideSearchSuggestions() {
        const container = document.getElementById('searchSuggestions');
        if (container) {
            container.style.display = 'none';
        }
    }

    /**
     * Get search suggestions
     * @returns {Array} - Search suggestions
     */
    getSearchSuggestions() {
        const searchTerm = document.getElementById('searchFilter')?.value.toLowerCase() || '';
        if (searchTerm.length < 2) return [];
        
        const suggestions = new Set();
        
        // Add matching cases
        this.cases.forEach(caseItem => {
            const name = `${caseItem.firstName} ${caseItem.lastName}`.toLowerCase();
            const location = `${caseItem.city} ${caseItem.state}`.toLowerCase();
            
            if (name.includes(searchTerm)) {
                suggestions.add(`${caseItem.firstName} ${caseItem.lastName}`);
            }
            if (location.includes(searchTerm)) {
                suggestions.add(`${caseItem.city}, ${caseItem.state}`);
            }
        });
        
        return Array.from(suggestions).slice(0, 5);
    }
}

// Initialize optimized cases page when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new OptimizedCasesPage();
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = OptimizedCasesPage;
}
