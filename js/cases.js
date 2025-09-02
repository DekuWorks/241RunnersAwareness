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
            search: ''
        };
        
        this.init();
    }
    
    init() {
        this.bindEvents();
        this.loadCases();
        this.updateStats();
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
        });
        
        document.getElementById('clearFilters').addEventListener('click', () => {
            this.clearFilters();
        });
        
        // Load more button
        document.getElementById('loadMoreButton').addEventListener('click', () => {
            this.loadMore();
        });
        
        // Retry button
        document.getElementById('retryButton').addEventListener('click', () => {
            this.loadCases();
        });
    }
    
    async loadCases() {
        try {
            this.showLoading();
            
            // Use the configured API base URL
            const apiUrl = window.APP_CONFIG?.API_BASE_URL || 'http://localhost:5248/api';
            const response = await fetch(`${apiUrl}/runners`);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            this.cases = data;
            this.filteredCases = [...this.cases];
            
            this.hideLoading();
            this.renderCases();
            this.updateStats();
            
        } catch (error) {
            console.error('Error loading cases:', error);
            
            // Show sample data when API fails (for development/demo purposes)
            if (window.APP_CONFIG?.ENVIRONMENT === 'development') {
                console.log('Showing sample data due to API failure');
                this.cases = this.getSampleData();
                this.filteredCases = [...this.cases];
                this.hideLoading();
                this.renderCases();
                this.updateStats();
            } else {
                this.showError();
            }
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
        });
        
        this.currentPage = 1;
        this.renderCases();
        this.updateStats();
    }
    
    clearFilters() {
        this.filters = {
            status: '',
            location: '',
            urgency: '',
            search: ''
        };
        
        // Reset form elements
        document.getElementById('statusFilter').value = '';
        document.getElementById('locationFilter').value = '';
        document.getElementById('urgencyFilter').value = '';
        document.getElementById('searchFilter').value = '';
        
        this.filteredCases = [...this.cases];
        this.currentPage = 1;
        this.renderCases();
        this.updateStats();
    }
    
    getSampleData() {
        return [
            {
                id: 1,
                runnerId: "RUN001",
                firstName: "Sarah",
                lastName: "Johnson",
                age: 28,
                gender: "Female",
                height: "5'6\"",
                weight: "140 lbs",
                eyeColor: "Blue",
                hairColor: "Brown",
                city: "Austin",
                state: "TX",
                status: "Missing",
                isUrgent: true,
                dateReported: "2025-01-15",
                description: "Sarah was last seen leaving her apartment complex on the morning of January 15th. She was wearing a blue jacket and carrying a black backpack.",
                identifyingMarks: "Small birthmark on left wrist",
                medicalConditions: "Asthma"
            },
            {
                id: 2,
                runnerId: "RUN002",
                firstName: "Michael",
                lastName: "Chen",
                age: 34,
                gender: "Male",
                height: "6'0\"",
                weight: "180 lbs",
                eyeColor: "Brown",
                hairColor: "Black",
                city: "Houston",
                state: "TX",
                status: "Missing",
                isUrgent: false,
                dateReported: "2025-01-10",
                description: "Michael was last seen at his workplace. He didn't show up for his shift and hasn't been in contact with family or friends.",
                identifyingMarks: "Tattoo of a dragon on right forearm",
                medicalConditions: "None known"
            },
            {
                id: 3,
                runnerId: "RUN003",
                firstName: "Emily",
                lastName: "Rodriguez",
                age: 22,
                gender: "Female",
                height: "5'4\"",
                weight: "125 lbs",
                eyeColor: "Green",
                hairColor: "Blonde",
                city: "Dallas",
                state: "TX",
                status: "Found",
                isUrgent: false,
                dateReported: "2025-01-05",
                description: "Emily was found safe at a friend's house. She had been staying there without informing her family.",
                identifyingMarks: "Piercing in left eyebrow",
                medicalConditions: "None known"
            }
        ];
    }
    
    renderCases() {
        const grid = document.getElementById('casesGrid');
        const startIndex = 0;
        const endIndex = this.currentPage * this.itemsPerPage;
        const casesToShow = this.filteredCases.slice(startIndex, endIndex);
        
        if (this.filteredCases.length === 0) {
            this.showEmpty();
            return;
        }
        
        this.hideEmpty();
        
        grid.innerHTML = casesToShow.map(caseItem => this.createCaseCard(caseItem)).join('');
        
        // Show/hide load more button
        const loadMoreContainer = document.getElementById('loadMoreContainer');
        if (endIndex < this.filteredCases.length) {
            loadMoreContainer.style.display = 'block';
        } else {
            loadMoreContainer.style.display = 'none';
        }
        
        // Update count
        document.getElementById('showingCount').textContent = Math.min(endIndex, this.filteredCases.length);
        document.getElementById('totalCount').textContent = this.filteredCases.length;
    }
    
    createCaseCard(caseItem) {
        const urgencyClass = caseItem.isUrgent ? 'urgent' : '';
        const statusClass = caseItem.status.toLowerCase().replace(' ', '-');
        const dateReported = new Date(caseItem.dateReported).toLocaleDateString();
        
        return `
            <div class="case-card ${urgencyClass}">
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
                    <a href="map.html?case=${caseItem.id}" class="btn-secondary">View on Map</a>
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