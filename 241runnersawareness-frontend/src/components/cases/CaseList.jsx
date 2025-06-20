import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { fetchCases, deleteCase } from '../../features/cases/casesSlice';
import { toast } from 'react-toastify';

const statusColors = {
  missing: 'bg-yellow-100 text-yellow-800',
  found: 'bg-green-100 text-green-800',
  urgent: 'bg-red-100 text-red-800',
  resolved: 'bg-blue-100 text-blue-800',
  default: 'bg-gray-100 text-gray-800',
};

const CaseList = ({ onEdit }) => {
  const dispatch = useDispatch();
  const { cases, loading, error, totalPages } = useSelector((state) => state.cases);
  const { user } = useSelector((state) => state.auth);
  const [currentPage, setCurrentPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [limit] = useState(10);

  useEffect(() => {
    dispatch(fetchCases({ page: currentPage, limit, search: searchTerm, status: statusFilter }));
  }, [dispatch, currentPage, limit, searchTerm, statusFilter]);

  const handleDelete = async (caseId) => {
    try {
      await dispatch(deleteCase(caseId)).unwrap();
      toast.success('Case deleted successfully');
    } catch (err) {
      toast.error(err.message || 'Failed to delete case');
    }
  };

  const handleSearch = (e) => {
    setSearchTerm(e.target.value);
    setCurrentPage(1);
  };

  const handleStatusFilterChange = (e) => {
    setStatusFilter(e.target.value);
    setCurrentPage(1);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (error) {
    return <div className="text-red-500 text-center p-4">{error}</div>;
  }

  return (
    <div className="bg-white shadow-md rounded-lg overflow-hidden">
      <div className="p-4 flex justify-between items-center flex-wrap gap-4">
        <h2 className="text-xl font-bold">Cases</h2>
        <div className="flex items-center gap-4">
          <select
            value={statusFilter}
            onChange={handleStatusFilterChange}
            className="px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Statuses</option>
            <option value="missing">Missing</option>
            <option value="found">Found</option>
            <option value="urgent">Urgent</option>
            <option value="resolved">Resolved</option>
          </select>
          <input
            type="text"
            placeholder="Search cases..."
            value={searchTerm}
            onChange={handleSearch}
            className="px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Title</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {cases.map((caseItem) => (
            <tr key={caseItem.id}>
              <td className="px-6 py-4 whitespace-nowrap">{caseItem.title}</td>
              <td className="px-6 py-4 whitespace-nowrap">
                <span
                  className={`capitalize px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                    statusColors[caseItem.status] || statusColors.default
                  }`}
                >
                  {caseItem.status}
                </span>
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                {user?.role === 'admin' && (
                  <>
                    <button onClick={() => onEdit(caseItem)} className="text-indigo-600 hover:text-indigo-900 mr-4">
                      Edit
                    </button>
                    <button onClick={() => handleDelete(caseItem.id)} className="text-red-600 hover:text-red-900">
                      Delete
                    </button>
                  </>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {/* Pagination */}
      <div className="flex justify-center mt-6 p-4">
        <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
          <button
            onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))}
            disabled={currentPage === 1}
            className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium"
          >
            Previous
          </button>
          {[...Array(totalPages || 1)].map((_, index) => (
            <button
              key={index + 1}
              onClick={() => setCurrentPage(index + 1)}
              className={`relative inline-flex items-center px-4 py-2 border border-gray-300 bg-white text-sm font-medium ${
                currentPage === index + 1 ? 'z-10 bg-blue-50 border-blue-500 text-blue-600' : ''
              }`}
            >
              {index + 1}
            </button>
          ))}
          <button
            onClick={() => setCurrentPage((prev) => Math.min(prev + 1, totalPages))}
            disabled={currentPage === totalPages}
            className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium"
          >
            Next
          </button>
        </nav>
      </div>
    </div>
  );
};

export default CaseList; 