/**
 * 241 Runners Awareness - Authentication Utilities
 * 
 * This file contains authentication functions for the frontend application.
 * All authentication now goes through the Azure backend API.
 * 
 * Updated with input validation, error handling, and security measures.
 */

// Import validation schemas
import { validateFormData, signupSchema, loginSchema, sanitizeInput } from './validation.js';

// API Configuration
let API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api';

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
  // Check for authentication data in multiple possible localStorage keys
  const user = localStorage.getItem('user');
  const raAuth = localStorage.getItem('ra_auth');
  const adminToken = localStorage.getItem('ra_admin_token');
  const jwtToken = localStorage.getItem('jwtToken');
  
  // Determine if user is authenticated
  let isAuthenticated = false;
  let userData = null;
  let userRole = null;
  
  if (user) {
    try {
      userData = JSON.parse(user);
      isAuthenticated = true;
      userRole = userData.role;
    } catch (e) {
      console.warn('Error parsing user data:', e);
    }
  } else if (raAuth) {
    try {
      userData = JSON.parse(raAuth);
      isAuthenticated = !!(userData.token || userData.accessToken);
      userRole = userData.user?.role;
    } catch (e) {
      console.warn('Error parsing ra_auth data:', e);
    }
  } else if (adminToken) {
    isAuthenticated = true;
    userRole = localStorage.getItem('ra_admin_role');
    const adminUser = localStorage.getItem('ra_admin_user');
    if (adminUser) {
      try {
        userData = JSON.parse(adminUser);
        userRole = userData.role;
      } catch (e) {
        console.warn('Error parsing admin user data:', e);
      }
    }
  } else if (jwtToken) {
    isAuthenticated = true;
    // Try to get role from other sources
    userRole = localStorage.getItem('ra_admin_role');
  }
  
  // Get navigation elements
  const signupLink = document.getElementById('signupLink');
  const loginLink = document.getElementById('loginLink');
  const profileLink = document.getElementById('profileLink');
  const dashboardLink = document.getElementById('dashboardLink');
  const runnerProfileLink = document.getElementById('runnerProfileLink');
  const userManagementLink = document.getElementById('userManagementLink');
  const logoutBtn = document.getElementById('logoutBtn');
  const adminLink = document.getElementById('adminLink');

  if (isAuthenticated) {
    // User is logged in - hide auth links, show logout and profile
    if (signupLink) signupLink.style.display = 'none';
    if (loginLink) loginLink.style.display = 'none';
    if (profileLink) profileLink.style.display = 'inline-block';
    if (logoutBtn) logoutBtn.style.display = 'inline-block';
    
    // Show dashboard link for all authenticated users
    if (dashboardLink) {
      dashboardLink.style.display = 'inline-block';
    }
    // Show runner profile link for all authenticated users
    if (runnerProfileLink) {
      runnerProfileLink.style.display = 'inline-block';
    }
    // Show admin link for admin users only
    if (adminLink && (userRole === 'admin' || userRole === 'Admin')) {
      adminLink.style.display = 'inline-block';
    }
    // Show user management links for admin users only
    if (userManagementLink && (userRole === 'admin' || userRole === 'Admin')) {
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
    if (adminLink) adminLink.style.display = 'none';
  }
}

/**
 * Clear all authentication data from localStorage
 */
function clearAllAuth() {
  localStorage.removeItem('user');
  localStorage.removeItem('ra_auth');
  localStorage.removeItem('ra_admin_token');
  localStorage.removeItem('ra_admin_role');
  localStorage.removeItem('ra_admin_user');
  localStorage.removeItem('jwtToken');
  updateNavigation();
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
        // Validate input data
        const validation = validateFormData({ email, password }, loginSchema);
        if (!validation.success) {
            if (window.errorHandler) {
                window.errorHandler.handleError('Validation Error', { errors: validation.errors });
            }
            return false;
        }

        // Sanitize inputs
        const sanitizedEmail = sanitizeInput(email);
        const sanitizedPassword = sanitizeInput(password);

        // Show loading state
        if (window.loadingManager) {
            window.loadingManager.showSpinner('Logging in...', 'login');
        }

        // Set timeout for request
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 30000); // 30 second timeout

        const response = await fetch(`${API_BASE_URL}/v1/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ 
                email: sanitizedEmail, 
                password: sanitizedPassword 
            }),
            signal: controller.signal
        });

        clearTimeout(timeoutId);

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
            
            if (window.errorHandler) {
                window.errorHandler.showToast('Login successful!', 'success');
            }
            
            // Redirect based on role
            // If user is admin, redirect to full admin dashboard
            // If user has dual roles, they can access both admin and user features
            if (userData.role === 'admin' || (userData.allRoles && userData.allRoles.includes('admin'))) {
                // Always redirect admins to full admin dashboard from regular login
                window.location.href = 'admin/admindash.html';
            } else {
                // Regular users go to profile
                window.location.href = 'profile.html';
            }
            
            return true;
        } else {
            const errorData = await response.json();
            let errorMessage = 'Login failed';
            
            // Handle specific error cases
            switch (response.status) {
                case 401:
                    errorMessage = 'Invalid email or password';
                    break;
                case 429:
                    errorMessage = 'Too many login attempts. Please try again later.';
                    break;
                case 500:
                    errorMessage = 'Server error. Please try again later.';
                    break;
                default:
                    errorMessage = errorData.message || 'Login failed';
            }
            
            throw new Error(errorMessage);
        }
    } catch (error) {
        console.error('Login error:', error);
        
        let errorMessage = 'Login failed. Please try again.';
        
        if (error.name === 'AbortError') {
            errorMessage = 'Login request timed out. Please try again.';
        } else if (error.message) {
            errorMessage = error.message;
        }
        
        if (window.errorHandler) {
            window.errorHandler.handleError('Authentication Error', error, { 
                action: 'login',
                email: email 
            });
        }
        
        return false;
    } finally {
        // Hide loading state
        if (window.loadingManager) {
            window.loadingManager.hideSpinner('login');
        }
    }
}

async function handleRegister(userData) {
    try {
        // Validate input data
        const validation = validateFormData(userData, signupSchema);
        if (!validation.success) {
            if (window.errorHandler) {
                window.errorHandler.handleError('Validation Error', { errors: validation.errors });
            }
            return false;
        }

        // Sanitize inputs
        const sanitizedData = {
            firstName: sanitizeInput(userData.firstName),
            lastName: sanitizeInput(userData.lastName),
            email: sanitizeInput(userData.email),
            password: sanitizeInput(userData.password),
            confirmPassword: sanitizeInput(userData.confirmPassword),
            role: userData.role,
            agreeToTerms: userData.agreeToTerms
        };

        // Show loading state
        if (window.loadingManager) {
            window.loadingManager.showSpinner('Creating account...', 'register');
        }

        // Set timeout for request
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 30000); // 30 second timeout

        const response = await fetch(`${API_BASE_URL}/v1/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(sanitizedData),
            signal: controller.signal
        });

        clearTimeout(timeoutId);

        const result = await response.json();
        
        if (response.ok && result.success) {
            if (window.errorHandler) {
                window.errorHandler.showToast('Registration successful! Please log in.', 'success');
            }
            return true;
        } else {
            let errorMessage = 'Registration failed';
            
            // Handle specific error cases
            switch (response.status) {
                case 400:
                    errorMessage = 'Please check your input and try again.';
                    break;
                case 409:
                    errorMessage = 'An account with this email already exists.';
                    break;
                case 429:
                    errorMessage = 'Too many registration attempts. Please try again later.';
                    break;
                case 500:
                    errorMessage = 'Server error. Please try again later.';
                    break;
                default:
                    errorMessage = result.message || 'Registration failed';
            }
            
            throw new Error(errorMessage);
        }
    } catch (error) {
        console.error('Registration error:', error);
        
        let errorMessage = 'Registration failed. Please try again.';
        
        if (error.name === 'AbortError') {
            errorMessage = 'Registration request timed out. Please try again.';
        } else if (error.message) {
            errorMessage = error.message;
        }
        
        if (window.errorHandler) {
            window.errorHandler.handleError('Authentication Error', error, { 
                action: 'register',
                email: userData.email 
            });
        }
        
        return false;
    } finally {
        // Hide loading state
        if (window.loadingManager) {
            window.loadingManager.hideSpinner('register');
        }
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
    return user && (user.role === 'admin' || (user.allRoles && user.allRoles.includes('admin')));
}

/**
 * Check if current user has a specific role
 * @param {string} role - Role to check for
 * @returns {boolean} - True if current user has the role
 */
function hasRole(role) {
    const user = getCurrentUser();
    if (!user) return false;
    
    // Check primary role
    if (user.role === role) return true;
    
    // Check additional roles
    if (user.allRoles && user.allRoles.includes(role)) return true;
    
    return false;
}

/**
 * Get the primary user role (for UI display)
 * @returns {string} - Primary user role
 */
function getPrimaryUserRole() {
    const user = getCurrentUser();
    if (!user) return 'user';
    
    // If user is admin but has additional roles, return the first non-admin role
    if (user.role === 'admin' && user.allRoles && user.allRoles.length > 1) {
        return user.primaryUserRole || user.allRoles.find(r => r !== 'admin') || 'user';
    }
    
    return user.role || 'user';
}

/**
 * Check if current user is an admin user (has admin privileges)
 * @returns {boolean} - True if user has admin privileges
 */
function isAdminUser() {
    const user = getCurrentUser();
    return user && (user.role === 'admin' || user.isAdminUser === true);
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

// Check if user has admin role (handled by isAdmin() function above)

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
