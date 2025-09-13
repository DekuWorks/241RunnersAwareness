/**
 * Admin Profile Management System
 * Handles individual admin profiles with real-time updates
 */

class AdminProfileManager {
    constructor() {
        this.currentAdmin = null;
        this.profileUpdateCallbacks = [];
        this.activityLog = [];
        this.maxActivityLogSize = 100;
        
        console.log('👤 AdminProfileManager initialized');
    }

    /**
     * Initialize admin profile
     */
    async initialize() {
        try {
            console.log('🚀 Initializing admin profile...');
            
            // Load current admin data
            await this.loadCurrentAdmin();
            
            // Set up real-time profile updates
            this.setupRealTimeUpdates();
            
            // Start activity tracking
            this.startActivityTracking();
            
            console.log('✅ Admin profile initialized');
        } catch (error) {
            console.error('❌ Failed to initialize admin profile:', error);
        }
    }

    /**
     * Load current admin data
     */
    async loadCurrentAdmin() {
        try {
            const token = this.getAuthToken();
            if (!token) {
                throw new Error('No authentication token found');
            }

            const response = await fetch(`${this.getApiBaseUrl()}/Auth/me`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const adminData = await response.json();
            this.currentAdmin = adminData;
            
            // Store in localStorage for quick access
            localStorage.setItem('ra_admin_user', JSON.stringify(adminData));
            
            console.log('✅ Current admin loaded:', adminData);
            this.notifyProfileUpdateCallbacks(adminData);
            
            return adminData;
        } catch (error) {
            console.error('❌ Failed to load current admin:', error);
            throw error;
        }
    }

    /**
     * Update admin profile
     */
    async updateProfile(profileData) {
        try {
            console.log('📝 Updating admin profile...');
            
            const token = this.getAuthToken();
            if (!token) {
                throw new Error('No authentication token found');
            }

            const response = await fetch(`${this.getApiBaseUrl()}/Auth/profile`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(profileData)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            const updatedData = await response.json();
            
            // Update local data
            this.currentAdmin = { ...this.currentAdmin, ...updatedData.user };
            localStorage.setItem('ra_admin_user', JSON.stringify(this.currentAdmin));
            
            // Log activity
            this.logActivity('profile_updated', {
                changes: Object.keys(profileData),
                timestamp: new Date().toISOString()
            });
            
            // Broadcast profile change
            if (window.AdminRealtime) {
                await window.AdminRealtime.broadcastAdminProfileChange('profile_updated', this.currentAdmin);
            }
            
            console.log('✅ Admin profile updated successfully');
            this.notifyProfileUpdateCallbacks(this.currentAdmin);
            
            return updatedData;
        } catch (error) {
            console.error('❌ Failed to update admin profile:', error);
            throw error;
        }
    }

    /**
     * Change admin password
     */
    async changePassword(currentPassword, newPassword) {
        try {
            console.log('🔐 Changing admin password...');
            
            const token = this.getAuthToken();
            if (!token) {
                throw new Error('No authentication token found');
            }

            const response = await fetch(`${this.getApiBaseUrl()}/Auth/change-password`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    currentPassword,
                    newPassword,
                    confirmPassword: newPassword
                })
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            const result = await response.json();
            
            // Log activity
            this.logActivity('password_changed', {
                timestamp: new Date().toISOString()
            });
            
            // Broadcast profile change
            if (window.AdminRealtime) {
                await window.AdminRealtime.broadcastAdminProfileChange('password_changed', {
                    adminId: this.currentAdmin.id,
                    timestamp: new Date().toISOString()
                });
            }
            
            console.log('✅ Admin password changed successfully');
            
            return result;
        } catch (error) {
            console.error('❌ Failed to change admin password:', error);
            throw error;
        }
    }

    /**
     * Get admin profile data
     */
    getProfile() {
        return this.currentAdmin;
    }

    /**
     * Get admin display name
     */
    getDisplayName() {
        if (!this.currentAdmin) return 'Unknown Admin';
        return `${this.currentAdmin.firstName} ${this.currentAdmin.lastName}`;
    }

    /**
     * Get admin initials
     */
    getInitials() {
        if (!this.currentAdmin) return 'UA';
        return `${this.currentAdmin.firstName.charAt(0)}${this.currentAdmin.lastName.charAt(0)}`.toUpperCase();
    }

    /**
     * Get admin role
     */
    getRole() {
        return this.currentAdmin?.role || 'unknown';
    }

    /**
     * Check if admin has specific permission
     */
    hasPermission(permission) {
        if (!this.currentAdmin) return false;
        
        // For now, all admins have all permissions
        // This can be extended with role-based permissions
        return this.currentAdmin.role === 'admin';
    }

    /**
     * Set up real-time profile updates
     */
    setupRealTimeUpdates() {
        if (window.AdminRealtime) {
            // Listen for profile changes from other admins
            window.AdminRealtime.on('adminProfileChanged', (changeData) => {
                console.log('📱 Received admin profile change:', changeData);
                
                // If it's our own profile, update local data
                if (changeData.adminData.adminId === this.currentAdmin?.id) {
                    this.currentAdmin = { ...this.currentAdmin, ...changeData.adminData };
                    localStorage.setItem('ra_admin_user', JSON.stringify(this.currentAdmin));
                    this.notifyProfileUpdateCallbacks(this.currentAdmin);
                }
                
                // Show notification
                this.showNotification(`Admin profile ${changeData.operation} by ${changeData.changedBy}`, 'info');
            });
        }
    }

    /**
     * Start activity tracking
     */
    startActivityTracking() {
        // Track various admin activities
        this.trackActivity('dashboard_loaded');
        
        // Set up periodic activity updates
        setInterval(() => {
            this.trackActivity('dashboard_active');
        }, 300000); // Every 5 minutes
    }

    /**
     * Track admin activity
     */
    trackActivity(activity, data = null) {
        const activityEntry = {
            activity,
            data,
            timestamp: new Date().toISOString(),
            adminId: this.currentAdmin?.id,
            adminName: this.getDisplayName()
        };
        
        this.activityLog.unshift(activityEntry);
        
        // Keep only recent activities
        if (this.activityLog.length > this.maxActivityLogSize) {
            this.activityLog = this.activityLog.slice(0, this.maxActivityLogSize);
        }
        
        // Notify real-time system
        if (window.AdminRealtime) {
            window.AdminRealtime.notifyAdminActivity(activity, data);
        }
        
        console.log('📊 Activity tracked:', activityEntry);
    }

    /**
     * Log admin activity
     */
    logActivity(activity, data = null) {
        this.trackActivity(activity, data);
    }

    /**
     * Get activity log
     */
    getActivityLog(limit = 50) {
        return this.activityLog.slice(0, limit);
    }

    /**
     * Get recent activities
     */
    getRecentActivities(hours = 24) {
        const cutoffTime = new Date(Date.now() - (hours * 60 * 60 * 1000));
        return this.activityLog.filter(activity => 
            new Date(activity.timestamp) > cutoffTime
        );
    }

    /**
     * Profile update callbacks
     */
    onProfileUpdate(callback) {
        this.profileUpdateCallbacks.push(callback);
    }

    notifyProfileUpdateCallbacks(profileData) {
        this.profileUpdateCallbacks.forEach(callback => {
            try {
                callback(profileData);
            } catch (error) {
                console.error('❌ Error in profile update callback:', error);
            }
        });
    }

    /**
     * Utility methods
     */
    getAuthToken() {
        return localStorage.getItem('ra_admin_token');
    }

    getApiBaseUrl() {
        return window.APP_CONFIG?.API_BASE_URL || 'https://241runners-api-v2.azurewebsites.net/api';
    }

    showNotification(message, type = 'info') {
        if (typeof showToast === 'function') {
            showToast(message, type, 5000);
        } else {
            console.log(`📢 ${type.toUpperCase()}: ${message}`);
        }
    }

    /**
     * Export profile data
     */
    exportProfile() {
        return {
            profile: this.currentAdmin,
            activityLog: this.activityLog,
            exportDate: new Date().toISOString()
        };
    }

    /**
     * Clear profile data
     */
    clearProfile() {
        this.currentAdmin = null;
        this.activityLog = [];
        localStorage.removeItem('ra_admin_user');
        console.log('🧹 Admin profile data cleared');
    }
}

// Create global instance
window.AdminProfileManager = new AdminProfileManager();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminProfileManager;
}
