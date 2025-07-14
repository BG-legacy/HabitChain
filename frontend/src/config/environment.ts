// Environment Configuration
export const config = {
  // API Configuration
  apiUrl: process.env.REACT_APP_API_URL || 'https://habitchain.onrender.com/api',
  
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
  apiTimeout: 30000, // 30 seconds - increased from 10 seconds to handle slower backend responses
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
  // In development, if running on localhost, use the local API for development
  if (isLocalhost() && config.isDevelopment) {
    return 'https://habitchain.onrender.com/api';
  }
  
  return config.apiUrl;
};

export default config; 