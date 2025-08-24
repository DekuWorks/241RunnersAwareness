import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getPublicCase } from '../features/cases/casesSlice';

const PublicCaseView = () => {
  const { slug } = useParams();
  const [caseData, setCaseData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchPublicCase = async () => {
      try {
        setLoading(true);
        const response = await fetch(`/api/cases/public/${slug}`);
        
        if (!response.ok) {
          if (response.status === 404) {
            setError('Case not found or not publicly accessible');
          } else {
            setError('Failed to load case information');
          }
          return;
        }

        const data = await response.json();
        setCaseData(data);
      } catch (err) {
        setError('Failed to load case information');
        console.error('Error fetching public case:', err);
      } finally {
        setLoading(false);
      }
    };

    if (slug) {
      fetchPublicCase();
    }
  }, [slug]);

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

  const handleShare = () => {
    if (navigator.share) {
      navigator.share({
        title: caseData?.title || 'Missing Person Case',
        text: `Help us find: ${caseData?.title}`,
        url: window.location.href,
      });
    } else {
      // Fallback: copy to clipboard
      navigator.clipboard.writeText(window.location.href).then(() => {
        alert('Link copied to clipboard!');
      });
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
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
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-lg shadow">
            <div className="px-4 py-5 sm:p-6 text-center">
              <div className="text-red-600 mb-2">⚠️</div>
              <p className="text-red-600 mb-4">{error}</p>
              <Link
                to="/cases"
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                View All Cases
              </Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!caseData) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-lg shadow">
            <div className="px-4 py-5 sm:p-6 text-center">
              <p className="text-gray-600">Case not found</p>
              <Link
                to="/cases"
                className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                View All Cases
              </Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div>
              <div className="flex items-center space-x-4">
                <Link
                  to="/cases"
                  className="text-blue-600 hover:text-blue-800"
                >
                  ← Back to Cases
                </Link>
                <h1 className="text-3xl font-bold text-gray-900">
                  {caseData.title}
                </h1>
              </div>
              <p className="mt-2 text-gray-600">
                Case #{caseData.caseNumber} • Last updated {new Date(caseData.lastUpdatedAt || caseData.createdAt).toLocaleDateString()}
              </p>
            </div>
            <div className="flex space-x-3">
              <button
                onClick={handleShare}
                className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
              >
                📤 Share
              </button>
            </div>
          </div>
        </div>

        {/* Case Information */}
        <div className="bg-white rounded-lg shadow mb-6">
          <div className="p-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-4">Case Information</h3>
                <dl className="space-y-4">
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Status</dt>
                    <dd className="mt-1">{getStatusBadge(caseData.status)}</dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Risk Level</dt>
                    <dd className="mt-1">{getRiskBadge(caseData.riskLevel)}</dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Last Seen Date</dt>
                    <dd className="mt-1 text-sm text-gray-900">
                      {caseData.lastSeenDate ? new Date(caseData.lastSeenDate).toLocaleDateString() : 'Not specified'}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Last Seen Location</dt>
                    <dd className="mt-1 text-sm text-gray-900">
                      {caseData.lastSeenLocation || 'Not specified'}
                    </dd>
                  </div>
                  {caseData.lawEnforcementCaseNumber && (
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Law Enforcement Case Number</dt>
                      <dd className="mt-1 text-sm text-gray-900">{caseData.lawEnforcementCaseNumber}</dd>
                    </div>
                  )}
                  {caseData.investigatingAgency && (
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Investigating Agency</dt>
                      <dd className="mt-1 text-sm text-gray-900">{caseData.investigatingAgency}</dd>
                    </div>
                  )}
                </dl>
              </div>

              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-4">Description</h3>
                <p className="text-sm text-gray-700 whitespace-pre-wrap">
                  {caseData.description}
                </p>
              </div>
            </div>

            {caseData.individual && (
              <div className="mt-8">
                <h3 className="text-lg font-medium text-gray-900 mb-4">Missing Person Information</h3>
                <div className="bg-gray-50 rounded-lg p-4">
                  <dl className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Name</dt>
                      <dd className="mt-1 text-sm text-gray-900">{caseData.individual.name}</dd>
                    </div>
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Age</dt>
                      <dd className="mt-1 text-sm text-gray-900">{caseData.individual.age}</dd>
                    </div>
                    {caseData.individual.height && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Height</dt>
                        <dd className="mt-1 text-sm text-gray-900">{caseData.individual.height}</dd>
                      </div>
                    )}
                    {caseData.individual.weight && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Weight</dt>
                        <dd className="mt-1 text-sm text-gray-900">{caseData.individual.weight}</dd>
                      </div>
                    )}
                    {caseData.individual.hairColor && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Hair Color</dt>
                        <dd className="mt-1 text-sm text-gray-900">{caseData.individual.hairColor}</dd>
                      </div>
                    )}
                    {caseData.individual.eyeColor && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Eye Color</dt>
                        <dd className="mt-1 text-sm text-gray-900">{caseData.individual.eyeColor}</dd>
                      </div>
                    )}
                  </dl>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Public Updates */}
        {caseData.updates && caseData.updates.length > 0 && (
          <div className="bg-white rounded-lg shadow">
            <div className="p-6">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Public Updates</h3>
              
              <div className="space-y-6">
                {caseData.updates.map((update) => (
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
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {/* Contact Information */}
        <div className="bg-blue-50 rounded-lg p-6 mt-6">
          <h3 className="text-lg font-semibold text-blue-900 mb-2">Have Information?</h3>
          <p className="text-blue-700 mb-4">
            If you have any information about this case, please contact the investigating agency or use the contact information below.
          </p>
          <div className="flex flex-wrap gap-3">
            {caseData.investigatingAgency && (
              <div className="bg-white rounded-lg p-3 flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900">Investigating Agency</p>
                <p className="text-sm text-gray-600">{caseData.investigatingAgency}</p>
              </div>
            )}
            {caseData.lawEnforcementCaseNumber && (
              <div className="bg-white rounded-lg p-3 flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900">Case Number</p>
                <p className="text-sm text-gray-600">{caseData.lawEnforcementCaseNumber}</p>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="mt-8 text-center text-sm text-gray-500">
          <p>
            This case is managed by 241 Runners Awareness. 
            <Link to="/about" className="text-blue-600 hover:text-blue-800 ml-1">
              Learn more about our mission
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default PublicCaseView;
