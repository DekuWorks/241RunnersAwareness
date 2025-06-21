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
import AdminHome from './pages/AdminHome';
import AuditLogPage from './pages/AuditLogPage';
import CasesPage from './pages/CasesPage';
import MyCase from './pages/MyCase';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

function App() {
  const user = useSelector((state) => state.auth.user);

  return (
    <Router>
      <ToastContainer
        position="top-right"
        autoClose={5000}
        hideProgressBar={false}
        newestOnTop={false}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="light"
      />
      <Layout>
        <Routes>
          {/* Public Routes */}
          <Route path="/" element={<Layout />}>
            <Route index element={<Home />} />
            <Route 
              path="login" 
              element={user ? <Navigate to="/admin" replace /> : <LoginForm />} 
            />

            {/* Protected User Route */}
            <Route element={<ProtectedRoute />}>
              <Route path="mycase" element={<MyCase />} />
            </Route>

            {/* Protected Admin Routes */}
            <Route element={<ProtectedRoute adminOnly />}>
              <Route path="admin" element={<AdminDashboard />}>
                <Route index element={<AdminHome />} />
                <Route path="users" element={<UsersPage />} />
                <Route path="cases" element={<CasesPage />} />
                <Route path="settings" element={<SettingsPage />} />
                <Route path="audit-logs" element={<AuditLogPage />} />
              </Route>
            </Route>

            {/* 404 Route */}
            <Route path="*" element={<NotFound />} />
          </Route>
        </Routes>
      </Layout>
    </Router>
  );
}

export default App;
