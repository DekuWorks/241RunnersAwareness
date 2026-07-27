/**
 * ============================================
 * 241 RUNNERS AWARENESS - ADMIN REALTIME UPDATES
 * ============================================
 * 
 * Real-time admin updates using SignalR with polling fallback
 * P1 Implementation: Real-Time Admin Updates
 */

// ===== CONFIGURATION =====
const REALTIME_CONFIG = {
    signalRUrl: 'https://two41runners-api.onrender.com/adminHub',
    pollingInterval: 30000, // 30 seconds fallback
    maxRetries: 5,
    retryDelay: 2000,
    debounceDelay: 500
};

// ===== STATE MANAGEMENT =====
let realtimeState = {
    connection: null,
    isConnected: false,
    isPolling: false,
    pollingTimer: null,
    retryCount: 0,
    lastDataVersion: null,
    eventQueue: [],
    debounceTimer: null
};

// ===== SIGNALR CONNECTION =====

/**
 * Initialize SignalR connection
 */
async function initializeSignalR() {
    try {
        console.log('🔄 Initializing SignalR connection...');
        
        // Check if SignalR is available
        if (typeof signalR === 'undefined') {
            console.warn('⚠️ SignalR not available, falling back to polling');
            startPollingFallback();
            return false;
        }
        
        // Create connection
        realtimeState.connection = new signalR.HubConnectionBuilder()
            .withUrl(REALTIME_CONFIG.signalRUrl, {
                accessTokenFactory: () => {
                    const token = localStorage.getItem('ra_admin_token');
                    return token || '';
                },
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect([0, 2000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build();
        
        // Set up event handlers
        setupSignalREventHandlers();
        
        // Start connection
        await realtimeState.connection.start();
        
        realtimeState.isConnected = true;
        realtimeState.retryCount = 0;
        
        console.log('✅ SignalR connected successfully');
        
        // Join admin group
        await realtimeState.connection.invoke('JoinAdminGroup');
        
        return true;
        
    } catch (error) {
        console.error('❌ SignalR connection failed:', error);
        realtimeState.retryCount++;
        
        if (realtimeState.retryCount < REALTIME_CONFIG.maxRetries) {
            console.log(`🔄 Retrying SignalR connection (attempt ${realtimeState.retryCount + 1})`);
            setTimeout(initializeSignalR, REALTIME_CONFIG.retryDelay * realtimeState.retryCount);
        } else {
            console.log('🔄 Max retries reached, falling back to polling');
            startPollingFallback();
        }
        
        return false;
    }
}

/**
 * Set up SignalR event handlers
 */
function setupSignalREventHandlers() {
    if (!realtimeState.connection) return;
    
    // Connection events
    realtimeState.connection.onclose((error) => {
        console.log('🔌 SignalR connection closed:', error);
        realtimeState.isConnected = false;
        
        if (error) {
            console.log('🔄 Connection lost, attempting to reconnect...');
            setTimeout(initializeSignalR, REALTIME_CONFIG.retryDelay);
        }
    });
    
    realtimeState.connection.onreconnecting((error) => {
        console.log('🔄 SignalR reconnecting:', error);
        realtimeState.isConnected = false;
    });
    
    realtimeState.connection.onreconnected((connectionId) => {
        console.log('✅ SignalR reconnected:', connectionId);
        realtimeState.isConnected = true;
        realtimeState.retryCount = 0;
    });
    
    // Data update events
    realtimeState.connection.on('UserChanged', (data) => {
        console.log('👤 User changed:', data);
        handleDataUpdate('user', data);
    });
    
    realtimeState.connection.on('RunnerChanged', (data) => {
        console.log('🏃 Runner changed:', data);
        handleDataUpdate('runner', data);
    });
    
    realtimeState.connection.on('AdminChanged', (data) => {
        console.log('👨‍💼 Admin changed:', data);
        handleDataUpdate('admin', data);
    });
    
    realtimeState.connection.on('PublicCaseChanged', (data) => {
        console.log('📋 Public case changed:', data);
        handleDataUpdate('publicCase', data);
    });
    
    // System events
    realtimeState.connection.on('SystemStatusChanged', (data) => {
        console.log('⚙️ System status changed:', data);
        handleSystemUpdate(data);
    });
    
    realtimeState.connection.on('DataVersionChanged', (version) => {
        console.log('📊 Data version changed:', version);
        handleDataVersionUpdate(version);
    });
}

// ===== POLLING FALLBACK =====

/**
 * Start polling fallback when SignalR is not available
 */
function startPollingFallback() {
    if (realtimeState.isPolling) return;
    
    console.log('🔄 Starting polling fallback...');
    realtimeState.isPolling = true;
    
    // Initial poll
    performPollingCheck();
    
    // Set up polling timer
    realtimeState.pollingTimer = setInterval(performPollingCheck, REALTIME_CONFIG.pollingInterval);
}

/**
 * Perform polling check for data updates
 */
async function performPollingCheck() {
    try {
        console.log('🔄 Performing polling check...');
        
        // Check data version
        const versionResponse = await fetch('/api/data-version', {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('ra_admin_token')}`,
                'Cache-Control': 'no-cache'
            }
        });
        
        if (versionResponse.ok) {
            const versionData = await versionResponse.json();
            const currentVersion = versionData.version;
            
            if (realtimeState.lastDataVersion && currentVersion !== realtimeState.lastDataVersion) {
                console.log('📊 Data version changed, refreshing dashboard');
                handleDataVersionUpdate(currentVersion);
            }
            
            realtimeState.lastDataVersion = currentVersion;
        }
        
    } catch (error) {
        console.error('❌ Polling check failed:', error);
    }
}

/**
 * Stop polling fallback
 */
function stopPollingFallback() {
    if (realtimeState.pollingTimer) {
        clearInterval(realtimeState.pollingTimer);
        realtimeState.pollingTimer = null;
    }
    realtimeState.isPolling = false;
    console.log('🛑 Polling fallback stopped');
}

// ===== EVENT HANDLING =====

/**
 * Handle data update events with debouncing
 */
function handleDataUpdate(type, data) {
    // Add to event queue
    realtimeState.eventQueue.push({ type, data, timestamp: Date.now() });
    
    // Debounce processing
    if (realtimeState.debounceTimer) {
        clearTimeout(realtimeState.debounceTimer);
    }
    
    realtimeState.debounceTimer = setTimeout(() => {
        processEventQueue();
    }, REALTIME_CONFIG.debounceDelay);
}

/**
 * Process queued events
 */
function processEventQueue() {
    if (realtimeState.eventQueue.length === 0) return;
    
    console.log(`📊 Processing ${realtimeState.eventQueue.length} queued events`);
    
    // Group events by type
    const eventsByType = {};
    realtimeState.eventQueue.forEach(event => {
        if (!eventsByType[event.type]) {
            eventsByType[event.type] = [];
        }
        eventsByType[event.type].push(event);
    });
    
    // Process each type
    Object.keys(eventsByType).forEach(type => {
        const events = eventsByType[type];
        const latestEvent = events[events.length - 1]; // Get the latest event
        
        switch (type) {
            case 'user':
                handleUserUpdate(latestEvent.data);
                break;
            case 'runner':
                handleRunnerUpdate(latestEvent.data);
                break;
            case 'admin':
                handleAdminUpdate(latestEvent.data);
                break;
            case 'publicCase':
                handlePublicCaseUpdate(latestEvent.data);
                break;
        }
    });
    
    // Clear processed events
    realtimeState.eventQueue = [];
}

/**
 * Handle user updates
 */
function handleUserUpdate(data) {
    console.log('👤 Handling user update:', data);
    
    // Update dashboard stats
    if (window.updateDashboardStats) {
        window.updateDashboardStats();
    }
    
    // Update user lists if visible
    if (window.refreshUserList) {
        window.refreshUserList();
    }
    
    // Show notification
    if (window.AdminAuth && window.AdminAuth.showToast) {
        window.AdminAuth.showToast(`User ${data.operation}: ${data.user?.email || 'Unknown'}`, 'info', 3000);
    }
}

/**
 * Handle runner updates
 */
function handleRunnerUpdate(data) {
    console.log('🏃 Handling runner update:', data);
    
    // Update dashboard stats
    if (window.updateDashboardStats) {
        window.updateDashboardStats();
    }
    
    // Update runner lists if visible
    if (window.refreshRunnerList) {
        window.refreshRunnerList();
    }
    
    // Show notification
    if (window.AdminAuth && window.AdminAuth.showToast) {
        window.AdminAuth.showToast(`Runner ${data.operation}: ${data.runner?.name || 'Unknown'}`, 'info', 3000);
    }
}

/**
 * Handle admin updates
 */
function handleAdminUpdate(data) {
    console.log('👨‍💼 Handling admin update:', data);
    
    // Update dashboard stats
    if (window.updateDashboardStats) {
        window.updateDashboardStats();
    }
    
    // Update admin lists if visible
    if (window.refreshAdminList) {
        window.refreshAdminList();
    }
    
    // Show notification
    if (window.AdminAuth && window.AdminAuth.showToast) {
        window.AdminAuth.showToast(`Admin ${data.operation}: ${data.admin?.email || 'Unknown'}`, 'info', 3000);
    }
}

/**
 * Handle public case updates
 */
function handlePublicCaseUpdate(data) {
    console.log('📋 Handling public case update:', data);
    
    // Update dashboard stats
    if (window.updateDashboardStats) {
        window.updateDashboardStats();
    }
    
    // Update public case lists if visible
    if (window.refreshPublicCaseList) {
        window.refreshPublicCaseList();
    }
    
    // Show notification
    if (window.AdminAuth && window.AdminAuth.showToast) {
        window.AdminAuth.showToast(`Public case ${data.operation}: ${data.case?.title || 'Unknown'}`, 'info', 3000);
    }
}

/**
 * Handle system status updates
 */
function handleSystemUpdate(data) {
    console.log('⚙️ Handling system update:', data);
    
    // Update system status display
    const statusElement = document.getElementById('systemStatus');
    if (statusElement) {
        statusElement.textContent = data.status === 'healthy' ? 'Healthy' : 'Error';
        statusElement.className = `stat-number ${data.status === 'healthy' ? 'status-healthy' : 'status-error'}`;
    }
    
    // Show notification for status changes
    if (window.AdminAuth && window.AdminAuth.showToast) {
        const message = data.status === 'healthy' ? 'System is healthy' : 'System error detected';
        const type = data.status === 'healthy' ? 'success' : 'error';
        window.AdminAuth.showToast(message, type, 5000);
    }
}

/**
 * Handle data version updates
 */
function handleDataVersionUpdate(version) {
    console.log('📊 Handling data version update:', version);
    
    // Update last updated timestamp
    const timestampElement = document.getElementById('lastUpdated');
    if (timestampElement) {
        const now = new Date();
        timestampElement.textContent = `${now.toLocaleDateString()} at ${now.toLocaleTimeString()}`;
    }
    
    // Trigger dashboard refresh
    if (window.refreshDashboard) {
        window.refreshDashboard();
    }
}

// ===== CONNECTION MANAGEMENT =====

/**
 * Initialize real-time updates
 */
async function initializeRealtimeUpdates() {
    console.log('🚀 Initializing real-time updates...');
    
    try {
        // Try SignalR first
        const signalRSuccess = await initializeSignalR();
        
        if (!signalRSuccess) {
            console.log('🔄 SignalR failed, using polling fallback');
            startPollingFallback();
        }
        
        console.log('✅ Real-time updates initialized');
        return true;
        
    } catch (error) {
        console.error('❌ Failed to initialize real-time updates:', error);
        startPollingFallback();
        return false;
    }
}

/**
 * Disconnect real-time updates
 */
function disconnectRealtimeUpdates() {
    console.log('🛑 Disconnecting real-time updates...');
    
    // Stop SignalR connection
    if (realtimeState.connection) {
        realtimeState.connection.stop();
        realtimeState.connection = null;
    }
    
    // Stop polling
    stopPollingFallback();
    
    // Clear timers
    if (realtimeState.debounceTimer) {
        clearTimeout(realtimeState.debounceTimer);
        realtimeState.debounceTimer = null;
    }
    
    realtimeState.isConnected = false;
    console.log('✅ Real-time updates disconnected');
}

/**
 * Get connection status
 */
function getConnectionStatus() {
    return {
        isConnected: realtimeState.isConnected,
        isPolling: realtimeState.isPolling,
        retryCount: realtimeState.retryCount,
        lastDataVersion: realtimeState.lastDataVersion
    };
}

// ===== EXPORTS =====

// Make functions available globally
window.AdminRealtime = {
    initializeRealtimeUpdates,
    disconnectRealtimeUpdates,
    getConnectionStatus,
    handleDataUpdate,
    handleSystemUpdate,
    handleDataVersionUpdate
};

// Auto-initialize if this script is loaded on an admin page
if (window.location.pathname.includes('/admin/') && window.location.pathname !== '/admin/login.html') {
    document.addEventListener('DOMContentLoaded', () => {
        // Wait for admin auth to be ready
        setTimeout(() => {
            if (window.AdminAuth && window.AdminAuth.isAuthenticated()) {
                initializeRealtimeUpdates();
            }
        }, 1000);
    });
}
