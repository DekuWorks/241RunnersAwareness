/**
 * ============================================
 * 241 RUNNERS AWARENESS - AUTHENTICATION REDUX SLICE
 * ============================================
 * 
 * This file manages all authentication-related state using Redux Toolkit.
 * It handles user registration, login, logout, and multi-factor authentication.
 * 
 * Features:
 * - Traditional email/password authentication
 * - Google OAuth integration
 * - Two-factor authentication (2FA)
 * - Persistent login state with localStorage
 * - Secure token management
 * 
 * State Structure:
 * - user: Current authenticated user data
 * - loading: API request loading states
 * - error: Authentication error messages
 * - isSuccess: Success state for UI feedback
 * - twoFARequired: 2FA verification status
 * - twoFAEmail: Email for 2FA verification
 */

// Redux Toolkit imports for async operations and state management
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";

// HTTP client for API requests
import axios from 'axios';

// API configuration
import { API_BASE_URL } from '../../config/api';

// API endpoint for authentication operations
const API_URL = `${API_BASE_URL}/auth`;

/**
 * ============================================
 * ASYNC THUNKS - API OPERATIONS
 * ============================================
 * 
 * These functions handle asynchronous API calls for authentication.
 * They automatically dispatch loading, success, and error states.
 */

/**
 * User Registration
 * 
 * Registers a new user account with email verification.
 * Does not automatically log in the user after registration.
 * 
 * @param {Object} userData - User registration data (email, password, etc.)
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Registration result
 */
export const register = createAsyncThunk('auth/register', async (userData, thunkAPI) => {
  try {
    const response = await axios.post(`${API_URL}/register`, userData);
    return response.data;
  } catch (error) {
    const message = (error.response && error.response.data && error.response.data.message) || error.message || error.toString();
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Traditional Login
 * 
 * Authenticates user with email and password.
 * Supports 2FA verification flow.
 * Stores user data in localStorage for persistence.
 * 
 * @param {Object} userData - Login credentials (email, password)
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Login result with user data and token
 */
export const login = createAsyncThunk('auth/login', async (userData, thunkAPI) => {
  try {
    const response = await axios.post(`${API_URL}/login`, userData);
    if (response.data.token) {
      localStorage.setItem('user', JSON.stringify(response.data));
    }
    return response.data;
  } catch (error) {
    const message = (error.response && error.response.data && error.response.data.message) || error.message || error.toString();
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Google OAuth Login
 * 
 * Authenticates user using Google OAuth token.
 * Integrates with Google Sign-In API.
 * Stores user data in localStorage for persistence.
 * 
 * @param {Object} params - Object containing Google token
 * @param {string} params.token - Google OAuth ID token
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Login result with user data and token
 */
export const loginWithGoogle = createAsyncThunk('auth/googleLogin', async ({ token }, thunkAPI) => {
  try {
    const response = await axios.post(`${API_URL}/google-login`, { IdToken: token });
     if (response.data.token) {
      localStorage.setItem('user', JSON.stringify(response.data));
    }
    return response.data;
  } catch (error) {
    const message = (error.response && error.response.data && error.response.data.message) || error.message || error.toString();
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Two-Factor Authentication Verification
 * 
 * Completes login process when 2FA is required.
 * Verifies TOTP (Time-based One-Time Password) from authenticator app.
 * 
 * @param {Object} params - 2FA verification data
 * @param {string} params.email - User email address
 * @param {string} params.totp - TOTP code from authenticator
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Login completion result
 */
export const loginWith2FA = createAsyncThunk('auth/loginWith2FA', async ({ email, totp }, thunkAPI) => {
  try {
    const response = await axios.post(`${API_URL}/2fa/verify`, { email, totp });
    if (response.data.token) {
      localStorage.setItem('user', JSON.stringify(response.data));
    }
    return response.data;
  } catch (error) {
    const message = (error.response && error.response.data && error.response.data.message) || error.message || error.toString();
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * User Logout
 * 
 * Logs out the current user and cleans up all authentication data.
 * Revokes Google OAuth session if applicable.
 * Clears localStorage and resets state.
 * 
 * @param {void} _ - Unused parameter
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Logout result
 */
export const logout = createAsyncThunk('auth/logout', async (_, thunkAPI) => {
  try {
    // Clear local storage
    localStorage.removeItem('user');
    
    // Revoke Google session if available
    if (window.google && window.google.accounts) {
      try {
        await window.google.accounts.oauth2.revoke(localStorage.getItem('google_access_token'));
        localStorage.removeItem('google_access_token');
      } catch (error) {
        console.log('Google session revocation failed:', error);
      }
    }
    
    return { success: true };
  } catch (error) {
    return thunkAPI.rejectWithValue(error.message);
  }
});

/**
 * ============================================
 * INITIAL STATE
 * ============================================
 * 
 * Defines the initial state for the authentication slice.
 * Attempts to restore user session from localStorage on app load.
 */
const initialState = {
  // User data - restored from localStorage or null if not authenticated
  user: localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')) : null,
  
  // Loading state for API operations
  loading: false,
  
  // Error messages from failed operations
  error: null,
  
  // Success state for UI feedback
  isSuccess: false,
  
  // Two-factor authentication state
  twoFARequired: false,
  twoFAEmail: '',
};

/**
 * ============================================
 * AUTHENTICATION SLICE
 * ============================================
 * 
 * Redux Toolkit slice that manages authentication state.
 * Handles all state updates for authentication operations.
 */
const authSlice = createSlice({
  name: "auth",
  initialState,
  
  // Synchronous reducers for immediate state updates
  reducers: {
    /**
     * Reset Authentication State
     * 
     * Clears loading, error, success, and 2FA states.
     * Used to reset the slice state between operations.
     */
    reset: (state) => {
      state.loading = false;
      state.error = null;
      state.isSuccess = false;
      state.twoFARequired = false;
      state.twoFAEmail = '';
    }
  },
  
  // Async reducers for handling API operation states
  extraReducers: (builder) => {
    builder
      // Registration operation states
      .addCase(register.pending, (state) => {
        state.loading = true;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.loading = false;
        state.isSuccess = true;
        // Don't log in user on register, they need to verify
      })
      .addCase(register.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
        state.user = null;
      })
      
      // Traditional login operation states
      .addCase(login.pending, (state) => {
        state.loading = true;
        state.twoFARequired = false;
        state.twoFAEmail = '';
      })
      .addCase(login.fulfilled, (state, action) => {
        state.loading = false;
        if (action.payload && action.payload.requiresVerification && !action.payload.token) {
          // 2FA required, do not set user yet
          state.twoFARequired = true;
          state.twoFAEmail = action.meta.arg.email;
          state.user = null;
        } else {
          state.isSuccess = true;
          state.user = action.payload;
        }
      })
      .addCase(login.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
        state.user = null;
        state.twoFARequired = false;
        state.twoFAEmail = '';
      })
      
      // Google OAuth login operation states
      .addCase(loginWithGoogle.pending, (state) => {
        state.loading = true;
      })
      .addCase(loginWithGoogle.fulfilled, (state, action) => {
        state.loading = false;
        state.isSuccess = true;
        state.user = action.payload;
      })
      .addCase(loginWithGoogle.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
        state.user = null;
      })
      
      // Two-factor authentication operation states
      .addCase(loginWith2FA.pending, (state) => {
        state.loading = true;
      })
      .addCase(loginWith2FA.fulfilled, (state, action) => {
        state.loading = false;
        state.isSuccess = true;
        state.user = action.payload;
        state.twoFARequired = false;
        state.twoFAEmail = '';
      })
      .addCase(loginWith2FA.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
        state.user = null;
      })
      
      // Logout operation states
      .addCase(logout.fulfilled, (state) => {
        state.user = null;
      });
  },
});

// Export actions and reducer
export const { reset } = authSlice.actions;
export default authSlice.reducer;

