// Environment Configuration
// Centralized configuration for all environment variables

const environment = import.meta.env.MODE || 'development';

const config = {
  development: {
    API_BASE_URL: 'http://localhost:5000/api',
    GOOGLE_CLIENT_ID: '933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com',
    APP_URL: 'http://localhost:5173',
    NODE_ENV: 'development'
  },
  
  production: {
    API_BASE_URL: 'https://api.241runnersawareness.org/api',
    GOOGLE_CLIENT_ID: '933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com',
    APP_URL: 'https://241runnersawareness.org',
    NODE_ENV: 'production'
  }
};

// Export current environment config
export const currentConfig = config[environment];

// Individual exports for easy access
export const API_BASE_URL = currentConfig.API_BASE_URL;
export const GOOGLE_CLIENT_ID = currentConfig.GOOGLE_CLIENT_ID;
export const APP_URL = currentConfig.APP_URL;
export const NODE_ENV = currentConfig.NODE_ENV;

// Helper functions
export const isDevelopment = () => NODE_ENV === 'development';
export const isProduction = () => NODE_ENV === 'production';

// Debug logging in development
if (isDevelopment()) {
  console.log('🔧 Environment Configuration:', currentConfig);
}

export default currentConfig; 