import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import axios from 'axios';
import { API_BASE_URL } from '../../config/api';

const API_URL = `${API_BASE_URL}/auth`;

// Async Thunks
export const register = createAsyncThunk('auth/register', async (userData, thunkAPI) => {
  try {
    const response = await axios.post(`${API_URL}/register`, userData);
    return response.data;
  } catch (error) {
    const message = (error.response && error.response.data && error.response.data.message) || error.message || error.toString();
    return thunkAPI.rejectWithValue(message);
  }
});

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

const initialState = {
  user: localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')) : null,
  loading: false,
  error: null,
  isSuccess: false,
  twoFARequired: false,
  twoFAEmail: '',
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    reset: (state) => {
      state.loading = false;
      state.error = null;
      state.isSuccess = false;
      state.twoFARequired = false;
      state.twoFAEmail = '';
    }
  },
  extraReducers: (builder) => {
    builder
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
      .addCase(logout.fulfilled, (state) => {
        state.user = null;
      });
  },
});

export const { reset } = authSlice.actions;
export default authSlice.reducer;

