/**
 * Admin Monitoring System
 * Real-time monitoring of users and runners across all admin dashboards
 */

class AdminMonitoringSystem {
    constructor() {
        this.users = new Map();
        this.runners = new Map();
        this.admins = new Map();
        this.publicCases = new Map();
        this.monitoringCallbacks = new Map();
        this.lastUpdateTimes = new Map();
        this.updateInterval = null;
        this.isMonitoring = false;
        
        console.log('📊 AdminMonitoringSystem initialized');
    }

    /**
     * Initialize monitoring system
     */
    async initialize() {
        try {
            console.log('🚀 Initializing admin monitoring system...');
            
            // Set up real-time event handlers
            this.setupRealTimeHandlers();
            
            // Start monitoring
            this.startMonitoring();
            
            console.log('✅ Admin monitoring system initialized');
        } catch (error) {
            console.error('❌ Failed to initialize monitoring system:', error);
        }
    }

    /**
     * Start monitoring
     */
    startMonitoring() {
        if (this.isMonitoring) return;
        
        this.isMonitoring = true;
        console.log('📊 Starting admin monitoring...');
        
        // Initial data load
        this.loadAllData();
        
        // Set up periodic updates (fallback for when real-time is not available)
        this.updateInterval = setInterval(() => {
            if (!window.AdminRealtime?.isConnected) {
                this.loadAllData();
            }
        }, 60000); // Update every minute if real-time is not available
    }

    /**
     * Stop monitoring
     */
    stopMonitoring() {
        this.isMonitoring = false;
        
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
            this.updateInterval = null;
        }
        
        console.log('⏹️ Admin monitoring stopped');
    }

    /**
     * Set up real-time event handlers
     */
    setupRealTimeHandlers() {
        if (!window.AdminRealtime) return;

        // User changes
        window.AdminRealtime.on('userChanged', (changeData) => {
            this.handleUserChange(changeData);
        });

        // Runner changes
        window.AdminRealtime.on('runnerChanged', (changeData) => {
            this.handleRunnerChange(changeData);
        });

        // Admin changes
        window.AdminRealtime.on('adminProfileChanged', (changeData) => {
            this.handleAdminChange(changeData);
        });

        // Data version changes
        window.AdminRealtime.on('dataVersionChanged', (changeData) => {
            this.handleDataVersionChange(changeData);
        });

        // Admin activity
        window.AdminRealtime.on('adminActivity', (activityData) => {
            this.handleAdminActivity(activityData);
        });
    }

    /**
     * Load all data
     */
    async loadAllData() {
        try {
            console.log('📥 Loading all monitoring data...');
            
            const [usersData, adminsData, publicCasesData] = await Promise.allSettled([
                this.loadUsers(),
                this.loadAdmins(),
                this.loadPublicCases()
            ]);

            // Handle results
            if (usersData.status === 'fulfilled') {
                console.log('✅ Users loaded:', usersData.value.length);
            } else {
                console.error('❌ Failed to load users:', usersData.reason);
            }

            if (adminsData.status === 'fulfilled') {
                console.log('✅ Admins loaded:', adminsData.value.length);
            } else {
                console.error('❌ Failed to load admins:', adminsData.reason);
            }

            if (publicCasesData.status === 'fulfilled') {
                console.log('✅ Public cases loaded:', publicCasesData.value.length);
            } else {
                console.error('❌ Failed to load public cases:', publicCasesData.reason);
            }

            // Notify callbacks
            this.notifyDataUpdate('all', {
                users: this.users.size,
                admins: this.admins.size,
                publicCases: this.publicCases.size,
                timestamp: new Date().toISOString()
            });

        } catch (error) {
            console.error('❌ Error loading monitoring data:', error);
        }
    }

    /**
     * Load users
     */
    async loadUsers() {
        try {
            // For now, we'll use mock data since the admin endpoint isn't deployed yet
            // TODO: Update to use real API endpoint when available
            const mockUsers = [
                { id: 1, email: "test@example.com", firstName: "Test", lastName: "User", role: "user", isActive: true, lastLoginAt: new Date().toISOString() },
                { id: 2, email: "dekuworks1@gmail.com", firstName: "Marcus", lastName: "Brown", role: "admin", isActive: true, lastLoginAt: new Date().toISOString() },
                { id: 3, email: "danielcarey9770@yahoo.com", firstName: "Daniel", lastName: "Carey", role: "admin", isActive: true, lastLoginAt: new Date().toISOString() },
                { id: 4, email: "lthomas3350@gmail.com", firstName: "Lisa", lastName: "Thomas", role: "admin", isActive: true, lastLoginAt: new Date().toISOString() },
                { id: 5, email: "tinaleggins@yahoo.com", firstName: "Tina", lastName: "Matthews", role: "admin", isActive: true, lastLoginAt: new Date().toISOString() },
                { id: 6, email: "mmelasky@iplawconsulting.com", firstName: "Mark", lastName: "Melasky", role: "admin", isActive: true, lastLoginAt: new Date().toISOString() },
                { id: 7, email: "ralphfrank900@gmail.com", firstName: "Ralph", lastName: "Frank", role: "admin", isActive: true, lastLoginAt: new Date().toISOString() }
            ];

            // Update users map
            this.users.clear();
            mockUsers.forEach(user => {
                this.users.set(user.id, user);
            });

            this.lastUpdateTimes.set('users', new Date().toISOString());
            return mockUsers;
        } catch (error) {
            console.error('❌ Error loading users:', error);
            return [];
        }
    }

    /**
     * Load admins
     */
    async loadAdmins() {
        try {
            // Filter admin users from the loaded users
            const adminUsers = Array.from(this.users.values()).filter(user => user.role === 'admin');
            
            // Update admins map
            this.admins.clear();
            adminUsers.forEach(admin => {
                this.admins.set(admin.id, admin);
            });

            this.lastUpdateTimes.set('admins', new Date().toISOString());
            return adminUsers;
        } catch (error) {
            console.error('❌ Error loading admins:', error);
            return [];
        }
    }

    /**
     * Load public cases
     */
    async loadPublicCases() {
        try {
            // Mock data for public cases
            const mockCases = [
                { id: 1, title: "Case 1", status: "open", createdAt: new Date().toISOString() },
                { id: 2, title: "Case 2", status: "closed", createdAt: new Date().toISOString() },
                { id: 3, title: "Case 3", status: "pending", createdAt: new Date().toISOString() }
            ];

            // Update public cases map
            this.publicCases.clear();
            mockCases.forEach(case_ => {
                this.publicCases.set(case_.id, case_);
            });

            this.lastUpdateTimes.set('publicCases', new Date().toISOString());
            return mockCases;
        } catch (error) {
            console.error('❌ Error loading public cases:', error);
            return [];
        }
    }

    /**
     * Handle user changes
     */
    handleUserChange(changeData) {
        console.log('👤 Handling user change:', changeData);
        
        const { operation, userData, changedBy, timestamp } = changeData;
        
        switch (operation) {
            case 'created':
                this.users.set(userData.id, userData);
                this.showNotification(`New user created by ${changedBy}`, 'success');
                break;
            case 'updated':
                this.users.set(userData.id, userData);
                this.showNotification(`User updated by ${changedBy}`, 'info');
                break;
            case 'deleted':
                this.users.delete(userData.id);
                this.showNotification(`User deleted by ${changedBy}`, 'warning');
                break;
            case 'activated':
                if (this.users.has(userData.id)) {
                    this.users.get(userData.id).isActive = true;
                }
                this.showNotification(`User activated by ${changedBy}`, 'success');
                break;
            case 'deactivated':
                if (this.users.has(userData.id)) {
                    this.users.get(userData.id).isActive = false;
                }
                this.showNotification(`User deactivated by ${changedBy}`, 'warning');
                break;
        }
        
        this.lastUpdateTimes.set('users', timestamp);
        this.notifyDataUpdate('users', { operation, userData, changedBy, timestamp });
    }

    /**
     * Handle runner changes
     */
    handleRunnerChange(changeData) {
        console.log('🏃 Handling runner change:', changeData);
        
        const { operation, runnerData, changedBy, timestamp } = changeData;
        
        switch (operation) {
            case 'created':
                this.runners.set(runnerData.id, runnerData);
                this.showNotification(`New runner created by ${changedBy}`, 'success');
                break;
            case 'updated':
                this.runners.set(runnerData.id, runnerData);
                this.showNotification(`Runner updated by ${changedBy}`, 'info');
                break;
            case 'deleted':
                this.runners.delete(runnerData.id);
                this.showNotification(`Runner deleted by ${changedBy}`, 'warning');
                break;
        }
        
        this.lastUpdateTimes.set('runners', timestamp);
        this.notifyDataUpdate('runners', { operation, runnerData, changedBy, timestamp });
    }

    /**
     * Handle admin changes
     */
    handleAdminChange(changeData) {
        console.log('👤 Handling admin change:', changeData);
        
        const { operation, adminData, changedBy, timestamp } = changeData;
        
        switch (operation) {
            case 'profile_updated':
                if (this.admins.has(adminData.id)) {
                    this.admins.set(adminData.id, { ...this.admins.get(adminData.id), ...adminData });
                }
                this.showNotification(`Admin profile updated by ${changedBy}`, 'info');
                break;
            case 'password_changed':
                this.showNotification(`Admin password changed by ${changedBy}`, 'info');
                break;
            case 'role_changed':
                if (this.admins.has(adminData.id)) {
                    this.admins.get(adminData.id).role = adminData.role;
                }
                this.showNotification(`Admin role changed by ${changedBy}`, 'warning');
                break;
        }
        
        this.lastUpdateTimes.set('admins', timestamp);
        this.notifyDataUpdate('admins', { operation, adminData, changedBy, timestamp });
    }

    /**
     * Handle data version changes
     */
    handleDataVersionChange(changeData) {
        console.log('📊 Handling data version change:', changeData);
        
        const { dataType, version, changedBy, timestamp } = changeData;
        
        // Reload the specific data type
        switch (dataType) {
            case 'users':
                this.loadUsers();
                break;
            case 'runners':
                this.loadRunners();
                break;
            case 'admins':
                this.loadAdmins();
                break;
            case 'public_cases':
                this.loadPublicCases();
                break;
        }
        
        this.showNotification(`Data updated: ${dataType} v${version} by ${changedBy}`, 'info');
        this.notifyDataUpdate('version', { dataType, version, changedBy, timestamp });
    }

    /**
     * Handle admin activity
     */
    handleAdminActivity(activityData) {
        console.log('📱 Handling admin activity:', activityData);
        
        const { activity, adminName, adminEmail, timestamp } = activityData;
        
        // Log the activity
        this.logActivity(activity, {
            adminName,
            adminEmail,
            timestamp
        });
        
        // Notify callbacks
        this.notifyDataUpdate('activity', activityData);
    }

    /**
     * Get monitoring statistics
     */
    getStatistics() {
        return {
            users: {
                total: this.users.size,
                active: Array.from(this.users.values()).filter(u => u.isActive).length,
                inactive: Array.from(this.users.values()).filter(u => !u.isActive).length,
                lastUpdate: this.lastUpdateTimes.get('users')
            },
            admins: {
                total: this.admins.size,
                active: Array.from(this.admins.values()).filter(a => a.isActive).length,
                lastUpdate: this.lastUpdateTimes.get('admins')
            },
            runners: {
                total: this.runners.size,
                lastUpdate: this.lastUpdateTimes.get('runners')
            },
            publicCases: {
                total: this.publicCases.size,
                open: Array.from(this.publicCases.values()).filter(c => c.status === 'open').length,
                closed: Array.from(this.publicCases.values()).filter(c => c.status === 'closed').length,
                pending: Array.from(this.publicCases.values()).filter(c => c.status === 'pending').length,
                lastUpdate: this.lastUpdateTimes.get('publicCases')
            },
            monitoring: {
                isActive: this.isMonitoring,
                realTimeConnected: window.AdminRealtime?.isConnected || false,
                lastDataLoad: new Date().toISOString()
            }
        };
    }

    /**
     * Get users data
     */
    getUsers() {
        return Array.from(this.users.values());
    }

    /**
     * Get admins data
     */
    getAdmins() {
        return Array.from(this.admins.values());
    }

    /**
     * Get runners data
     */
    getRunners() {
        return Array.from(this.runners.values());
    }

    /**
     * Get public cases data
     */
    getPublicCases() {
        return Array.from(this.publicCases.values());
    }

    /**
     * Get specific user
     */
    getUser(userId) {
        return this.users.get(userId);
    }

    /**
     * Get specific admin
     */
    getAdmin(adminId) {
        return this.admins.get(adminId);
    }

    /**
     * Event handling system
     */
    on(eventName, callback) {
        if (!this.monitoringCallbacks.has(eventName)) {
            this.monitoringCallbacks.set(eventName, []);
        }
        this.monitoringCallbacks.get(eventName).push(callback);
    }

    off(eventName, callback) {
        if (this.monitoringCallbacks.has(eventName)) {
            const handlers = this.monitoringCallbacks.get(eventName);
            const index = handlers.indexOf(callback);
            if (index > -1) {
                handlers.splice(index, 1);
            }
        }
    }

    notifyDataUpdate(dataType, data) {
        if (this.monitoringCallbacks.has(dataType)) {
            this.monitoringCallbacks.get(dataType).forEach(callback => {
                try {
                    callback(data);
                } catch (error) {
                    console.error(`❌ Error in monitoring callback for ${dataType}:`, error);
                }
            });
        }
    }

    /**
     * Activity logging
     */
    logActivity(activity, data) {
        const activityEntry = {
            activity,
            data,
            timestamp: new Date().toISOString()
        };
        
        console.log('📊 Activity logged:', activityEntry);
    }

    /**
     * Utility methods
     */
    showNotification(message, type = 'info') {
        if (typeof showToast === 'function') {
            showToast(message, type, 5000);
        } else {
            console.log(`📢 ${type.toUpperCase()}: ${message}`);
        }
    }

    /**
     * Export monitoring data
     */
    exportData() {
        return {
            users: this.getUsers(),
            admins: this.getAdmins(),
            runners: this.getRunners(),
            publicCases: this.getPublicCases(),
            statistics: this.getStatistics(),
            exportDate: new Date().toISOString()
        };
    }

    /**
     * Clear monitoring data
     */
    clearData() {
        this.users.clear();
        this.runners.clear();
        this.admins.clear();
        this.publicCases.clear();
        this.lastUpdateTimes.clear();
        console.log('🧹 Monitoring data cleared');
    }
}

// Create global instance
window.AdminMonitoringSystem = new AdminMonitoringSystem();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminMonitoringSystem;
}
