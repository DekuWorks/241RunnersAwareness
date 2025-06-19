import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { settingsAPI } from '../../services/api';
import { toast } from 'react-toastify';

// Async thunks
export const fetchAdminData = createAsyncThunk(
  'admin/fetchData',
  async (_, { rejectWithValue }) => {
    try {
      const response = await settingsAPI.getSettings();
      return response.data;
    } catch (error) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch admin data');
    }
  }
);

export const updateAdminSettings = createAsyncThunk(
  'admin/updateSettings',
  async (settings, { rejectWithValue }) => {
    try {
      const response = await settingsAPI.updateSettings(settings);
      toast.success('Settings updated successfully');
      return response.data;
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to update settings');
      return rejectWithValue(error.response?.data?.message || 'Failed to update settings');
    }
  }
);

// Slice
const adminSlice = createSlice({
  name: 'admin',
  initialState: {
    data: null,
    loading: false,
    error: null,
  },
  reducers: {
    setAdminData: (state, action) => {
      state.data = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchAdminData.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAdminData.fulfilled, (state, action) => {
        state.loading = false;
        state.data = action.payload;
        state.error = null;
      })
      .addCase(fetchAdminData.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateAdminSettings.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateAdminSettings.fulfilled, (state, action) => {
        state.loading = false;
        state.data = action.payload;
        state.error = null;
      })
      .addCase(updateAdminSettings.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { setAdminData } = adminSlice.actions;
export default adminSlice.reducer; 