/**
 * Real-time Admin Dashboard System
 * Handles SignalR connections and real-time updates across all admin dashboards
 */

class AdminRealtime {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.isConnecting = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 1000; // Start with 1 second
        this.maxReconnectDelay = 30000; // Max 30 seconds
        this.pingInterval = null;
        this.eventHandlers = new Map();
        this.connectionStatusCallbacks = [];
        this.adminActivityCallbacks = [];
        
        // API Configuration
        this.apiBaseUrl = window.APP_CONFIG?.API_BASE_URL || 'https://241runners-api.azurewebsites.net/api';
        this.signalRUrl = this.apiBaseUrl.replace('/api', '/hubs/admin');
        
        console.log('🔌 AdminRealtime initialized with SignalR URL:', this.signalRUrl);
    }

    /**
     * Initialize the real-time connection
     */
    async initialize() {
        try {
            console.log('🚀 Initializing real-time admin connection...');
            
            if (this.isConnecting || this.isConnected) {
                console.log('⚠️ Connection already in progress or established');
                return;
            }

            this.isConnecting = true;
            this.updateConnectionStatus(false, false);

            // Get authentication token
            const token = this.getAuthToken();
            if (!token) {
                console.error('❌ No authentication token found');
                this.isConnecting = false;
                this.updateConnectionStatus(false, false);
                return;
            }

            // Create SignalR connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(this.signalRUrl, {
                    accessTokenFactory: () => token,
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
                    skipNegotiation: false
                })
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: (retryContext) => {
                        if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
                            const delay = Math.min(this.reconnectDelay * Math.pow(2, retryContext.previousRetryCount), this.maxReconnectDelay);
                            console.log(`🔄 Reconnection attempt ${retryContext.previousRetryCount + 1} in ${delay}ms`);
                            return delay;
                        }
                        return null; // Stop reconnecting
                    }
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Set up event handlers
            this.setupEventHandlers();

            // Start connection
            await this.connection.start();
            
            this.isConnected = true;
            this.isConnecting = false;
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;

            console.log('✅ Real-time connection established');
            this.updateConnectionStatus(true, false);

            // Start ping to keep connection alive
            this.startPing();

            // Notify admin activity
            await this.notifyAdminActivity('dashboard_connected');

        } catch (error) {
            console.error('❌ Failed to initialize real-time connection:', error);
            this.isConnecting = false;
            this.updateConnectionStatus(false, false);
            
            // Fallback to polling mode
            this.startPollingMode();
        }
    }

    /**
     * Set up SignalR event handlers
     */
    setupEventHandlers() {
        if (!this.connection) return;

        // Connection events
        this.connection.onclose((error) => {
            console.log('🔌 Real-time connection closed:', error);
            this.isConnected = false;
            this.updateConnectionStatus(false, false);
            this.stopPing();
        });

        this.connection.onreconnecting((error) => {
            console.log('🔄 Real-time connection reconnecting:', error);
            this.isConnected = false;
            this.updateConnectionStatus(false, true);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('✅ Real-time connection reconnected:', connectionId);
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.updateConnectionStatus(true, false);
            this.startPing();
        });

        // Admin events
        this.connection.on('AdminConnected', (adminData) => {
            console.log('👤 Admin connected:', adminData);
            this.triggerEvent('adminConnected', adminData);
            this.showAdminNotification(`${adminData.adminName} joined the admin dashboard`, 'info');
        });

        this.connection.on('AdminDisconnected', (adminData) => {
            console.log('👋 Admin disconnected:', adminData);
            this.triggerEvent('adminDisconnected', adminData);
            this.showAdminNotification(`${adminData.adminName} left the admin dashboard`, 'info');
        });

        this.connection.on('CurrentAdmins', (admins) => {
            console.log('👥 Current online admins:', admins);
            this.triggerEvent('currentAdmins', admins);
        });

        this.connection.on('OnlineAdmins', (admins) => {
            console.log('👥 Online admins updated:', admins);
            this.triggerEvent('onlineAdmins', admins);
        });

        // Data change events
        this.connection.on('UserChanged', (changeData) => {
            console.log('👤 User changed:', changeData);
            this.triggerEvent('userChanged', changeData);
            this.showAdminNotification(`User ${changeData.operation} by ${changeData.changedBy}`, 'success');
        });

        this.connection.on('RunnerChanged', (changeData) => {
            console.log('🏃 Runner changed:', changeData);
            this.triggerEvent('runnerChanged', changeData);
            this.showAdminNotification(`Runner ${changeData.operation} by ${changeData.changedBy}`, 'success');
        });

        this.connection.on('AdminProfileChanged', (changeData) => {
            console.log('👤 Admin profile changed:', changeData);
            this.triggerEvent('adminProfileChanged', changeData);
            this.showAdminNotification(`Admin profile ${changeData.operation} by ${changeData.changedBy}`, 'info');
        });

        this.connection.on('DataVersionChanged', (changeData) => {
            console.log('📊 Data version changed:', changeData);
            this.triggerEvent('dataVersionChanged', changeData);
            this.showAdminNotification(`Data updated: ${changeData.dataType} by ${changeData.changedBy}`, 'info');
        });

        this.connection.on('AdminActivity', (activityData) => {
            console.log('📱 Admin activity:', activityData);
            this.triggerEvent('adminActivity', activityData);
            this.notifyAdminActivityCallbacks(activityData);
        });

        // New user registration notification
        this.connection.on('NewUserRegistration', (notificationData) => {
            console.log('🎉 New user registration:', notificationData);
            this.triggerEvent('newUserRegistration', notificationData);
            this.showNewUserRegistrationNotification(notificationData);
        });

        // Ping/Pong
        this.connection.on('Pong', (timestamp) => {
            console.log('🏓 Pong received:', new Date(timestamp));
        });
    }

    /**
     * Start ping to keep connection alive
     */
    startPing() {
        if (this.pingInterval) {
            clearInterval(this.pingInterval);
        }

        this.pingInterval = setInterval(async () => {
            if (this.isConnected && this.connection) {
                try {
                    await this.connection.invoke('Ping');
                } catch (error) {
                    console.error('❌ Ping failed:', error);
                }
            }
        }, 30000); // Ping every 30 seconds
    }

    /**
     * Stop ping
     */
    stopPing() {
        if (this.pingInterval) {
            clearInterval(this.pingInterval);
            this.pingInterval = null;
        }
    }

    /**
     * Start polling mode as fallback
     */
    startPollingMode() {
        console.log('🔄 Starting polling mode as fallback');
        this.updateConnectionStatus(false, true);
        
        // Poll for updates every 30 seconds
        setInterval(() => {
            if (!this.isConnected) {
                this.pollForUpdates();
            }
        }, 30000);
    }

    /**
     * Poll for updates when real-time connection is not available
     */
    async pollForUpdates() {
        try {
            // This would call API endpoints to check for updates
            // For now, we'll just log that polling is happening
            console.log('🔄 Polling for updates...');
        } catch (error) {
            console.error('❌ Polling failed:', error);
        }
    }

    /**
     * Notify admin activity
     */
    async notifyAdminActivity(activity, data = null) {
        if (this.isConnected && this.connection) {
            try {
                await this.connection.invoke('AdminActivity', activity, data);
            } catch (error) {
                console.error('❌ Failed to notify admin activity:', error);
            }
        }
    }

    /**
     * Broadcast user change
     */
    async broadcastUserChange(operation, userData) {
        if (this.isConnected && this.connection) {
            try {
                await this.connection.invoke('UserChanged', operation, userData, this.getCurrentAdminName());
                console.log(`📢 Broadcasted user ${operation}:`, userData);
            } catch (error) {
                console.error('❌ Failed to broadcast user change:', error);
            }
        }
    }

    /**
     * Broadcast runner change
     */
    async broadcastRunnerChange(operation, runnerData) {
        if (this.isConnected && this.connection) {
            try {
                await this.connection.invoke('RunnerChanged', operation, runnerData, this.getCurrentAdminName());
                console.log(`📢 Broadcasted runner ${operation}:`, runnerData);
            } catch (error) {
                console.error('❌ Failed to broadcast runner change:', error);
            }
        }
    }

    /**
     * Broadcast admin profile change
     */
    async broadcastAdminProfileChange(operation, adminData) {
        if (this.isConnected && this.connection) {
            try {
                await this.connection.invoke('AdminProfileChanged', operation, adminData, this.getCurrentAdminName());
                console.log(`📢 Broadcasted admin profile ${operation}:`, adminData);
            } catch (error) {
                console.error('❌ Failed to broadcast admin profile change:', error);
            }
        }
    }

    /**
     * Broadcast data version change
     */
    async broadcastDataVersionChange(dataType, version) {
        if (this.isConnected && this.connection) {
            try {
                await this.connection.invoke('DataVersionChanged', dataType, version, this.getCurrentAdminName());
                console.log(`📢 Broadcasted data version change: ${dataType} v${version}`);
            } catch (error) {
                console.error('❌ Failed to broadcast data version change:', error);
            }
        }
    }

    /**
     * Get online admins
     */
    async getOnlineAdmins() {
        if (this.isConnected && this.connection) {
            try {
                await this.connection.invoke('GetOnlineAdmins');
            } catch (error) {
                console.error('❌ Failed to get online admins:', error);
            }
        }
    }

    /**
     * Event handling system
     */
    on(eventName, callback) {
        if (!this.eventHandlers.has(eventName)) {
            this.eventHandlers.set(eventName, []);
        }
        this.eventHandlers.get(eventName).push(callback);
    }

    off(eventName, callback) {
        if (this.eventHandlers.has(eventName)) {
            const handlers = this.eventHandlers.get(eventName);
            const index = handlers.indexOf(callback);
            if (index > -1) {
                handlers.splice(index, 1);
            }
        }
    }

    triggerEvent(eventName, data) {
        if (this.eventHandlers.has(eventName)) {
            this.eventHandlers.get(eventName).forEach(callback => {
                try {
                    callback(data);
                } catch (error) {
                    console.error(`❌ Error in event handler for ${eventName}:`, error);
                }
            });
        }
    }

    /**
     * Connection status callbacks
     */
    onConnectionStatusChange(callback) {
        this.connectionStatusCallbacks.push(callback);
    }

    updateConnectionStatus(isConnected, isPolling) {
        this.connectionStatusCallbacks.forEach(callback => {
            try {
                callback(isConnected, isPolling);
            } catch (error) {
                console.error('❌ Error in connection status callback:', error);
            }
        });
    }

    /**
     * Admin activity callbacks
     */
    onAdminActivity(callback) {
        this.adminActivityCallbacks.push(callback);
    }

    notifyAdminActivityCallbacks(activityData) {
        this.adminActivityCallbacks.forEach(callback => {
            try {
                callback(activityData);
            } catch (error) {
                console.error('❌ Error in admin activity callback:', error);
            }
        });
    }

    /**
     * Utility methods
     */
    getAuthToken() {
        return localStorage.getItem('ra_admin_token');
    }

    getCurrentAdminName() {
        const adminData = localStorage.getItem('ra_admin_user');
        if (adminData) {
            try {
                const admin = JSON.parse(adminData);
                return `${admin.firstName} ${admin.lastName}`;
            } catch (error) {
                console.error('❌ Error parsing admin data:', error);
            }
        }
        return 'Unknown Admin';
    }

    showAdminNotification(message, type = 'info') {
        if (typeof showToast === 'function') {
            showToast(message, type, 5000);
        } else {
            console.log(`📢 ${type.toUpperCase()}: ${message}`);
        }
    }

    showNewUserRegistrationNotification(notificationData) {
        const userData = notificationData.userData;
        const message = `🎉 New user registered: ${userData.fullName} (${userData.role})`;
        
        // Show enhanced notification with user details
        if (typeof showToast === 'function') {
            showToast(message, 'success', 8000);
        } else {
            console.log(`🎉 NEW USER REGISTRATION: ${message}`);
        }

        // Trigger custom event for dashboard updates
        this.triggerEvent('newUserRegistration', notificationData);
        
        // Update user count if dashboard is visible
        if (typeof updateUserCount === 'function') {
            updateUserCount();
        }

        // Refresh user list if it's currently displayed
        if (typeof refreshUserList === 'function') {
            setTimeout(() => refreshUserList(), 1000);
        }
    }

    /**
     * Disconnect and cleanup
     */
    async disconnect() {
        try {
            console.log('🔌 Disconnecting real-time connection...');
            
            this.stopPing();
            
            if (this.connection) {
                await this.connection.stop();
                this.connection = null;
            }
            
            this.isConnected = false;
            this.isConnecting = false;
            this.updateConnectionStatus(false, false);
            
            console.log('✅ Real-time connection disconnected');
        } catch (error) {
            console.error('❌ Error disconnecting:', error);
        }
    }

    /**
     * Get connection status
     */
    getStatus() {
        return {
            isConnected: this.isConnected,
            isConnecting: this.isConnecting,
            reconnectAttempts: this.reconnectAttempts,
            connectionState: this.connection?.state || 'Disconnected'
        };
    }
}

// Create global instance
window.AdminRealtime = new AdminRealtime();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminRealtime;
}