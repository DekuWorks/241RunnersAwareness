import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { logout } from '../features/auth/authSlice';

const AdminDashboard = () => {
  const { user } = useSelector((state) => state.auth);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  const linkStyles = "block py-3 px-4 text-white transition duration-200 hover:bg-red-700";
  const activeLinkStyles = {
    backgroundColor: '#dc2626', // red-600
  };

  return (
    <div className="flex h-screen bg-gray-200">
      {/* Sidebar */}
      <div className="w-64 bg-black text-white shadow-lg">
        <div className="p-6">
          <h2 className="text-2xl font-bold">Admin Panel</h2>
        </div>
        <nav className="mt-4">
          <NavLink to="/admin" end className={linkStyles} style={({isActive}) => isActive ? activeLinkStyles : undefined}>
            Dashboard
          </NavLink>
          <NavLink to="/admin/users" className={linkStyles} style={({isActive}) => isActive ? activeLinkStyles : undefined}>
            Users
          </NavLink>
          <NavLink to="/admin/cases" className={linkStyles} style={({isActive}) => isActive ? activeLinkStyles : undefined}>
            Cases
          </NavLink>
          <NavLink to="/admin/audit-logs" className={linkStyles} style={({isActive}) => isActive ? activeLinkStyles : undefined}>
            Audit Logs
          </NavLink>
          <NavLink to="/admin/settings" className={linkStyles} style={({isActive}) => isActive ? activeLinkStyles : undefined}>
            Settings
          </NavLink>
        </nav>
      </div>

      {/* Main content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="flex justify-between items-center p-4 bg-white border-b-2 border-gray-200">
          <h1 className="text-2xl font-semibold text-gray-800">Welcome, {user?.name}</h1>
          <button
            onClick={handleLogout}
            className="px-4 py-2 bg-red-600 text-white rounded-md font-bold hover:bg-red-700 transition-colors duration-300"
          >
            Logout
          </button>
        </header>
        <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-100 p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default AdminDashboard;