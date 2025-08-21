/**
 * ============================================
 * 241 RUNNERS AWARENESS - MAIN REACT APPLICATION
 * ============================================
 * 
 * This is the primary React application component that handles routing
 * and navigation for the 241 Runners Awareness platform.
 * 
 * Application Structure:
 * - Public pages: Home, About, Cases, Map, DNA Tracking, Privacy, Terms, Auth
 * - Protected admin routes with role-based access control
 * - Responsive layout with navigation components
 * - Progressive Web App capabilities
 * 
 * Key Features:
 * - React Router for client-side navigation
 * - Protected routes for admin functionality
 * - Nested routing for admin dashboard
 * - Fallback 404 handling
 */

// React Router imports for navigation and routing
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";

// Layout and navigation components
import Layout from "./components/Layout";
import ProtectedRoute from "./components/ProtectedRoute";

// Public page components - accessible to all users
import Home from "./pages/Home";
import About from "./pages/About";
import Cases from "./pages/Cases";
import Map from "./pages/Map";
import DNATracking from "./pages/DNATracking";
import Privacy from "./pages/Privacy";
import Terms from "./pages/Terms";
import Offline from "./pages/Offline";
import RegisterPage from "./pages/RegisterPage";
import LoginPage from "./pages/LoginPage";
import ForgotPassword from "./pages/ForgotPassword";
import ResetPassword from "./pages/ResetPassword";

// Admin page components - protected by authentication and role
import AdminDashboard from "./pages/AdminDashboard";
import UsersPage from "./pages/UsersPage";
import SettingsPage from "./pages/SettingsPage";

// Utility components
import NotFound from "./pages/NotFound.jsx";

/**
 * Main App Component
 * 
 * Sets up the routing structure for the entire application.
 * Uses React Router v6 with nested routes and protected access.
 */
function App() {
  return (
    <Router>
      <Routes>
        {/* 
          ============================================
          MAIN LAYOUT ROUTE
          ============================================
          
          All routes are wrapped in the Layout component which provides:
          - Consistent navigation header
          - Footer
          - Responsive design wrapper
          - Authentication state management
        */}
        <Route path="/" element={<Layout />}>
          
          {/* 
            ============================================
            PUBLIC ROUTES
            ============================================
            
            These routes are accessible to all users without authentication.
            They provide the core functionality of the platform.
          */}
          
          {/* Home page - Landing page with mission and features */}
          <Route index element={<Home />} />
          
          {/* About page - Information about the organization */}
          <Route path="about" element={<About />} />
          
          {/* Cases page - Display missing persons cases */}
          <Route path="cases" element={<Cases />} />
          
          {/* Map page - Interactive map with case locations */}
          <Route path="map" element={<Map />} />
          
          {/* DNA Tracking page - Information about DNA services */}
          <Route path="dna-tracking" element={<DNATracking />} />
          
          {/* Legal pages - Privacy policy and terms of use */}
          <Route path="privacy" element={<Privacy />} />
          <Route path="terms" element={<Terms />} />
          
          {/* Offline page - Fallback when user is offline */}
          <Route path="offline" element={<Offline />} />
          
          {/* Authentication pages - User registration and login */}
          <Route path="register" element={<RegisterPage />} />
          <Route path="login" element={<LoginPage />} />
          <Route path="forgot-password" element={<ForgotPassword />} />
          <Route path="reset-password" element={<ResetPassword />} />

          {/* 
            ============================================
            PROTECTED ADMIN ROUTES
            ============================================
            
            These routes require authentication and admin role.
            ProtectedRoute component handles access control.
          */}
          <Route
            path="admin"
            element={
              <ProtectedRoute role="admin">
                <AdminDashboard />
              </ProtectedRoute>
            }
          >
            {/* 
              Nested admin routes within the dashboard
              These provide specific admin functionality
            */}
            
            {/* User management - View and manage user accounts */}
            <Route path="users" element={<UsersPage />} />
            
            {/* Settings - Application configuration and preferences */}
            <Route path="settings" element={<SettingsPage />} />
          </Route>

          {/* 
            ============================================
            FALLBACK ROUTE
            ============================================
            
            Catches all unmatched routes and displays 404 page.
            Must be the last route in the configuration.
          */}
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
