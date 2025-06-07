import { Outlet, Link } from 'react-router-dom';

const Layout = () => {
  return (
    <>
      <nav style={{ padding: '1rem', background: '#f8f9fa' }}>
        <Link to="/">Home</Link> | <Link to="/admin">Admin</Link>
      </nav>
      <main style={{ padding: '2rem' }}>
        <Outlet />
      </main>
    </>
  );
};

export default Layout;
