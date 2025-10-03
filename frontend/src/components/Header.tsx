import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { logout } from '../store/slices/authSlice';

const Header: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  const getRoleBadgeClass = (role: string) => {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'role-badge admin';
      case 'user':
        return 'role-badge user';
      default:
        return 'role-badge';
    }
  };

  const getRoleDisplayName = (role: string) => {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'Administrator';
      case 'user':
        return 'User';
      default:
        return role;
    }
  };

  return (
    <header className="header">
      <div className="header-container">
        <div className="header-left">
          <Link to="/profile" className="logo">
            <h1>241 Runners Awareness</h1>
          </Link>
        </div>
        
        <nav className="header-nav">
          <Link to="/profile" className="nav-link">
            Profile
          </Link>
          <Link to="/runners" className="nav-link">
            Runners
          </Link>
        </nav>

        <div className="header-right">
          {user && (
            <div className="user-info">
              <span className="user-name">
                {user.firstName} {user.lastName}
              </span>
              <span className={getRoleBadgeClass(user.role)}>
                {getRoleDisplayName(user.role)}
              </span>
            </div>
          )}
          <button onClick={handleLogout} className="logout-btn">
            Logout
          </button>
        </div>
      </div>
    </header>
  );
};

export default Header;
