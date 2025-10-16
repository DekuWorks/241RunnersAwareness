/**
 * Real-time SignalR client for 241 Runners Awareness
 * Handles JWT authentication and connection management
 */

class RealtimeClient {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 1000; // Start with 1 second
    }

    async initialize() {
        try {
            // Get API base URL from config
            const apiBaseUrl = window.API_BASE_URL || 'https://241runners-api-v2.azurewebsites.net';
            const baseUrl = apiBaseUrl.replace('/api', '');
            
            // Determine which hub to use based on user role and current page
            const userRole = localStorage.getItem('ra_admin_role') || 'user';
            const currentPage = window.location.pathname;
            
            let hubUrl;
            if (userRole === 'admin') {
                hubUrl = baseUrl + '/hubs/admin';    // AdminHub for admin users
            } else if (currentPage.includes('/admin/')) {
                hubUrl = baseUrl + '/hubs/admin';    // AdminHub for admin pages
            } else if (currentPage.includes('/profile.html')) {
                hubUrl = baseUrl + '/hubs/alerts';   // AlertsHub for user profile
            } else {
                hubUrl = baseUrl + '/hubs/alerts';   // AlertsHub for regular users
            }
            
            console.log('🔌 Connecting to SignalR hub:', hubUrl);
            console.log('👤 User role:', userRole, '→ Using', userRole === 'admin' ? 'AdminHub' : 'AlertsHub');
            
            // Get authentication token - prioritize the same keys as mobile app
            const token = localStorage.getItem("ra_admin_token") || localStorage.getItem("jwtToken") || localStorage.getItem("241runners_access_token");
            console.log('🔑 Using token for SignalR:', token ? 'Present' : 'Missing');
            
            // Create SignalR connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl, {
                    transport: signalR.HttpTransportType.WebSockets,
                    skipNegotiation: true,
                    accessTokenFactory: () => {
                        const authToken = localStorage.getItem("ra_admin_token") || localStorage.getItem("jwtToken") || localStorage.getItem("241runners_access_token");
                        console.log('🔑 SignalR accessTokenFactory token:', authToken ? `Present (${authToken.length} chars)` : 'Missing');
                        return authToken || "";
                    }
                })
                .withAutomaticReconnect([0, 2000, 10000, 30000]) // Reconnect attempts
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Set up event handlers
            this.setupEventHandlers();

            // Start connection
            await this.start();
        } catch (error) {
            console.error('❌ Failed to initialize real-time connection:', error);
            this.handleConnectionError(error);
        }
    }

    setupEventHandlers() {
        // Connection state handlers
        this.connection.onclose((error) => {
            console.warn('🔌 SignalR connection closed:', error);
            this.isConnected = false;
            this.updateConnectionStatus('disconnected');
        });

        this.connection.onreconnecting((error) => {
            console.log('🔄 SignalR reconnecting...', error);
            this.updateConnectionStatus('reconnecting');
        });

        this.connection.onreconnected((connectionId) => {
            console.log('✅ SignalR reconnected:', connectionId);
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;
            this.updateConnectionStatus('connected');
        });

        // Message handlers
        this.connection.on("NewUserRegistration", (data) => {
            console.log('👤 New user registration:', data);
            this.handleNewUserRegistration(data);
        });

        this.connection.on("UserUpdated", (data) => {
            console.log('👤 User updated:', data);
            this.handleUserUpdated(data);
        });

        this.connection.on("UserDeleted", (data) => {
            console.log('👤 User deleted:', data);
            this.handleUserDeleted(data);
        });

        this.connection.on("CaseCreated", (data) => {
            console.log('📋 New case created:', data);
            this.handleCaseCreated(data);
        });

        this.connection.on("CaseUpdated", (data) => {
            console.log('📋 Case updated:', data);
            this.handleCaseUpdated(data);
        });

        this.connection.on("caseDeleted", (data) => {
            console.log('🗑️ Case deleted:', data);
            this.handleCaseDeleted(data);
        });

        this.connection.on("SystemNotification", (data) => {
            console.log('🔔 System notification:', data);
            this.handleSystemNotification(data);
        });

        // Admin-specific event handlers
        this.connection.on("AdminConnected", (data) => {
            console.log('👑 Admin connected:', data);
            this.handleAdminConnected(data);
        });

        this.connection.on("AdminDisconnected", (data) => {
            console.log('👑 Admin disconnected:', data);
            this.handleAdminDisconnected(data);
        });

        this.connection.on("CurrentAdmins", (data) => {
            console.log('👥 Current admins:', data);
            this.handleCurrentAdmins(data);
        });

        this.connection.on("UserChanged", (data) => {
            console.log('👤 User changed:', data);
            this.handleUserChanged(data);
        });

        this.connection.on("RunnerChanged", (data) => {
            console.log('🏃 Runner changed:', data);
            this.handleRunnerChanged(data);
        });

        this.connection.on("AdminProfileChanged", (data) => {
            console.log('👑 Admin profile changed:', data);
            this.handleAdminProfileChanged(data);
        });

        this.connection.on("DataVersionChanged", (data) => {
            console.log('📊 Data version changed:', data);
            this.handleDataVersionChanged(data);
        });

        this.connection.on("AdminActivity", (data) => {
            console.log('👑 Admin activity:', data);
            this.handleAdminActivity(data);
        });

        this.connection.on("OnlineAdmins", (data) => {
            console.log('👥 Online admins:', data);
            this.handleOnlineAdmins(data);
        });

        this.connection.on("Pong", (data) => {
            console.log('🏓 Pong received:', data);
            this.handlePong(data);
        });
    }

    async start() {
        // Check if already connected or connecting
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            console.log('🔔 SignalR already connected');
            this.isConnected = true;
            return;
        }

        if (this.connection?.state === signalR.HubConnectionState.Connecting) {
            console.log('🔄 SignalR connection already in progress');
            return;
        }

        if (this.connection?.state === signalR.HubConnectionState.Reconnecting) {
            console.log('🔄 SignalR reconnection in progress');
            return;
        }

        try {
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;
            console.log('🔔 Real-time connected');
            this.updateConnectionStatus('connected');
        } catch (error) {
            console.warn('⚠️ Real-time connection failed:', error);
            this.handleConnectionError(error);
        }
    }

    async startConnection() {
        let retries = 0;
        const maxRetries = 5;
        
        while (retries < maxRetries) {
            try {
                await this.start();
                return; // Success, exit the loop
            } catch (error) {
                retries++;
                console.warn(`⚠️ Connection attempt ${retries}/${maxRetries} failed:`, error);
                
                if (retries >= maxRetries) {
                    console.error('❌ Max reconnection attempts reached');
                    this.updateConnectionStatus('failed');
                    return;
                }
                
                // Exponential backoff: 1s, 2s, 4s, 8s, 16s
                const delay = Math.pow(2, retries - 1) * 1000;
                console.log(`⏳ Retrying in ${delay}ms...`);
                await new Promise(resolve => setTimeout(resolve, delay));
            }
        }
    }

    async stop() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
            console.log('🔌 Real-time connection stopped');
            this.updateConnectionStatus('disconnected');
        }
    }

    isConnected() {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }

    handleConnectionError(error) {
        this.isConnected = false;
        this.updateConnectionStatus('error');
        
        // Implement custom retry logic if needed
        if (this.reconnectAttempts < this.maxReconnectAttempts) {
            this.reconnectAttempts++;
            console.log(`🔄 Retrying connection in ${this.reconnectDelay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
            
            setTimeout(() => {
                this.start();
            }, this.reconnectDelay);
            
            // Exponential backoff
            this.reconnectDelay = Math.min(this.reconnectDelay * 2, 30000);
        } else {
            console.error('❌ Max reconnection attempts reached');
            this.updateConnectionStatus('failed');
        }
    }

    updateConnectionStatus(status) {
        const statusElement = document.getElementById('connectionText');
        const dotElement = document.querySelector('.connection-dot');
        
        if (statusElement) {
            switch (status) {
                case 'connected':
                    statusElement.textContent = 'Live Database Connection';
                    if (dotElement) dotElement.className = 'connection-dot live';
                    break;
                case 'reconnecting':
                    statusElement.textContent = 'Reconnecting...';
                    if (dotElement) dotElement.className = 'connection-dot reconnecting';
                    break;
                case 'disconnected':
                    statusElement.textContent = 'Connection Lost';
                    if (dotElement) dotElement.className = 'connection-dot disconnected';
                    break;
                case 'error':
                case 'failed':
                    statusElement.textContent = 'Connection Failed';
                    if (dotElement) dotElement.className = 'connection-dot error';
                    break;
                default:
                    statusElement.textContent = 'Connecting...';
                    if (dotElement) dotElement.className = 'connection-dot connecting';
            }
        }
    }

    // Message handlers
    handleNewUserRegistration(data) {
        // Refresh users list
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`New user registered: ${data.userData?.email || 'Unknown'}`, 'info');
        }
    }

    handleUserUpdated(data) {
        // Refresh users list
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
    }

    handleUserDeleted(data) {
        // Refresh users list
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`User deleted: ${data.userEmail || 'Unknown'}`, 'warning');
        }
    }

    handleCaseCreated(data) {
        // Refresh cases list
        if (typeof loadPublicCases === 'function') {
            loadPublicCases();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`New case created: ${data.caseData?.title || 'Unknown'}`, 'info');
        }
    }

    handleCaseUpdated(data) {
        // Refresh cases list
        if (typeof loadPublicCases === 'function') {
            loadPublicCases();
        }
    }

    handleCaseDeleted(data) {
        // Refresh cases list
        if (typeof loadPublicCases === 'function') {
            loadPublicCases();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`Case deleted: ${data.data?.title || 'Unknown'}`, 'warning');
        }
    }

    handleSystemNotification(data) {
        // Show system notification
        if (typeof showToast === 'function') {
            showToast(data.message || 'System notification', data.type || 'info');
        }
    }

    // Admin-specific event handlers
    handleAdminConnected(data) {
        // Show notification about admin connection
        if (typeof showToast === 'function') {
            showToast(`${data.adminName || 'Admin'} connected`, 'info');
        }
        
        // Refresh admin list if function exists
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
    }

    handleAdminDisconnected(data) {
        // Show notification about admin disconnection
        if (typeof showToast === 'function') {
            showToast(`${data.adminName || 'Admin'} disconnected`, 'info');
        }
        
        // Refresh admin list if function exists
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
    }

    handleCurrentAdmins(data) {
        // Update admin list in dashboard
        if (typeof updateAdminList === 'function') {
            updateAdminList(data);
        }
    }

    handleUserChanged(data) {
        // Refresh users list
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`User ${data.operation || 'changed'} by ${data.changedBy || 'admin'}`, 'info');
        }
    }

    handleRunnerChanged(data) {
        // Refresh runners list
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`Runner ${data.operation || 'changed'} by ${data.changedBy || 'admin'}`, 'info');
        }
    }

    handleAdminProfileChanged(data) {
        // Refresh admin profile if needed
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`Admin profile ${data.operation || 'changed'} by ${data.changedBy || 'admin'}`, 'info');
        }
    }

    handleDataVersionChanged(data) {
        // Refresh dashboard data when data version changes
        if (typeof refreshDashboardData === 'function') {
            refreshDashboardData();
        }
        
        // Show notification
        if (typeof showToast === 'function') {
            showToast(`${data.dataType || 'Data'} updated by ${data.changedBy || 'admin'}`, 'info');
        }
    }

    handleAdminActivity(data) {
        // Log admin activity
        console.log(`Admin activity: ${data.activity} by ${data.adminName}`);
        
        // Show notification for important activities
        if (data.activity && ['user_management', 'system_config', 'bulk_operations'].includes(data.activity)) {
            if (typeof showToast === 'function') {
                showToast(`Admin activity: ${data.activity}`, 'info');
            }
        }
    }

    handleOnlineAdmins(data) {
        // Update online admins list
        if (typeof updateOnlineAdmins === 'function') {
            updateOnlineAdmins(data);
        }
    }

    handlePong(data) {
        // Update last ping time
        this.lastPing = data;
        console.log('🏓 Ping response received');
    }

    // Public methods
    isConnectionActive() {
        return this.isConnected && this.connection && this.connection.state === signalR.HubConnectionState.Connected;
    }

    async sendMessage(method, ...args) {
        if (this.isConnectionActive()) {
            try {
                await this.connection.invoke(method, ...args);
                return true;
            } catch (error) {
                console.error('Failed to send message:', error);
                return false;
            }
        }
        return false;
    }
}

// Create global instance
window.realtimeClient = new RealtimeClient();

// Initialize when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
    if (window.realtimeClient) {
        window.realtimeClient.initialize();
    }
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = RealtimeClient;
}
