import { Link, NavLink } from "react-router-dom";
import { useSelector, useDispatch } from "react-redux";
import { logout } from "../features/auth/authSlice";
import { useNavigate } from "react-router-dom";

const NavBar = () => {
  const user = useSelector((state) => state.auth.user);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate("/login");
  };

  return (
    <nav className="bg-black text-white shadow-md fixed top-0 w-full z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex items-center">
            <Link to="/" className="text-2xl font-bold text-white">
              241 Runners
            </Link>
          </div>
          <div className="flex items-center">
            <div className="hidden sm:flex sm:space-x-4">
              <Link
                to="/"
                className="text-white no-underline px-4 py-2 rounded-full border-2 border-white font-bold transition-colors duration-300 hover:bg-red-600"
              >
                Home
              </Link>
              {user?.role === "admin" && (
                <Link
                  to="/admin"
                  className="text-white no-underline px-4 py-2 rounded-full border-2 border-white font-bold transition-colors duration-300 hover:bg-red-600"
                >
                  Admin Dashboard
                </Link>
              )}
              {user && user.role !== 'admin' && (
                <NavLink to="/mycase" className="hover:underline text-white no-underline px-4 py-2 rounded-full border-2 border-white font-bold transition-colors duration-300 hover:bg-red-600">
                  My Case
                </NavLink>
              )}
            </div>
            <div className="ml-4 flex items-center">
              {user ? (
                <div className="flex items-center space-x-4">
                  <span className="text-sm">Welcome, {user.name}</span>
                  <button
                    onClick={handleLogout}
                    className="px-4 py-2 rounded-full border-2 border-white font-bold transition-colors duration-300 bg-red-600 hover:bg-red-700"
                  >
                    Logout
                  </button>
                </div>
              ) : (
                <Link
                  to="/login"
                  className="px-4 py-2 rounded-full border-2 border-white font-bold transition-colors duration-300 bg-blue-600 hover:bg-blue-700"
                >
                  Login
                </Link>
              )}
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default NavBar;
  
  