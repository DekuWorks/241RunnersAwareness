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
 * - Backend API integration with fallback to mock authentication
 *
 * Usage:
 * Include this file in all HTML pages and call updateNavigation() on page load.
 */

// API Configuration
const API_BASE_URL = 'http://localhost:5113/api';

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
      window.location.href = 'index.html';
    }, 1500);
  }
}

/**
 * ============================================
 * BACKEND API INTEGRATION
 * ============================================
 *
 * Functions to interact with the .NET backend API
 * with fallback to mock authentication if backend is unavailable.
 */

async function handleLogin(email, password) {
  try {
    // Try to connect to the backend API
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
        email: email,
        role: 'admin', // Backend will provide actual role
        accessToken: data.accessToken,
        refreshToken: data.refreshToken
      };
      
      localStorage.setItem('user', JSON.stringify(userData));
      localStorage.setItem('userToken', data.accessToken);
      
      showNotification('Login successful!', 'success');
      
      // Redirect based on role
      if (userData.role === 'admin') {
        window.location.href = 'admin-dashboard.html';
      } else {
        window.location.href = 'index.html';
      }
      
      return true;
    } else {
      throw new Error('Login failed');
    }
  } catch (error) {
    console.log('Backend login failed, using mock authentication:', error);
    
    // Fallback to mock authentication
    return handleMockLogin(email, password);
  }
}

async function handleRegister(email, password, fullName) {
  try {
    // Try to connect to the backend API
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ 
        email, 
        password, 
        displayName: fullName 
      }),
    });

    if (response.ok) {
      showNotification('Registration successful! Please log in.', 'success');
      return true;
    } else {
      throw new Error('Registration failed');
    }
  } catch (error) {
    console.log('Backend registration failed, using mock authentication:', error);
    
    // Fallback to mock registration
    return handleMockRegister(email, password, fullName);
  }
}

/**
 * ============================================
 * MOCK AUTHENTICATION (FALLBACK)
 * ============================================
 *
 * Mock authentication functions used when backend is unavailable.
 */

function handleMockLogin(email, password) {
  // Demo credentials for testing
  const demoCredentials = {
    'admin@example.com': { password: 'ChangeMe123!', role: 'admin', name: 'System Admin' },
    'test@example.com': { password: 'password123', role: 'user', name: 'Test User' },
    'dekuworks1@gmail.com': { password: 'marcus2025', role: 'admin', name: 'Marcus Brown' },
    'danielcarey9770@gmail.com': { password: 'daniel2025', role: 'admin', name: 'Daniel Carey' }
  };

  const userCreds = demoCredentials[email];
  
  if (userCreds && userCreds.password === password) {
    const userData = {
      email: email,
      role: userCreds.role,
      name: userCreds.name
    };
    
    localStorage.setItem('user', JSON.stringify(userData));
    showNotification('Login successful!', 'success');
    
    // Redirect based on role
    if (userData.role === 'admin') {
      window.location.href = 'admin-dashboard.html';
    } else {
      window.location.href = 'index.html';
    }
    
    return true;
  } else {
    showNotification('Invalid email or password.', 'error');
    return false;
  }
}

function handleMockRegister(email, password, fullName) {
  // For mock registration, just show success and redirect to login
  showNotification('Registration successful! Please log in.', 'success');
  setTimeout(() => {
    window.location.href = 'login.html';
  }, 1500);
  return true;
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
    email: 'admin@example.com',
    password: 'ChangeMe123!',
    fullName: 'System Admin',
    role: 'admin',
    phoneNumber: '(555) 123-4567',
    organization: '241 Runners Awareness',
    credentials: 'System Administrator',
    specialization: 'Platform Management',
    yearsOfExperience: '5+'
  },
  {
    email: 'test@example.com',
    password: 'password123',
    fullName: 'Test User',
    role: 'user',
    phoneNumber: '(555) 234-5678'
  },
  {
    email: 'dekuworks1@gmail.com',
    password: 'marcus2025',
    fullName: 'Marcus Brown',
    role: 'admin',
    phoneNumber: '(555) 345-6789',
    organization: '241 Runners Awareness',
    credentials: 'Co-Founder',
    specialization: 'Operations',
    yearsOfExperience: '3+'
  },
  {
    email: 'danielcarey9770@gmail.com',
    password: 'daniel2025',
    fullName: 'Daniel Carey',
    role: 'admin',
    phoneNumber: '(555) 456-7890',
    organization: '241 Runners Awareness',
    credentials: 'Co-Founder',
    specialization: 'Technology',
    yearsOfExperience: '4+'
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
 * Handle login with fallback to mock authentication
 * @param {string} email - User email
 * @param {string} password - User password
 */
async function handleLogin(email, password) {
  try {
    // Try to connect to the backend API first
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
        userId: data.user.userId
      };
      
      localStorage.setItem('user', JSON.stringify(userData));
      localStorage.setItem('userToken', data.token);
      
      showNotification('Login successful!', 'success');
      
      // Redirect based on role
      if (userData.role === 'admin') {
        window.location.href = 'admin-dashboard.html';
      } else {
        window.location.href = 'index.html';
      }
      
      return true;
    } else {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Login failed');
    }
  } catch (error) {
    console.log('Backend login failed, using mock authentication:', error);
    
    // Fallback to mock authentication
    try {
      const result = await mockLogin(email, password);
      localStorage.setItem('user', JSON.stringify(result.user));
      showNotification('Login successful! Redirecting...', 'success');
      setTimeout(() => {
        // Redirect admin users to dashboard, others to home page
        if (result.user.role === 'admin') {
          window.location.href = 'admin-dashboard.html';
        } else {
          window.location.href = 'index.html';
        }
      }, 1500);
      return true;
    } catch (mockError) {
      showNotification(mockError.message || 'Login failed', 'error');
      return false;
    }
  }
}

/**
 * Handle registration with fallback to mock authentication
 * @param {Object} userData - User registration data
 */
async function handleRegister(userData) {
  try {
    // Try to connect to the backend API first
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: userData.email,
        password: userData.password,
        fullName: userData.fullName,
        phoneNumber: userData.phoneNumber || '',
        role: userData.role || 'user',
        // Role-specific fields
        relationshipToRunner: userData.relationshipToRunner,
        licenseNumber: userData.licenseNumber,
        organization: userData.organization,
        credentials: userData.credentials,
        specialization: userData.specialization,
        yearsOfExperience: userData.yearsOfExperience,
        // Common fields
        address: userData.address,
        city: userData.city,
        state: userData.state,
        zipCode: userData.zipCode,
        emergencyContactName: userData.emergencyContactName,
        emergencyContactPhone: userData.emergencyContactPhone,
        emergencyContactRelationship: userData.emergencyContactRelationship
      }),
    });

    if (response.ok) {
      const data = await response.json();
      showNotification('Registration successful! Please check your email and phone for verification.', 'success');
      setTimeout(() => {
        window.location.href = 'login.html';
      }, 2000);
      return true;
    } else {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Registration failed');
    }
  } catch (error) {
    console.log('Backend registration failed, using mock authentication:', error);
    
    // Fallback to mock registration
    try {
      const result = await mockRegister(userData);
      showNotification('Registration successful! Please log in.', 'success');
      setTimeout(() => {
        window.location.href = 'login.html';
      }, 1500);
      return true;
    } catch (mockError) {
      showNotification(mockError.message || 'Registration failed', 'error');
      return false;
    }
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
