/**
 * ============================================
 * 241 RUNNERS AWARENESS - REACT APPLICATION ENTRY POINT
 * ============================================
 * 
 * This is the main entry point for the React application.
 * It sets up the root component, Redux store, and global providers.
 * 
 * Application Architecture:
 * - React 18 with createRoot API
 * - Redux Toolkit for state management
 * - Google OAuth for authentication
 * - Progressive Web App with service worker
 * - Strict Mode for development debugging
 * 
 * Provider Hierarchy:
 * 1. React.StrictMode - Development mode checks
 * 2. Redux Provider - Global state management
 * 3. Google OAuth Provider - Authentication services
 * 4. App Component - Main application
 */

// Core React imports
import React from 'react'
import ReactDOM from 'react-dom/client'

// Main application component
import App from './App.jsx'

// Global styles and theming
import './index.css'

// Redux state management
import { Provider } from 'react-redux'
import { store } from './store'

// Google OAuth authentication
import { GoogleOAuthProvider } from '@react-oauth/google';
import { GOOGLE_CLIENT_ID } from './config/api';

/**
 * Service Worker Registration
 * 
 * Registers the service worker for PWA functionality including:
 * - Offline caching
 * - Background sync
 * - Push notifications
 * - App shell caching
 */
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then((registration) => {
        console.log('Service Worker registered successfully:', registration);
      })
      .catch((error) => {
        console.error('Service Worker registration failed:', error);
      });
  });
}

/**
 * Application Root Setup
 * 
 * Creates the root React element and renders the application
 * with all necessary providers and global configurations.
 * 
 * Provider Stack:
 * - StrictMode: Enables additional development checks and warnings
 * - Redux Provider: Provides global state management to all components
 * - Google OAuth Provider: Enables Google Sign-In functionality
 * - App Component: Main application with routing and layout
 */
ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <Provider store={store}>
      <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
        <App />
      </GoogleOAuthProvider>
    </Provider>
  </React.StrictMode>
)
