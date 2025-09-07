/**
 * ============================================
 * 241 RUNNERS AWARENESS - ADMIN REALTIME CLIENT
 * ============================================
 * 
 * P1 Implementation: Real-Time Admin Updates
 * SignalR client for real-time admin dashboard updates
 */

// ===== CONFIGURATION =====
const REALTIME_CONFIG = {
    hubUrl: 'https://241runners-api.azurewebsites.net/admin-hub',
    reconnectInterval: 5000,
    maxReconnectAttempts: 10,
    pollingFallbackInterval: 30000, // 30 seconds
    debounceDelay: 1000
};

// ===== STATE MANAGEMENT =====
let realtimeState = {
    connection: null,
    isConnected: false,
    reconnectAttempts: 0,
    pollingTimer: null,
    lastDataVersion: null,
    eventHandlers: new Map(),
    isPollingMode: false
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
        
        // Get auth token
        const authData = getAuthData();
        if (!authData.token) {
            console.error('❌ No auth token available for SignalR');
            startPollingFallback();
            return false;
        }
        
        // Create connection
        realtimeState.connection = new signalR.HubConnectionBuilder()
            .withUrl(REALTIME_CONFIG.hubUrl, {
                accessTokenFactory: () => authData.token,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect([0, 2000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build();
        
        // Set up event handlers
        setupSignalREventHandlers();
        
        // Start connection
        await realtimeState.connection.start();
        
        console.log('✅ SignalR connected successfully');
        realtimeState.isConnected = true;
        realtimeState.reconnectAttempts = 0;
        realtimeState.isPollingMode = false;
        
        // Stop polling if it was running
        stopPollingFallback();
        
        // Join admin group
        await realtimeState.connection.invoke('JoinAdminGroup');
        
        updateConnectionStatus(true);
        return true;
        
    } catch (error) {
        console.error('❌ SignalR connection failed:', error);
        realtimeState.isConnected = false;
        startPollingFallback();
        return false;
    }
}

/**
 * Setup SignalR event handlers
 */
function setupSignalREventHandlers() {
    if (!realtimeState.connection) return;
    
    // Connection events
    realtimeState.connection.onclose((error) => {
        console.log('🔌 SignalR connection closed:', error);
        realtimeState.isConnected = false;
        updateConnectionStatus(false);
        
        // Start polling fallback
        if (!realtimeState.isPollingMode) {
            startPollingFallback();
        }
    });
    
    realtimeState.connection.onreconnecting((error) => {
        console.log('🔄 SignalR reconnecting:', error);
        updateConnectionStatus(false, true);
    });
    
    realtimeState.connection.onreconnected((connectionId) => {
        console.log('✅ SignalR reconnected:', connectionId);
        realtimeState.isConnected = true;
        realtimeState.reconnectAttempts = 0;
        realtimeState.isPollingMode = false;
        updateConnectionStatus(true);
        stopPollingFallback();
    });
    
    // Admin events
    realtimeState.connection.on('AdminJoined', (data) => {
        console.log('👤 Admin joined:', data);
        triggerEvent('adminJoined', data);
    });
    
    realtimeState.connection.on('AdminLeft', (data) => {
        console.log('👋 Admin left:', data);
        triggerEvent('adminLeft', data);
    });
    
    // Data change events
    realtimeState.connection.on('UserChanged', (data) => {
        console.log('👥 User changed:', data);
        debounceEvent('userChanged', data);
    });
    
    realtimeState.connection.on('RunnerChanged', (data) => {
        console.log('🏃 Runner changed:', data);
        debounceEvent('runnerChanged', data);
    });
    
    realtimeState.connection.on('AdminChanged', (data) => {
        console.log('👨‍💼 Admin changed:', data);
        debounceEvent('adminChanged', data);
    });
    
    realtimeState.connection.on('PublicCaseChanged', (data) => {
        console.log('📋 Public case changed:', data);
        debounceEvent('publicCaseChanged', data);
    });
    
    realtimeState.connection.on('SystemStatusChanged', (data) => {
        console.log('⚡ System status changed:', data);
        triggerEvent('systemStatusChanged', data);
    });
    
    realtimeState.connection.on('DataVersionChanged', (data) => {
        console.log('📊 Data version changed:', data);
        triggerEvent('dataVersionChanged', data);
    });
    
    realtimeState.connection.on('ConnectionInfo', (data) => {
        console.log('🔗 Connection info:', data);
        triggerEvent('connectionInfo', data);
    });
}

// ===== POLLING FALLBACK =====

/**
 * Start polling fallback when SignalR is not available
 */
function startPollingFallback() {
    if (realtimeState.pollingTimer) {
        return; // Already polling
    }
    
    console.log('🔄 Starting polling fallback...');
    realtimeState.isPollingMode = true;
    updateConnectionStatus(false, true);
    
    // Initial poll
    pollForUpdates();
    
    // Set up periodic polling
    realtimeState.pollingTimer = setInterval(pollForUpdates, REALTIME_CONFIG.pollingFallbackInterval);
}

/**
 * Stop polling fallback
 */
function stopPollingFallback() {
    if (realtimeState.pollingTimer) {
        clearInterval(realtimeState.pollingTimer);
        realtimeState.pollingTimer = null;
        console.log('🛑 Polling fallback stopped');
    }
}

/**
 * Poll for updates using ETag
 */
async function pollForUpdates() {
    try {
        console.log('🔄 Polling for updates...');
        
        // Check data version
        const response = await fetchWithAuth('/Admin/data-version', {
            headers: {
                'If-None-Match': realtimeState.lastDataVersion || ''
            }
        });
        
        if (response.status === 304) {
            // No changes
            return;
        }
        
        const data = await response.json();
        const newVersion = response.headers.get('ETag');
        
        if (newVersion && newVersion !== realtimeState.lastDataVersion) {
            console.log('📊 Data version changed:', newVersion);
            realtimeState.lastDataVersion = newVersion;
            
            // Trigger refresh event
            triggerEvent('dataVersionChanged', {
                version: newVersion,
                timestamp: new Date().toISOString()
            });
        }
        
    } catch (error) {
        console.error('❌ Polling failed:', error);
    }
}

// ===== EVENT SYSTEM =====

/**
 * Register event handler
 * @param {string} eventName - Event name
 * @param {Function} handler - Event handler function
 */
function onRealtimeEvent(eventName, handler) {
    if (!realtimeState.eventHandlers.has(eventName)) {
        realtimeState.eventHandlers.set(eventName, []);
    }
    realtimeState.eventHandlers.get(eventName).push(handler);
}

/**
 * Remove event handler
 * @param {string} eventName - Event name
 * @param {Function} handler - Event handler function
 */
function offRealtimeEvent(eventName, handler) {
    if (!realtimeState.eventHandlers.has(eventName)) {
        return;
    }
    
    const handlers = realtimeState.eventHandlers.get(eventName);
    const index = handlers.indexOf(handler);
    if (index > -1) {
        handlers.splice(index, 1);
    }
}

/**
 * Trigger event
 * @param {string} eventName - Event name
 * @param {*} data - Event data
 */
function triggerEvent(eventName, data) {
    if (!realtimeState.eventHandlers.has(eventName)) {
        return;
    }
    
    const handlers = realtimeState.eventHandlers.get(eventName);
    handlers.forEach(handler => {
        try {
            handler(data);
        } catch (error) {
            console.error(`❌ Error in event handler for ${eventName}:`, error);
        }
    });
}

/**
 * Debounced event trigger
 * @param {string} eventName - Event name
 * @param {*} data - Event data
 */
let debounceTimers = new Map();

function debounceEvent(eventName, data) {
    if (debounceTimers.has(eventName)) {
        clearTimeout(debounceTimers.get(eventName));
    }
    
    debounceTimers.set(eventName, setTimeout(() => {
        triggerEvent(eventName, data);
        debounceTimers.delete(eventName);
    }, REALTIME_CONFIG.debounceDelay));
}

// ===== CONNECTION STATUS =====

/**
 * Update connection status UI
 * @param {boolean} isConnected - Connection status
 * @param {boolean} isPolling - Polling mode status
 */
function updateConnectionStatus(isConnected, isPolling = false) {
    const statusElement = document.getElementById('connectionStatus');
    const textElement = document.getElementById('connectionText');
    
    if (statusElement && textElement) {
        if (isConnected) {
            statusElement.className = 'connection-status connected';
            textElement.textContent = 'Real-time Connected';
        } else if (isPolling) {
            statusElement.className = 'connection-status';
            textElement.textContent = 'Polling Mode';
        } else {
            statusElement.className = 'connection-status disconnected';
            textElement.textContent = 'Disconnected';
        }
    }
}

// ===== BROADCAST FUNCTIONS =====

/**
 * Broadcast user change
 * @param {string} operation - Operation type (create, update, delete)
 * @param {Object} userData - User data
 */
async function broadcastUserChange(operation, userData) {
    if (realtimeState.connection && realtimeState.isConnected) {
        try {
            await realtimeState.connection.invoke('BroadcastUserChange', operation, userData);
        } catch (error) {
            console.error('❌ Failed to broadcast user change:', error);
        }
    }
}

/**
 * Broadcast runner change
 * @param {string} operation - Operation type (create, update, delete)
 * @param {Object} runnerData - Runner data
 */
async function broadcastRunnerChange(operation, runnerData) {
    if (realtimeState.connection && realtimeState.isConnected) {
        try {
            await realtimeState.connection.invoke('BroadcastRunnerChange', operation, runnerData);
        } catch (error) {
            console.error('❌ Failed to broadcast runner change:', error);
        }
    }
}

/**
 * Broadcast admin change
 * @param {string} operation - Operation type (create, update, delete)
 * @param {Object} adminData - Admin data
 */
async function broadcastAdminChange(operation, adminData) {
    if (realtimeState.connection && realtimeState.isConnected) {
        try {
            await realtimeState.connection.invoke('BroadcastAdminChange', operation, adminData);
        } catch (error) {
            console.error('❌ Failed to broadcast admin change:', error);
        }
    }
}

/**
 * Broadcast public case change
 * @param {string} operation - Operation type (create, update, delete)
 * @param {Object} caseData - Case data
 */
async function broadcastPublicCaseChange(operation, caseData) {
    if (realtimeState.connection && realtimeState.isConnected) {
        try {
            await realtimeState.connection.invoke('BroadcastPublicCaseChange', operation, caseData);
        } catch (error) {
            console.error('❌ Failed to broadcast public case change:', error);
        }
    }
}

// ===== INITIALIZATION =====

/**
 * Initialize real-time system
 */
async function initializeRealtime() {
    console.log('🚀 Initializing real-time system...');
    
    try {
        // Check authentication
        if (!isAuthenticated()) {
            console.warn('⚠️ Not authenticated, skipping real-time initialization');
            return false;
        }
        
        // Initialize SignalR
        const signalRSuccess = await initializeSignalR();
        
        if (!signalRSuccess) {
            console.log('🔄 SignalR failed, using polling fallback');
        }
        
        console.log('✅ Real-time system initialized');
        return true;
        
    } catch (error) {
        console.error('❌ Failed to initialize real-time system:', error);
        startPollingFallback();
        return false;
    }
}

/**
 * Disconnect real-time system
 */
function disconnectRealtime() {
    console.log('🔌 Disconnecting real-time system...');
    
    // Stop polling
    stopPollingFallback();
    
    // Disconnect SignalR
    if (realtimeState.connection) {
        realtimeState.connection.stop();
        realtimeState.connection = null;
    }
    
    realtimeState.isConnected = false;
    realtimeState.isPollingMode = false;
    
    updateConnectionStatus(false);
    
    console.log('✅ Real-time system disconnected');
}

// ===== EXPORTS =====

// Make functions available globally
window.AdminRealtime = {
    initialize: initializeRealtime,
    disconnect: disconnectRealtime,
    on: onRealtimeEvent,
    off: offRealtimeEvent,
    broadcastUserChange,
    broadcastRunnerChange,
    broadcastAdminChange,
    broadcastPublicCaseChange,
    isConnected: () => realtimeState.isConnected,
    isPolling: () => realtimeState.isPollingMode,
    getState: () => ({ ...realtimeState })
};

// Auto-initialize if authenticated
document.addEventListener('DOMContentLoaded', () => {
    if (typeof isAuthenticated === 'function' && isAuthenticated()) {
        initializeRealtime();
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    disconnectRealtime();
});
