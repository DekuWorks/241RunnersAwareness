/**
 * ============================================
 * 241 RUNNERS AWARENESS - AUTHENTICATION UTILITIES
 * ============================================
 * 
 * This file provides authentication functionality for the static site,
 * including login, signup, token management, and API communication.
 */

// Configuration
let API_BASE_URL = 'http://localhost:5113/api';

// Load configuration
async function loadConfig() {
  try {
    const response = await fetch('/config.json');
    const config = await response.json();
    API_BASE_URL = config.API_BASE_URL || API_BASE_URL;
  } catch (error) {
    console.warn('Failed to load config.json, using default API URL:', error);
  }
}

// Initialize config on load
loadConfig();

/**
 * ============================================
 * AUTHENTICATION STATE MANAGEMENT
 * ============================================
 */

// Get authentication data from localStorage
function getAuth() {
  try {
    const authData = localStorage.getItem('ra_auth');
    return authData ? JSON.parse(authData) : null;
  } catch (error) {
    console.error('Error parsing auth data:', error);
    return null;
  }
}

// Save authentication data to localStorage
function saveAuth(authData) {
  try {
    localStorage.setItem('ra_auth', JSON.stringify(authData));
    // Dispatch custom event for other components to listen to
    window.dispatchEvent(new CustomEvent('authChanged', { detail: authData }));
  } catch (error) {
    console.error('Error saving auth data:', error);
  }
}

// Clear authentication data
function clearAuth() {
  try {
    localStorage.removeItem('ra_auth');
    window.dispatchEvent(new CustomEvent('authChanged', { detail: null }));
  } catch (error) {
    console.error('Error clearing auth data:', error);
  }
}

// Check if user is authenticated
function isAuthenticated() {
  const auth = getAuth();
  return auth && auth.token && auth.user;
}

// Get auth token
function getAuthToken() {
  const auth = getAuth();
  return auth ? auth.token : null;
}

/**
 * ============================================
 * API COMMUNICATION
 * ============================================
 */

// Make authenticated API request
async function apiRequest(endpoint, options = {}) {
  const token = getAuthToken();
  
  const defaultOptions = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  if (token) {
    defaultOptions.headers.Authorization = `Bearer ${token}`;
  }

  const finalOptions = {
    ...defaultOptions,
    ...options,
    headers: {
      ...defaultOptions.headers,
      ...options.headers,
    },
  };

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, finalOptions);
    
    // Handle 401 Unauthorized
    if (response.status === 401) {
      handle401();
      throw new Error('Authentication required');
    }

    // Handle 403 Forbidden
    if (response.status === 403) {
      throw new Error('Access denied');
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    console.error('API request failed:', error);
    throw error;
  }
}

// Handle 401 Unauthorized responses
function handle401() {
  clearAuth();
  // Redirect to login page if not already there
  if (!window.location.pathname.includes('login.html')) {
    window.location.href = '/login.html';
  }
}

/**
 * ============================================
 * AUTHENTICATION OPERATIONS
 * ============================================
 */

// Login user
async function login(email, password) {
  try {
    const response = await apiRequest('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });

    const authData = {
      token: response.token,
      user: response.user,
    };

    saveAuth(authData);
    return authData;
  } catch (error) {
    console.error('Login failed:', error);
    throw error;
  }
}

// Signup user
async function signup(email, password, fullName) {
  try {
    const response = await apiRequest('/auth/register-simple', {
      method: 'POST',
      body: JSON.stringify({ email, password, fullName }),
    });

    const authData = {
      token: response.token,
      user: response.user,
    };

    saveAuth(authData);
    return authData;
  } catch (error) {
    console.error('Signup failed:', error);
    throw error;
  }
}

// Get current user profile
async function getCurrentUser() {
  try {
    const response = await apiRequest('/auth/me');
    return response;
  } catch (error) {
    console.error('Failed to get current user:', error);
    throw error;
  }
}

// Logout user
function logout() {
  clearAuth();
  window.location.href = '/login.html';
}

/**
 * ============================================
 * FORM VALIDATION
 * ============================================
 */

// Validate email format
function validateEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

// Validate password strength
function validatePassword(password) {
  return password && password.length >= 6;
}

// Validate form fields
function validateForm(fields) {
  const errors = {};

  if (fields.email && !validateEmail(fields.email)) {
    errors.email = 'Please enter a valid email address';
  }

  if (fields.password && !validatePassword(fields.password)) {
    errors.password = 'Password must be at least 6 characters long';
  }

  if (fields.confirmPassword && fields.password !== fields.confirmPassword) {
    errors.confirmPassword = 'Passwords do not match';
  }

  if (fields.fullName && !fields.fullName.trim()) {
    errors.fullName = 'Full name is required';
  }

  return errors;
}

/**
 * ============================================
 * UI HELPERS
 * ============================================
 */

// Show error message
function showError(message, containerId = 'errorContainer') {
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = `
      <div class="alert alert-error">
        <p>${message}</p>
      </div>
    `;
    container.scrollIntoView({ behavior: 'smooth' });
  }
}

// Show success message
function showSuccess(message, containerId = 'successContainer') {
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = `
      <div class="alert alert-success">
        <p>${message}</p>
      </div>
    `;
  }
}

// Clear messages
function clearMessages(containerId = 'errorContainer') {
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = '';
  }
}

// Show loading state
function showLoading(button, text = 'Loading...') {
  if (button) {
    button.disabled = true;
    button.dataset.originalText = button.textContent;
    button.innerHTML = `
      <span class="loading-spinner"></span>
      ${text}
    `;
  }
}

// Hide loading state
function hideLoading(button) {
  if (button) {
    button.disabled = false;
    button.textContent = button.dataset.originalText || button.textContent;
  }
}

// Redirect with delay
function redirectWithDelay(url, delay = 1000) {
  setTimeout(() => {
    window.location.href = url;
  }, delay);
}

/**
 * ============================================
 * PROTECTED ROUTE CHECK
 * ============================================
 */

// Check if current page requires authentication
function requireAuth() {
  if (!isAuthenticated()) {
    window.location.href = '/login.html';
    return false;
  }
  return true;
}

// Check if user is already authenticated (redirect to dashboard)
function redirectIfAuthenticated() {
  if (isAuthenticated()) {
    window.location.href = '/dashboard.html';
    return true;
  }
  return false;
}

/**
 * ============================================
 * EXPORTS
 * ============================================
 */

// Make functions available globally
window.Auth = {
  // State management
  getAuth,
  saveAuth,
  clearAuth,
  isAuthenticated,
  getAuthToken,
  
  // API operations
  apiRequest,
  login,
  signup,
  getCurrentUser,
  logout,
  
  // Validation
  validateEmail,
  validatePassword,
  validateForm,
  
  // UI helpers
  showError,
  showSuccess,
  clearMessages,
  showLoading,
  hideLoading,
  redirectWithDelay,
  
  // Route protection
  requireAuth,
  redirectIfAuthenticated,
};
