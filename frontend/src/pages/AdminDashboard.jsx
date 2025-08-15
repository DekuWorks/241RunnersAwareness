import React, { useState, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import SEO from '../components/SEO';

const AdminDashboard = () => {
  const user = useSelector(state => state.auth.user);
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');
  const [stats, setStats] = useState({
    totalUsers: 0,
    totalCases: 0,
    activeCases: 0,
    totalProducts: 0,
    recentActivity: []
  });

  useEffect(() => {
    // Check if user is admin
    if (!user || user.role !== 'admin') {
      navigate('/login');
      return;
    }

    // Load admin stats (mock data for now)
    loadAdminStats();
  }, [user, navigate]);

  const loadAdminStats = async () => {
    // Mock stats - replace with actual API calls
    setStats({
      totalUsers: 156,
      totalCases: 23,
      activeCases: 8,
      totalProducts: 12,
      recentActivity: [
        { type: 'user_registered', message: 'New user registered: john@example.com', time: '2 hours ago' },
        { type: 'case_created', message: 'New case created: Missing Person Report #241-2025-001', time: '4 hours ago' },
        { type: 'product_added', message: 'New product added: 241 Awareness Hoodie', time: '1 day ago' },
        { type: 'dna_sample', message: 'DNA sample submitted for case #241-2025-001', time: '2 days ago' }
      ]
    });
  };

  const adminTabs = [
    { id: 'overview', label: 'Overview', icon: '📊' },
    { id: 'users', label: 'Users', icon: '👥' },
    { id: 'cases', label: 'Cases', icon: '📋' },
    { id: 'products', label: 'Shop', icon: '🛍️' },
    { id: 'dna', label: 'DNA Tracking', icon: '🧬' },
    { id: 'analytics', label: 'Analytics', icon: '📈' },
    { id: 'settings', label: 'Settings', icon: '⚙️' }
  ];

  const renderOverview = () => (
    <div className="admin-overview">
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">👥</div>
          <div className="stat-content">
            <h3>{stats.totalUsers}</h3>
            <p>Total Users</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">📋</div>
          <div className="stat-content">
            <h3>{stats.totalCases}</h3>
            <p>Total Cases</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🔍</div>
          <div className="stat-content">
            <h3>{stats.activeCases}</h3>
            <p>Active Cases</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🛍️</div>
          <div className="stat-content">
            <h3>{stats.totalProducts}</h3>
            <p>Products</p>
          </div>
        </div>
      </div>

      <div className="recent-activity">
        <h3>Recent Activity</h3>
        <div className="activity-list">
          {stats.recentActivity.map((activity, index) => (
            <div key={index} className="activity-item">
              <div className="activity-icon">
                {activity.type === 'user_registered' && '👤'}
                {activity.type === 'case_created' && '📋'}
                {activity.type === 'product_added' && '🛍️'}
                {activity.type === 'dna_sample' && '🧬'}
              </div>
              <div className="activity-content">
                <p>{activity.message}</p>
                <span className="activity-time">{activity.time}</span>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="quick-actions">
        <h3>Quick Actions</h3>
        <div className="action-buttons">
          <Link to="/cases" className="action-btn">
            <span>📋</span>
            <span>View Cases</span>
          </Link>
          <Link to="/users" className="action-btn">
            <span>👥</span>
            <span>Manage Users</span>
          </Link>
          <Link to="/shop" className="action-btn">
            <span>🛍️</span>
            <span>Shop Management</span>
          </Link>
          <Link to="/dna-tracking" className="action-btn">
            <span>🧬</span>
            <span>DNA Tracking</span>
          </Link>
        </div>
      </div>
    </div>
  );

  const renderUsers = () => (
    <div className="admin-users">
      <div className="section-header">
        <h2>User Management</h2>
        <button className="btn-primary">Add New User</button>
      </div>
      <div className="users-table">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Marcus Brown</td>
              <td>dekuworks1@gmail.com</td>
              <td><span className="role-badge admin">Admin</span></td>
              <td><span className="status-badge active">Active</span></td>
              <td>
                <button className="btn-small">Edit</button>
                <button className="btn-small btn-danger">Delete</button>
              </td>
            </tr>
            <tr>
              <td>Daniel Carey</td>
              <td>danielcarey9770@gmail.com</td>
              <td><span className="role-badge admin">Admin</span></td>
              <td><span className="status-badge active">Active</span></td>
              <td>
                <button className="btn-small">Edit</button>
                <button className="btn-small btn-danger">Delete</button>
              </td>
            </tr>
            <tr>
              <td>Test User</td>
              <td>test@example.com</td>
              <td><span className="role-badge user">User</span></td>
              <td><span className="status-badge active">Active</span></td>
              <td>
                <button className="btn-small">Edit</button>
                <button className="btn-small btn-danger">Delete</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderCases = () => (
    <div className="admin-cases">
      <div className="section-header">
        <h2>Case Management</h2>
        <button className="btn-primary">Create New Case</button>
      </div>
      <div className="cases-summary">
        <div className="case-stats">
          <div className="case-stat">
            <h4>Total Cases</h4>
            <p>{stats.totalCases}</p>
          </div>
          <div className="case-stat">
            <h4>Active Cases</h4>
            <p>{stats.activeCases}</p>
          </div>
          <div className="case-stat">
            <h4>Resolved Cases</h4>
            <p>{stats.totalCases - stats.activeCases}</p>
          </div>
        </div>
      </div>
      <div className="cases-table">
        <table>
          <thead>
            <tr>
              <th>Case ID</th>
              <th>Subject</th>
              <th>Status</th>
              <th>Created</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>#241-2025-001</td>
              <td>Missing Person Report</td>
              <td><span className="status-badge active">Active</span></td>
              <td>2025-01-15</td>
              <td>
                <button className="btn-small">View</button>
                <button className="btn-small">Edit</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderContent = () => {
    switch (activeTab) {
      case 'overview':
        return renderOverview();
      case 'users':
        return renderUsers();
      case 'cases':
        return renderCases();
      case 'products':
        return <div className="admin-section"><h2>Shop Management</h2><p>Product management interface coming soon...</p></div>;
      case 'dna':
        return <div className="admin-section"><h2>DNA Tracking</h2><p>DNA tracking interface coming soon...</p></div>;
      case 'analytics':
        return <div className="admin-section"><h2>Analytics</h2><p>Analytics dashboard coming soon...</p></div>;
      case 'settings':
        return <div className="admin-section"><h2>Settings</h2><p>Admin settings coming soon...</p></div>;
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
        keywords={['admin', 'dashboard', 'management', 'users', 'cases', 'analytics']}
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
  