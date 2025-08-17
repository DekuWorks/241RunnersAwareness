import React, { useState, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import SEO from '../components/SEO';
import { API_BASE_URL } from '../config/environment';

const AdminDashboard = () => {
  const user = useSelector(state => state.auth.user);
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [systemStatus, setSystemStatus] = useState({
    totalUsers: 0,
    adminUsers: 0,
    regularUsers: 0,
    totalIndividuals: 0,
    totalProducts: 0,
    totalOrders: 0
  });

  // User management state
  const [userEmails, setUserEmails] = useState([]);
  const [newUserEmail, setNewUserEmail] = useState('');

  useEffect(() => {
    // Check if user is admin
    if (!user || user.role !== 'admin') {
      navigate('/login');
      return;
    }

    loadSystemStatus();
    loadUserEmails();
  }, [user, navigate]);

  const loadSystemStatus = async () => {
    try {
      setLoading(true);
      // Mock data for now - replace with actual API call
      setSystemStatus({
        totalUsers: 5,
        adminUsers: 2,
        regularUsers: 3,
        totalIndividuals: 25,
        totalProducts: 12,
        totalOrders: 8
      });
    } catch (error) {
      console.error('Error loading system status:', error);
      setMessage('Error loading system status');
    } finally {
      setLoading(false);
    }
  };

  const loadUserEmails = () => {
    const savedEmails = localStorage.getItem('authorizedUserEmails');
    if (savedEmails) {
      setUserEmails(JSON.parse(savedEmails));
    }
  };

  const addUserEmail = () => {
    if (newUserEmail && !userEmails.includes(newUserEmail)) {
      const updatedEmails = [...userEmails, newUserEmail];
      setUserEmails(updatedEmails);
      setNewUserEmail('');
      localStorage.setItem('authorizedUserEmails', JSON.stringify(updatedEmails));
      setMessage('✅ Email added successfully!');
    }
  };

  const removeUserEmail = (email) => {
    const updatedEmails = userEmails.filter(e => e !== email);
    setUserEmails(updatedEmails);
    localStorage.setItem('authorizedUserEmails', JSON.stringify(updatedEmails));
    setMessage('✅ Email removed successfully!');
  };

  const clearAllEmails = () => {
    if (userEmails.length === 0) {
      setMessage('No emails to clear');
      return;
    }

    if (window.confirm('Are you sure you want to clear all emails?')) {
      setUserEmails([]);
      localStorage.removeItem('authorizedUserEmails');
      setMessage('✅ All emails cleared successfully!');
    }
  };

  const adminTabs = [
    { id: 'overview', label: 'Overview', icon: '📊' },
    { id: 'user-emails', label: 'User Emails', icon: '📧' },
    { id: 'settings', label: 'Settings', icon: '⚙️' }
  ];

  const renderOverview = () => (
    <div className="admin-overview">
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">👥</div>
          <div className="stat-content">
            <h3>{systemStatus.totalUsers}</h3>
            <p>Total Users</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">👑</div>
          <div className="stat-content">
            <h3>{systemStatus.adminUsers}</h3>
            <p>Admin Users</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">👤</div>
          <div className="stat-content">
            <h3>{systemStatus.regularUsers}</h3>
            <p>Regular Users</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">📋</div>
          <div className="stat-content">
            <h3>{systemStatus.totalIndividuals}</h3>
            <p>Individuals</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🛍️</div>
          <div className="stat-content">
            <h3>{systemStatus.totalProducts}</h3>
            <p>Products</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">📦</div>
          <div className="stat-content">
            <h3>{systemStatus.totalOrders}</h3>
            <p>Orders</p>
          </div>
        </div>
      </div>

      <div className="quick-actions">
        <h3>Quick Actions</h3>
        <div className="action-buttons">
          <button 
            className="action-btn"
            onClick={() => setActiveTab('user-emails')}
          >
            <span>📧</span>
            <span>Manage User Emails</span>
          </button>
          <button 
            className="action-btn"
            onClick={() => setActiveTab('settings')}
          >
            <span>⚙️</span>
            <span>System Settings</span>
          </button>
          <button 
            className="action-btn"
            onClick={exportData}
          >
            <span>📤</span>
            <span>Export Data</span>
          </button>
        </div>
      </div>
    </div>
  );

  const renderUserEmails = () => (
    <div className="user-emails">
      <div className="section-header">
        <h2>📧 User Email Management</h2>
        <p>Manage authorized user emails for the platform.</p>
      </div>

      <div className="email-management">
        <div className="add-email-section">
          <h3>Add User Email</h3>
          <div className="email-input-group">
            <input
              type="email"
              value={newUserEmail}
              onChange={(e) => setNewUserEmail(e.target.value)}
              placeholder="user@example.com"
              className="email-input"
              onKeyPress={(e) => e.key === 'Enter' && addUserEmail()}
            />
            <button 
              onClick={addUserEmail}
              className="btn-secondary"
              disabled={!newUserEmail}
            >
              Add Email
            </button>
          </div>
        </div>

        <div className="email-list-section">
          <h3>Authorized User Emails ({userEmails.length})</h3>
          {userEmails.length === 0 ? (
            <p className="no-emails">No user emails added yet.</p>
          ) : (
            <div className="email-list">
              {userEmails.map((email, index) => (
                <div key={index} className="email-item">
                  <span className="email-text">{email}</span>
                  <button 
                    onClick={() => removeUserEmail(email)}
                    className="btn-small btn-danger"
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="email-actions">
          <button 
            onClick={clearAllEmails}
            className="btn-secondary"
            disabled={userEmails.length === 0}
          >
            Clear All Emails
          </button>
          <button 
            onClick={() => {
              localStorage.setItem('authorizedUserEmails', JSON.stringify(userEmails));
              setMessage('✅ User emails saved successfully!');
            }}
            className="btn-primary"
          >
            Save Emails
          </button>
        </div>
      </div>
    </div>
  );

  const renderSettings = () => (
    <div className="admin-settings">
      <div className="section-header">
        <h2>⚙️ Admin Settings</h2>
        <p>Configure system settings and preferences.</p>
      </div>

      <div className="settings-grid">
        <div className="setting-card">
          <h3>System Information</h3>
          <div className="setting-item">
            <span>Current Admin:</span>
            <span>{user?.email}</span>
          </div>
          <div className="setting-item">
            <span>Last Login:</span>
            <span>{user?.lastLoginAt ? new Date(user.lastLoginAt).toLocaleString() : 'N/A'}</span>
          </div>
          <div className="setting-item">
            <span>Account Created:</span>
            <span>{user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'N/A'}</span>
          </div>
        </div>

        <div className="setting-card">
          <h3>Database Status</h3>
          <div className="setting-item">
            <span>Total Records:</span>
            <span>{systemStatus.totalUsers + systemStatus.totalIndividuals + systemStatus.totalProducts + systemStatus.totalOrders}</span>
          </div>
          <div className="setting-item">
            <span>Authorized Emails:</span>
            <span>{userEmails.length}</span>
          </div>
        </div>
      </div>
    </div>
  );

  const exportData = () => {
    const data = {
      userEmails: userEmails,
      exportDate: new Date().toISOString(),
      totalUsers: systemStatus.totalUsers,
      systemStatus: systemStatus
    };

    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `admin-export-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    setMessage('✅ Data exported successfully!');
  };

  const renderContent = () => {
    switch (activeTab) {
      case 'overview':
        return renderOverview();
      case 'user-emails':
        return renderUserEmails();
      case 'settings':
        return renderSettings();
      default:
        return renderOverview();
    }
  };

  if (!user || user.role !== 'admin') {
    return <div>Access denied. Admin privileges required.</div>;
  }

  return (
    <>
      <SEO 
        title="Admin Dashboard"
        description="Administrative dashboard for 241 Runners Awareness platform management"
        keywords={['admin', 'dashboard', 'management', 'users', 'emails', 'analytics']}
        url="/admin"
      />
      
      <div className="admin-dashboard">
        <div className="admin-header">
          <h1>Admin Dashboard</h1>
          <div className="admin-user-info">
            <span>Welcome, {user.fullName}</span>
            <Link to="/" className="btn-secondary">Back to Site</Link>
          </div>
        </div>

        {message && (
          <div className={`message ${message.includes('✅') ? 'success' : message.includes('❌') ? 'error' : 'info'}`}>
            {message}
            <button onClick={() => setMessage('')} className="message-close">×</button>
          </div>
        )}

        <div className="admin-content">
          <div className="admin-sidebar">
            <nav className="admin-nav">
              {adminTabs.map(tab => (
                <button
                  key={tab.id}
                  className={`admin-nav-item ${activeTab === tab.id ? 'active' : ''}`}
                  onClick={() => setActiveTab(tab.id)}
                >
                  <span className="nav-icon">{tab.icon}</span>
                  <span className="nav-label">{tab.label}</span>
                </button>
              ))}
            </nav>
          </div>

          <div className="admin-main">
            {renderContent()}
          </div>
        </div>
      </div>
    </>
  );
};

export default AdminDashboard;
  