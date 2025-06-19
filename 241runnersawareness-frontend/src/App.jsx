import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { useSelector } from "react-redux";
import Layout from "./components/Layout";
import Home from "./pages/Home";
import AdminDashboard from "./pages/AdminDashboard";
import UsersPage from "./pages/UsersPage";
import SettingsPage from "./pages/SettingsPage";
import NotFound from "./pages/NotFound";
import LoginForm from "./components/LoginForm";
import ProtectedRoute from "./components/ProtectedRoute";

function App() {
  const user = useSelector((state) => state.auth.user);

  return (
    <Router>
      <Routes>
        {/* Public Routes */}
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route 
            path="login" 
            element={user ? <Navigate to="/admin" replace /> : <LoginForm />} 
          />

          {/* Protected Admin Routes */}
          <Route
            path="admin"
            element={
              <ProtectedRoute role="admin">
                <AdminDashboard />
              </ProtectedRoute>
            }
          >
            <Route index element={<div className="p-4">
              <h2 className="text-2xl font-bold mb-4">Welcome to Admin Dashboard</h2>
              <div className="bg-white p-6 rounded-lg shadow">
                <p className="text-gray-600">Select an option from the sidebar to manage your application.</p>
              </div>
            </div>} />
            <Route path="users" element={<UsersPage />} />
            <Route path="settings" element={<SettingsPage />} />
          </Route>

          {/* 404 Route */}
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
