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
            const hubUrl = baseUrl + '/hubs/alerts'; // Use AlertsHub which is the main hub
            
            console.log('🔌 Connecting to SignalR hub:', hubUrl);
            
            // Get authentication token
            const token = localStorage.getItem("jwtToken") || localStorage.getItem("ra_admin_token") || localStorage.getItem("ra_auth");
            console.log('🔑 Using token for SignalR:', token ? 'Present' : 'Missing');
            
            // Create SignalR connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl, {
                    accessTokenFactory: () => {
                        const authToken = localStorage.getItem("jwtToken") || localStorage.getItem("ra_admin_token");
                        return authToken || "";
                    },
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
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

        this.connection.on("SystemNotification", (data) => {
            console.log('🔔 System notification:', data);
            this.handleSystemNotification(data);
        });
    }

    async start() {
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

    async stop() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
            console.log('🔌 Real-time connection stopped');
            this.updateConnectionStatus('disconnected');
        }
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

    handleSystemNotification(data) {
        // Show system notification
        if (typeof showToast === 'function') {
            showToast(data.message || 'System notification', data.type || 'info');
        }
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
