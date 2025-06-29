// Unified Authentication Utilities
// This file provides consistent authentication functions across the application

/**
 * Unified logout function that handles both Google session and local storage cleanup
 * @param {Function} navigate - Navigation function (optional)
 * @param {string} redirectPath - Path to redirect to after logout (default: '/')
 */
export const unifiedLogout = async (navigate = null, redirectPath = '/') => {
  try {
    // Clear local storage
    localStorage.removeItem('user');
    localStorage.removeItem('google_access_token');
    
    // Revoke Google session if available
    if (window.google && window.google.accounts) {
      try {
        const accessToken = localStorage.getItem('google_access_token');
        if (accessToken) {
          await window.google.accounts.oauth2.revoke(accessToken);
        }
      } catch (error) {
        console.log('Google session revocation failed:', error);
      }
    }
    
    // Navigate if function provided
    if (navigate && typeof navigate === 'function') {
      navigate(redirectPath);
    } else if (typeof window !== 'undefined') {
      // Fallback for non-React environments
      window.location.href = redirectPath;
    }
    
    return { success: true };
  } catch (error) {
    console.error('Logout error:', error);
    return { success: false, error: error.message };
  }
};

/**
 * Check if user is authenticated
 * @returns {boolean}
 */
export const isAuthenticated = () => {
  try {
    const user = localStorage.getItem('user');
    if (!user) return false;
    
    const userData = JSON.parse(user);
    return userData && userData.token;
  } catch (error) {
    console.error('Error checking authentication:', error);
    return false;
  }
};

/**
 * Get current user data
 * @returns {Object|null}
 */
export const getCurrentUser = () => {
  try {
    const user = localStorage.getItem('user');
    if (!user) return null;
    
    const userData = JSON.parse(user);
    return userData.user || null;
  } catch (error) {
    console.error('Error getting current user:', error);
    return null;
  }
};

/**
 * Get authentication token
 * @returns {string|null}
 */
export const getAuthToken = () => {
  try {
    const user = localStorage.getItem('user');
    if (!user) return null;
    
    const userData = JSON.parse(user);
    return userData.token || null;
  } catch (error) {
    console.error('Error getting auth token:', error);
    return null;
  }
};

/**
 * Store user authentication data
 * @param {Object} authData - Authentication response data
 */
export const storeAuthData = (authData) => {
  try {
    localStorage.setItem('user', JSON.stringify(authData));
  } catch (error) {
    console.error('Error storing auth data:', error);
  }
};

/**
 * Clear all authentication data
 */
export const clearAuthData = () => {
  try {
    localStorage.removeItem('user');
    localStorage.removeItem('google_access_token');
  } catch (error) {
    console.error('Error clearing auth data:', error);
  }
};

/**
 * Check if user has specific role
 * @param {string} role - Role to check
 * @returns {boolean}
 */
export const hasRole = (role) => {
  try {
    const user = getCurrentUser();
    return user && user.role === role;
  } catch (error) {
    console.error('Error checking user role:', error);
    return false;
  }
};

/**
 * Check if user is admin
 * @returns {boolean}
 */
export const isAdmin = () => {
  return hasRole('admin');
};

/**
 * Redirect user based on their role
 * @param {Function} navigate - Navigation function
 */
export const redirectByRole = (navigate) => {
  try {
    const user = getCurrentUser();
    if (!user) {
      navigate('/login');
      return;
    }
    
    switch (user.role) {
      case 'admin':
        navigate('/admin');
        break;
      case 'aba_therapist':
        navigate('/therapist-dashboard');
        break;
      case 'parent':
      case 'caregiver':
      case 'adoptive_parent':
        navigate('/parent-dashboard');
        break;
      default:
        navigate('/');
    }
  } catch (error) {
    console.error('Error redirecting by role:', error);
    navigate('/');
  }
}; 