import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { auditAPI } from '../../services/api';

// Async thunks
export const fetchAuditLogs = createAsyncThunk(
  'audit/fetchLogs',
  async (params, { rejectWithValue }) => {
    try {
      const response = await auditAPI.getLogs(params);
      return response.data;
    } catch (error) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch audit logs');
    }
  }
);

// Slice
const auditLogSlice = createSlice({
  name: 'audit',
  initialState: {
    logs: [],
    loading: false,
    error: null,
    totalPages: 1,
  },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchAuditLogs.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAuditLogs.fulfilled, (state, action) => {
        state.loading = false;
        state.logs = action.payload.logs;
        state.totalPages = action.payload.totalPages;
        state.error = null;
      })
      .addCase(fetchAuditLogs.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export default auditLogSlice.reducer; 