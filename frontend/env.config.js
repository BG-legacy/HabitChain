// Environment Configuration
// This file can be used to set environment variables for different environments

const config = {
  // Production environment (deployed)
  production: {
    REACT_APP_API_URL: 'https://habitchain.onrender.com/api',
    REACT_APP_ENV: 'production',
    REACT_APP_ENABLE_ANALYTICS: 'false',
    REACT_APP_ENABLE_DEBUG_MODE: 'false',
    REACT_APP_NAME: 'HabitChain',
    REACT_APP_VERSION: '1.0.0'
  },

  // Development environment (local)
  development: {
    REACT_APP_API_URL: 'http://localhost:8080/api',
    REACT_APP_ENV: 'development', 
    REACT_APP_ENABLE_ANALYTICS: 'false',
    REACT_APP_ENABLE_DEBUG_MODE: 'true',
    REACT_APP_NAME: 'HabitChain',
    REACT_APP_VERSION: '1.0.0'
  }
};

// Export the configuration for the current environment
const currentEnv = process.env.NODE_ENV || 'development';
module.exports = config[currentEnv] || config.development;

// To use this configuration:
// 1. Copy this file to .env.js in your build process
// 2. Or set these values in your deployment environment
// 3. Or create a .env file manually with these values 