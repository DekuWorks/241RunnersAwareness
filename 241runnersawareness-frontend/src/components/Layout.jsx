import { Outlet } from 'react-router-dom'

const Layout = () => {
  return (
    <div className="flex min-h-screen">
      <aside className="w-64 bg-gray-200 p-4">
        Sidebar content here
      </aside>
      <main className="flex-1 p-6">
        <Outlet />
      </main>
    </div>
  )
}

export default Layout
