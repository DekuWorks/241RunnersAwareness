import { Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { logout, reset } from '../features/auth/authSlice';

const NavBar = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user } = useSelector((state) => state.auth);

  const handleLogout = () => {
    dispatch(logout());
    dispatch(reset());
    navigate('/');
  };

  return (
    <header>
      <h1>
        <Link to="/" style={{ color: 'white', textDecoration: 'none' }}>
          241Runners Awareness
        </Link>
      </h1>
      <nav className="navbar">
        <div className="nav-links">
          <Link to="/" className="home">Home</Link>
          <a href="https://www.241runnersawareness.org/about_us.html" className="about" target="_blank" rel="noopener noreferrer">About Us</a>
          <a href="https://linktr.ee/241Runners" className="socials" target="_blank" rel="noopener noreferrer">Socials</a>
          <a href="https://usatriathlonfoundation.salsalabs.org/241RunnersAwareness/index.html" className="donate" target="_blank" rel="noopener noreferrer">Donate</a>

          {user ? (
            <>
              {user.role === 'admin' && <Link to="/admin" className="login">Admin</Link>}
              <button onClick={handleLogout} className="logout">
                Logout
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="login">Log In</Link>
              <Link to="/register" className="signup">Sign Up</Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
};

export default NavBar;
  
  