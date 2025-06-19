import { Link } from 'react-router-dom'

const Layout = () => {
  return (
    <div className="flex min-h-screen">
      <aside className="w-64 bg-gray-200 p-4">
        <h2 className="text-lg font-bold mb-4">Admin Panel</h2>
        <ul className="space-y-2"> // Navigation links for the admin panel
          <li><Link to="/admin">Dashboard</Link></li>
          <li><Link to="/admin/users">Users</Link></li>
          <li><Link to="/admin/settings">Settings</Link></li>
        </ul>
      </aside>

      <main className="flex-1 p-6 bg-white">
        <Outlet />
      </main>
    </div>
  )
}

export default Layout
