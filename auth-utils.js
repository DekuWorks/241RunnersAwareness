/**
 * ============================================
 * 241 RUNNERS AWARENESS - AUTHENTICATION UTILITIES
 * ============================================
 *
 * This file contains reusable authentication functions for the static HTML pages.
 * It provides consistent logout functionality and navigation state management
 * across all pages of the website.
 *
 * Features:
 * - Universal logout functionality
 * - Navigation state management
 * - Google OAuth session cleanup
 * - User feedback and notifications
 * - Mock authentication for development/testing
 *
 * Usage:
 * Include this file in all HTML pages and call updateNavigation() on page load.
 */

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
  const dashboardLink = document.getElementById('dashboardLink');
  const logoutBtn = document.getElementById('logoutBtn');

  if (user) {
    // User is logged in - hide auth links, show dashboard and logout
    if (signupLink) signupLink.style.display = 'none';
    if (loginLink) loginLink.style.display = 'none';
    if (dashboardLink) dashboardLink.style.display = 'inline-block';
    if (logoutBtn) logoutBtn.style.display = 'inline-block';
  } else {
    // User is not logged in - show auth links, hide dashboard and logout
    if (signupLink) signupLink.style.display = 'inline-block';
    if (loginLink) loginLink.style.display = 'inline-block';
    if (dashboardLink) dashboardLink.style.display = 'none';
    if (logoutBtn) logoutBtn.style.display = 'none';
  }
}

/**
 * ============================================
 * LOGOUT FUNCTIONALITY
 * ============================================
 *
 * Handles user logout across all pages with proper cleanup.
 */

function handleLogout() {
  // Clear all authentication data
  localStorage.removeItem('user');
  localStorage.removeItem('google_access_token');
  localStorage.removeItem('userToken');

  // Revoke Google session if available
  if (window.google && window.google.accounts) {
    try {
      window.google.accounts.oauth2.revoke(localStorage.getItem('google_access_token'));
    } catch (error) {
      console.log('Google session revocation failed:', error);
    }
  }

  // Update navigation to show login/signup links
  updateNavigation();

  // Show logout confirmation message
  showNotification('You have been successfully logged out.', 'success');

  // Redirect to home page if not already there
  if (window.location.pathname !== '/' && window.location.pathname !== '/index.html') {
    setTimeout(() => {
      window.location.href = '/index.html';
    }, 1500);
  }
}

/**
 * ============================================
 * MOCK AUTHENTICATION SYSTEM
 * ============================================
 *
 * Provides mock authentication for development and testing
 * when the backend is not available.
 */

// Mock user database for development
const mockUsers = [
  {
    email: 'test@example.com',
    password: process.env.MOCK_TEST_PASSWORD || 'password123',
    fullName: 'Test User',
    role: 'GeneralUser'
  },
  {
    email: 'dekuworks1@gmail.com',
    password: process.env.MOCK_MARCUS_PASSWORD || 'marcus2025',
    fullName: 'Marcus Brown',
    role: 'admin'
  },
  {
    email: 'danielcarey9770@gmail.com',
    password: process.env.MOCK_DANIEL_PASSWORD || 'daniel2025',
    fullName: 'Daniel Carey',
    role: 'admin'
  }
];

/**
 * Mock login function
 * @param {string} email - User email
 * @param {string} password - User password
 * @returns {Promise<Object>} - Login result
 */
async function mockLogin(email, password) {
  // Simulate network delay
  await new Promise(resolve => setTimeout(resolve, 1000));

  const user = mockUsers.find(u => u.email === email && u.password === password);

  if (user) {
    return {
      success: true,
      message: 'Login successful!',
      user: {
        id: Date.now(),
        email: user.email,
        fullName: user.fullName,
        role: user.role,
        token: 'mock-jwt-token-' + Date.now()
      }
    };
  } else {
    throw new Error('Invalid email or password');
  }
}

/**
 * Mock registration function
 * @param {Object} userData - User registration data
 * @returns {Promise<Object>} - Registration result
 */
async function mockRegister(userData) {
  // Simulate network delay
  await new Promise(resolve => setTimeout(resolve, 1000));

  // Check if user already exists
  const existingUser = mockUsers.find(u => u.email === userData.email);
  if (existingUser) {
    throw new Error('User with this email already exists');
  }

  // Validate password match
  if (userData.password !== userData.confirmPassword) {
    throw new Error('Passwords do not match');
  }

  // Add user to mock database
  const newUser = {
    email: userData.email,
    password: userData.password,
    fullName: userData.fullName,
    role: userData.role || 'GeneralUser'
  };

  mockUsers.push(newUser);

  return {
    success: true,
    message: 'Registration successful!',
    user: {
      id: Date.now(),
      email: newUser.email,
      fullName: newUser.fullName,
      role: newUser.role,
      token: 'mock-jwt-token-' + Date.now()
    }
  };
}

/**
 * ============================================
 * UNIVERSAL AUTHENTICATION HANDLERS
 * ============================================
 *
 * Functions that can be called from any page to handle authentication.
 */

/**
 * Handle login with real backend
 * @param {string} email - User email
 * @param {string} password - User password
 */
async function handleLogin(email, password) {
  try {
    const response = await fetch('http://localhost:5113/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password })
    });

    const data = await response.json();

    if (response.ok) {
      localStorage.setItem('user', JSON.stringify(data));
      showNotification('Login successful! Redirecting...', 'success');
      setTimeout(() => {
        window.location.href = 'dashboard.html';
      }, 1500);
    } else {
      showNotification(data.message || 'Login failed', 'error');
    }
  } catch (error) {
    console.error('Login error:', error);
    showNotification('Network error. Please try again.', 'error');
  }
}

/**
 * Handle registration with real backend
 * @param {Object} userData - User registration data
 */
async function handleRegister(userData) {
  try {
    const response = await fetch('http://localhost:5113/api/auth/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData)
    });

    const data = await response.json();

    if (response.ok) {
      localStorage.setItem('user', JSON.stringify(data));
      showNotification('Registration successful! Redirecting...', 'success');
      setTimeout(() => {
        window.location.href = 'dashboard.html';
      }, 1500);
    } else {
      showNotification(data.message || 'Registration failed', 'error');
    }
  } catch (error) {
    console.error('Registration error:', error);
    showNotification('Network error. Please try again.', 'error');
  }
}

/**
 * ============================================
 * NOTIFICATION SYSTEM
 * ============================================
 *
 * Provides user feedback for authentication actions.
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
