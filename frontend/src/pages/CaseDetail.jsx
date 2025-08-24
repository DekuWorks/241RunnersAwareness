import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getCase, updateCase } from '../features/cases/casesSlice';
import { fetchUpdates, addUpdate } from '../features/cases/caseUpdatesSlice';
import UpdateComposer from '../components/UpdateComposer';

const CaseDetail = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { id } = useParams();
  const [activeTab, setActiveTab] = useState('overview');
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState({});

  const { currentCase, loading, error } = useSelector((state) => state.cases);
  const { updates, loading: updatesLoading } = useSelector((state) => state.caseUpdates);
  const { user } = useSelector((state) => state.auth);

  const caseUpdates = updates[id] || [];

  useEffect(() => {
    if (id) {
      dispatch(getCase(id));
      dispatch(fetchUpdates(id));
    }
  }, [dispatch, id]);

  useEffect(() => {
    if (currentCase) {
      setEditForm({
        title: currentCase.title || '',
        description: currentCase.description || '',
        status: currentCase.status || 'active',
        riskLevel: currentCase.riskLevel || 'medium',
        lastSeenLocation: currentCase.lastSeenLocation || '',
        lastSeenDate: currentCase.lastSeenDate ? currentCase.lastSeenDate.split('T')[0] : '',
        lawEnforcementCaseNumber: currentCase.lawEnforcementCaseNumber || '',
        investigatingAgency: currentCase.investigatingAgency || '',
        isPublic: currentCase.isPublic || false,
      });
    }
  }, [currentCase]);

  const handleEditSubmit = async (e) => {
    e.preventDefault();
    try {
      await dispatch(updateCase({ id, ...editForm })).unwrap();
      setIsEditing(false);
    } catch (error) {
      console.error('Failed to update case:', error);
    }
  };

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setEditForm(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const getStatusBadge = (status) => {
    const statusConfig = {
      active: { color: 'bg-green-100 text-green-800', label: 'Active' },
      missing: { color: 'bg-yellow-100 text-yellow-800', label: 'Missing' },
      found: { color: 'bg-blue-100 text-blue-800', label: 'Found' },
      closed: { color: 'bg-gray-100 text-gray-800', label: 'Closed' },
    };

    const config = statusConfig[status] || statusConfig.closed;
    return (
      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.color}`}>
        {config.label}
      </span>
    );
  };

  const getRiskBadge = (riskLevel) => {
    const riskConfig = {
      low: { color: 'bg-green-100 text-green-800', label: 'Low' },
      medium: { color: 'bg-yellow-100 text-yellow-800', label: 'Medium' },
      high: { color: 'bg-orange-100 text-orange-800', label: 'High' },
      critical: { color: 'bg-red-100 text-red-800', label: 'Critical' },
    };

    const config = riskConfig[riskLevel] || riskConfig.medium;
    return (
      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.color}`}>
        {config.label}
      </span>
    );
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-lg shadow">
            <div className="px-4 py-5 sm:p-6">
              <div className="animate-pulse">
                <div className="h-8 bg-gray-200 rounded w-1/3 mb-4"></div>
                <div className="space-y-3">
                  {[...Array(6)].map((_, i) => (
                    <div key={i} className="h-4 bg-gray-200 rounded"></div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-lg shadow">
            <div className="px-4 py-5 sm:p-6 text-center">
              <div className="text-red-600 mb-2">⚠️</div>
              <p className="text-red-600">{error}</p>
              <Link
                to="/dashboard/cases"
                className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                Back to Cases
              </Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!currentCase) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-lg shadow">
            <div className="px-4 py-5 sm:p-6 text-center">
              <p className="text-gray-600">Case not found</p>
              <Link
                to="/dashboard/cases"
                className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                Back to Cases
              </Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div>
              <div className="flex items-center space-x-4">
                <Link
                  to="/dashboard/cases"
                  className="text-blue-600 hover:text-blue-800"
                >
                  ← Back to Cases
                </Link>
                <h1 className="text-3xl font-bold text-gray-900">
                  {currentCase.title}
                </h1>
              </div>
              <p className="mt-2 text-gray-600">
                Case #{currentCase.caseNumber} • Created {new Date(currentCase.createdAt).toLocaleDateString()}
              </p>
            </div>
            <div className="flex space-x-3">
              {currentCase.isPublic && (
                <Link
                  to={`/cases/${currentCase.publicSlug}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  🌐 Public View
                </Link>
              )}
              <button
                onClick={() => setIsEditing(!isEditing)}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                {isEditing ? 'Cancel Edit' : 'Edit Case'}
              </button>
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="mb-6">
          <nav className="flex space-x-8">
            <button
              onClick={() => setActiveTab('overview')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'overview'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Overview
            </button>
            <button
              onClick={() => setActiveTab('updates')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'updates'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Updates ({caseUpdates.length})
            </button>
          </nav>
        </div>

        {/* Tab Content */}
        {activeTab === 'overview' && (
          <div className="bg-white rounded-lg shadow">
            {isEditing ? (
              <form onSubmit={handleEditSubmit} className="p-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-2">
                      Case Title
                    </label>
                    <input
                      type="text"
                      id="title"
                      name="title"
                      value={editForm.title}
                      onChange={handleInputChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div>
                    <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-2">
                      Status
                    </label>
                    <select
                      id="status"
                      name="status"
                      value={editForm.status}
                      onChange={handleInputChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    >
                      <option value="active">Active</option>
                      <option value="missing">Missing</option>
                      <option value="found">Found</option>
                      <option value="closed">Closed</option>
                    </select>
                  </div>

                  <div>
                    <label htmlFor="riskLevel" className="block text-sm font-medium text-gray-700 mb-2">
                      Risk Level
                    </label>
                    <select
                      id="riskLevel"
                      name="riskLevel"
                      value={editForm.riskLevel}
                      onChange={handleInputChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    >
                      <option value="low">Low</option>
                      <option value="medium">Medium</option>
                      <option value="high">High</option>
                      <option value="critical">Critical</option>
                    </select>
                  </div>

                  <div>
                    <label htmlFor="lastSeenDate" className="block text-sm font-medium text-gray-700 mb-2">
                      Last Seen Date
                    </label>
                    <input
                      type="date"
                      id="lastSeenDate"
                      name="lastSeenDate"
                      value={editForm.lastSeenDate}
                      onChange={handleInputChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
                      Description
                    </label>
                    <textarea
                      id="description"
                      name="description"
                      value={editForm.description}
                      onChange={handleInputChange}
                      rows={4}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label htmlFor="lastSeenLocation" className="block text-sm font-medium text-gray-700 mb-2">
                      Last Seen Location
                    </label>
                    <input
                      type="text"
                      id="lastSeenLocation"
                      name="lastSeenLocation"
                      value={editForm.lastSeenLocation}
                      onChange={handleInputChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div>
                    <label htmlFor="lawEnforcementCaseNumber" className="block text-sm font-medium text-gray-700 mb-2">
                      Law Enforcement Case Number
                    </label>
                    <input
                      type="text"
                      id="lawEnforcementCaseNumber"
                      name="lawEnforcementCaseNumber"
                      value={editForm.lawEnforcementCaseNumber}
                      onChange={handleInputChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div>
                    <label htmlFor="investigatingAgency" className="block text-sm font-medium text-gray-700 mb-2">
                      Investigating Agency
                    </label>
                    <input
                      type="text"
                      id="investigatingAgency"
                      name="investigatingAgency"
                      value={editForm.investigatingAgency}
                      onChange={handleInputChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <div className="flex items-center">
                      <input
                        type="checkbox"
                        id="isPublic"
                        name="isPublic"
                        checked={editForm.isPublic}
                        onChange={handleInputChange}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      />
                      <label htmlFor="isPublic" className="ml-2 block text-sm text-gray-700">
                        Make this case public (visible to all users)
                      </label>
                    </div>
                  </div>
                </div>

                <div className="mt-6 flex justify-end space-x-3">
                  <button
                    type="button"
                    onClick={() => setIsEditing(false)}
                    className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 border border-transparent rounded-md text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
                  >
                    Save Changes
                  </button>
                </div>
              </form>
            ) : (
              <div className="p-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Case Information</h3>
                    <dl className="space-y-4">
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Status</dt>
                        <dd className="mt-1">{getStatusBadge(currentCase.status)}</dd>
                      </div>
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Risk Level</dt>
                        <dd className="mt-1">{getRiskBadge(currentCase.riskLevel)}</dd>
                      </div>
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Last Seen Date</dt>
                        <dd className="mt-1 text-sm text-gray-900">
                          {currentCase.lastSeenDate ? new Date(currentCase.lastSeenDate).toLocaleDateString() : 'Not specified'}
                        </dd>
                      </div>
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Last Seen Location</dt>
                        <dd className="mt-1 text-sm text-gray-900">
                          {currentCase.lastSeenLocation || 'Not specified'}
                        </dd>
                      </div>
                      {currentCase.lawEnforcementCaseNumber && (
                        <div>
                          <dt className="text-sm font-medium text-gray-500">Law Enforcement Case Number</dt>
                          <dd className="mt-1 text-sm text-gray-900">{currentCase.lawEnforcementCaseNumber}</dd>
                        </div>
                      )}
                      {currentCase.investigatingAgency && (
                        <div>
                          <dt className="text-sm font-medium text-gray-500">Investigating Agency</dt>
                          <dd className="mt-1 text-sm text-gray-900">{currentCase.investigatingAgency}</dd>
                        </div>
                      )}
                    </dl>
                  </div>

                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Description</h3>
                    <p className="text-sm text-gray-700 whitespace-pre-wrap">
                      {currentCase.description}
                    </p>
                  </div>
                </div>

                {currentCase.individual && (
                  <div className="mt-8">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Associated Individual</h3>
                    <div className="bg-gray-50 rounded-lg p-4">
                      <dl className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                          <dt className="text-sm font-medium text-gray-500">Name</dt>
                          <dd className="mt-1 text-sm text-gray-900">{currentCase.individual.name}</dd>
                        </div>
                        <div>
                          <dt className="text-sm font-medium text-gray-500">Age</dt>
                          <dd className="mt-1 text-sm text-gray-900">{currentCase.individual.age}</dd>
                        </div>
                      </dl>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        )}

        {activeTab === 'updates' && (
          <div className="space-y-6">
            {/* Add Update */}
            <div className="bg-white rounded-lg shadow">
              <div className="p-6">
                <h3 className="text-lg font-medium text-gray-900 mb-4">Add Update</h3>
                <UpdateComposer caseId={id} />
              </div>
            </div>

            {/* Updates List */}
            <div className="bg-white rounded-lg shadow">
              <div className="p-6">
                <h3 className="text-lg font-medium text-gray-900 mb-4">Case Updates</h3>
                
                {updatesLoading ? (
                  <div className="animate-pulse space-y-4">
                    {[...Array(3)].map((_, i) => (
                      <div key={i} className="border-l-4 border-gray-200 pl-4">
                        <div className="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
                        <div className="h-3 bg-gray-200 rounded w-3/4"></div>
                      </div>
                    ))}
                  </div>
                ) : caseUpdates.length === 0 ? (
                  <div className="text-center py-8">
                    <div className="text-gray-500 mb-2">📝</div>
                    <p className="text-gray-500">No updates yet. Add the first update above.</p>
                  </div>
                ) : (
                  <div className="space-y-6">
                    {caseUpdates.map((update) => (
                      <div key={update.id} className="border-l-4 border-blue-500 pl-4">
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center space-x-2 mb-2">
                              <h4 className="text-sm font-medium text-gray-900">{update.title}</h4>
                              {update.isUrgent && (
                                <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-red-100 text-red-800">
                                  Urgent
                                </span>
                              )}
                            </div>
                            <p className="text-sm text-gray-700 mb-2 whitespace-pre-wrap">
                              {update.content}
                            </p>
                            <div className="flex items-center space-x-4 text-xs text-gray-500">
                              <span>📅 {new Date(update.updateDate).toLocaleDateString()}</span>
                              {update.location && (
                                <span>📍 {update.location}</span>
                              )}
                              <span>👤 {update.createdByUser?.firstName || 'Unknown'}</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default CaseDetail;
