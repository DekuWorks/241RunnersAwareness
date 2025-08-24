/**
 * ============================================
 * 241 RUNNERS AWARENESS - RUNNERS DASHBOARD
 * ============================================
 * 
 * This file handles the dashboard functionality for displaying
 * and managing runner profiles.
 */

// Global state
let runners = [];
let filteredRunners = [];
let currentUser = null;

/**
 * ============================================
 * INITIALIZATION
 * ============================================
 */

// Initialize dashboard when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
  // Wait for Auth to be available
  if (window.Auth) {
    initializeDashboard();
  } else {
    // Wait for Auth to load
    window.addEventListener('load', function() {
      if (window.Auth) {
        initializeDashboard();
      }
    });
  }
});

async function initializeDashboard() {
  try {
    // Show loading state
    showLoadingState();

    // Get current user
    currentUser = await window.Auth.getCurrentUser();
    
    // Load runners data
    await loadRunners();
    
    // Setup event listeners
    setupEventListeners();
    
    // Hide loading and show content
    showDashboardContent();
    
  } catch (error) {
    console.error('Failed to initialize dashboard:', error);
    window.Auth.showError('Failed to load dashboard. Please try again.');
  }
}

/**
 * ============================================
 * DATA LOADING
 * ============================================
 */

// Load runners data from API
async function loadRunners() {
  try {
    // Get linked individuals from user profile
    if (currentUser && currentUser.linkedIndividuals) {
      runners = currentUser.linkedIndividuals;
    } else {
      // Fallback: load individuals directly
      const response = await window.Auth.apiRequest('/individuals');
      runners = response || [];
    }

    // Update filtered runners
    filteredRunners = [...runners];
    
    // Update UI
    updateStats();
    updateRunnersGrid();
    
  } catch (error) {
    console.error('Failed to load runners:', error);
    throw error;
  }
}

/**
 * ============================================
 * UI UPDATES
 * ============================================
 */

// Update dashboard statistics
function updateStats() {
  const totalRunners = runners.length;
  const activeRunners = runners.filter(r => r.status === 'Active').length;
  const missingRunners = runners.filter(r => r.status === 'Missing').length;
  const totalCases = runners.reduce((sum, r) => sum + (r.casesCount || 0), 0);

  document.getElementById('totalRunners').textContent = totalRunners;
  document.getElementById('activeRunners').textContent = activeRunners;
  document.getElementById('missingRunners').textContent = missingRunners;
  document.getElementById('totalCases').textContent = totalCases;
}

// Update runners grid display
function updateRunnersGrid() {
  const runnersGrid = document.getElementById('runnersGrid');
  const emptyState = document.getElementById('emptyState');

  if (filteredRunners.length === 0) {
    runnersGrid.classList.add('hidden');
    emptyState.classList.remove('hidden');
    return;
  }

  runnersGrid.classList.remove('hidden');
  emptyState.classList.add('hidden');

  runnersGrid.innerHTML = filteredRunners.map(runner => `
    <div class="runner-card" onclick="viewRunner(${runner.id})">
      <div class="runner-header">
        <h3 class="runner-name">${runner.firstName} ${runner.lastName}</h3>
        <p class="runner-id">Runner ID: ${runner.id}</p>
        <span class="runner-status ${runner.status.toLowerCase()}">${runner.status}</span>
      </div>
      <div class="runner-body">
        <div class="runner-info">
          <div class="runner-info-item">
            <span class="runner-info-label">Cases:</span>
            <span class="runner-info-value">${runner.casesCount || 0}</span>
          </div>
          <div class="runner-info-item">
            <span class="runner-info-label">Added:</span>
            <span class="runner-info-value">${formatDate(runner.createdAt)}</span>
          </div>
        </div>
      </div>
    </div>
  `).join('');
}

// Show loading state
function showLoadingState() {
  document.getElementById('loadingState').classList.remove('hidden');
  document.getElementById('dashboardContent').classList.add('hidden');
}

// Show dashboard content
function showDashboardContent() {
  document.getElementById('loadingState').classList.add('hidden');
  document.getElementById('dashboardContent').classList.remove('hidden');
}

/**
 * ============================================
 * EVENT LISTENERS
 * ============================================
 */

// Setup event listeners
function setupEventListeners() {
  // Search functionality
  const searchInput = document.getElementById('runnerSearch');
  if (searchInput) {
    searchInput.addEventListener('input', handleSearch);
  }

  // Status filter
  const statusFilter = document.getElementById('statusFilter');
  if (statusFilter) {
    statusFilter.addEventListener('change', handleStatusFilter);
  }
}

// Handle search input
function handleSearch(event) {
  const searchTerm = event.target.value.toLowerCase();
  filterRunners(searchTerm, document.getElementById('statusFilter').value);
}

// Handle status filter
function handleStatusFilter(event) {
  const statusFilter = event.target.value;
  filterRunners(document.getElementById('runnerSearch').value, statusFilter);
}

// Filter runners based on search and status
function filterRunners(searchTerm, statusFilter) {
  filteredRunners = runners.filter(runner => {
    const matchesSearch = !searchTerm || 
      runner.firstName.toLowerCase().includes(searchTerm) ||
      runner.lastName.toLowerCase().includes(searchTerm) ||
      runner.id.toString().includes(searchTerm);
    
    const matchesStatus = !statusFilter || runner.status === statusFilter;
    
    return matchesSearch && matchesStatus;
  });

  updateRunnersGrid();
}

/**
 * ============================================
 * NAVIGATION
 * ============================================
 */

// View runner profile
function viewRunner(runnerId) {
  window.location.href = `runner.html?id=${runnerId}`;
}

// Add new runner
function addNewRunner() {
  window.location.href = 'new-runner.html';
}

/**
 * ============================================
 * UTILITY FUNCTIONS
 * ============================================
 */

// Format date for display
function formatDate(dateString) {
  if (!dateString) return 'N/A';
  
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString();
  } catch (error) {
    return 'N/A';
  }
}

// Format relative time
function formatRelativeTime(dateString) {
  if (!dateString) return 'N/A';
  
  try {
    const date = new Date(dateString);
    const now = new Date();
    const diffInSeconds = Math.floor((now - date) / 1000);
    
    if (diffInSeconds < 60) {
      return 'Just now';
    } else if (diffInSeconds < 3600) {
      const minutes = Math.floor(diffInSeconds / 60);
      return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
    } else if (diffInSeconds < 86400) {
      const hours = Math.floor(diffInSeconds / 3600);
      return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    } else {
      const days = Math.floor(diffInSeconds / 86400);
      return `${days} day${days > 1 ? 's' : ''} ago`;
    }
  } catch (error) {
    return 'N/A';
  }
}

// Get status color class
function getStatusColorClass(status) {
  switch (status?.toLowerCase()) {
    case 'active':
      return 'active';
    case 'inactive':
      return 'inactive';
    case 'missing':
      return 'missing';
    default:
      return 'inactive';
  }
}

/**
 * ============================================
 * ERROR HANDLING
 * ============================================
 */

// Handle API errors
function handleApiError(error) {
  console.error('API Error:', error);
  
  if (error.message.includes('401')) {
    window.Auth.showError('Session expired. Please sign in again.');
    setTimeout(() => {
      window.location.href = '/login.html';
    }, 2000);
  } else if (error.message.includes('403')) {
    window.Auth.showError('Access denied. You don\'t have permission to view this data.');
  } else if (error.message.includes('Network')) {
    window.Auth.showError('Network error. Please check your connection and try again.');
  } else {
    window.Auth.showError('An error occurred. Please try again.');
  }
}

/**
 * ============================================
 * EXPORTS
 * ============================================
 */

// Make functions available globally
window.Runners = {
  // Data management
  loadRunners,
  updateStats,
  updateRunnersGrid,
  
  // Navigation
  viewRunner,
  addNewRunner,
  
  // Filtering
  filterRunners,
  handleSearch,
  handleStatusFilter,
  
  // Utilities
  formatDate,
  formatRelativeTime,
  getStatusColorClass,
  
  // Error handling
  handleApiError,
};
