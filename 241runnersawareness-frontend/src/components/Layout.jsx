import { Outlet } from "react-router-dom";
import NavBar from "./NavBar";

const Layout = () => {
  return (
    <div className="min-h-screen bg-gray-100">
      <NavBar />
      <main className="pt-36">
        <Outlet />
      </main>
    </div>
  );
};

export default Layout;
