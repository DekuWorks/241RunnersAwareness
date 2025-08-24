/**
 * ============================================
 * 241 RUNNERS AWARENESS - CASE UPDATES REDUX SLICE
 * ============================================
 * 
 * This file manages all case update-related state using Redux Toolkit.
 * It handles case update CRUD operations and real-time updates.
 * 
 * Features:
 * - Fetch case updates
 * - Add new case updates
 * - Real-time update notifications
 * - Loading and error states
 * - Toast notifications
 * 
 * State Structure:
 * - updates: Array of case updates
 * - currentUpdate: Currently selected update
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
 * Fetch Case Updates
 * 
 * Retrieves all updates for a specific case.
 * 
 * @param {number} caseId - Case ID
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Array of case updates
 */
export const fetchUpdates = createAsyncThunk('caseUpdates/fetchUpdates', async (caseId, thunkAPI) => {
  try {
    const response = await axios.get(`${API_URL}/${caseId}/updates`, {
      headers: getAuthHeaders()
    });
    return { caseId, updates: response.data };
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to fetch case updates';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * Add Case Update
 * 
 * Adds a new update to a specific case.
 * 
 * @param {Object} updateData - Object containing caseId and update data
 * @param {Object} thunkAPI - Redux Toolkit thunk API
 * @returns {Promise} Created update data
 */
export const addUpdate = createAsyncThunk('caseUpdates/addUpdate', async ({ caseId, ...updateData }, thunkAPI) => {
  try {
    const response = await axios.post(`${API_URL}/${caseId}/updates`, updateData, {
      headers: getAuthHeaders()
    });
    return { caseId, update: response.data };
  } catch (error) {
    const message = (error.response?.data?.message) || error.message || 'Failed to add case update';
    return thunkAPI.rejectWithValue(message);
  }
});

/**
 * ============================================
 * REDUX SLICE
 * ============================================
 */

const initialState = {
  updates: {}, // Keyed by caseId: { caseId: [updates] }
  currentUpdate: null,
  loading: {
    fetchUpdates: false,
    addUpdate: false
  },
  error: null,
  isSuccess: false,
  toast: {
    show: false,
    message: '',
    type: 'success' // 'success', 'error', 'info', 'warning'
  }
};

const caseUpdatesSlice = createSlice({
  name: 'caseUpdates',
  initialState,
  reducers: {
    // Reset state
    reset: (state) => {
      state.updates = {};
      state.currentUpdate = null;
      state.loading = initialState.loading;
      state.error = null;
      state.isSuccess = false;
      state.toast = initialState.toast;
    },

    // Clear updates for a specific case
    clearUpdatesForCase: (state, action) => {
      const caseId = action.payload;
      delete state.updates[caseId];
    },

    // Clear current update
    clearCurrentUpdate: (state) => {
      state.currentUpdate = null;
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

    // Add update to case (optimistic update)
    addUpdateToCase: (state, action) => {
      const { caseId, update } = action.payload;
      if (!state.updates[caseId]) {
        state.updates[caseId] = [];
      }
      state.updates[caseId].unshift(update);
    },

    // Update specific update in case
    updateUpdateInCase: (state, action) => {
      const { caseId, updateId, updateData } = action.payload;
      if (state.updates[caseId]) {
        const index = state.updates[caseId].findIndex(update => update.id === updateId);
        if (index !== -1) {
          state.updates[caseId][index] = { ...state.updates[caseId][index], ...updateData };
        }
      }
    },

    // Remove update from case
    removeUpdateFromCase: (state, action) => {
      const { caseId, updateId } = action.payload;
      if (state.updates[caseId]) {
        state.updates[caseId] = state.updates[caseId].filter(update => update.id !== updateId);
      }
    },

    // Set current update
    setCurrentUpdate: (state, action) => {
      state.currentUpdate = action.payload;
    }
  },
  extraReducers: (builder) => {
    builder
      // Fetch Updates
      .addCase(fetchUpdates.pending, (state) => {
        state.loading.fetchUpdates = true;
        state.error = null;
      })
      .addCase(fetchUpdates.fulfilled, (state, action) => {
        state.loading.fetchUpdates = false;
        const { caseId, updates } = action.payload;
        state.updates[caseId] = updates;
        state.isSuccess = true;
      })
      .addCase(fetchUpdates.rejected, (state, action) => {
        state.loading.fetchUpdates = false;
        state.error = action.payload;
        state.isSuccess = false;
      })

      // Add Update
      .addCase(addUpdate.pending, (state) => {
        state.loading.addUpdate = true;
        state.error = null;
      })
      .addCase(addUpdate.fulfilled, (state, action) => {
        state.loading.addUpdate = false;
        const { caseId, update } = action.payload;
        
        // Add to updates list
        if (!state.updates[caseId]) {
          state.updates[caseId] = [];
        }
        state.updates[caseId].unshift(update);
        
        state.isSuccess = true;
        state.toast = {
          show: true,
          message: 'Case update added successfully!',
          type: 'success'
        };
      })
      .addCase(addUpdate.rejected, (state, action) => {
        state.loading.addUpdate = false;
        state.error = action.payload;
        state.isSuccess = false;
        state.toast = {
          show: true,
          message: action.payload,
          type: 'error'
        };
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
  clearUpdatesForCase,
  clearCurrentUpdate,
  clearError,
  clearSuccess,
  showToast,
  hideToast,
  addUpdateToCase,
  updateUpdateInCase,
  removeUpdateFromCase,
  setCurrentUpdate
} = caseUpdatesSlice.actions;

export default caseUpdatesSlice.reducer;

/**
 * ============================================
 * SELECTORS
 * ============================================
 */

// Select all updates for a case
export const selectUpdatesForCase = (state, caseId) => 
  state.caseUpdates.updates[caseId] || [];

// Select current update
export const selectCurrentUpdate = (state) => state.caseUpdates.currentUpdate;

// Select loading states
export const selectLoading = (state) => state.caseUpdates.loading;

// Select error
export const selectError = (state) => state.caseUpdates.error;

// Select success state
export const selectIsSuccess = (state) => state.caseUpdates.isSuccess;

// Select toast
export const selectToast = (state) => state.caseUpdates.toast;

// Select update by ID for a specific case
export const selectUpdateById = (state, caseId, updateId) => {
  const updates = state.caseUpdates.updates[caseId] || [];
  return updates.find(update => update.id === updateId);
};

// Select urgent updates for a case
export const selectUrgentUpdatesForCase = (state, caseId) => {
  const updates = state.caseUpdates.updates[caseId] || [];
  return updates.filter(update => update.isUrgent);
};

// Select public updates for a case
export const selectPublicUpdatesForCase = (state, caseId) => {
  const updates = state.caseUpdates.updates[caseId] || [];
  return updates.filter(update => update.isPublic);
};

// Select updates by type for a case
export const selectUpdatesByTypeForCase = (state, caseId, updateType) => {
  const updates = state.caseUpdates.updates[caseId] || [];
  return updates.filter(update => update.updateType === updateType);
};

// Select latest update for a case
export const selectLatestUpdateForCase = (state, caseId) => {
  const updates = state.caseUpdates.updates[caseId] || [];
  return updates.length > 0 ? updates[0] : null;
};

// Select updates count for a case
export const selectUpdatesCountForCase = (state, caseId) => {
  const updates = state.caseUpdates.updates[caseId] || [];
  return updates.length;
};

// Select all cases with updates
export const selectAllCasesWithUpdates = (state) => {
  return Object.keys(state.caseUpdates.updates);
};

// Select cases with recent updates (within last 24 hours)
export const selectCasesWithRecentUpdates = (state) => {
  const now = new Date();
  const oneDayAgo = new Date(now.getTime() - 24 * 60 * 60 * 1000);
  
  return Object.entries(state.caseUpdates.updates)
    .filter(([caseId, updates]) => {
      if (updates.length === 0) return false;
      const latestUpdate = new Date(updates[0].createdAt);
      return latestUpdate > oneDayAgo;
    })
    .map(([caseId]) => parseInt(caseId));
};
