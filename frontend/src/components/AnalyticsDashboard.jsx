import React, { useState, useEffect } from 'react';
import { Line, Bar, Pie, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend
);

const AnalyticsDashboard = () => {
  const [analyticsData, setAnalyticsData] = useState({
    mapInteractions: [],
    authIssues: [],
    caseSearches: [],
    urgentAlerts: [],
    performanceMetrics: [],
    userActions: []
  });
  const [loading, setLoading] = useState(true);
  const [dateRange, setDateRange] = useState('7d'); // 7d, 30d, 90d
  const [selectedMetric, setSelectedMetric] = useState('all');

  useEffect(() => {
    fetchAnalyticsData();
  }, [dateRange]);

  const fetchAnalyticsData = async () => {
    try {
      setLoading(true);
      // TODO: Replace with actual API call
      const mockData = generateMockAnalyticsData();
      setAnalyticsData(mockData);
    } catch (error) {
      console.error('Failed to fetch analytics data:', error);
    } finally {
      setLoading(false);
    }
  };

  const generateMockAnalyticsData = () => {
    const days = dateRange === '7d' ? 7 : dateRange === '30d' ? 30 : 90;
    const data = [];
    
    for (let i = days - 1; i >= 0; i--) {
      const date = new Date();
      date.setDate(date.getDate() - i);
      data.push({
        date: date.toISOString().split('T')[0],
        mapInteractions: Math.floor(Math.random() * 50) + 10,
        authIssues: Math.floor(Math.random() * 5) + 1,
        caseSearches: Math.floor(Math.random() * 20) + 5,
        urgentAlerts: Math.floor(Math.random() * 3) + 0,
        userActions: Math.floor(Math.random() * 100) + 20
      });
    }

    return {
      mapInteractions: data,
      authIssues: data,
      caseSearches: data,
      urgentAlerts: data,
      performanceMetrics: [
        { endpoint: '/api/auth/login', avgResponseTime: 245, successRate: 98.5 },
        { endpoint: '/api/cases/search', avgResponseTime: 189, successRate: 99.2 },
        { endpoint: '/api/notifications/urgent-alert', avgResponseTime: 156, successRate: 99.8 },
        { endpoint: '/api/map/interactions', avgResponseTime: 89, successRate: 99.9 }
      ],
      userActions: data
    };
  };

  const mapInteractionsChartData = {
    labels: analyticsData.mapInteractions.map(d => d.date),
    datasets: [
      {
        label: 'Map Interactions',
        data: analyticsData.mapInteractions.map(d => d.mapInteractions),
        borderColor: 'rgb(59, 130, 246)',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        tension: 0.4
      }
    ]
  };

  const authIssuesChartData = {
    labels: analyticsData.authIssues.map(d => d.date),
    datasets: [
      {
        label: 'Authentication Issues',
        data: analyticsData.authIssues.map(d => d.authIssues),
        backgroundColor: 'rgba(220, 38, 38, 0.8)',
        borderColor: 'rgb(220, 38, 38)',
        borderWidth: 1
      }
    ]
  };

  const urgentAlertsChartData = {
    labels: analyticsData.urgentAlerts.map(d => d.date),
    datasets: [
      {
        label: 'Urgent Alerts',
        data: analyticsData.urgentAlerts.map(d => d.urgentAlerts),
        backgroundColor: 'rgba(239, 68, 68, 0.8)',
        borderColor: 'rgb(239, 68, 68)',
        borderWidth: 1
      }
    ]
  };

  const performanceChartData = {
    labels: analyticsData.performanceMetrics.map(m => m.endpoint.split('/').pop()),
    datasets: [
      {
        label: 'Average Response Time (ms)',
        data: analyticsData.performanceMetrics.map(m => m.avgResponseTime),
        backgroundColor: 'rgba(16, 185, 129, 0.8)',
        borderColor: 'rgb(16, 185, 129)',
        borderWidth: 1
      }
    ]
  };

  const successRateChartData = {
    labels: analyticsData.performanceMetrics.map(m => m.endpoint.split('/').pop()),
    datasets: [
      {
        label: 'Success Rate (%)',
        data: analyticsData.performanceMetrics.map(m => m.successRate),
        backgroundColor: [
          'rgba(16, 185, 129, 0.8)',
          'rgba(59, 130, 246, 0.8)',
          'rgba(245, 158, 11, 0.8)',
          'rgba(239, 68, 68, 0.8)'
        ],
        borderColor: [
          'rgb(16, 185, 129)',
          'rgb(59, 130, 246)',
          'rgb(245, 158, 11)',
          'rgb(239, 68, 68)'
        ],
        borderWidth: 1
      }
    ]
  };

  const chartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
      },
      title: {
        display: true,
        text: 'Analytics Dashboard'
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  const pieChartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'bottom',
      }
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="analytics-dashboard p-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Analytics Dashboard</h1>
        <p className="text-gray-600">Monitor system usage, performance, and user behavior</p>
      </div>

      {/* Controls */}
      <div className="mb-6 flex gap-4">
        <select
          value={dateRange}
          onChange={(e) => setDateRange(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="7d">Last 7 Days</option>
          <option value="30d">Last 30 Days</option>
          <option value="90d">Last 90 Days</option>
        </select>

        <select
          value={selectedMetric}
          onChange={(e) => setSelectedMetric(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="all">All Metrics</option>
          <option value="map">Map Interactions</option>
          <option value="auth">Authentication</option>
          <option value="alerts">Urgent Alerts</option>
          <option value="performance">Performance</option>
        </select>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center">
            <div className="p-2 bg-blue-100 rounded-lg">
              <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-1.447-.894L15 4m0 13V4m-6 3l6-3" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Total Map Interactions</p>
              <p className="text-2xl font-bold text-gray-900">
                {analyticsData.mapInteractions.reduce((sum, d) => sum + d.mapInteractions, 0)}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center">
            <div className="p-2 bg-red-100 rounded-lg">
              <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Urgent Alerts</p>
              <p className="text-2xl font-bold text-gray-900">
                {analyticsData.urgentAlerts.reduce((sum, d) => sum + d.urgentAlerts, 0)}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center">
            <div className="p-2 bg-green-100 rounded-lg">
              <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Success Rate</p>
              <p className="text-2xl font-bold text-gray-900">
                {Math.round(analyticsData.performanceMetrics.reduce((sum, m) => sum + m.successRate, 0) / analyticsData.performanceMetrics.length)}%
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center">
            <div className="p-2 bg-purple-100 rounded-lg">
              <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Active Users</p>
              <p className="text-2xl font-bold text-gray-900">
                {analyticsData.userActions.reduce((sum, d) => sum + d.userActions, 0)}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Map Interactions */}
        {(selectedMetric === 'all' || selectedMetric === 'map') && (
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-semibold mb-4">Map Interactions</h3>
            <Line data={mapInteractionsChartData} options={chartOptions} />
          </div>
        )}

        {/* Authentication Issues */}
        {(selectedMetric === 'all' || selectedMetric === 'auth') && (
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-semibold mb-4">Authentication Issues</h3>
            <Bar data={authIssuesChartData} options={chartOptions} />
          </div>
        )}

        {/* Urgent Alerts */}
        {(selectedMetric === 'all' || selectedMetric === 'alerts') && (
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-semibold mb-4">Urgent Alerts</h3>
            <Bar data={urgentAlertsChartData} options={chartOptions} />
          </div>
        )}

        {/* Performance Metrics */}
        {(selectedMetric === 'all' || selectedMetric === 'performance') && (
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-semibold mb-4">API Performance</h3>
            <Bar data={performanceChartData} options={chartOptions} />
          </div>
        )}
      </div>

      {/* Success Rate Chart */}
      {(selectedMetric === 'all' || selectedMetric === 'performance') && (
        <div className="mt-6 bg-white p-6 rounded-lg shadow-md">
          <h3 className="text-lg font-semibold mb-4">API Success Rates</h3>
          <div className="w-full max-w-2xl">
            <Doughnut data={successRateChartData} options={pieChartOptions} />
          </div>
        </div>
      )}

      {/* Real-time Stats */}
      <div className="mt-8 bg-white p-6 rounded-lg shadow-md">
        <h3 className="text-lg font-semibold mb-4">Real-time Statistics</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="text-center">
            <p className="text-sm text-gray-600">Current Active Sessions</p>
            <p className="text-2xl font-bold text-blue-600">24</p>
          </div>
          <div className="text-center">
            <p className="text-sm text-gray-600">Pending Urgent Alerts</p>
            <p className="text-2xl font-bold text-red-600">2</p>
          </div>
          <div className="text-center">
            <p className="text-sm text-gray-600">System Uptime</p>
            <p className="text-2xl font-bold text-green-600">99.9%</p>
          </div>
        </div>
      </div>

      {/* Styles */}
      <style jsx>{`
        .analytics-dashboard {
          background: #f8fafc;
          min-height: 100vh;
        }
      `}</style>
    </div>
  );
};

export default AnalyticsDashboard; 