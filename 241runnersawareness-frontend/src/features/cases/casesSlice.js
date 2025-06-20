import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { casesAPI } from '../../services/api';
import { toast } from 'react-toastify';

// Async thunks
export const fetchCases = createAsyncThunk(
  'cases/fetchCases',
  async (params, { rejectWithValue }) => {
    try {
      const response = await casesAPI.getCases(params);
      return response.data;
    } catch (error) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch cases');
    }
  }
);

export const createCase = createAsyncThunk(
  'cases/createCase',
  async (caseData, { rejectWithValue }) => {
    try {
      const response = await casesAPI.createCase(caseData);
      toast.success('Case created successfully');
      return response.data;
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to create case');
      return rejectWithValue(error.response?.data?.message || 'Failed to create case');
    }
  }
);

export const updateCase = createAsyncThunk(
  'cases/updateCase',
  async ({ id, caseData }, { rejectWithValue }) => {
    try {
      const response = await casesAPI.updateCase(id, caseData);
      toast.success('Case updated successfully');
      return response.data;
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to update case');
      return rejectWithValue(error.response?.data?.message || 'Failed to update case');
    }
  }
);

export const deleteCase = createAsyncThunk(
  'cases/deleteCase',
  async (id, { rejectWithValue }) => {
    try {
      await casesAPI.deleteCase(id);
      toast.success('Case deleted successfully');
      return id;
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to delete case');
      return rejectWithValue(error.response?.data?.message || 'Failed to delete case');
    }
  }
);

export const updateCaseStatus = createAsyncThunk(
  'cases/updateCaseStatus',
  async ({ id, status }, { rejectWithValue }) => {
    try {
      const response = await casesAPI.updateCaseStatus(id, { status });
      toast.success('Case status updated successfully');
      return response.data;
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to update case status');
      return rejectWithValue(error.response?.data?.message || 'Failed to update case status');
    }
  }
);

// Slice
const casesSlice = createSlice({
  name: 'cases',
  initialState: {
    cases: [],
    loading: false,
    error: null,
    totalPages: 1,
  },
  reducers: {},
  extraReducers: (builder) => {
    builder
      // Fetch cases
      .addCase(fetchCases.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchCases.fulfilled, (state, action) => {
        state.loading = false;
        state.cases = action.payload.cases;
        state.totalPages = action.payload.totalPages;
        state.error = null;
      })
      .addCase(fetchCases.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      // Create case
      .addCase(createCase.fulfilled, (state, action) => {
        state.cases.push(action.payload);
      })
      // Update case
      .addCase(updateCase.fulfilled, (state, action) => {
        const index = state.cases.findIndex((c) => c.id === action.payload.id);
        if (index !== -1) {
          state.cases[index] = action.payload;
        }
      })
      // Delete case
      .addCase(deleteCase.fulfilled, (state, action) => {
        state.cases = state.cases.filter((c) => c.id !== action.payload);
      })
      // Update case status
      .addCase(updateCaseStatus.fulfilled, (state, action) => {
        const index = state.cases.findIndex((c) => c.id === action.payload.id);
        if (index !== -1) {
          state.cases[index] = action.payload;
        }
      });
  },
});

export default casesSlice.reducer; 