/**
 * ============================================
 * 241 RUNNERS AWARENESS - ADMIN USER MANAGEMENT
 * ============================================
 * 
 * This file provides comprehensive admin user management functionality
 * including cloud storage integration, password changes, and user validation.
 * 
 * Features:
 * - Cloud-based admin user storage
 * - Secure password change functionality
 * - Admin user validation
 * - Session management
 * - Real-time synchronization with backend
 */

// API Configuration
const API_BASE_URL = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1' 
  ? 'http://localhost:5113/api' 
  : 'https://241runnersawareness-api.azurewebsites.net/api';

// Admin User Management Class
class AdminUserManager {
  constructor() {
    this.currentAdmin = null;
    this.adminUsers = [];
    this.isInitialized = false;
  }

  /**
   * Initialize the admin user manager
   */
  async initialize() {
    try {
      // Load admin users from cloud storage
      await this.loadAdminUsers();
      
      // Check for existing admin session
      const adminSession = localStorage.getItem('adminSession');
      if (adminSession) {
        const session = JSON.parse(adminSession);
        if (this.isSessionValid(session)) {
          this.currentAdmin = session.admin;
          return true;
        } else {
          this.clearAdminSession();
        }
      }
      
      this.isInitialized = true;
      return true;
    } catch (error) {
      console.error('Error initializing admin user manager:', error);
      return false;
    }
  }

  /**
   * Load admin users from cloud storage
   */
  async loadAdminUsers() {
    try {
      // Try to load from backend first
      const response = await fetch(`${API_BASE_URL}/admin/admins`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const users = await response.json();
        this.adminUsers = users;
      } else {
        console.log('Backend admin users not available, using local storage');
        const storedAdmins = localStorage.getItem('adminUsers');
        if (storedAdmins) {
          this.adminUsers = JSON.parse(storedAdmins);
        } else {
          // Initialize with default admin users
          this.adminUsers = this.getDefaultAdminUsers();
          this.saveAdminUsers();
        }
      }
    } catch (error) {
      console.log('Backend not available, using local storage:', error);
      const storedAdmins = localStorage.getItem('adminUsers');
      if (storedAdmins) {
        this.adminUsers = JSON.parse(storedAdmins);
      } else {
        this.adminUsers = this.getDefaultAdminUsers();
        this.saveAdminUsers();
      }
    }
  }

  /**
   * Get default admin users (fallback only)
   */
  getDefaultAdminUsers() {
    // This should only be used as a fallback when backend is not available
    console.log('Using default admin users as fallback');
    return [];
  }

  /**
   * Save admin users to cloud storage
   */
  async saveAdminUsers() {
    try {
      // Save to backend if available
      const response = await fetch(`${API_BASE_URL}/admin/users/bulk`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        },
        body: JSON.stringify(this.adminUsers)
      });

      if (!response.ok) {
        throw new Error('Backend save failed');
      }
    } catch (error) {
      console.log('Backend not available, saving to local storage:', error);
      // Fallback to local storage
      localStorage.setItem('adminUsers', JSON.stringify(this.adminUsers));
    }
  }

  /**
   * Authenticate admin user
   */
  async authenticateAdmin(email, password) {
    try {
      // Try backend authentication first
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
      });

      if (response.ok) {
        const data = await response.json();
        const user = await this.getUserByEmail(email);
        
        if (user && (user.role === 'admin' || user.role === 'superadmin')) {
          this.currentAdmin = user;
          this.createAdminSession(user, data.accessToken);
          return { success: true, user, token: data.accessToken };
        } else {
          return { success: false, message: 'User is not an admin' };
        }
      } else {
        // Fallback to local authentication
        return this.authenticateAdminLocal(email, password);
      }
    } catch (error) {
      console.log('Backend authentication failed, using local:', error);
      // Temporary fallback for deployment period
      if (email === 'dekuworks1@gmail.com' && password === 'marcus2025') {
        const tempUser = {
          email: 'dekuworks1@gmail.com',
          firstName: 'Marcus',
          lastName: 'Brown',
          role: 'admin',
          userId: 'temp-admin-1'
        };
        this.currentAdmin = tempUser;
        this.createAdminSession(tempUser, 'temp-token');
        return { success: true, user: tempUser, token: 'temp-token', message: 'Temporary admin access granted (backend deploying)' };
      }
      return this.authenticateAdminLocal(email, password);
    }
  }

  /**
   * Local admin authentication (fallback only)
   */
  authenticateAdminLocal(email, password) {
    // This should only be used as a fallback when backend is not available
    console.log('Using local authentication fallback');
    
    const user = this.adminUsers.find(u => 
      u.email.toLowerCase() === email.toLowerCase() && 
      u.isActive
    );

    if (!user) {
      return { success: false, message: 'Admin user not found' };
    }

    if (this.verifyPassword(password, user.passwordHash)) {
      this.currentAdmin = user;
      this.createAdminSession(user);
      return { success: true, user };
    } else {
      return { success: false, message: 'Invalid password' };
    }
  }

  /**
   * Change admin password
   */
  async changeAdminPassword(currentPassword, newPassword) {
    if (!this.currentAdmin) {
      return { success: false, message: 'No admin session found' };
    }

    try {
      // Verify current password
      if (!this.verifyPassword(currentPassword, this.currentAdmin.passwordHash)) {
        return { success: false, message: 'Current password is incorrect' };
      }

      // Validate new password
      if (!this.validatePassword(newPassword)) {
        return { 
          success: false, 
          message: 'Password must be at least 8 characters with uppercase, lowercase, number, and special character' 
        };
      }

      // Try backend password change first
      const response = await fetch(`${API_BASE_URL}/admin/reset-admin-password`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        },
        body: JSON.stringify({
          email: this.currentAdmin.email,
          currentPassword,
          newPassword
        })
      });

      if (response.ok) {
        // Update local user
        this.currentAdmin.passwordHash = this.hashPassword(newPassword);
        await this.saveAdminUsers();
        this.updateAdminSession();
        
        return { success: true, message: 'Password changed successfully' };
      } else {
        const errorData = await response.json();
        return { success: false, message: errorData.message || 'Password change failed' };
      }
    } catch (error) {
      console.log('Backend password change failed, using local:', error);
      return this.changeAdminPasswordLocal(currentPassword, newPassword);
    }
  }

  /**
   * Local password change
   */
  changeAdminPasswordLocal(currentPassword, newPassword) {
    // Verify current password
    if (!this.verifyPassword(currentPassword, this.currentAdmin.passwordHash)) {
      return { success: false, message: 'Current password is incorrect' };
    }

    // Update password
    this.currentAdmin.passwordHash = this.hashPassword(newPassword);
    this.saveAdminUsers();
    this.updateAdminSession();

    return { success: true, message: 'Password changed successfully' };
  }

  /**
   * Get user by email
   */
  async getUserByEmail(email) {
    try {
      // Try to get from backend first
      const response = await fetch(`${API_BASE_URL}/admin/admins`, {
        headers: {
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const users = await response.json();
        return users.find(u => u.email.toLowerCase() === email.toLowerCase());
      }
    } catch (error) {
      console.log('Backend user lookup failed:', error);
    }

    // Fallback to local admin users
    return this.adminUsers.find(u => u.email.toLowerCase() === email.toLowerCase());
  }

  /**
   * Create admin session
   */
  createAdminSession(user, token = null) {
    const session = {
      admin: user,
      token: token,
      createdAt: new Date().toISOString(),
      expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString() // 24 hours
    };

    localStorage.setItem('adminSession', JSON.stringify(session));
    if (token) {
      localStorage.setItem('adminToken', token);
    }
  }

  /**
   * Update admin session
   */
  updateAdminSession() {
    const session = JSON.parse(localStorage.getItem('adminSession'));
    if (session) {
      session.admin = this.currentAdmin;
      localStorage.setItem('adminSession', JSON.stringify(session));
    }
  }

  /**
   * Check if session is valid
   */
  isSessionValid(session) {
    if (!session || !session.expiresAt) return false;
    return new Date(session.expiresAt) > new Date();
  }

  /**
   * Clear admin session
   */
  clearAdminSession() {
    localStorage.removeItem('adminSession');
    localStorage.removeItem('adminToken');
    this.currentAdmin = null;
  }

  /**
   * Hash password (simple implementation - in production use bcrypt)
   */
  hashPassword(password) {
    // Simple hash for demo - in production use proper hashing
    return btoa(password + '_salt_241runners');
  }

  /**
   * Verify password
   */
  verifyPassword(password, hash) {
    return this.hashPassword(password) === hash;
  }

  /**
   * Validate password strength
   */
  validatePassword(password) {
    const minLength = 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasSpecialChar = /[@$!%*?&]/.test(password);

    return password.length >= minLength && 
           hasUpperCase && 
           hasLowerCase && 
           hasNumbers && 
           hasSpecialChar;
  }

  /**
   * Get current admin
   */
  getCurrentAdmin() {
    return this.currentAdmin;
  }

  /**
   * Check if user is admin
   */
  isAdmin() {
    return this.currentAdmin !== null;
  }

  /**
   * Create new admin user
   */
  async createAdmin(adminData) {
    try {
      const response = await fetch(`${API_BASE_URL}/admin/create-admin`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        },
        body: JSON.stringify(adminData)
      });

      if (response.ok) {
        const result = await response.json();
        // Refresh admin users list
        await this.loadAdminUsers();
        return { success: true, message: result.message, userId: result.userId };
      } else {
        const errorData = await response.json();
        return { success: false, message: errorData.message || 'Failed to create admin user' };
      }
    } catch (error) {
      console.error('Error creating admin user:', error);
      return { success: false, message: 'Error creating admin user' };
    }
  }

  /**
   * Get all admin users from backend
   */
  async getAllAdminUsers() {
    try {
      const response = await fetch(`${API_BASE_URL}/admin/admins`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        }
      });

      if (response.ok) {
        const adminUsers = await response.json();
        return { success: true, users: adminUsers };
      } else {
        return { success: false, message: 'Failed to fetch admin users' };
      }
    } catch (error) {
      console.error('Error fetching admin users:', error);
      return { success: false, message: 'Error fetching admin users' };
    }
  }

  /**
   * Update admin user
   */
  async updateAdmin(userId, adminData) {
    try {
      const response = await fetch(`${API_BASE_URL}/admin/update-admin/${userId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        },
        body: JSON.stringify(adminData)
      });

      if (response.ok) {
        const result = await response.json();
        await this.loadAdminUsers(); // Refresh admin users list
        return { success: true, message: result.message };
      } else {
        const errorData = await response.json();
        return { success: false, message: errorData.message || 'Failed to update admin user' };
      }
    } catch (error) {
      console.error('Error updating admin user:', error);
      return { success: false, message: 'Error updating admin user' };
    }
  }

  /**
   * Delete admin user
   */
  async deleteAdmin(userId) {
    try {
      const response = await fetch(`${API_BASE_URL}/admin/delete-admin/${userId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        }
      });

      if (response.ok) {
        const result = await response.json();
        await this.loadAdminUsers(); // Refresh admin users list
        return { success: true, message: result.message };
      } else {
        const errorData = await response.json();
        return { success: false, message: errorData.message || 'Failed to delete admin user' };
      }
    } catch (error) {
      console.error('Error deleting admin user:', error);
      return { success: false, message: 'Error deleting admin user' };
    }
  }

  /**
   * Reset admin password (by another admin)
   */
  async resetAdminPassword(userId, newPassword) {
    try {
      const response = await fetch(`${API_BASE_URL}/admin/reset-admin-password`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        },
        body: JSON.stringify({
          userId: userId,
          newPassword: newPassword
        })
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message };
      } else {
        const errorData = await response.json();
        return { success: false, message: errorData.message || 'Failed to reset password' };
      }
    } catch (error) {
      console.error('Error resetting admin password:', error);
      return { success: false, message: 'Error resetting password' };
    }
  }

  /**
   * Get admin user by ID
   */
  async getAdminById(userId) {
    try {
      const response = await fetch(`${API_BASE_URL}/admin/admins/${userId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        }
      });

      if (response.ok) {
        const adminUser = await response.json();
        return { success: true, user: adminUser };
      } else {
        return { success: false, message: 'Failed to fetch admin user' };
      }
    } catch (error) {
      console.error('Error fetching admin user:', error);
      return { success: false, message: 'Error fetching admin user' };
    }
  }

  /**
   * Logout admin
   */
  logout() {
    this.clearAdminSession();
    window.location.href = 'admin-login.html';
  }
}

// Global admin user manager instance
const adminUserManager = new AdminUserManager();

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { AdminUserManager, adminUserManager };
}
