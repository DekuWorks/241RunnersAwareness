import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";

// Create async thunk for login
export const loginAsync = createAsyncThunk(
  'auth/loginAsync',
  async ({ email, password }, { rejectWithValue }) => {
    try {
      // Replace this with your actual API call
      const response = await fetch('https://api.241runnersawareness.org/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        const error = await response.json();
        return rejectWithValue(error.message || 'Login failed');
      }

      const data = await response.json();
      return data;
    } catch (error) {
      return rejectWithValue(error.message || 'Login failed');
    }
  }
);

const initialState = {
  user: null,
  loading: false,
  error: null,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    logout: (state) => {
      state.user = null;
      state.error = null;
      state.loading = false;
      // Clear any stored tokens or user data here
      localStorage.removeItem('token');
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(loginAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.user = action.payload;
        state.error = null;
        // Store the token if it's included in the response
        if (action.payload.token) {
          localStorage.setItem('token', action.payload.token);
        }
      })
      .addCase(loginAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Login failed';
        state.user = null;
      });
  },
});

export const { logout } = authSlice.actions;
export default authSlice.reducer;

