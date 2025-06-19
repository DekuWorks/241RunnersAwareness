import { Outlet } from "react-router-dom";
import NavBar from "./NavBar";

const Layout = () => {
  return (
    <div className="min-h-screen bg-gray-50">
      <NavBar />
      <main>
        <Outlet />
      </main>
    </div>
  );
};

export default Layout;
