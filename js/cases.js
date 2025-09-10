// Cases Page JavaScript
class CasesPage {
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
        // Filter events
        document.getElementById('statusFilter').addEventListener('change', (e) => {
            this.filters.status = e.target.value;
            this.applyFilters();
        });
        
        document.getElementById('locationFilter').addEventListener('change', (e) => {
            this.filters.location = e.target.value;
            this.applyFilters();
        });
        
        document.getElementById('urgencyFilter').addEventListener('change', (e) => {
            this.filters.urgency = e.target.value;
            this.applyFilters();
        });
        
        document.getElementById('searchFilter').addEventListener('input', (e) => {
            this.filters.search = e.target.value;
            this.applyFilters();
            this.addToSearchHistory(e.target.value);
        });
        
        // Advanced filters
        document.getElementById('ageRangeFilter')?.addEventListener('change', (e) => {
            this.filters.ageRange = e.target.value;
            this.applyFilters();
        });
        
        document.getElementById('genderFilter')?.addEventListener('change', (e) => {
            this.filters.gender = e.target.value;
            this.applyFilters();
        });
        
        document.getElementById('dateRangeFilter')?.addEventListener('change', (e) => {
            this.filters.dateRange = e.target.value;
            this.applyFilters();
        });
        
        document.getElementById('clearFilters').addEventListener('click', () => {
            this.clearFilters();
        });
        
        // Save search button
        document.getElementById('saveSearch')?.addEventListener('click', () => {
            this.saveCurrentSearch();
        });
        
        // Load more button
        document.getElementById('loadMoreButton').addEventListener('click', () => {
            this.loadMore();
        });
        
        // Retry button
        document.getElementById('retryButton').addEventListener('click', () => {
            this.loadCases();
        });
        
        // Search suggestions
        document.getElementById('searchFilter').addEventListener('focus', () => {
            this.showSearchSuggestions();
        });
        
        document.getElementById('searchFilter').addEventListener('blur', () => {
            setTimeout(() => this.hideSearchSuggestions(), 200);
        });
    }
    
    async loadCases() {
        try {
            console.log('🔄 Loading cases from API...');
            this.showLoading();
            
            // Use the configured API base URL
            const apiUrl = window.APP_CONFIG?.API_BASE_URL || 'https://241runners-api.azurewebsites.net/api';
            console.log('📡 API URL:', apiUrl);
            
            // Load NamUs public cases for Houston area
            let namusCases = [];
            try {
                const params = new URLSearchParams({
                    region: 'houston',
                    page: 1,
                    pageSize: 100 // Get more cases to combine
                });

                console.log('🌐 Fetching public cases with params:', params.toString());
                const namusResponse = await fetch(`${apiUrl}/cases/publiccases?${params}`);
                console.log('📊 Public cases response status:', namusResponse.status);
                
                if (namusResponse.ok) {
                    const responseData = await namusResponse.json() || {};
                    namusCases = responseData.cases || [];
                    console.log('✅ Public cases loaded:', namusCases.length);
                } else {
                    console.error('❌ Public cases API failed:', namusResponse.status, namusResponse.statusText);
                    if (namusResponse.status === 404) {
                        console.error('🔧 The /publiccases endpoint is not available yet. Backend deployment may still be in progress.');
                    }
                    
                    // Show error UI for API failures
                    if (window.errorUI) {
                        window.errorUI.showErrorUI(
                            'Unable to load cases from the server. Please try again later.',
                            {
                                title: 'Cases Loading Error',
                                retryFunction: () => this.loadCases(),
                                showRetry: true
                            }
                        );
                    }
                }
            } catch (error) {
                console.error('❌ Network error fetching NamUs cases:', error.message);
            }

            // If no cases loaded, show empty state
            if (namusCases.length === 0) {
                this.cases = [];
                this.filteredCases = [];
                this.hideLoading();
                this.showEmpty();
                this.updateStats();
                return;
            }

            // Use NamUs cases directly
            this.cases = this.normalizeNamusCases(namusCases);
            this.filteredCases = [...this.cases];
            
            this.hideLoading();
            this.renderCases();
            this.updateStats();
            
        } catch (error) {
            console.error('Error loading cases:', error);
            this.showError();
        }
    }
    
    applyFilters() {
        this.filteredCases = this.cases.filter(caseItem => {
            // Status filter
            if (this.filters.status && caseItem.status !== this.filters.status) {
                return false;
            }
            
            // Location filter
            if (this.filters.location && caseItem.city !== this.filters.location) {
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
            if (this.filters.ageRange) {
                const [minAge, maxAge] = this.filters.ageRange.split('-').map(Number);
                if (caseItem.age < minAge || caseItem.age > maxAge) {
                    return false;
                }
            }
            
            // Gender filter
            if (this.filters.gender && caseItem.gender !== this.filters.gender) {
                return false;
            }
            
            // Date range filter
            if (this.filters.dateRange) {
                const caseDate = new Date(caseItem.dateReported);
                const today = new Date();
                
                switch (this.filters.dateRange) {
                    case 'today':
                        if (!this.isSameDay(caseDate, today)) return false;
                        break;
                    case 'week':
                        const weekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
                        if (caseDate < weekAgo) return false;
                        break;
                    case 'month':
                        const monthAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
                        if (caseDate < monthAgo) return false;
                        break;
                }
            }
            
            // Advanced search filter with multiple criteria
            if (this.filters.search) {
                const searchTerm = this.filters.search.toLowerCase();
                const searchableFields = [
                    caseItem.firstName,
                    caseItem.lastName,
                    caseItem.description,
                    caseItem.city,
                    caseItem.state,
                    caseItem.runnerId,
                    caseItem.identifyingMarks,
                    caseItem.medicalConditions
                ].filter(Boolean).join(' ').toLowerCase();
                
                // Check if search term is in any field
                if (!searchableFields.includes(searchTerm)) {
                    // Try partial matching for better results
                    const words = searchTerm.split(' ').filter(word => word.length > 2);
                    const hasMatch = words.some(word => searchableFields.includes(word));
                    if (!hasMatch) {
                        return false;
                    }
                }
            }
            
            return true;
        });
        
        this.currentPage = 1;
        this.renderCases();
        this.updateStats();
        this.updateFilterSummary();
    }
    
    isSameDay(date1, date2) {
        return date1.getFullYear() === date2.getFullYear() &&
               date1.getMonth() === date2.getMonth() &&
               date1.getDate() === date2.getDate();
    }

    // Normalize API cases to standard format
    normalizeNamusCases(apiCases) {
        if (!apiCases || apiCases.length === 0) {
            return [];
        }

        return apiCases.map(caseItem => {
            // Extract runner information if available
            const runner = caseItem.Runner || {};
            const fullName = runner.Name || 'Unknown';
            const nameParts = fullName.split(' ');
            
            return {
                id: caseItem.Id || `case-${Date.now()}-${Math.random()}`,
                source: 'api',
                runnerId: caseItem.RunnerId || 'N/A',
                firstName: nameParts[0] || '',
                lastName: nameParts.slice(1).join(' ') || '',
                fullName: fullName,
                age: this.calculateAge(runner.DateOfBirth),
                gender: runner.Gender || 'Unknown',
                height: this.extractHeight(runner.PhysicalDescription),
                weight: this.extractWeight(runner.PhysicalDescription),
                eyeColor: this.extractEyeColor(runner.PhysicalDescription),
                hairColor: this.extractHairColor(runner.PhysicalDescription),
                city: this.extractCity(caseItem.LastSeenLocation),
                state: this.extractState(caseItem.LastSeenLocation),
                status: caseItem.Status || 'Open',
                isUrgent: caseItem.Priority === 'High' || caseItem.Priority === 'Urgent',
                dateReported: caseItem.CreatedAt ? 
                    new Date(caseItem.CreatedAt).toISOString() : 
                    new Date().toISOString(),
                description: caseItem.Description || 'No description available',
                identifyingMarks: runner.PhysicalDescription || 'No identifying marks available',
                medicalConditions: runner.MedicalConditions || 'No medical conditions reported',
                photoUrl: runner.ProfileImageUrl,
                agency: '241 Runners Awareness',
                contactPerson: caseItem.ContactPersonName,
                contactPhone: caseItem.ContactPersonPhone,
                contactEmail: caseItem.ContactPersonEmail
            };
        }).sort((a, b) => new Date(b.dateReported) - new Date(a.dateReported));
    }

    // Helper functions for data extraction
    calculateAge(dateOfBirth) {
        if (!dateOfBirth) return null;
        const birthDate = new Date(dateOfBirth);
        const today = new Date();
        let age = today.getFullYear() - birthDate.getFullYear();
        const monthDiff = today.getMonth() - birthDate.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
            age--;
        }
        return age;
    }

    extractHeight(physicalDescription) {
        if (!physicalDescription) return 'Unknown';
        const heightMatch = physicalDescription.match(/(\d+['"]?\s*\d*["']?)/i);
        return heightMatch ? heightMatch[1] : 'Unknown';
    }

    extractWeight(physicalDescription) {
        if (!physicalDescription) return 'Unknown';
        const weightMatch = physicalDescription.match(/(\d+)\s*lbs?/i);
        return weightMatch ? `${weightMatch[1]} lbs` : 'Unknown';
    }

    extractEyeColor(physicalDescription) {
        if (!physicalDescription) return 'Unknown';
        const eyeColors = ['brown', 'blue', 'green', 'hazel', 'gray', 'grey'];
        for (const color of eyeColors) {
            if (physicalDescription.toLowerCase().includes(color)) {
                return color.charAt(0).toUpperCase() + color.slice(1);
            }
        }
        return 'Unknown';
    }

    extractHairColor(physicalDescription) {
        if (!physicalDescription) return 'Unknown';
        const hairColors = ['black', 'brown', 'blonde', 'red', 'gray', 'grey', 'white'];
        for (const color of hairColors) {
            if (physicalDescription.toLowerCase().includes(color)) {
                return color.charAt(0).toUpperCase() + color.slice(1);
            }
        }
        return 'Unknown';
    }

    extractCity(location) {
        if (!location) return 'Unknown';
        // Try to extract city from location string
        const parts = location.split(',');
        return parts[0] ? parts[0].trim() : 'Unknown';
    }

    extractState(location) {
        if (!location) return 'TX';
        // Try to extract state from location string
        const parts = location.split(',');
        if (parts.length > 1) {
            const state = parts[1].trim();
            return state.length === 2 ? state.toUpperCase() : 'TX';
        }
        return 'TX';
    }

    // Map API status to our standard status
    mapNamusStatus(apiStatus) {
        switch (apiStatus) {
            case 'Active':
                return 'Active';
            case 'Safe':
                return 'Safe';
            case 'Missing':
                return 'Missing';
            case 'Found':
                return 'Found';
            case 'Investigating':
                return 'Investigating';
            case 'Closed':
                return 'Closed';
            case 'Cancelled':
                return 'Cancelled';
            default:
                return 'Active';
        }
    }
    
    clearFilters() {
        this.filters = {
            status: '',
            location: '',
            urgency: '',
            search: '',
            ageRange: '',
            gender: '',
            dateRange: ''
        };
        
        // Reset form elements
        document.getElementById('statusFilter').value = '';
        document.getElementById('locationFilter').value = '';
        document.getElementById('urgencyFilter').value = '';
        document.getElementById('searchFilter').value = '';
        
        // Reset advanced filters if they exist
        if (document.getElementById('ageRangeFilter')) {
            document.getElementById('ageRangeFilter').value = '';
        }
        if (document.getElementById('genderFilter')) {
            document.getElementById('genderFilter').value = '';
        }
        if (document.getElementById('dateRangeFilter')) {
            document.getElementById('dateRangeFilter').value = '';
        }
        
        this.filteredCases = [...this.cases];
        this.currentPage = 1;
        this.renderCases();
        this.updateStats();
        this.updateFilterSummary();
    }
    
    // Search History Management
    addToSearchHistory(searchTerm) {
        if (!searchTerm || searchTerm.length < 3) return;
        
        // Remove if already exists
        this.searchHistory = this.searchHistory.filter(term => term !== searchTerm);
        
        // Add to beginning
        this.searchHistory.unshift(searchTerm);
        
        // Keep only last 10 searches
        this.searchHistory = this.searchHistory.slice(0, 10);
        
        // Save to localStorage
        localStorage.setItem('searchHistory', JSON.stringify(this.searchHistory));
        
        this.renderSearchHistory();
    }
    
    loadSearchHistory() {
        try {
            const saved = localStorage.getItem('searchHistory');
            return saved ? JSON.parse(saved) : [];
        } catch (error) {
            console.error('Error loading search history:', error);
            return [];
        }
    }
    
    renderSearchHistory() {
        const historyContainer = document.getElementById('searchHistory');
        if (!historyContainer || this.searchHistory.length === 0) return;
        
        historyContainer.innerHTML = `
            <div class="search-history">
                <h4>Recent Searches</h4>
                <div class="history-items">
                    ${this.searchHistory.map(term => `
                        <button class="history-item" onclick="casesPage.useSearchTerm('${term}')">
                            ${term}
                        </button>
                    `).join('')}
                </div>
            </div>
        `;
    }
    
    useSearchTerm(term) {
        document.getElementById('searchFilter').value = term;
        this.filters.search = term;
        this.applyFilters();
    }
    
    // Saved Searches Management
    saveCurrentSearch() {
        const searchName = prompt('Enter a name for this search:');
        if (!searchName) return;
        
        const savedSearch = {
            name: searchName,
            filters: { ...this.filters },
            timestamp: new Date().toISOString()
        };
        
        this.savedSearches.push(savedSearch);
        localStorage.setItem('savedSearches', JSON.stringify(this.savedSearches));
        
        this.renderSavedSearches();
        this.showMessage(`Search "${searchName}" saved successfully!`, 'success');
    }
    
    loadSavedSearches() {
        try {
            const saved = localStorage.getItem('savedSearches');
            return saved ? JSON.parse(saved) : [];
        } catch (error) {
            console.error('Error loading saved searches:', error);
            return [];
        }
    }
    
    renderSavedSearches() {
        const savedContainer = document.getElementById('savedSearches');
        if (!savedContainer || this.savedSearches.length === 0) return;
        
        savedContainer.innerHTML = `
            <div class="saved-searches">
                <h4>Saved Searches</h4>
                <div class="saved-items">
                    ${this.savedSearches.map((search, index) => `
                        <div class="saved-item">
                            <button class="saved-name" onclick="casesPage.loadSavedSearch(${index})">
                                ${search.name}
                            </button>
                            <button class="saved-delete" onclick="casesPage.deleteSavedSearch(${index})">
                                🗑️
                            </button>
                        </div>
                    `).join('')}
                </div>
            </div>
        `;
    }
    
    loadSavedSearch(index) {
        const search = this.savedSearches[index];
        if (!search) return;
        
        // Apply saved filters
        this.filters = { ...search.filters };
        
        // Update form elements
        document.getElementById('statusFilter').value = this.filters.status || '';
        document.getElementById('locationFilter').value = this.filters.location || '';
        document.getElementById('urgencyFilter').value = this.filters.urgency || '';
        document.getElementById('searchFilter').value = this.filters.search || '';
        
        if (document.getElementById('ageRangeFilter')) {
            document.getElementById('ageRangeFilter').value = this.filters.ageRange || '';
        }
        if (document.getElementById('genderFilter')) {
            document.getElementById('genderFilter').value = this.filters.gender || '';
        }
        if (document.getElementById('dateRangeFilter')) {
            document.getElementById('dateRangeFilter').value = this.filters.dateRange || '';
        }
        
        this.applyFilters();
        this.showMessage(`Loaded saved search: ${search.name}`, 'success');
    }
    
    deleteSavedSearch(index) {
        if (confirm('Are you sure you want to delete this saved search?')) {
            this.savedSearches.splice(index, 1);
            localStorage.setItem('savedSearches', JSON.stringify(this.savedSearches));
            this.renderSavedSearches();
        }
    }
    
    // Search Suggestions
    showSearchSuggestions() {
        const suggestionsContainer = document.getElementById('searchSuggestions');
        if (!suggestionsContainer) return;
        
        const currentSearch = this.filters.search.toLowerCase();
        if (currentSearch.length < 2) {
            suggestionsContainer.style.display = 'none';
            return;
        }
        
        // Generate suggestions based on current data
        const suggestions = this.generateSearchSuggestions(currentSearch);
        
        if (suggestions.length > 0) {
            suggestionsContainer.innerHTML = suggestions.map(suggestion => `
                <div class="suggestion-item" onclick="casesPage.useSearchTerm('${suggestion}')">
                    ${suggestion}
                </div>
            `).join('');
            suggestionsContainer.style.display = 'block';
        } else {
            suggestionsContainer.style.display = 'none';
        }
    }
    
    generateSearchSuggestions(searchTerm) {
        const suggestions = new Set();
        
        this.cases.forEach(caseItem => {
            const fields = [
                caseItem.firstName,
                caseItem.lastName,
                caseItem.city,
                caseItem.state
            ].filter(Boolean);
            
            fields.forEach(field => {
                if (field.toLowerCase().includes(searchTerm)) {
                    suggestions.add(field);
                }
            });
        });
        
        return Array.from(suggestions).slice(0, 5);
    }
    
    hideSearchSuggestions() {
        const suggestionsContainer = document.getElementById('searchSuggestions');
        if (suggestionsContainer) {
            suggestionsContainer.style.display = 'none';
        }
    }
    
    // Filter Summary
    updateFilterSummary() {
        const summaryContainer = document.getElementById('filterSummary');
        if (!summaryContainer) return;
        
        const activeFilters = Object.entries(this.filters)
            .filter(([key, value]) => value && key !== 'search')
            .map(([key, value]) => `${key}: ${value}`);
        
        if (activeFilters.length > 0) {
            summaryContainer.innerHTML = `
                <div class="filter-summary">
                    <strong>Active Filters:</strong> ${activeFilters.join(', ')}
                    <button onclick="casesPage.clearFilters()" class="clear-all-btn">Clear All</button>
                </div>
            `;
            summaryContainer.style.display = 'block';
        } else {
            summaryContainer.style.display = 'none';
        }
    }
    
    // Message Display
    showMessage(message, type = 'info') {
        const messageContainer = document.getElementById('messageContainer');
        if (!messageContainer) return;
        
        messageContainer.innerHTML = `
            <div class="message message-${type}">
                ${message}
                <button onclick="this.parentElement.remove()" class="message-close">×</button>
            </div>
        `;
        
        messageContainer.style.display = 'block';
        
        // Auto-hide after 3 seconds
        setTimeout(() => {
            if (messageContainer.firstChild) {
                messageContainer.firstChild.remove();
                messageContainer.style.display = 'none';
            }
        }, 3000);
    }
    
    getSampleData() {
        return []; // Return empty array - no mock data for live site
    }
    
    renderCases() {
        if (this.filteredCases.length === 0) {
            this.showEmpty();
            return;
        }
        
        this.hideEmpty();
        
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const casesToShow = this.filteredCases.slice(startIndex, endIndex);
        
        const casesGrid = document.getElementById('casesGrid');
        casesGrid.innerHTML = casesToShow.map(caseItem => this.renderCaseCard(caseItem)).join('');
        
        // Show/hide load more button
        const loadMoreContainer = document.getElementById('loadMoreContainer');
        if (endIndex < this.filteredCases.length) {
            loadMoreContainer.style.display = 'block';
        } else {
            loadMoreContainer.style.display = 'none';
        }
        
        // Update counts
        document.getElementById('showingCount').textContent = Math.min(endIndex, this.filteredCases.length);
        document.getElementById('totalCount').textContent = this.filteredCases.length;
    }
    
    renderCaseCard(caseItem) {
        return `
            <div class="case-card ${caseItem.isUrgent ? 'urgent' : ''}">
                <div class="case-header">
                    <div class="case-id">${caseItem.runnerId}</div>
                    <div class="case-status ${caseItem.status.toLowerCase().replace(' ', '-')}">${caseItem.status}</div>
                    ${caseItem.source === 'namus' ? 
                        '<span class="source-badge namus-badge">NamUs</span>' : 
                        '<span class="source-badge local-badge">Local</span>'
                    }
                </div>
                
                <div class="case-content">
                    <h3 class="case-name">${caseItem.firstName} ${caseItem.lastName}</h3>
                    <p class="case-description">${caseItem.description || 'No description available'}</p>
                    
                    <div class="case-details">
                        <div class="detail-item">
                            <span class="detail-label">Age:</span>
                            <span class="detail-value">${caseItem.age}</span>
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
                            <span class="detail-value">${new Date(caseItem.dateReported).toLocaleDateString()}</span>
                        </div>
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
                    <a href="https://241runnersawareness.org/runner.html?id=${caseItem.id}" class="btn-primary">View Details</a>
                    <a href="https://241runnersawareness.org/map.html?case=${caseItem.id}" class="btn-secondary">View on Map</a>
                </div>
            </div>
        `;
    }
    
    
    loadMore() {
        this.currentPage++;
        this.renderCases();
    }
    
    updateStats() {
        const totalCases = this.cases.length;
        const urgentCases = this.cases.filter(c => c.isUrgent).length;
        
        const sevenDaysAgo = new Date();
        sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
        const recentCases = this.cases.filter(c => {
            const caseDate = new Date(c.dateReported);
            return caseDate >= sevenDaysAgo;
        }).length;
        
        document.getElementById('totalCases').textContent = totalCases;
        document.getElementById('urgentCases').textContent = urgentCases;
        document.getElementById('recentCases').textContent = recentCases;
    }
    
    showLoading() {
        document.getElementById('loadingState').style.display = 'flex';
        document.getElementById('errorState').style.display = 'none';
        document.getElementById('emptyState').style.display = 'none';
        document.getElementById('casesGrid').style.display = 'none';
    }
    
    hideLoading() {
        document.getElementById('loadingState').style.display = 'none';
        document.getElementById('casesGrid').style.display = 'grid';
    }
    
    showError() {
        document.getElementById('loadingState').style.display = 'none';
        document.getElementById('errorState').style.display = 'flex';
        document.getElementById('emptyState').style.display = 'none';
        document.getElementById('casesGrid').style.display = 'none';
    }
    
    showEmpty() {
        document.getElementById('loadingState').style.display = 'none';
        document.getElementById('errorState').style.display = 'none';
        document.getElementById('emptyState').style.display = 'flex';
        document.getElementById('casesGrid').style.display = 'none';
        document.getElementById('loadMoreContainer').style.display = 'none';
    }

    showDeploymentStatus() {
        document.getElementById('loadingState').style.display = 'none';
        document.getElementById('errorState').style.display = 'none';
        document.getElementById('emptyState').style.display = 'none';
        document.getElementById('casesGrid').style.display = 'none';
        document.getElementById('loadMoreContainer').style.display = 'none';
        
        // Update the empty state to show deployment status
        const emptyState = document.getElementById('emptyState');
        if (emptyState) {
            emptyState.innerHTML = `
                <div class="deployment-status">
                    <div class="deployment-icon">🚀</div>
                    <h3>Backend Deployment in Progress</h3>
                    <p>The NamUs data integration is being deployed to our servers. This may take a few minutes.</p>
                    <div class="deployment-info">
                        <p><strong>Status:</strong> <span class="status-pending">Deploying...</span></p>
                        <p><strong>What's happening:</strong> Database migrations and API endpoints are being set up</p>
                    </div>
                    <button id="checkDeploymentStatus" class="btn btn-primary">Check Status</button>
                    <p class="deployment-note">You can refresh this page in a few minutes to see the cases.</p>
                </div>
            `;
            
            // Add event listener for check status button
            document.getElementById('checkDeploymentStatus').addEventListener('click', () => {
                this.loadCases();
            });
        }
    }
    
    hideEmpty() {
        document.getElementById('emptyState').style.display = 'none';
        document.getElementById('casesGrid').style.display = 'grid';
    }
}

// Initialize the page when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new CasesPage();
});

// Mobile navigation toggle
document.addEventListener('DOMContentLoaded', () => {
    const navToggle = document.getElementById('navToggle');
    const navMenu = document.getElementById('navMenu');
    
    if (navToggle && navMenu) {
        navToggle.addEventListener('click', () => {
            navMenu.classList.toggle('active');
            navToggle.classList.toggle('active');
        });
    }
}); 