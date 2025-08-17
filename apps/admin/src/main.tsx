import React from "react"; import ReactDOM from "react-dom/client";
import { createBrowserRouter, RouterProvider, Navigate, Outlet } from "react-router-dom";
import { Provider, useSelector } from "react-redux"; import { store } from "./store";
import "./index.css"; import Login from "./pages/Login"; import Dashboard from "./pages/Dashboard"; import AdminUsers from "./pages/AdminUsers";

function RequireAuth(){ const token = useSelector((s:any)=>s.auth.accessToken); return token ? <Outlet/> : <Navigate to="/login" replace/>; }
function RequireRole({roles}:{roles:string[]}){ const user = useSelector((s:any)=>s.auth.user); if(!user) return <Navigate to="/login" replace/>; const ok = user.roles?.some((r:string)=>roles.includes(r)); return ok ? <Outlet/> : <Navigate to="/dashboard" replace/>; }

const router = createBrowserRouter([
  { path:"/login", element:<Login/> },
  { element:<RequireAuth/>, children:[
    { path:"/dashboard", element:<Dashboard/> },
    { element:<RequireRole roles={["Admin","Manager"]}/>, children:[
      { path:"/admin/users", element:<AdminUsers/> }
    ] }
  ]},
  { path:"/", element:<Navigate to="/dashboard" replace/> }
]);

ReactDOM.createRoot(document.getElementById("root")!).render(
  <Provider store={store}><RouterProvider router={router}/></Provider>
);
