import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Link } from 'react-router-dom';
import { listIndividuals, selectIndividuals, selectIndividualsStatus, selectIndividualsError } from '../features/individuals/individualsSlice';

const RunnersListPage = () => {
  const dispatch = useDispatch();
  const individuals = useSelector(selectIndividuals);
  const status = useSelector(selectIndividualsStatus);
  const error = useSelector(selectIndividualsError);
  
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  useEffect(() => {
    dispatch(listIndividuals({ page: 1, pageSize: 50 }));
  }, [dispatch]);

  const filteredIndividuals = individuals.filter(individual => {
    const matchesSearch = individual.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         individual.runnerId?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || individual.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleSearch = (e) => {
    setSearchTerm(e.target.value);
  };

  const handleStatusFilter = (e) => {
    setStatusFilter(e.target.value);
  };

  if (status === 'loading') {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="animate-pulse">
            <div className="h-8 bg-gray-200 rounded w-1/4 mb-4"></div>
            <div className="h-4 bg-gray-200 rounded w-1/2 mb-8"></div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {[1, 2, 3, 4, 5, 6].map(i => (
                <div key={i} className="bg-white rounded-lg shadow p-6">
                  <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </div>
              ))}
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
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
            {error}
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
              <h1 className="text-3xl font-bold text-gray-900">My Runners</h1>
              <p className="mt-2 text-gray-600">
                Manage your runner profiles and track their information.
              </p>
            </div>
            <Link
              to="/runners/new"
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
            >
              Add New Runner
            </Link>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg shadow p-6 mb-8">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-2">
                Search Runners
              </label>
              <input
                type="text"
                id="search"
                value={searchTerm}
                onChange={handleSearch}
                placeholder="Search by name or runner ID..."
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <div>
              <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-2">
                Filter by Status
              </label>
              <select
                id="status"
                value={statusFilter}
                onChange={handleStatusFilter}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Missing">Missing</option>
              </select>
            </div>
          </div>
        </div>

        {/* Runners Grid */}
        {filteredIndividuals.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-12 text-center">
            <div className="text-gray-500 mb-4">
              <span className="text-6xl">👤</span>
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              {searchTerm || statusFilter ? 'No runners found' : 'No runners yet'}
            </h3>
            <p className="text-gray-500 mb-6">
              {searchTerm || statusFilter 
                ? 'Try adjusting your search or filter criteria.'
                : 'Get started by adding your first runner profile.'
              }
            </p>
            <Link
              to="/runners/new"
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
            >
              Add New Runner
            </Link>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredIndividuals.map((individual) => (
              <Link
                key={individual.id}
                to={`/runners/${individual.id}`}
                className="bg-white rounded-lg shadow hover:shadow-lg transition-shadow duration-200"
              >
                <div className="p-6">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-medium text-gray-900 truncate">
                      {individual.fullName}
                    </h3>
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${
                      individual.status === 'Active' ? 'bg-green-100 text-green-800' :
                      individual.status === 'Missing' ? 'bg-red-100 text-red-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      {individual.status}
                    </span>
                  </div>
                  
                  <div className="space-y-2 text-sm text-gray-600">
                    {individual.runnerId && (
                      <p><span className="font-medium">Runner ID:</span> {individual.runnerId}</p>
                    )}
                    {individual.city && individual.state && (
                      <p><span className="font-medium">Location:</span> {individual.city}, {individual.state}</p>
                    )}
                    {individual.casesCount > 0 && (
                      <p><span className="font-medium">Cases:</span> {individual.casesCount}</p>
                    )}
                    <p><span className="font-medium">Added:</span> {new Date(individual.createdAt).toLocaleDateString()}</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default RunnersListPage;
