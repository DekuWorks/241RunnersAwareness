import axios from 'axios';
import store from '../store';
import { logout } from '../features/auth/authSlice';

// Create axios instance
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5113/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - attach Authorization header
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

// Response interceptor - handle 401 errors
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      // Clear token and user data
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      // Dispatch logout action
      store.dispatch(logout());
      
      // Redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Helper functions
export const apiClient = {
  get: (url: string, config = {}) => api.get(url, config),
  post: (url: string, data = {}, config = {}) => api.post(url, data, config),
  put: (url: string, data = {}, config = {}) => api.put(url, data, config),
  patch: (url: string, data = {}, config = {}) => api.patch(url, data, config),
  delete: (url: string, config = {}) => api.delete(url, config),
};

export default api;
