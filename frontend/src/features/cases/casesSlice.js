/**
 * ============================================
 * 241 RUNNERS AWARENESS - CASE MANAGEMENT REDUX SLICE
 * ============================================
 * 
 * This file manages all case-related state using Redux Toolkit.
 * It handles case CRUD operations, case updates, and public case viewing.
 * 
 * Features:
 * - Fetch user's cases
 * - Create new cases
 * - Update existing cases
 * - View case details
 * - Public case viewing
 * - Case updates management
 * - Loading and error states
 * - Toast notifications
 * 
 * State Structure:
 * - cases: Array of user's cases
 * - currentCase: Currently selected case details
 * - publicCase: Public case data for sharing
 * - loading: API request loading states
 * - error: Error messages
 * - isSuccess: Success state for UI feedback
 */

import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import axios from 'axios';
import { API_BASE_URL } from '../../config/api';

const API_URL = `${API_BASE_URL}/cases`;

// Helper function to get auth token
const getAuthToken = () => {
  const user = JSON.parse(localStorage.getItem('user'));
  return user?.token;
};

// Helper function to create auth headers
const getAuthHeaders = () => ({
  'Authorization': `Bearer ${getAuthToken()}`,
  'Content-Type': 'application/json'
});

/**
 * ============================================
 * ASYNC THUNKS - API OPERATIONS
 * ============================================
 */

/**
 * Fetch User's Cases
 * 
 * Retrieves all cases owned by the current user.
 * 
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Array of user's cases
 */
export const fetchMyCases = createAsyncThunk('cases/fetchMyCases', async (_, thunkAPI) => {
  try {
    const response = await axios.get(`${API_URL}/mine`, {
      headers: getAuthHeaders()
    });
    return response.data;
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to fetch cases';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Create New Case
 * 
 * Creates a new case with the provided data.
 * 
 * @param {Object} caseData - Case creation data
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Created case data
 */
export const createCase = createAsyncThunk('cases/createCase', async (caseData, thunkAPI) => {
  try {
    const response = await axios.post(API_URL, caseData, {
      headers: getAuthHeaders()
    });
    return response.data;
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to create case';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Get Case Details
 * 
 * Retrieves detailed information about a specific case.
 * 
 * @param {number} caseId - Case ID
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Case details
 */
export const getCase = createAsyncThunk('cases/getCase', async (caseId, thunkAPI) => {
  try {
    const response = await axios.get(`${API_URL}/${caseId}`, {
      headers: getAuthHeaders()
    });
    return response.data;
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to fetch case';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * List Cases by Individual
 * 
 * Retrieves all cases for a specific individual.
 * 
 * @param {number} individualId - Individual ID
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Array of cases for the individual
 */
export const listCasesByIndividual = createAsyncThunk('cases/listCasesByIndividual', async (individualId, thunkAPI) => {
  try {
    const response = await axios.get(`${API_URL}?individualId=${individualId}`, {
      headers: getAuthHeaders()
    });
    return response.data;
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to fetch cases for individual';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Update Case
 * 
 * Updates an existing case with new data.
 * 
 * @param {Object} updateData - Object containing caseId and update fields
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Updated case data
 */
export const updateCase = createAsyncThunk('cases/updateCase', async ({ caseId, ...updateData }, thunkAPI) => {
  try {
    const response = await axios.patch(`${API_URL}/${caseId}`, updateData, {
      headers: getAuthHeaders()
    });
    return response.data;
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to update case';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Get Public Case
 * 
 * Retrieves public case information by slug.
 * 
 * @param {string} slug - Public case slug
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Public case data
 */
export const getPublicCase = createAsyncThunk('cases/getPublicCase', async (slug, thunkAPI) => {
  try {
    const response = await axios.get(`${API_URL}/public/${slug}`);
    return response.data;
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to fetch public case';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * ============================================
 * REDUX SLICE
 * ============================================
 */

const initialState = {
  cases: [],
  currentCase: null,
  publicCase: null,
  loading: {
    fetchCases: false,
    createCase: false,
    getCase: false,
    updateCase: false,
    getPublicCase: false
  },
  error: null,
  isSuccess: false,
  toast: {
    show: false,
    message: '',
    type: 'success' // 'success', 'error', 'info', 'warning'
  }
};

const casesSlice = createSlice({
  name: 'cases',
  initialState,
  reducers: {
    // Reset state
    reset: (state) => {
      state.cases = [];
      state.currentCase = null;
      state.publicCase = null;
      state.loading = initialState.loading;
      state.error = null;
      state.isSuccess = false;
      state.toast = initialState.toast;
    },

    // Clear current case
    clearCurrentCase: (state) => {
      state.currentCase = null;
    },

    // Clear public case
    clearPublicCase: (state) => {
      state.publicCase = null;
    },

    // Clear error
    clearError: (state) => {
      state.error = null;
    },

    // Clear success state
    clearSuccess: (state) => {
      state.isSuccess = false;
    },

    // Show toast notification
    showToast: (state, action) => {
      state.toast = {
        show: true,
        message: action.payload.message,
        type: action.payload.type || 'success'
      };
    },

    // Hide toast notification
    hideToast: (state) => {
      state.toast.show = false;
    },

    // Update case in list (optimistic update)
    updateCaseInList: (state, action) => {
      const updatedCase = action.payload;
      const index = state.cases.findIndex(c => c.id === updatedCase.id);
      if (index !== -1) {
        state.cases[index] = { ...state.cases[index], ...updatedCase };
      }
    },

    // Add case to list (optimistic update)
    addCaseToList: (state, action) => {
      state.cases.unshift(action.payload);
    }
  },
  extraReducers: (builder) => {
    builder
      // Fetch My Cases
      .addCase(fetchMyCases.pending, (state) => {
        state.loading.fetchCases = true;
        state.error = null;
      })
      .addCase(fetchMyCases.fulfilled, (state, action) => {
        state.loading.fetchCases = false;
        state.cases = action.payload;
        state.isSuccess = true;
      })
      .addCase(fetchMyCases.rejected, (state, action) => {
        state.loading.fetchCases = false;
        state.error = action.payload;
        state.isSuccess = false;
      })

      // Create Case
      .addCase(createCase.pending, (state) => {
        state.loading.createCase = true;
        state.error = null;
      })
      .addCase(createCase.fulfilled, (state, action) => {
        state.loading.createCase = false;
        state.cases.unshift(action.payload);
        state.isSuccess = true;
        state.toast = {
          show: true,
          message: 'Case created successfully!',
          type: 'success'
        };
      })
      .addCase(createCase.rejected, (state, action) => {
        state.loading.createCase = false;
        state.error = action.payload;
        state.isSuccess = false;
        state.toast = {
          show: true,
          message: action.payload,
          type: 'error'
        };
      })

      // Get Case
      .addCase(getCase.pending, (state) => {
        state.loading.getCase = true;
        state.error = null;
      })
      .addCase(getCase.fulfilled, (state, action) => {
        state.loading.getCase = false;
        state.currentCase = action.payload;
        state.isSuccess = true;
      })
      .addCase(getCase.rejected, (state, action) => {
        state.loading.getCase = false;
        state.error = action.payload;
        state.isSuccess = false;
      })

      // Update Case
      .addCase(updateCase.pending, (state) => {
        state.loading.updateCase = true;
        state.error = null;
      })
      .addCase(updateCase.fulfilled, (state, action) => {
        state.loading.updateCase = false;
        // Update case in list
        const index = state.cases.findIndex(c => c.id === action.payload.id);
        if (index !== -1) {
          state.cases[index] = { ...state.cases[index], ...action.payload };
        }
        // Update current case if it's the same
        if (state.currentCase && state.currentCase.id === action.payload.id) {
          state.currentCase = { ...state.currentCase, ...action.payload };
        }
        state.isSuccess = true;
        state.toast = {
          show: true,
          message: 'Case updated successfully!',
          type: 'success'
        };
      })
      .addCase(updateCase.rejected, (state, action) => {
        state.loading.updateCase = false;
        state.error = action.payload;
        state.isSuccess = false;
        state.toast = {
          show: true,
          message: action.payload,
          type: 'error'
        };
      })

      // Get Public Case
      .addCase(getPublicCase.pending, (state) => {
        state.loading.getPublicCase = true;
        state.error = null;
      })
      .addCase(getPublicCase.fulfilled, (state, action) => {
        state.loading.getPublicCase = false;
        state.publicCase = action.payload;
        state.isSuccess = true;
      })
      .addCase(getPublicCase.rejected, (state, action) => {
        state.loading.getPublicCase = false;
        state.error = action.payload;
        state.isSuccess = false;
      });
  }
});

/**
 * ============================================
 * EXPORTS
 * ============================================
 */

export const {
  reset,
  clearCurrentCase,
  clearPublicCase,
  clearError,
  clearSuccess,
  showToast,
  hideToast,
  updateCaseInList,
  addCaseToList
} = casesSlice.actions;

export default casesSlice.reducer;

/**
 * ============================================
 * SELECTORS
 * ============================================
 */

// Select all cases
export const selectAllCases = (state) => state.cases.cases;

// Select current case
export const selectCurrentCase = (state) => state.cases.currentCase;

// Select public case
export const selectPublicCase = (state) => state.cases.publicCase;

// Select loading states
export const selectLoading = (state) => state.cases.loading;

// Select error
export const selectError = (state) => state.cases.error;

// Select success state
export const selectIsSuccess = (state) => state.cases.isSuccess;

// Select toast
export const selectToast = (state) => state.cases.toast;

// Select case by ID
export const selectCaseById = (state, caseId) =>
  state.cases.cases.find(c => c.id === caseId);

// Select cases by status
export const selectCasesByStatus = (state, status) =>
  state.cases.cases.filter(c => c.status === status);

// Select cases by priority
export const selectCasesByPriority = (state, priority) =>
  state.cases.cases.filter(c => c.priority === priority);

// Select urgent cases
export const selectUrgentCases = (state) =>
  state.cases.cases.filter(c => c.priority === 'High' || c.priority === 'Critical');

// Select public cases
export const selectPublicCases = (state) =>
  state.cases.cases.filter(c => c.isPublic);

// Select cases for a specific individual
export const selectCasesForIndividual = (state, individualId) =>
  state.cases.cases.filter(c => c.individualId === individualId);
