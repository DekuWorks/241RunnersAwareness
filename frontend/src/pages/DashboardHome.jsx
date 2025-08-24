import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Link } from 'react-router-dom';
import { fetchMyCases } from '../features/cases/casesSlice';

const DashboardHome = () => {
  const dispatch = useDispatch();
  const { user } = useSelector((state) => state.auth);
  const { cases, loading } = useSelector((state) => state.cases);
  const [stats, setStats] = useState({
    totalCases: 0,
    activeCases: 0,
    recentUpdates: 0,
  });

  useEffect(() => {
    dispatch(fetchMyCases());
  }, [dispatch]);

  useEffect(() => {
    if (cases) {
      setStats({
        totalCases: cases.length,
        activeCases: cases.filter(c => c.status === 'active' || c.status === 'missing').length,
        recentUpdates: cases.reduce((total, c) => total + (c.updatesCount || 0), 0),
      });
    }
  }, [cases]);

  const quickActions = [
    {
      title: 'Report New Case',
      description: 'Create a new missing person case',
      icon: '📝',
      link: '/dashboard/reports/new',
      color: 'bg-blue-500 hover:bg-blue-600',
    },
    {
      title: 'My Cases',
      description: 'View and manage your cases',
      icon: '📋',
      link: '/dashboard/cases',
      color: 'bg-green-500 hover:bg-green-600',
    },
    {
      title: 'View Map',
      description: 'See cases on interactive map',
      icon: '🗺️',
      link: '/map',
      color: 'bg-purple-500 hover:bg-purple-600',
    },
    {
      title: 'Update Profile',
      description: 'Edit your profile information',
      icon: '👤',
      link: '/settings',
      color: 'bg-orange-500 hover:bg-orange-600',
    },
  ];

  const recentCases = cases?.slice(0, 3) || [];

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Welcome Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            Welcome back, {user?.firstName || user?.email}!
          </h1>
          <p className="mt-2 text-gray-600">
            Here's what's happening with your cases and how you can help.
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center">
                  <span className="text-blue-600 text-lg">📊</span>
                </div>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Total Cases</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {loading ? '...' : stats.totalCases}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="w-8 h-8 bg-yellow-100 rounded-lg flex items-center justify-center">
                  <span className="text-yellow-600 text-lg">⚠️</span>
                </div>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Active Cases</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {loading ? '...' : stats.activeCases}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center">
                  <span className="text-green-600 text-lg">📝</span>
                </div>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Recent Updates</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {loading ? '...' : stats.recentUpdates}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="mb-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Quick Actions</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {quickActions.map((action) => (
              <Link
                key={action.title}
                to={action.link}
                className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow duration-200"
              >
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <span className="text-2xl">{action.icon}</span>
                  </div>
                  <div className="ml-4">
                    <h3 className="text-sm font-medium text-gray-900">{action.title}</h3>
                    <p className="text-sm text-gray-500">{action.description}</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </div>

        {/* Recent Cases */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-gray-900">Recent Cases</h2>
            <Link
              to="/dashboard/cases"
              className="text-blue-600 hover:text-blue-800 text-sm font-medium"
            >
              View all cases →
            </Link>
          </div>

          {loading ? (
            <div className="bg-white rounded-lg shadow p-6">
              <div className="animate-pulse">
                <div className="h-4 bg-gray-200 rounded w-3/4 mb-4"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2 mb-4"></div>
                <div className="h-4 bg-gray-200 rounded w-2/3"></div>
              </div>
            </div>
          ) : recentCases.length === 0 ? (
            <div className="bg-white rounded-lg shadow p-6 text-center">
              <div className="text-gray-500 mb-4">
                <span className="text-4xl">📋</span>
              </div>
              <h3 className="text-lg font-medium text-gray-900 mb-2">No cases yet</h3>
              <p className="text-gray-500 mb-4">
                Get started by reporting your first missing person case.
              </p>
              <Link
                to="/dashboard/reports/new"
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                Report New Case
              </Link>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {recentCases.map((caseItem) => (
                <Link
                  key={caseItem.id}
                  to={`/dashboard/cases/${caseItem.id}`}
                  className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow duration-200"
                >
                  <div className="flex items-start justify-between mb-2">
                    <h3 className="text-lg font-medium text-gray-900 truncate">
                      {caseItem.title}
                    </h3>
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      caseItem.status === 'active' ? 'bg-green-100 text-green-800' :
                      caseItem.status === 'missing' ? 'bg-yellow-100 text-yellow-800' :
                      caseItem.status === 'found' ? 'bg-blue-100 text-blue-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      {caseItem.status}
                    </span>
                  </div>
                  <p className="text-sm text-gray-500 mb-3 line-clamp-2">
                    {caseItem.description}
                  </p>
                  <div className="flex items-center text-xs text-gray-400">
                    <span>📅 {new Date(caseItem.createdAt).toLocaleDateString()}</span>
                    {caseItem.updatesCount > 0 && (
                      <span className="ml-4">📝 {caseItem.updatesCount} updates</span>
                    )}
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>

        {/* Help Section */}
        <div className="bg-blue-50 rounded-lg p-6">
          <h2 className="text-lg font-semibold text-blue-900 mb-2">Need Help?</h2>
          <p className="text-blue-700 mb-4">
            If you need assistance with reporting a case or have questions about the platform, 
            our support team is here to help.
          </p>
          <div className="flex flex-wrap gap-3">
            <a
              href="mailto:support@241runnersawareness.org"
              className="inline-flex items-center px-3 py-2 border border-blue-300 text-sm font-medium rounded-md text-blue-700 bg-white hover:bg-blue-50"
            >
              📧 Contact Support
            </a>
            <Link
              to="/about"
              className="inline-flex items-center px-3 py-2 border border-blue-300 text-sm font-medium rounded-md text-blue-700 bg-white hover:bg-blue-50"
            >
              ℹ️ About Us
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardHome;
