import axios from 'axios';
import { toast } from 'react-toastify';
import { store } from '../store';
import { logout } from '../features/auth/authSlice';

const BASE_URL = 'https://api.241runnersawareness.org/api';

// Create axios instance with default config
const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for handling errors globally
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Extract a meaningful error message
    const message = error.response?.data?.message || error.response?.data || error.message;
    
    // Show a toast notification
    if (message) {
      toast.error(message);
    }

    // Reject the promise to allow for specific component-level error handling if needed
    return Promise.reject(error);
  }
);

// Auth API calls
export const authAPI = {
  login: (credentials) => api.post('/auth/login', credentials),
  logout: () => api.post('/auth/logout'),
  getCurrentUser: () => api.get('/auth/me'),
  refreshToken: () => api.post('/auth/refresh'),
};

// Users API calls with pagination
export const usersAPI = {
  getUsers: (params) => api.get('/users', { 
    params: {
      page: params?.page || 1,
      limit: params?.limit || 10,
      search: params?.search,
      ...params
    }
  }),
  getUser: (id) => api.get(`/users/${id}`),
  createUser: (userData) => api.post('/users', userData),
  updateUser: (id, userData) => api.put(`/users/${id}`, userData),
  deleteUser: (id) => api.delete(`/users/${id}`),
  getMyCase: () => api.get('/individual/mycase'),
};

// Cases API calls
export const casesAPI = {
  getCases: (params) => api.get('/cases', { 
    params: {
      page: params?.page || 1,
      limit: params?.limit || 10,
      search: params?.search,
      ...params
    }
  }),
  getCase: (id) => api.get(`/cases/${id}`),
  createCase: (caseData) => api.post('/cases', caseData),
  updateCase: (id, caseData) => api.put(`/cases/${id}`, caseData),
  deleteCase: (id) => api.delete(`/cases/${id}`),
  updateCaseStatus: (id, status) => api.patch(`/cases/${id}/status`, status),
};

// Audit API calls
export const auditAPI = {
  getLogs: (params) => api.get('/audit-logs', {
    params: {
      page: params?.page || 1,
      limit: params?.limit || 15,
      ...params,
    },
  }),
};

// Settings API calls
export const settingsAPI = {
  getSettings: () => api.get('/settings'),
  updateSettings: (settings) => api.put('/settings', settings),
};

export default api; 