// My Cases Page JavaScript
class MyCasesPage {
    constructor() {
        this.cases = [];
        this.filteredCases = [];
        this.currentPage = 1;
        this.itemsPerPage = 12;
        this.filters = {
            status: '',
            urgency: '',
            search: ''
        };
        this.currentUser = null;
        
        this.init();
    }
    
    async init() {
        // Check authentication first
        if (!await this.checkAuth()) {
            window.location.href = 'login.html';
            return;
        }
        
        this.bindEvents();
        this.loadMyCases();
        this.updateStats();
    }
    
    async checkAuth() {
        try {
            const token = localStorage.getItem('token');
            if (!token) {
                return false;
            }
            
            // Verify token is valid
            const response = await fetch('https://241runners-api.azurewebsites.net/api/auth/verify', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (!response.ok) {
                localStorage.removeItem('token');
                return false;
            }
            
            const userData = await response.json();
            this.currentUser = userData.user;
            return true;
            
        } catch (error) {
            console.error('Auth check failed:', error);
            localStorage.removeItem('token');
            return false;
        }
    }
    
    bindEvents() {
        // Filter events
        document.getElementById('statusFilter').addEventListener('change', (e) => {
            this.filters.status = e.target.value;
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
            this.loadMyCases();
        });
        
        // Logout button
        document.getElementById('logoutBtn').addEventListener('click', (e) => {
            e.preventDefault();
            this.logout();
        });
    }
    
    async loadMyCases() {
        try {
            this.showLoading();
            
            const token = localStorage.getItem('token');
            const response = await fetch('https://241runners-api.azurewebsites.net/api/runners/my-cases', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
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
            console.error('Error loading my cases:', error);
            this.showError();
        }
    }
    
    applyFilters() {
        this.filteredCases = this.cases.filter(caseItem => {
            // Status filter
            if (this.filters.status && caseItem.status !== this.filters.status) {
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
            urgency: '',
            search: ''
        };
        
        // Reset form elements
        document.getElementById('statusFilter').value = '';
        document.getElementById('urgencyFilter').value = '';
        document.getElementById('searchFilter').value = '';
        
        this.filteredCases = [...this.cases];
        this.currentPage = 1;
        this.renderCases();
        this.updateStats();
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
                    <a href="report-case.html?edit=${caseItem.id}" class="btn-secondary">Edit Case</a>
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
        const activeCases = this.cases.filter(c => c.status === 'Missing').length;
        const resolvedCases = this.cases.filter(c => ['Found', 'Safe', 'Deceased'].includes(c.status)).length;
        
        document.getElementById('myTotalCases').textContent = totalCases;
        document.getElementById('myActiveCases').textContent = activeCases;
        document.getElementById('myResolvedCases').textContent = resolvedCases;
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
    
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = 'index.html';
    }
}

// Initialize the page when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new MyCasesPage();
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