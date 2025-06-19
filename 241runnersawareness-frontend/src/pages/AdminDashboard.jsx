import { useSelector } from 'react-redux';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { logout } from '../features/auth/authSlice';

const AdminDashboard = () => {
  const user = useSelector((state) => state.auth.user);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

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
                to="/admin/settings"
                className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
              >
                Settings
              </Link>
            </div>
          </nav>

          {/* Main Content Area */}
          <main className="flex-1 bg-white shadow rounded-lg p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
  