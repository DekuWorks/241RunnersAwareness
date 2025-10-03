import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'https://241runners-api-v2.azurewebsites.net/api/v1';

// Types
interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  phoneNumber?: string;
  createdAt: string;
}

interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface LoginCredentials {
  email: string;
  password: string;
}

interface RegisterData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

// Initial state
const initialState: AuthState = {
  user: null,
  token: localStorage.getItem('accessToken'),
  refreshToken: localStorage.getItem('refreshToken'),
  isAuthenticated: !!localStorage.getItem('accessToken'),
  isLoading: false,
  error: null,
};

// Async thunks
export const login = createAsyncThunk(
  'auth/login',
  async (credentials: LoginCredentials, { rejectWithValue }) => {
    try {
      const response = await axios.post(`${API_URL}/auth/login`, credentials);
      const { accessToken, refreshToken, user } = response.data;
      
      // Store tokens in localStorage
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      return { accessToken, refreshToken, user };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Login failed');
    }
  }
);

export const register = createAsyncThunk(
  'auth/register',
  async (userData: RegisterData, { rejectWithValue }) => {
    try {
      const response = await axios.post(`${API_URL}/auth/register`, userData);
      const { accessToken, user } = response.data;
      
      // Store token in localStorage
      localStorage.setItem('accessToken', accessToken);
      
      return { accessToken, user };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Registration failed');
    }
  }
);

export const fetchMe = createAsyncThunk(
  'auth/fetchMe',
  async (_, { rejectWithValue }) => {
    try {
      const token = localStorage.getItem('accessToken');
      if (!token) {
        throw new Error('No access token');
      }
      
      const response = await axios.get(`${API_URL}/auth/me`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      
      return response.data.user;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user');
    }
  }
);

// Auth slice
const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.refreshToken = null;
      state.isAuthenticated = false;
      state.error = null;
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Login
      .addCase(login.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.isLoading = false;
        state.token = action.payload.accessToken;
        state.refreshToken = action.payload.refreshToken;
        state.user = action.payload.user;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(login.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Register
      .addCase(register.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.isLoading = false;
        state.token = action.payload.accessToken;
        state.user = action.payload.user;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(register.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Fetch Me
      .addCase(fetchMe.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchMe.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(fetchMe.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
        // If fetchMe fails, user might not be authenticated
        if (action.payload === 'No access token') {
          state.isAuthenticated = false;
          state.token = null;
          state.user = null;
        }
      });
  },
});

export const { logout, clearError } = authSlice.actions;
export default authSlice.reducer;
