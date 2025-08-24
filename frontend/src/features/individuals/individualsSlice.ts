import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { apiClient } from '../../lib/api';

// Types
export interface Individual {
  id: number;
  runnerId: string;
  fullName: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  dateOfBirth?: string;
  age?: number;
  gender?: string;
  status: string;
  city?: string;
  state?: string;
  lastSeenDate?: string;
  lastSeenLocation?: string;
  createdAt: string;
  updatedAt: string;
  casesCount: number;
  primaryPhoto?: {
    id: number;
    imageUrl: string;
    caption?: string;
    isPrimary: boolean;
    uploadedAt: string;
  };
  ownerUserId?: string;
  ownerName?: string;
}

export interface IndividualDetail extends Individual {
  address?: string;
  zipCode?: string;
  phoneNumber?: string;
  email?: string;
  height?: string;
  weight?: string;
  hairColor?: string;
  eyeColor?: string;
  identifyingMarks?: string;
  medicalConditions?: string;
  medications?: string;
  allergies?: string;
  emergencyContacts?: string;
  photos: Array<{
    id: number;
    imageUrl: string;
    caption?: string;
    imageType: string;
    isPrimary: boolean;
    uploadedAt: string;
  }>;
  activities: Array<{
    id: number;
    activityType: string;
    title: string;
    description?: string;
    location?: string;
    latitude?: number;
    longitude?: number;
    createdAt: string;
  }>;
  cases: Array<{
    id: number;
    caseNumber: string;
    title: string;
    status: string;
    createdAt: string;
  }>;
}

export interface IndividualSearchRequest {
  q?: string;
  status?: string;
  runnerId?: string;
  page?: number;
  pageSize?: number;
}

export interface IndividualSearchResponse {
  individuals: Individual[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface IndividualsState {
  individuals: Individual[];
  currentIndividual: IndividualDetail | null;
  searchResponse: IndividualSearchResponse | null;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}

const initialState: IndividualsState = {
  individuals: [],
  currentIndividual: null,
  searchResponse: null,
  status: 'idle',
  error: null,
};

// Async thunks
export const listIndividuals = createAsyncThunk(
  'individuals/listIndividuals',
  async (params: IndividualSearchRequest, { rejectWithValue }) => {
    try {
      const queryParams = new URLSearchParams();
      if (params.q) queryParams.append('q', params.q);
      if (params.status) queryParams.append('status', params.status);
      if (params.runnerId) queryParams.append('runnerId', params.runnerId);
      if (params.page) queryParams.append('page', params.page.toString());
      if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString());

      const response = await apiClient.get(`/individuals?${queryParams.toString()}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch individuals');
    }
  }
);

export const getIndividual = createAsyncThunk(
  'individuals/getIndividual',
  async (id: number, { rejectWithValue }) => {
    try {
      const response = await apiClient.get(`/individuals/${id}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch individual');
    }
  }
);

export const createIndividual = createAsyncThunk(
  'individuals/createIndividual',
  async (payload: any, { rejectWithValue }) => {
    try {
      const response = await apiClient.post('/individuals', payload);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create individual');
    }
  }
);

export const updateIndividual = createAsyncThunk(
  'individuals/updateIndividual',
  async ({ id, payload }: { id: number; payload: any }, { rejectWithValue }) => {
    try {
      const response = await apiClient.put(`/individuals/${id}`, payload);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update individual');
    }
  }
);

// Slice
const individualsSlice = createSlice({
  name: 'individuals',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    clearCurrentIndividual: (state) => {
      state.currentIndividual = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // List Individuals
      .addCase(listIndividuals.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(listIndividuals.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.searchResponse = action.payload;
        state.individuals = action.payload.individuals;
      })
      .addCase(listIndividuals.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload as string;
      })
      // Get Individual
      .addCase(getIndividual.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(getIndividual.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.currentIndividual = action.payload;
      })
      .addCase(getIndividual.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload as string;
      })
      // Create Individual
      .addCase(createIndividual.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(createIndividual.fulfilled, (state, action) => {
        state.status = 'succeeded';
        // Add to list if it's not already there
        const exists = state.individuals.find(i => i.id === action.payload.id);
        if (!exists) {
          state.individuals.unshift(action.payload);
        }
      })
      .addCase(createIndividual.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload as string;
      })
      // Update Individual
      .addCase(updateIndividual.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(updateIndividual.fulfilled, (state, action) => {
        state.status = 'succeeded';
        // Update in list
        const index = state.individuals.findIndex(i => i.id === action.payload.id);
        if (index !== -1) {
          state.individuals[index] = action.payload;
        }
        // Update current individual if it's the same one
        if (state.currentIndividual?.id === action.payload.id) {
          state.currentIndividual = action.payload;
        }
      })
      .addCase(updateIndividual.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload as string;
      });
  },
});

export const { clearError, clearCurrentIndividual } = individualsSlice.actions;

// Selectors
export const selectIndividuals = (state: { individuals: IndividualsState }) => state.individuals.individuals;
export const selectCurrentIndividual = (state: { individuals: IndividualsState }) => state.individuals.currentIndividual;
export const selectIndividualsStatus = (state: { individuals: IndividualsState }) => state.individuals.status;
export const selectIndividualsError = (state: { individuals: IndividualsState }) => state.individuals.error;
export const selectIndividualsSearchResponse = (state: { individuals: IndividualsState }) => state.individuals.searchResponse;

export const selectMyIndividuals = (state: { individuals: IndividualsState }) => 
  state.individuals.individuals;

export const selectIndividualById = (state: { individuals: IndividualsState }, id: number) => 
  state.individuals.individuals.find(i => i.id === id);

export default individualsSlice.reducer;
