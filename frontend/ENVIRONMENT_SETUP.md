# Frontend Environment Setup

This guide explains how to set up environment variables for the HabitChain frontend.

## Quick Setup (Recommended)

Create a `.env` file in the `frontend/` directory:

```bash
cd frontend
touch .env
```

Then add the following content to the `.env` file:

```env
# Frontend Environment Variables

# API Configuration
REACT_APP_API_URL=https://habitchain.onrender.com/api

# Environment
REACT_APP_ENV=production

# Feature Flags
REACT_APP_ENABLE_ANALYTICS=false
REACT_APP_ENABLE_DEBUG_MODE=false

# App Information
REACT_APP_NAME=HabitChain
REACT_APP_VERSION=1.0.0
```

## For Local Development

If you want to run the frontend locally against a local backend, create a `.env.local` file:

```env
# Local Development Environment Variables

# API Configuration (local backend)
REACT_APP_API_URL=http://localhost:8080/api

# Environment
REACT_APP_ENV=development

# Feature Flags
REACT_APP_ENABLE_ANALYTICS=false
REACT_APP_ENABLE_DEBUG_MODE=true

# App Information
REACT_APP_NAME=HabitChain
REACT_APP_VERSION=1.0.0
```

## Environment Variables Explained

### Required Variables

- `REACT_APP_API_URL`: The base URL for your backend API
  - **Production**: `https://habitchain.onrender.com/api`
  - **Development**: `http://localhost:8080/api`

- `REACT_APP_ENV`: The environment mode
  - **Production**: `production`
  - **Development**: `development`

### Optional Variables

- `REACT_APP_ENABLE_ANALYTICS`: Enable/disable analytics tracking
- `REACT_APP_ENABLE_DEBUG_MODE`: Enable/disable debug mode
- `REACT_APP_NAME`: Application name
- `REACT_APP_VERSION`: Application version

## File Priority

React loads environment variables in this order:
1. `.env.local` (loaded in all environments except test)
2. `.env.development`, `.env.production` (loaded based on NODE_ENV)
3. `.env`

## Deployment

### Vercel Deployment
In your Vercel dashboard, set these environment variables:
- `REACT_APP_API_URL=https://habitchain.onrender.com/api`
- `REACT_APP_ENV=production`

### Other Platforms
Set the environment variables in your deployment platform's environment configuration.

## Security Notes

- **Never commit `.env.local`** to version control
- **Only `REACT_APP_*` variables** are embedded in the build
- **All frontend env variables** are public in the built app
- **Never put secrets** in frontend environment variables

## Troubleshooting

If environment variables aren't working:
1. Ensure variable names start with `REACT_APP_`
2. Restart the development server after changes
3. Check the browser console for the actual values being used
4. Verify the `.env` file is in the correct directory (`frontend/`)

## Current Configuration

Your current configuration in `src/config/environment.ts` will use:
- Environment variables if available
- Fallback defaults if not set

This ensures the app works even without environment variables. 