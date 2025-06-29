// API Configuration
// This file manages API URLs and other configuration settings
import { API_BASE_URL, GOOGLE_CLIENT_ID, APP_URL } from './environment.js';

// Export configuration from environment
export { API_BASE_URL, GOOGLE_CLIENT_ID, APP_URL };

// Helper function to get API endpoint
export const getApiEndpoint = (endpoint) => {
  return `${API_BASE_URL}${endpoint}`;
};

// Common API endpoints
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    GOOGLE_LOGIN: '/auth/google-login',
    VERIFY_EMAIL: '/auth/verify-email',
    VERIFY_PHONE: '/auth/verify-phone',
    RESEND_VERIFICATION: '/auth/resend-verification'
  },
  USER: {
    PROFILE: '/user/profile',
    UPDATE: '/user/update'
  },
  INDIVIDUAL: {
    CREATE: '/individual/create',
    UPDATE: '/individual/update',
    GET: '/individual/get'
  }
};

// Legacy config export for backward compatibility
const config = {
  development: {
    API_BASE_URL,
    GOOGLE_CLIENT_ID,
    APP_URL
  },
  production: {
    API_BASE_URL,
    GOOGLE_CLIENT_ID,
    APP_URL
  }
};

export default config; 