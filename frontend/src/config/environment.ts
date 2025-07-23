// Environment Configuration
export const config = {
  // API Configuration
  apiUrl: process.env.REACT_APP_API_URL || 'http://localhost:8080/api',
  
  // Environment
  environment: process.env.REACT_APP_ENV || 'development',
  isDevelopment: process.env.NODE_ENV === 'development',
  isProduction: process.env.NODE_ENV === 'production',
  
  // Feature Flags
  enableAnalytics: process.env.REACT_APP_ENABLE_ANALYTICS === 'true',
  enableDebugMode: process.env.REACT_APP_ENABLE_DEBUG_MODE === 'true',
  
  // App Configuration
  appName: 'HabitChain',
  appVersion: '1.0.0',
  
  // Timeouts
  apiTimeout: 30000, // 30 seconds - allow time for AI generation
  tokenRefreshThreshold: 5 * 60 * 1000, // 5 minutes
  
  // Pagination
  defaultPageSize: 20,
  maxPageSize: 100,
};

// Helper functions
export const isLocalhost = (): boolean => {
  return window.location.hostname === 'localhost' || 
         window.location.hostname === '127.0.0.1';
};

export const getApiUrl = (): string => {
  // If a specific API URL is set via environment variable, use it
  if (process.env.REACT_APP_API_URL) {
    return process.env.REACT_APP_API_URL;
  }
  
  // Always use the production API on Render
  return 'https://habitchain.onrender.com/api';
};

export default config; 