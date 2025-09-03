/**
 * 241 Runners Awareness - Authentication Utilities
 * 
 * This file contains authentication functions for the frontend application.
 * All authentication now goes through the Azure backend API.
 */

// API Configuration
let API_BASE_URL = 'https://241runnersawareness-api.azurewebsites.net/api';

// Load API configuration
async function loadConfig() {
    try {
        const response = await fetch('/config.json');
        const config = await response.json();
        API_BASE_URL = config.API_BASE_URL;
    } catch (error) {
        console.warn('Failed to load config.json, using default API URL');
    }
}

// Initialize config on load
loadConfig();

/**
 * ============================================
 * NAVIGATION STATE MANAGEMENT
 * ============================================
 *
 * Manages the visibility of authentication-related navigation elements
 * based on the user's login status.
 */

function updateNavigation() {
  const user = localStorage.getItem('user');
  const signupLink = document.getElementById('signupLink');
  const loginLink = document.getElementById('loginLink');
  const profileLink = document.getElementById('profileLink');
  const dashboardLink = document.getElementById('dashboardLink');
  const runnerProfileLink = document.getElementById('runnerProfileLink');
  const userManagementLink = document.getElementById('userManagementLink');
  const logoutBtn = document.getElementById('logoutBtn');

  if (user) {
    // User is logged in - hide auth links, show logout and profile
    if (signupLink) signupLink.style.display = 'none';
    if (loginLink) loginLink.style.display = 'none';
    if (profileLink) profileLink.style.display = 'inline-block';
    if (logoutBtn) logoutBtn.style.display = 'inline-block';
    
    // Show dashboard link for all authenticated users
    const userData = JSON.parse(user);
    if (dashboardLink) {
      dashboardLink.style.display = 'inline-block';
    }
    // Show runner profile link for all authenticated users
    if (runnerProfileLink) {
      runnerProfileLink.style.display = 'inline-block';
    }
    // Show user management links for admin users only
    if (userManagementLink && userData.role === 'admin') {
      userManagementLink.style.display = 'inline-block';
    }
  } else {
    // User is not logged in - show auth links, hide logout and profile
    if (signupLink) signupLink.style.display = 'inline-block';
    if (loginLink) loginLink.style.display = 'inline-block';
    if (profileLink) profileLink.style.display = 'none';
    if (dashboardLink) dashboardLink.style.display = 'none';
    if (runnerProfileLink) runnerProfileLink.style.display = 'none';
    if (userManagementLink) userManagementLink.style.display = 'none';
    if (logoutBtn) logoutBtn.style.display = 'none';
  }
}

/**
 * ============================================
 * BACKEND API INTEGRATION
 * ============================================
 *
 * Functions to interact with the Azure backend API
 */

async function handleLogin(email, password) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ email, password }),
        });

        if (response.ok) {
            const data = await response.json();
            
            // Store user data and tokens
            const userData = {
                email: data.user.email,
                role: data.user.role,
                fullName: data.user.fullName,
                token: data.token,
                userId: data.user.id
            };
            
            localStorage.setItem('user', JSON.stringify(userData));
            localStorage.setItem('userToken', data.token);
            
            showNotification('Login successful!', 'success');
            
            // Redirect based on role
            if (userData.role === 'admin') {
                window.location.href = 'admin/index.html';
            } else {
                window.location.href = 'dashboard.html';
            }
            
            return true;
        } else {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Login failed');
        }
    } catch (error) {
        console.error('Login error:', error);
        showNotification(error.message || 'Login failed. Please try again.', 'error');
        return false;
    }
}

async function handleRegister(userData) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData),
        });

        const result = await response.json();
        
        if (result.success) {
            showNotification('Registration successful! Please log in.', 'success');
            return true;
        } else {
            throw new Error(result.message || 'Registration failed');
        }
    } catch (error) {
        console.error('Registration error:', error);
        showNotification(error.message || 'Registration failed. Please try again.', 'error');
        return false;
    }
}

/**
 * Handle logout
 */
function handleLogout() {
    // Clear all stored data
    localStorage.removeItem('user');
    localStorage.removeItem('userToken');
    
    // Redirect to login page
    window.location.href = '/login.html';
}

/**
 * Check if user is authenticated
 * @returns {boolean} - True if user is authenticated
 */
function isAuthenticated() {
    const user = localStorage.getItem('user');
    const token = localStorage.getItem('userToken');
    return !!(user && token);
}

/**
 * Get current user data
 * @returns {Object|null} - Current user data or null if not authenticated
 */
function getCurrentUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
}

/**
 * Get current user token
 * @returns {string|null} - Current user token or null if not authenticated
 */
function getUserToken() {
    return localStorage.getItem('userToken');
}

/**
 * Check if current user is admin
 * @returns {boolean} - True if current user is admin
 */
function isAdmin() {
    const user = getCurrentUser();
    return user && user.role === 'admin';
}

/**
 * Show notification message
 * @param {string} message - Message to display
 * @param {string} type - Type of notification (success, error, warning, info)
 */
function showNotification(message, type = 'info') {
    // Remove any existing notifications
  const existingNotifications = document.querySelectorAll('.auth-notification');
  existingNotifications.forEach(notification => {
    if (notification.parentNode) {
      notification.parentNode.removeChild(notification);
    }
  });

  // Create notification element
  const notification = document.createElement('div');
  notification.className = `auth-notification auth-notification-${type}`;
  notification.textContent = message;

  // Style the notification
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 15px 20px;
    border-radius: 5px;
    z-index: 10000;
    box-shadow: 0 2px 10px rgba(0,0,0,0.2);
    font-weight: 500;
    animation: slideIn 0.3s ease-out;
    max-width: 300px;
    word-wrap: break-word;
  `;

  // Set background color based on type
  switch (type) {
    case 'success':
      notification.style.background = '#4CAF50';
      notification.style.color = 'white';
      break;
    case 'error':
      notification.style.background = '#f44336';
      notification.style.color = 'white';
      break;
    case 'warning':
      notification.style.background = '#ff9800';
      notification.style.color = 'white';
      break;
    default:
      notification.style.background = '#2196F3';
      notification.style.color = 'white';
  }

  // Add to page
  document.body.appendChild(notification);

  // Remove notification after 3 seconds
  setTimeout(() => {
    if (notification.parentNode) {
      notification.style.animation = 'slideOut 0.3s ease-in';
      setTimeout(() => {
        if (notification.parentNode) {
          notification.parentNode.removeChild(notification);
        }
      }, 300);
    }
  }, 3000);
}

/**
 * ============================================
 * UTILITY FUNCTIONS
 * ============================================
 *
 * Helper functions for authentication and user management.
 */

// Check if user is currently logged in
function isUserLoggedIn() {
  return localStorage.getItem('user') !== null;
}

// Get current user data
function getCurrentUser() {
  const userData = localStorage.getItem('user');
  return userData ? JSON.parse(userData) : null;
}

// Check if user has admin role
function isAdmin() {
  const user = getCurrentUser();
  return user && user.role === 'admin';
}

/**
 * ============================================
 * PAGE INITIALIZATION
 * ============================================
 *
 * Functions to initialize authentication state on page load.
 */

function initializeAuth() {
  // Update navigation state
  updateNavigation();

  // Add logout button event listener
  const logoutBtn = document.getElementById('logoutBtn');
  if (logoutBtn) {
    logoutBtn.addEventListener('click', handleLogout);
  }

  // Add CSS animations if not already present
  if (!document.querySelector('#auth-animations')) {
    const style = document.createElement('style');
    style.id = 'auth-animations';
    style.textContent = `
      @keyframes slideIn {
        from {
          transform: translateX(100%);
          opacity: 0;
        }
        to {
          transform: translateX(0);
          opacity: 1;
        }
      }

      @keyframes slideOut {
        from {
          transform: translateX(0);
          opacity: 1;
        }
        to {
          transform: translateX(100%);
          opacity: 0;
        }
      }
    `;
    document.head.appendChild(style);
  }
}

// Auto-initialize when DOM is loaded
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initializeAuth);
} else {
  initializeAuth();
}
