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

// Add a response interceptor to handle errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized access
      localStorage.removeItem('token');
      store.dispatch(logout());
      toast.error('Your session has expired. Please log in again.');
      window.location.href = '/login';
    } else if (error.response?.status === 403) {
      toast.error('You do not have permission to perform this action.');
    } else if (error.response?.status === 500) {
      toast.error('An unexpected error occurred. Please try again later.');
    }
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
};

// Settings API calls
export const settingsAPI = {
  getSettings: () => api.get('/settings'),
  updateSettings: (settings) => api.put('/settings', settings),
};

export default api; 