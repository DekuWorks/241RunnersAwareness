import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { logout } from '../features/auth/authSlice';

const AdminDashboard = () => {
  const { user } = useSelector((state) => state.auth);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar */}
      <div className="w-64 bg-white shadow-md">
        <div className="p-5">
          <h2 className="text-xl font-bold text-gray-800">Admin Panel</h2>
        </div>
        <nav className="mt-5">
          <Link to="/admin" className="block py-2.5 px-4 rounded transition duration-200 hover:bg-blue-500 hover:text-white">
            Dashboard
          </Link>
          <Link to="/admin/users" className="block py-2.5 px-4 rounded transition duration-200 hover:bg-blue-500 hover:text-white">
            Users
          </Link>
          <Link to="/admin/cases" className="block py-2.5 px-4 rounded transition duration-200 hover:bg-blue-500 hover:text-white">
            Cases
          </Link>
          <Link to="/admin/audit-logs" className="block py-2.5 px-4 rounded transition duration-200 hover:bg-blue-500 hover:text-white">
            Audit Logs
          </Link>
          <Link to="/admin/settings" className="block py-2.5 px-4 rounded transition duration-200 hover:bg-blue-500 hover:text-white">
            Settings
          </Link>
        </nav>
      </div>

      {/* Main content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="flex justify-between items-center p-4 bg-white border-b">
          <h1 className="text-xl">Welcome, {user?.name}</h1>
          <button
            onClick={handleLogout}
            className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
          >
            Logout
          </button>
        </header>
        <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-100 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default AdminDashboard;
  `nk`