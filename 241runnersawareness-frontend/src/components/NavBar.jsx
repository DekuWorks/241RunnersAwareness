const NavBar = () => {
    return (
      <nav className="bg-gray-800 text-white p-4 shadow">
        <div className="container mx-auto flex justify-between items-center">
          <h1 className="text-xl font-bold">241Runners Awareness</h1>
          <ul className="flex gap-4 text-sm">
            <li><a href="/">Home</a></li>
            <li><a href="/admin">Admin</a></li>
          </ul>
        </div>
      </nav>
    )
  }
  
  export default NavBar
  
  