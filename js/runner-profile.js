/**
 * ============================================
 * 241 RUNNERS AWARENESS - RUNNER PROFILE
 * ============================================
 * 
 * This file handles the runner profile page functionality
 * for viewing individual runner details and cases.
 */

// Global state
let runnerId = null;
let runnerData = null;
let casesData = [];
let currentUser = null;

/**
 * ============================================
 * INITIALIZATION
 * ============================================
 */

// Initialize profile when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
  // Wait for Auth to be available
  if (window.Auth) {
    initializeProfile();
  } else {
    // Wait for Auth to load
    window.addEventListener('load', function() {
      if (window.Auth) {
        initializeProfile();
      }
    });
  }
});

async function initializeProfile() {
  try {
    // Get runner ID from URL
    runnerId = getRunnerIdFromUrl();
    if (!runnerId) {
      showError('Runner ID not found in URL');
      return;
    }

    // Get current user
    currentUser = await window.Auth.getCurrentUser();
    
    // Load runner data
    await loadRunnerData();
    
    // Load cases data
    await loadCasesData();
    
    // Update UI
    updateProfileUI();
    updateCasesUI();
    
    // Setup links
    setupLinks();
    
    // Hide loading and show content
    showProfileContent();
    
  } catch (error) {
    console.error('Failed to initialize profile:', error);
    handleProfileError(error);
  }
}

/**
 * ============================================
 * URL PARSING
 * ============================================
 */

// Get runner ID from URL parameters
function getRunnerIdFromUrl() {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('id');
}

/**
 * ============================================
 * DATA LOADING
 * ============================================
 */

// Load runner data from API
async function loadRunnerData() {
  try {
    const response = await window.Auth.apiRequest(`/individuals/${runnerId}`);
    runnerData = response;
  } catch (error) {
    console.error('Failed to load runner data:', error);
    throw error;
  }
}

// Load cases data for the runner
async function loadCasesData() {
  try {
    const response = await window.Auth.apiRequest(`/cases?individualId=${runnerId}`);
    casesData = response || [];
  } catch (error) {
    console.error('Failed to load cases data:', error);
    // Don't throw error for cases, just log it
    casesData = [];
  }
}

/**
 * ============================================
 * UI UPDATES
 * ============================================
 */

// Update profile UI with runner data
function updateProfileUI() {
  if (!runnerData) return;

  // Update header
  document.getElementById('runnerName').textContent = `${runnerData.firstName} ${runnerData.lastName}`;
  document.getElementById('runnerId').textContent = `Runner ID: ${runnerData.id}`;
  
  const statusElement = document.getElementById('runnerStatus');
  statusElement.textContent = runnerData.status;
  statusElement.className = `runner-status ${getStatusColorClass(runnerData.status)}`;

  // Update avatar
  const avatarElement = document.getElementById('runnerAvatar');
  if (runnerData.photoUrl) {
    avatarElement.src = runnerData.photoUrl;
  }

  // Update basic information
  document.getElementById('fullName').textContent = `${runnerData.firstName} ${runnerData.lastName}`;
  document.getElementById('dateOfBirth').textContent = formatDate(runnerData.dateOfBirth);
  document.getElementById('age').textContent = calculateAge(runnerData.dateOfBirth);
  document.getElementById('gender').textContent = runnerData.gender || 'N/A';
  document.getElementById('status').textContent = runnerData.status;

  // Update contact information
  document.getElementById('email').textContent = runnerData.email || 'N/A';
  document.getElementById('phoneNumber').textContent = runnerData.phoneNumber || 'N/A';
  document.getElementById('address').textContent = runnerData.address || 'N/A';
  document.getElementById('city').textContent = runnerData.city || 'N/A';
  document.getElementById('state').textContent = runnerData.state || 'N/A';
  document.getElementById('zipCode').textContent = runnerData.zipCode || 'N/A';

  // Update physical description
  document.getElementById('height').textContent = runnerData.height ? `${runnerData.height}"` : 'N/A';
  document.getElementById('weight').textContent = runnerData.weight ? `${runnerData.weight} lbs` : 'N/A';
  document.getElementById('hairColor').textContent = runnerData.hairColor || 'N/A';
  document.getElementById('eyeColor').textContent = runnerData.eyeColor || 'N/A';
  document.getElementById('identifyingMarks').textContent = runnerData.identifyingMarks || 'N/A';

  // Update medical information
  document.getElementById('medicalConditions').textContent = runnerData.medicalConditions || 'N/A';
  document.getElementById('medications').textContent = runnerData.medications || 'N/A';
  document.getElementById('allergies').textContent = runnerData.allergies || 'N/A';
  document.getElementById('emergencyContacts').textContent = runnerData.emergencyContacts || 'N/A';
}

// Update cases UI
function updateCasesUI() {
  const casesContainer = document.getElementById('casesContainer');
  
  if (casesData.length === 0) {
    casesContainer.innerHTML = `
      <div class="empty-state">
        <div class="empty-icon">📋</div>
        <h3 class="empty-title">No cases yet</h3>
        <p class="empty-description">
          No cases have been reported for this runner yet.
        </p>
      </div>
    `;
    return;
  }

  casesContainer.innerHTML = casesData.map(caseItem => `
    <div class="case-item" onclick="viewCase(${caseItem.id})">
      <div class="case-icon">📋</div>
      <div class="case-content">
        <h3 class="case-title">${caseItem.title || 'Untitled Case'}</h3>
        <p class="case-meta">
          Created: ${formatDate(caseItem.createdAt)} • 
          Last updated: ${formatRelativeTime(caseItem.lastUpdatedAt)}
        </p>
      </div>
      <span class="case-status ${getCaseStatusColorClass(caseItem.status)}">${caseItem.status}</span>
    </div>
  `).join('');
}

// Show profile content
function showProfileContent() {
  document.getElementById('loadingState').classList.add('hidden');
  document.getElementById('errorState').classList.add('hidden');
  document.getElementById('profileContent').classList.remove('hidden');
}

// Show error state
function showError(message) {
  document.getElementById('loadingState').classList.add('hidden');
  document.getElementById('profileContent').classList.add('hidden');
  
  const errorState = document.getElementById('errorState');
  const errorMessage = document.getElementById('errorMessage');
  
  errorMessage.textContent = message;
  errorState.classList.remove('hidden');
}

/**
 * ============================================
 * LINK SETUP
 * ============================================
 */

// Setup navigation links
function setupLinks() {
  // Edit runner link
  const editRunnerLink = document.getElementById('editRunnerLink');
  if (editRunnerLink) {
    editRunnerLink.href = `/app/runners/${runnerId}`;
  }

  // New case link
  const newCaseLink = document.getElementById('newCaseLink');
  if (newCaseLink) {
    newCaseLink.href = `/app/cases/new?individualId=${runnerId}`;
  }
}

/**
 * ============================================
 * NAVIGATION
 * ============================================
 */

// View case details
function viewCase(caseId) {
  window.location.href = `/app/cases/${caseId}`;
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

// Calculate age from date of birth
function calculateAge(dateOfBirth) {
  if (!dateOfBirth) return 'N/A';
  
  try {
    const birthDate = new Date(dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    
    return `${age} years old`;
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

// Get case status color class
function getCaseStatusColorClass(status) {
  switch (status?.toLowerCase()) {
    case 'active':
      return 'active';
    case 'resolved':
      return 'resolved';
    case 'closed':
      return 'closed';
    default:
      return 'active';
  }
}

/**
 * ============================================
 * ERROR HANDLING
 * ============================================
 */

// Handle profile errors
function handleProfileError(error) {
  console.error('Profile Error:', error);
  
  if (error.message.includes('404')) {
    showError('Runner not found. The runner profile you\'re looking for doesn\'t exist.');
  } else if (error.message.includes('403')) {
    showError('Access denied. You don\'t have permission to view this runner profile.');
  } else if (error.message.includes('401')) {
    window.Auth.showError('Session expired. Please sign in again.');
    setTimeout(() => {
      window.location.href = '/login.html';
    }, 2000);
  } else if (error.message.includes('Network')) {
    showError('Network error. Please check your connection and try again.');
  } else {
    showError('An error occurred while loading the runner profile. Please try again.');
  }
}

/**
 * ============================================
 * EXPORTS
 * ============================================
 */

// Make functions available globally
window.RunnerProfile = {
  // Data management
  loadRunnerData,
  loadCasesData,
  updateProfileUI,
  updateCasesUI,
  
  // Navigation
  viewCase,
  
  // Utilities
  formatDate,
  calculateAge,
  formatRelativeTime,
  getStatusColorClass,
  getCaseStatusColorClass,
  
  // Error handling
  handleProfileError,
};
