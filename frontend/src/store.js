/**
 * ============================================
 * 241 RUNNERS AWARENESS - REDUX STORE CONFIGURATION
 * ============================================
 * 
 * This file configures the Redux store using Redux Toolkit.
 * It sets up the global state management for the application.
 * 
 * Store Structure:
 * - auth: Authentication state (user info, tokens, login status)
 * - Future slices: alerts, users, cases, notifications, etc.
 * 
 * Redux Toolkit Benefits:
 * - Simplified store setup with configureStore
 * - Built-in DevTools integration
 * - Immutable update logic with Immer
 * - Action creators and reducers in one place
 */

// Redux Toolkit imports
import { configureStore } from '@reduxjs/toolkit'

// Feature slices - modular state management
import authReducer from './features/auth/authSlice'

/**
 * Redux Store Configuration
 * 
 * Creates the main Redux store with all feature reducers.
 * Each reducer manages a specific domain of the application state.
 * 
 * Current Reducers:
 * - auth: Handles user authentication, login/logout, user profile
 * 
 * Future Reducers (planned):
 * - alerts: System notifications and user alerts
 * - users: User management and profiles
 * - cases: Missing persons cases and data
 * - notifications: Real-time notifications
 * - shop: E-commerce and fundraising data
 * - dna: DNA tracking and identification data
 */
export const store = configureStore({
  reducer: {
    // Authentication state management
    auth: authReducer,
    
    // TODO: Add additional reducers as features are developed
    // alerts: alertsReducer,
    // users: usersReducer,
    // cases: casesReducer,
    // notifications: notificationsReducer,
    // shop: shopReducer,
    // dna: dnaReducer,
  },
  
  // Redux Toolkit automatically includes:
  // - Redux DevTools Extension
  // - Redux Thunk middleware
  // - Serializable state checking
  // - Immutable state updates
})
