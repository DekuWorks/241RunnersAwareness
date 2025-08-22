/**
 * ============================================
 * REDUX STORE CONFIGURATION
 * ============================================
 * 
 * This file configures the Redux store for the 241 Runners Awareness application.
 * It combines all reducers and applies middleware for development and production.
 * 
 * Store Structure:
 * - auth: Authentication and user state
 * - notifications: Real-time notification state
 * - ui: User interface state (loading, modals, etc.)
 * 
 * Middleware:
 * - Redux Toolkit's default middleware
 * - Redux Logger (development only)
 * - Redux Persist (optional, for state persistence)
 */

import { configureStore } from '@reduxjs/toolkit';
import authReducer from './features/auth/authSlice';

// Import other reducers as needed
// import notificationsReducer from './features/notifications/notificationsSlice';
// import uiReducer from './features/ui/uiSlice';

/**
 * Redux Store Configuration
 * 
 * Combines all reducers and applies middleware for the application.
 * Uses Redux Toolkit's configureStore for simplified setup.
 */
const store = configureStore({
  reducer: {
    // Core application state
    auth: authReducer,
    
    // Feature-specific state
    // notifications: notificationsReducer,
    // ui: uiReducer,
  },
  
  // Middleware configuration
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      // Configure serializable check for non-serializable values
      serializableCheck: {
        // Ignore specific action types that may contain non-serializable data
        ignoredActions: ['persist/PERSIST', 'persist/REHYDRATE'],
        // Ignore specific field paths in all actions
        ignoredActionPaths: ['meta.arg', 'payload.timestamp'],
        // Ignore specific field paths in state
        ignoredPaths: ['some.path.to.ignore'],
      },
    }),
  
  // Development tools configuration
  devTools: process.env.NODE_ENV !== 'production',
});

export default store;
