import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { logout, reset } from '../features/auth/authSlice';
import { unifiedLogout } from '../utils/authUtils';

const NavBar = () => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user } = useSelector((state) => state.auth);

  const handleLogout = async () => {
    // Use unified logout utility
    await unifiedLogout(navigate, '/');
    
    // Also dispatch Redux logout for state cleanup
    dispatch(logout());
    dispatch(reset());
    
    setIsMenuOpen(false);
  };

  const closeMenu = () => {
    setIsMenuOpen(false);
  }

  return (
    <header>
      <h1>
        <Link to="/" style={{ color: 'white', textDecoration: 'none' }}>
          241Runners Awareness
        </Link>
      </h1>
      <nav className="navbar">
        <button 
          className="toggle-button" 
          onClick={() => setIsMenuOpen(!isMenuOpen)}
          aria-controls="nav-links"
          aria-expanded={isMenuOpen}
          aria-label="Toggle navigation"
        >
          <span></span>
          <span></span>
          <span></span>
        </button>
        <div className={`nav-links ${isMenuOpen ? 'active' : ''}`} id="nav-links">
          <Link to="/" className="home" onClick={closeMenu}>Home</Link>
          <a href="https://www.241runnersawareness.org/about_us.html" className="about" target="_blank" rel="noopener noreferrer" onClick={closeMenu}>About Us</a>
          <a href="https://linktr.ee/241Runners" className="socials" target="_blank" rel="noopener noreferrer" onClick={closeMenu}>Socials</a>
          <a href="https://usatriathlonfoundation.salsalabs.org/241RunnersAwareness/index.html" className="donate" target="_blank" rel="noopener noreferrer" onClick={closeMenu}>Donate</a>

          {user ? (
            <>
              {user.role === 'admin' && <Link to="/admin" className="login" onClick={closeMenu}>Admin</Link>}
              <button onClick={handleLogout} className="logout">
                Logout
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="login" onClick={closeMenu}>Log In</Link>
              <Link to="/register" className="signup" onClick={closeMenu}>Sign Up</Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
};

export default NavBar;
  
  