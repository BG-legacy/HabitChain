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
  apiTimeout: 15000, // 15 seconds - optimized balance of speed and reliability
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
  
  // In production or when deployed, use the production API
  if (config.isProduction || !isLocalhost()) {
    return 'https://habitchain.onrender.com/api';
  }
  
  // For local development, also use production API (since local backend might not be running)
  return 'https://habitchain.onrender.com/api';
};

export default config; 