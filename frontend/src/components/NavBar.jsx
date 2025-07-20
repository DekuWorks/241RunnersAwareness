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
      <div className="header-bar">

        <img src="/241-logo.jpg" alt="241 Runners Awareness Logo" />
        <h1>
          <Link to="/" style={{ color: 'white', textDecoration: 'none' }}>
            241Runners Awareness
          </Link>
        </h1>
      </div>
      
      <nav className="navbar">
        <button 
          className="hamburger" 
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
          <Link to="/" onClick={closeMenu}>Home</Link>
          <a href="https://www.241runnersawareness.org/about_us.html" target="_blank" rel="noopener noreferrer" onClick={closeMenu}>About Us</a>
          <Link to="/cases" onClick={closeMenu}>Cases</Link>
          <Link to="/map" onClick={closeMenu}>Map</Link>
          <Link to="/shop" onClick={closeMenu}>🛍️ Shop</Link>
          <Link to="/dna-tracking" onClick={closeMenu}>🧬 DNA</Link>
          <a href="https://linktr.ee/241Runners" target="_blank" rel="noopener noreferrer" onClick={closeMenu}>Socials</a>
          <a href="https://usatriathlonfoundation.salsalabs.org/241RunnersAwareness/index.html" target="_blank" rel="noopener noreferrer" onClick={closeMenu}>Donate</a>

          {user ? (
            <>
              {user.role === 'admin' && <Link to="/admin" onClick={closeMenu}>Admin</Link>}
              <button onClick={handleLogout}>
                Logout
              </button>
            </>
          ) : (
            <>
              <Link to="/login" onClick={closeMenu}>Log In</Link>
              <Link to="/register" onClick={closeMenu}>Sign Up</Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
};

export default NavBar;
  
  