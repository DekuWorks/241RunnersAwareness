import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'https://241runners-api-v2.azurewebsites.net/api/v1';

// Types
interface Runner {
  id: number;
  userId: number;
  name: string;
  dateOfBirth: string;
  age: number;
  gender: string;
  status: 'Missing' | 'Found' | 'Resolved';
  physicalDescription?: string;
  medicalConditions?: string;
  medications?: string;
  allergies?: string;
  emergencyInstructions?: string;
  preferredRunningLocations?: string;
  typicalRunningTimes?: string;
  experienceLevel?: string;
  specialNeeds?: string;
  additionalNotes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  lastKnownLocation?: string;
  lastLocationUpdate?: string;
  preferredContactMethod?: string;
  profileImageUrl?: string;
  isProfileComplete: boolean;
  isVerified: boolean;
  verifiedAt?: string;
  verifiedBy?: string;
  userEmail?: string;
  userPhoneNumber?: string;
  userEmergencyContactName?: string;
  userEmergencyContactPhone?: string;
}

interface CreateRunnerData {
  name: string;
  dateOfBirth: string;
  gender: string;
  status: 'Missing' | 'Found' | 'Resolved';
  physicalDescription?: string;
  medicalConditions?: string;
  medications?: string;
  allergies?: string;
  emergencyInstructions?: string;
  preferredRunningLocations?: string;
  typicalRunningTimes?: string;
  experienceLevel?: string;
  specialNeeds?: string;
  additionalNotes?: string;
}

interface UpdateRunnerData extends Partial<CreateRunnerData> {
  id: number;
}

interface RunnersState {
  runners: Runner[];
  currentRunner: Runner | null;
  isLoading: boolean;
  error: string | null;
  total: number;
  page: number;
  pageSize: number;
  statusFilter: string | null;
  showAllRunners: boolean; // For admin toggle
}

// Initial state
const initialState: RunnersState = {
  runners: [],
  currentRunner: null,
  isLoading: false,
  error: null,
  total: 0,
  page: 1,
  pageSize: 25,
  statusFilter: null,
  showAllRunners: false,
};

// Helper function to get auth token
const getAuthToken = () => localStorage.getItem('accessToken');

// Async thunks
export const fetchRunners = createAsyncThunk(
  'runners/fetchRunners',
  async (params: { page?: number; pageSize?: number; status?: string; showAll?: boolean } = {}, { rejectWithValue }) => {
    try {
      const token = getAuthToken();
      if (!token) {
        throw new Error('No access token');
      }

      const { page = 1, pageSize = 25, status, showAll = false } = params;
      const queryParams = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });

      if (status) {
        queryParams.append('status', status);
      }

      const response = await axios.get(`${API_URL}/runner?${queryParams}`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });

      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch runners');
    }
  }
);

export const createRunner = createAsyncThunk(
  'runners/createRunner',
  async (runnerData: CreateRunnerData, { rejectWithValue }) => {
    try {
      const token = getAuthToken();
      if (!token) {
        throw new Error('No access token');
      }

      const response = await axios.post(`${API_URL}/runner`, runnerData, {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      return response.data.runner;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create runner');
    }
  }
);

export const updateRunner = createAsyncThunk(
  'runners/updateRunner',
  async (runnerData: UpdateRunnerData, { rejectWithValue }) => {
    try {
      const token = getAuthToken();
      if (!token) {
        throw new Error('No access token');
      }

      const { id, ...updateData } = runnerData;
      const response = await axios.put(`${API_URL}/runner/${id}`, updateData, {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      return response.data.runner;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update runner');
    }
  }
);

export const deleteRunner = createAsyncThunk(
  'runners/deleteRunner',
  async (id: number, { rejectWithValue }) => {
    try {
      const token = getAuthToken();
      if (!token) {
        throw new Error('No access token');
      }

      await axios.delete(`${API_URL}/runner/${id}`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });

      return id;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete runner');
    }
  }
);

// Runners slice
const runnersSlice = createSlice({
  name: 'runners',
  initialState,
  reducers: {
    setStatusFilter: (state, action: PayloadAction<string | null>) => {
      state.statusFilter = action.payload;
    },
    setShowAllRunners: (state, action: PayloadAction<boolean>) => {
      state.showAllRunners = action.payload;
    },
    setPage: (state, action: PayloadAction<number>) => {
      state.page = action.payload;
    },
    clearError: (state) => {
      state.error = null;
    },
    setCurrentRunner: (state, action: PayloadAction<Runner | null>) => {
      state.currentRunner = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch Runners
      .addCase(fetchRunners.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchRunners.fulfilled, (state, action) => {
        state.isLoading = false;
        state.runners = action.payload.runners;
        state.total = action.payload.total;
        state.page = action.payload.page;
        state.pageSize = action.payload.pageSize;
        state.error = null;
      })
      .addCase(fetchRunners.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Create Runner
      .addCase(createRunner.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createRunner.fulfilled, (state, action) => {
        state.isLoading = false;
        state.runners.unshift(action.payload);
        state.total += 1;
        state.error = null;
      })
      .addCase(createRunner.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Update Runner
      .addCase(updateRunner.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(updateRunner.fulfilled, (state, action) => {
        state.isLoading = false;
        const index = state.runners.findIndex(runner => runner.id === action.payload.id);
        if (index !== -1) {
          state.runners[index] = action.payload;
        }
        if (state.currentRunner?.id === action.payload.id) {
          state.currentRunner = action.payload;
        }
        state.error = null;
      })
      .addCase(updateRunner.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Delete Runner
      .addCase(deleteRunner.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(deleteRunner.fulfilled, (state, action) => {
        state.isLoading = false;
        state.runners = state.runners.filter(runner => runner.id !== action.payload);
        state.total -= 1;
        if (state.currentRunner?.id === action.payload) {
          state.currentRunner = null;
        }
        state.error = null;
      })
      .addCase(deleteRunner.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { setStatusFilter, setShowAllRunners, setPage, clearError, setCurrentRunner } = runnersSlice.actions;
export default runnersSlice.reducer;
