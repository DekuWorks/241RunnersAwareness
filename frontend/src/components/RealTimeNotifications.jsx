import React, { useEffect, useState, useRef } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { toast } from 'react-hot-toast';

const RealTimeNotifications = ({ userRole, caseId }) => {
  const [connection, setConnection] = useState(null);
  const [isConnected, setIsConnected] = useState(false);
  const [notifications, setNotifications] = useState([]);
  const [urgentAlerts, setUrgentAlerts] = useState([]);
  const connectionRef = useRef(null);

  useEffect(() => {
    // Initialize SignalR connection
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${process.env.REACT_APP_API_BASE_URL}/notificationHub`)
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    connectionRef.current = newConnection;

    // Set up connection event handlers
    newConnection.onclose(async () => {
      setIsConnected(false);
      console.log('SignalR connection closed');
    });

    newConnection.onreconnecting(() => {
      setIsConnected(false);
      console.log('SignalR reconnecting...');
    });

    newConnection.onreconnected(() => {
      setIsConnected(true);
      console.log('SignalR reconnected');
    });

    // Set up notification handlers
    newConnection.on('ReceiveUrgentAlert', (data) => {
      console.log('🚨 URGENT ALERT RECEIVED:', data);
      handleUrgentAlert(data);
    });

    newConnection.on('ReceiveLawEnforcementAlert', (data) => {
      console.log('👮 LAW ENFORCEMENT ALERT RECEIVED:', data);
      handleLawEnforcementAlert(data);
    });

    newConnection.on('ReceiveEmergencyAlert', (data) => {
      console.log('🚨 EMERGENCY ALERT RECEIVED:', data);
      handleEmergencyAlert(data);
    });

    newConnection.on('ReceiveMediaAlert', (data) => {
      console.log('📰 MEDIA ALERT RECEIVED:', data);
      handleMediaAlert(data);
    });

    newConnection.on('ReceiveCaseUpdate', (data) => {
      console.log('📋 CASE UPDATE RECEIVED:', data);
      handleCaseUpdate(data);
    });

    newConnection.on('ReceiveFoundNotification', (data) => {
      console.log('✅ FOUND NOTIFICATION RECEIVED:', data);
      handleFoundNotification(data);
    });

    newConnection.on('ReceiveTestMessage', (data) => {
      console.log('🧪 TEST MESSAGE RECEIVED:', data);
      handleTestMessage(data);
    });

    // Start connection
    startConnection();

    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, []);

  const startConnection = async () => {
    try {
      await connectionRef.current.start();
      setIsConnected(true);
      console.log('SignalR connected successfully');

      // Join appropriate groups based on user role
      await joinGroups();
    } catch (err) {
      console.error('SignalR connection failed:', err);
      setIsConnected(false);
    }
  };

  const joinGroups = async () => {
    try {
      // Join groups based on user role
      if (userRole === 'Admin') {
        await connectionRef.current.invoke('JoinAdmins');
        await connectionRef.current.invoke('JoinLawEnforcement');
        await connectionRef.current.invoke('JoinMedia');
      } else if (userRole === 'Caregiver') {
        await connectionRef.current.invoke('JoinLawEnforcement');
        await connectionRef.current.invoke('JoinEmergencyContacts');
      } else if (userRole === 'Parent') {
        await connectionRef.current.invoke('JoinEmergencyContacts');
      }

      // Join case-specific group if caseId is provided
      if (caseId) {
        await connectionRef.current.invoke('JoinCaseStakeholders', caseId);
      }

      console.log('Joined notification groups successfully');
    } catch (err) {
      console.error('Failed to join groups:', err);
    }
  };

  const handleUrgentAlert = (data) => {
    // Add to urgent alerts list
    setUrgentAlerts(prev => [data, ...prev.slice(0, 9)]); // Keep last 10

    // Show toast notification
    toast.error(
      <div>
        <strong>🚨 URGENT ALERT</strong>
        <br />
        {data.IndividualName} reported missing at {data.Location}
        <br />
        <small>Case ID: {data.CaseId}</small>
      </div>,
      {
        duration: 10000,
        position: 'top-center',
        style: {
          background: '#dc2626',
          color: 'white',
          border: '2px solid #b91c1c',
          fontSize: '14px',
          fontWeight: 'bold'
        }
      }
    );

    // Play alert sound
    playAlertSound();
  };

  const handleLawEnforcementAlert = (data) => {
    if (userRole === 'Admin' || userRole === 'Caregiver') {
      toast.error(
        <div>
          <strong>👮 LAW ENFORCEMENT ALERT</strong>
          <br />
          {data.IndividualName} missing at {data.Location}
          <br />
          <small>Special needs: {data.SpecialNeeds}</small>
        </div>,
        {
          duration: 8000,
          position: 'top-right'
        }
      );
    }
  };

  const handleEmergencyAlert = (data) => {
    toast.error(
      <div>
        <strong>🚨 EMERGENCY ALERT</strong>
        <br />
        {data.IndividualName} is missing
        <br />
        <small>Case ID: {data.CaseId}</small>
      </div>,
      {
        duration: 8000,
        position: 'top-left'
      }
    );
  };

  const handleMediaAlert = (data) => {
    if (userRole === 'Admin') {
      toast.info(
        <div>
          <strong>📰 MEDIA ALERT</strong>
          <br />
          {data.IndividualName} missing at {data.Location}
          <br />
          <small>Prepare media release</small>
        </div>,
        {
          duration: 6000,
          position: 'bottom-right'
        }
      );
    }
  };

  const handleCaseUpdate = (data) => {
    toast.success(
      <div>
        <strong>📋 CASE UPDATE</strong>
        <br />
        Case {data.CaseId} status: {data.Status}
        <br />
        <small>Updated by: {data.UpdatedBy}</small>
      </div>,
      {
        duration: 5000,
        position: 'bottom-left'
      }
    );
  };

  const handleFoundNotification = (data) => {
    // Remove from urgent alerts
    setUrgentAlerts(prev => prev.filter(alert => alert.CaseId !== data.CaseId));

    toast.success(
      <div>
        <strong>✅ GREAT NEWS!</strong>
        <br />
        {data.IndividualName} has been found!
        <br />
        <small>Location: {data.FoundLocation}</small>
      </div>,
      {
        duration: 10000,
        position: 'top-center',
        style: {
          background: '#10b981',
          color: 'white',
          border: '2px solid #059669',
          fontSize: '14px',
          fontWeight: 'bold'
        }
      }
    );

    // Play success sound
    playSuccessSound();
  };

  const handleTestMessage = (data) => {
    toast.info(
      <div>
        <strong>🧪 TEST MESSAGE</strong>
        <br />
        {data.Message}
        <br />
        <small>Connection ID: {data.ConnectionId}</small>
      </div>,
      {
        duration: 4000,
        position: 'bottom-center'
      }
    );
  };

  const playAlertSound = () => {
    try {
      const audio = new Audio('/sounds/alert.mp3');
      audio.volume = 0.5;
      audio.play().catch(err => console.log('Could not play alert sound:', err));
    } catch (err) {
      console.log('Alert sound not available:', err);
    }
  };

  const playSuccessSound = () => {
    try {
      const audio = new Audio('/sounds/success.mp3');
      audio.volume = 0.5;
      audio.play().catch(err => console.log('Could not play success sound:', err));
    } catch (err) {
      console.log('Success sound not available:', err);
    }
  };

  const sendTestMessage = async () => {
    try {
      await connectionRef.current.invoke('SendTestMessage', 'Test notification from frontend');
    } catch (err) {
      console.error('Failed to send test message:', err);
    }
  };

  const clearUrgentAlerts = () => {
    setUrgentAlerts([]);
  };

  return (
    <div className="real-time-notifications">
      {/* Connection Status */}
      <div className="connection-status">
        <div className={`status-indicator ${isConnected ? 'connected' : 'disconnected'}`}>
          <div className="status-dot"></div>
          <span>{isConnected ? 'Connected' : 'Disconnected'}</span>
        </div>
      </div>

      {/* Urgent Alerts Panel */}
      {urgentAlerts.length > 0 && (
        <div className="urgent-alerts-panel">
          <div className="panel-header">
            <h3>🚨 Active Urgent Alerts ({urgentAlerts.length})</h3>
            <button onClick={clearUrgentAlerts} className="clear-btn">
              Clear All
            </button>
          </div>
          <div className="alerts-list">
            {urgentAlerts.map((alert, index) => (
              <div key={`${alert.CaseId}-${index}`} className="urgent-alert-item">
                <div className="alert-header">
                  <strong>{alert.IndividualName}</strong>
                  <span className="alert-time">
                    {new Date(alert.Timestamp).toLocaleTimeString()}
                  </span>
                </div>
                <div className="alert-details">
                  <p><strong>Location:</strong> {alert.Location}</p>
                  <p><strong>Case ID:</strong> {alert.CaseId}</p>
                  <p><strong>Description:</strong> {alert.Description}</p>
                </div>
                <div className="alert-actions">
                  <button className="action-btn primary">View Details</button>
                  <button className="action-btn secondary">Contact LE</button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Test Button (Admin only) */}
      {userRole === 'Admin' && (
        <div className="test-controls">
          <button onClick={sendTestMessage} className="test-btn">
            Send Test Notification
          </button>
        </div>
      )}

      {/* Styles */}
      <style jsx>{`
        .real-time-notifications {
          position: fixed;
          top: 20px;
          right: 20px;
          z-index: 1000;
          max-width: 400px;
        }

        .connection-status {
          background: white;
          border-radius: 8px;
          padding: 10px;
          margin-bottom: 10px;
          box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        .status-indicator {
          display: flex;
          align-items: center;
          gap: 8px;
          font-size: 12px;
          font-weight: 600;
        }

        .status-dot {
          width: 8px;
          height: 8px;
          border-radius: 50%;
          animation: pulse 2s infinite;
        }

        .status-indicator.connected .status-dot {
          background: #10b981;
        }

        .status-indicator.disconnected .status-dot {
          background: #dc2626;
        }

        @keyframes pulse {
          0% { opacity: 1; }
          50% { opacity: 0.5; }
          100% { opacity: 1; }
        }

        .urgent-alerts-panel {
          background: white;
          border-radius: 8px;
          box-shadow: 0 4px 16px rgba(0,0,0,0.15);
          max-height: 500px;
          overflow-y: auto;
        }

        .panel-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 15px;
          border-bottom: 1px solid #e5e7eb;
          background: #fef2f2;
        }

        .panel-header h3 {
          margin: 0;
          color: #dc2626;
          font-size: 14px;
        }

        .clear-btn {
          background: #dc2626;
          color: white;
          border: none;
          border-radius: 4px;
          padding: 4px 8px;
          font-size: 12px;
          cursor: pointer;
        }

        .clear-btn:hover {
          background: #b91c1c;
        }

        .alerts-list {
          padding: 10px;
        }

        .urgent-alert-item {
          background: #fef2f2;
          border: 1px solid #fecaca;
          border-radius: 6px;
          padding: 12px;
          margin-bottom: 10px;
        }

        .alert-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 8px;
        }

        .alert-time {
          font-size: 11px;
          color: #6b7280;
        }

        .alert-details p {
          margin: 4px 0;
          font-size: 12px;
        }

        .alert-actions {
          display: flex;
          gap: 8px;
          margin-top: 8px;
        }

        .action-btn {
          border: none;
          border-radius: 4px;
          padding: 4px 8px;
          font-size: 11px;
          cursor: pointer;
        }

        .action-btn.primary {
          background: #3b82f6;
          color: white;
        }

        .action-btn.secondary {
          background: #6b7280;
          color: white;
        }

        .test-controls {
          background: white;
          border-radius: 8px;
          padding: 10px;
          margin-top: 10px;
          box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        .test-btn {
          background: #7c3aed;
          color: white;
          border: none;
          border-radius: 4px;
          padding: 8px 12px;
          font-size: 12px;
          cursor: pointer;
        }

        .test-btn:hover {
          background: #6d28d9;
        }

        @media (max-width: 768px) {
          .real-time-notifications {
            position: fixed;
            top: 10px;
            right: 10px;
            left: 10px;
            max-width: none;
          }
        }
      `}</style>
    </div>
  );
};

export default RealTimeNotifications; 