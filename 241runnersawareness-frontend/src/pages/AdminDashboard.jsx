import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { fetchCases, deleteCase, createCase, updateCase, updateCaseStatus } from '../features/cases/casesSlice';
import { toast } from 'react-toastify';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { logout } from '../features/auth/authSlice';
import CaseList from '../../components/cases/CaseList';
import CaseForm from '../../components/cases/CaseForm';

const AdminDashboard = () => {
  const user = useSelector((state) => state.auth.user);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { cases, loading, error, totalPages } = useSelector((state) => state.cases);
  const [currentPage, setCurrentPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [limit] = useState(10);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedCase, setSelectedCase] = useState(null);

  useEffect(() => {
    dispatch(fetchCases({ page: currentPage, limit, search: searchTerm }));
  }, [dispatch, currentPage, limit, searchTerm]);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

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
    setCurrentPage(1); // Reset to first page when searching
  };

  const handleOpenModal = (caseData = null) => {
    setSelectedCase(caseData);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setSelectedCase(null);
    setIsModalOpen(false);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-red-500 text-center p-4">
        {error}
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Dashboard Header */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <h1 className="text-3xl font-bold text-gray-900">Admin Dashboard</h1>
          <div className="flex items-center space-x-4">
            <span className="text-gray-700">Welcome, {user?.name}</span>
            <button
              onClick={handleLogout}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
            >
              Logout
            </button>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="flex">
          {/* Sidebar Navigation */}
          <nav className="w-64 bg-white shadow rounded-lg p-6 mr-8">
            <div className="space-y-4">
              <Link
                to="/admin"
                className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
              >
                Dashboard Home
              </Link>
              <Link
                to="/admin/users"
                className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
              >
                Manage Users
              </Link>
              <Link
                to="/admin/cases"
                className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
              >
                Cases
              </Link>
              <Link
                to="/admin/audit-logs"
                className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
              >
                Audit Logs
              </Link>
              <Link
                to="/admin/settings"
                className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
              >
                Settings
              </Link>
            </div>
          </nav>

          {/* Main Content Area */}
          <main className="flex-1 bg-white shadow rounded-lg p-6">
            <div className="flex justify-between items-center mb-6">
              <h1 className="text-2xl font-bold">Cases</h1>
              <div className="flex gap-4">
                <input
                  type="text"
                  placeholder="Search cases..."
                  value={searchTerm}
                  onChange={handleSearch}
                  className="px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            <div className="flex justify-end mb-6">
              {user?.role === 'admin' && (
                <button
                  onClick={() => handleOpenModal()}
                  className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
                >
                  Add Case
                </button>
              )}
            </div>

            <CaseList onEdit={handleOpenModal} />

            <CaseForm
              isOpen={isModalOpen}
              onClose={handleCloseModal}
              caseData={selectedCase}
            />

            {/* Pagination */}
            <div className="flex justify-center mt-6">
              <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                <button
                  onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))}
                  disabled={currentPage === 1}
                  className={`relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium ${
                    currentPage === 1 ? 'text-gray-300' : 'text-gray-500 hover:bg-gray-50'
                  }`}
                >
                  Previous
                </button>
                {[...Array(totalPages)].map((_, index) => (
                  <button
                    key={index + 1}
                    onClick={() => setCurrentPage(index + 1)}
                    className={`relative inline-flex items-center px-4 py-2 border border-gray-300 bg-white text-sm font-medium ${
                      currentPage === index + 1
                        ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                        : 'text-gray-500 hover:bg-gray-50'
                    }`}
                  >
                    {index + 1}
                  </button>
                ))}
                <button
                  onClick={() => setCurrentPage((prev) => Math.min(prev + 1, totalPages))}
                  disabled={currentPage === totalPages}
                  className={`relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium ${
                    currentPage === totalPages ? 'text-gray-300' : 'text-gray-500 hover:bg-gray-50'
                  }`}
                >
                  Next
                </button>
              </nav>
            </div>
          </main>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
  