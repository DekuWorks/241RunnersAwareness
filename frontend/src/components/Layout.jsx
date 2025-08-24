import { Outlet } from 'react-router-dom';
import NavBar from './NavBar';

const Layout = () => {
  return (
    <>
      <NavBar />
      <main>
        <Outlet />
      </main>
      <footer>
        <div className="footer-content">
          <p>&copy; 2025 241 Runners Awareness. All rights reserved.</p>
          <div className="footer-links">
            <a href="/privacy">Privacy Policy</a>
            <a href="/docs/privacy-policy.pdf" target="_blank" rel="noopener">Privacy Policy (PDF)</a>
            <a href="/terms">Terms of Use</a>
            <a href="/docs/terms-of-use.pdf" target="_blank" rel="noopener">Terms of Use (PDF)</a>
            <a href="/about">About Us</a>
          </div>
        </div>
      </footer>
    </>
  );
};

export default Layout;
