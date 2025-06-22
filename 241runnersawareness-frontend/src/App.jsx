import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Layout from "./components/Layout";
import Home from "./pages/Home";
import AdminDashboard from "./pages/AdminDashboard";
import UsersPage from "./pages/UsersPage";
import SettingsPage from "./pages/SettingsPage";
import NotFound from "./pages/NotFound.jsx";
import ProtectedRoute from "./components/ProtectedRoute";
import { GoogleOAuthProvider, GoogleLogin } from '@react-oauth/google';

function App() {
  return (
    <GoogleOAuthProvider clientId="933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com">
      <div style={{ display: 'flex', justifyContent: 'center', margin: '2rem 0' }}>
        <GoogleLogin
          onSuccess={credentialResponse => {
            alert('Google Login Success!\n' + JSON.stringify(credentialResponse));
            // Here you would send credentialResponse.credential to your backend for verification
          }}
          onError={() => {
            alert('Google Login Failed');
          }}
        />
      </div>
      <Router>
        <Routes>
          {/* Public Layout with NavBar */}
          <Route path="/" element={<Layout />}>
            <Route index element={<Home />} />

            {/* Admin Route — Protected */}
            <Route
              path="admin"
              element={
                <ProtectedRoute role="admin">
                  <AdminDashboard />
                </ProtectedRoute>
              }
            >
              {/* Nested Routes inside AdminDashboard */}
              <Route path="users" element={<UsersPage />} />
              <Route path="settings" element={<SettingsPage />} />
            </Route>

            {/* Fallback */}
            <Route path="*" element={<NotFound />} />
          </Route>
        </Routes>
      </Router>
    </GoogleOAuthProvider>
  );
}

export default App;
