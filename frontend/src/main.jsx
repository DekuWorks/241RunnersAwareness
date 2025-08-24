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
 * - SEO with react-helmet-async
 * - Strict Mode for development debugging
 * 
 * Provider Hierarchy:
 * 1. React.StrictMode - Development mode checks
 * 2. Redux Provider - Global state management
 * 3. Google OAuth Provider - Authentication services
 * 4. HelmetProvider - SEO and meta tag management
 * 5. App Component - Main application
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
import store from './store'

// Google OAuth authentication
import { GoogleOAuthProvider } from '@react-oauth/google';
import { GOOGLE_CLIENT_ID } from './config/api';

// SEO management
import { HelmetProvider } from 'react-helmet-async';
import * as Sentry from "@sentry/react";
import { BrowserTracing } from "@sentry/tracing";

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
 * - HelmetProvider: Manages SEO meta tags and document head
 * - App Component: Main application with routing and layout
 */

// Initialize Sentry for error monitoring
Sentry.init({
  dsn: "https://your-sentry-dsn@your-sentry-instance.ingest.sentry.io/your-project-id", // Replace with actual Sentry DSN
  integrations: [
    new BrowserTracing({
      tracePropagationTargets: ["localhost", "241runnersawareness.org", /^\//],
    }),
  ],
  // Performance Monitoring
  tracesSampleRate: 1.0,
  // Session Replay
  replaysSessionSampleRate: 0.1,
  replaysOnErrorSampleRate: 1.0,
  // Environment
  environment: import.meta.env.MODE,
  // Release tracking
  release: "241runners-awareness@1.0.0",
});

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <Provider store={store}>
      <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
        <HelmetProvider>
          <App />
        </HelmetProvider>
      </GoogleOAuthProvider>
    </Provider>
  </React.StrictMode>
)
