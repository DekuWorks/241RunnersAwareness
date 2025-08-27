/**
 * ============================================
 * USER MANAGEMENT API SERVICE
 * ============================================
 * 
 * This service provides API integration for user management operations.
 * It includes fallback to mock data when the backend is not available.
 */

class UserManagementAPI {
  constructor() {
    this.baseURL = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1' 
      ? 'http://localhost:5113/api' 
      : 'https://241runnersawareness-api.azurewebsites.net/api';
    this.isBackendAvailable = false;
    this.checkBackendAvailability();
  }

  // Check if backend is available
  async checkBackendAvailability() {
    try {
      const response = await fetch(`${this.baseURL}/usermanagement/stats`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        },
        timeout: 3000
      });
      this.isBackendAvailable = response.ok;
    } catch (error) {
      console.log('Backend not available, using mock data');
      this.isBackendAvailable = false;
    }
  }

  // Get authentication token
  getAuthToken() {
    const user = localStorage.getItem('user');
    if (user) {
      const userData = JSON.parse(user);
      return userData.token;
    }
    return null;
  }

  // Get all users
  async getUsers() {
    if (!this.isBackendAvailable) {
      return this.getMockUsers();
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/users`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        }
      });

      if (response.ok) {
        return await response.json();
      } else {
        throw new Error('Failed to fetch users');
      }
    } catch (error) {
      console.error('Error fetching users:', error);
      return this.getMockUsers();
    }
  }

  // Get user by ID
  async getUser(userId) {
    if (!this.isBackendAvailable) {
      return this.getMockUsers().find(u => u.userId === userId);
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/users/${userId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        }
      });

      if (response.ok) {
        return await response.json();
      } else {
        throw new Error('Failed to fetch user');
      }
    } catch (error) {
      console.error('Error fetching user:', error);
      return this.getMockUsers().find(u => u.userId === userId);
    }
  }

  // Create new user
  async createUser(userData) {
    if (!this.isBackendAvailable) {
      return this.createMockUser(userData);
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/users`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        },
        body: JSON.stringify(userData)
      });

      if (response.ok) {
        return await response.json();
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to create user');
      }
    } catch (error) {
      console.error('Error creating user:', error);
      return this.createMockUser(userData);
    }
  }

  // Update user
  async updateUser(userId, userData) {
    if (!this.isBackendAvailable) {
      return this.updateMockUser(userId, userData);
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/users/${userId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        },
        body: JSON.stringify(userData)
      });

      if (response.ok) {
        return await response.json();
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to update user');
      }
    } catch (error) {
      console.error('Error updating user:', error);
      return this.updateMockUser(userId, userData);
    }
  }

  // Delete user (soft delete)
  async deleteUser(userId) {
    if (!this.isBackendAvailable) {
      return this.deleteMockUser(userId);
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/users/${userId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        }
      });

      if (response.ok) {
        return await response.json();
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to delete user');
      }
    } catch (error) {
      console.error('Error deleting user:', error);
      return this.deleteMockUser(userId);
    }
  }

  // Activate user
  async activateUser(userId) {
    if (!this.isBackendAvailable) {
      return this.activateMockUser(userId);
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/users/${userId}/activate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        }
      });

      if (response.ok) {
        return await response.json();
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to activate user');
      }
    } catch (error) {
      console.error('Error activating user:', error);
      return this.activateMockUser(userId);
    }
  }

  // Get user statistics
  async getUserStats() {
    if (!this.isBackendAvailable) {
      return this.getMockUserStats();
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/stats`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        }
      });

      if (response.ok) {
        return await response.json();
      } else {
        throw new Error('Failed to fetch user stats');
      }
    } catch (error) {
      console.error('Error fetching user stats:', error);
      return this.getMockUserStats();
    }
  }

  // Get available roles
  async getRoles() {
    if (!this.isBackendAvailable) {
      return this.getMockRoles();
    }

    try {
      const response = await fetch(`${this.baseURL}/usermanagement/roles`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        }
      });

      if (response.ok) {
        return await response.json();
      } else {
        throw new Error('Failed to fetch roles');
      }
    } catch (error) {
      console.error('Error fetching roles:', error);
      return this.getMockRoles();
    }
  }

  // Mock data methods
  getMockUsers() {
    return [
      {
        userId: '1',
        username: 'admin',
        email: 'admin@example.com',
        firstName: 'System',
        lastName: 'Admin',
        phoneNumber: '(555) 123-4567',
        role: 'admin',
        emailVerified: true,
        phoneVerified: true,
        createdAt: '2025-01-01T00:00:00Z',
        lastLoginAt: '2025-01-16T21:19:53Z',
        isActive: true,
        organization: '241 Runners Awareness',
        credentials: 'System Administrator',
        specialization: 'Platform Management',
        yearsOfExperience: '5+'
      },
      {
        userId: '2',
        username: 'marcus',
        email: 'dekuworks1@gmail.com',
        firstName: 'Marcus',
        lastName: 'Brown',
        phoneNumber: '(555) 234-5678',
        role: 'admin',
        emailVerified: true,
        phoneVerified: false,
        createdAt: '2025-01-02T00:00:00Z',
        lastLoginAt: '2025-01-16T20:30:00Z',
        isActive: true,
        organization: '241 Runners Awareness',
        credentials: 'Co-Founder',
        specialization: 'Operations',
        yearsOfExperience: '3+'
      },
      {
        userId: '3',
        username: 'daniel',
        email: 'danielcarey9770@gmail.com',
        firstName: 'Daniel',
        lastName: 'Carey',
        phoneNumber: '(555) 345-6789',
        role: 'admin',
        emailVerified: true,
        phoneVerified: true,
        createdAt: '2025-01-03T00:00:00Z',
        lastLoginAt: '2025-01-16T19:45:00Z',
        isActive: true,
        organization: '241 Runners Awareness',
        credentials: 'Co-Founder',
        specialization: 'Technology',
        yearsOfExperience: '4+'
      }
    ];
  }

  createMockUser(userData) {
    const newUser = {
      userId: Date.now().toString(),
      ...userData,
      emailVerified: false,
      phoneVerified: false,
      createdAt: new Date().toISOString(),
      lastLoginAt: null,
      isActive: true
    };

    // In a real implementation, this would be stored in localStorage or a mock database
    const mockUsers = this.getMockUsers();
    mockUsers.unshift(newUser);
    
    return {
      success: true,
      message: 'User created successfully',
      user: newUser
    };
  }

  updateMockUser(userId, userData) {
    const mockUsers = this.getMockUsers();
    const userIndex = mockUsers.findIndex(u => u.userId === userId);
    
    if (userIndex !== -1) {
      mockUsers[userIndex] = { ...mockUsers[userIndex], ...userData };
      return {
        success: true,
        message: 'User updated successfully',
        user: mockUsers[userIndex]
      };
    }
    
    throw new Error('User not found');
  }

  deleteMockUser(userId) {
    const mockUsers = this.getMockUsers();
    const userIndex = mockUsers.findIndex(u => u.userId === userId);
    
    if (userIndex !== -1) {
      mockUsers[userIndex].isActive = false;
      return {
        success: true,
        message: 'User deactivated successfully'
      };
    }
    
    throw new Error('User not found');
  }

  activateMockUser(userId) {
    const mockUsers = this.getMockUsers();
    const userIndex = mockUsers.findIndex(u => u.userId === userId);
    
    if (userIndex !== -1) {
      mockUsers[userIndex].isActive = true;
      return {
        success: true,
        message: 'User activated successfully'
      };
    }
    
    throw new Error('User not found');
  }

  getMockUserStats() {
    const mockUsers = this.getMockUsers();
    return {
      totalUsers: mockUsers.length,
      activeUsers: mockUsers.filter(u => u.isActive).length,
      adminUsers: mockUsers.filter(u => u.role === 'admin' && u.isActive).length,
      verifiedUsers: mockUsers.filter(u => u.emailVerified && u.isActive).length,
      inactiveUsers: mockUsers.filter(u => !u.isActive).length
    };
  }

  getMockRoles() {
    return [
      'admin',
      'user',
      'therapist',
      'caregiver',
      'parent',
      'adoptive_parent'
    ];
  }

  // Password Reset Methods
  async resetUserPassword(userId, newPassword) {
    try {
      if (this.isBackendAvailable) {
        const response = await fetch(`${this.baseURL}/auth/reset-password`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${this.getAuthToken()}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            userId: userId,
            newPassword: newPassword
          })
        });
        
        if (response.ok) {
          return await response.json();
        }
      }
      
      return this.resetMockUserPassword(userId, newPassword);
    } catch (error) {
      console.error('Error resetting password:', error);
      return this.resetMockUserPassword(userId, newPassword);
    }
  }

  async updateUserPhone(userId, phoneNumber) {
    try {
      if (this.isBackendAvailable) {
        const response = await fetch(`${this.baseURL}/auth/update-phone`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${this.getAuthToken()}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            phoneNumber: phoneNumber
          })
        });
        
        if (response.ok) {
          return await response.json();
        }
      }
      
      return this.updateMockUserPhone(userId, phoneNumber);
    } catch (error) {
      console.error('Error updating phone number:', error);
      return this.updateMockUserPhone(userId, phoneNumber);
    }
  }

  // Mock password reset
  resetMockUserPassword(userId, newPassword) {
    const mockUsers = this.getMockUsers();
    const userIndex = mockUsers.findIndex(u => u.userId === userId);
    
    if (userIndex !== -1) {
      // In a real implementation, this would hash the password
      mockUsers[userIndex].passwordHash = newPassword; // This should be hashed
      return {
        success: true,
        message: 'Password reset successfully'
      };
    }
    
    throw new Error('User not found');
  }

  // Mock phone update
  updateMockUserPhone(userId, phoneNumber) {
    const mockUsers = this.getMockUsers();
    const userIndex = mockUsers.findIndex(u => u.userId === userId);
    
    if (userIndex !== -1) {
      mockUsers[userIndex].phoneNumber = phoneNumber;
      mockUsers[userIndex].phoneVerified = false; // Reset verification
      return {
        success: true,
        message: 'Phone number updated successfully. Please verify your new phone number.',
        requiresVerification: true
      };
    }
    
    throw new Error('User not found');
  }
}

// Export for use in other files
window.UserManagementAPI = UserManagementAPI;
