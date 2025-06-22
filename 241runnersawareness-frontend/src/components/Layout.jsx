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
        <p>&copy; 2024 241 Runners Awareness. All rights reserved.</p>
      </footer>
    </>
  );
};

export default Layout;
