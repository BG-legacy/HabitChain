# Environment Setup for HabitChain Backend

This document explains how to set up environment variables for the HabitChain backend application.

## Quick Setup

1. Copy the `env.template` file to `.env`:
   ```bash
   cp env.template .env
   ```

2. Edit the `.env` file and replace the placeholder values with your actual configuration:
   ```bash
   nano .env
   # or
   code .env
   ```

## Required Environment Variables

### OpenAI API Configuration
- `OPENAI_API_KEY`: Your OpenAI API key (get from https://platform.openai.com/api-keys)
- `OPENAI_MODEL`: The OpenAI model to use (default: gpt-4o-mini)
- `OPENAI_MAX_TOKENS`: Maximum tokens for AI responses (default: 1000)
- `OPENAI_TEMPERATURE`: AI response creativity (0.0-1.0, default: 0.7)

### JWT Configuration
- `JWT_SECRET_KEY`: Secret key for JWT token signing (must be at least 32 characters)
- `JWT_ISSUER`: JWT token issuer (default: HabitChainAPI)
- `JWT_AUDIENCE`: JWT token audience (default: HabitChainClient)
- `JWT_EXPIRE_MINUTES`: JWT token expiration time in minutes (default: 60)

### Database Configuration
- `DB_HOST`: PostgreSQL host (default: localhost)
- `DB_NAME`: Production database name (default: HabitChainDb)
- `DB_USERNAME`: Database username (default: postgres)
- `DB_PASSWORD`: Database password (default: postgres)
- `DEV_DB_NAME`: Development database name (default: HabitChainDb_Dev)

### Application Settings
- `ASPNETCORE_ENVIRONMENT`: Environment name (Development/Production)
- `ASPNETCORE_URLS`: Application URLs (default: http://localhost:5000;https://localhost:5001)

## Example .env File

```env
# OpenAI API Configuration
OPENAI_API_KEY=sk-your-actual-openai-api-key-here
OPENAI_MODEL=gpt-4o-mini
OPENAI_MAX_TOKENS=1000
OPENAI_TEMPERATURE=0.7

# JWT Configuration
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=HabitChainAPI
JWT_AUDIENCE=HabitChainClient
JWT_EXPIRE_MINUTES=60

# Database Configuration
DB_HOST=localhost
DB_NAME=HabitChainDb
DB_USERNAME=postgres
DB_PASSWORD=your-secure-password
DEV_DB_NAME=HabitChainDb_Dev

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5000;https://localhost:5001
```

## Security Notes

1. **Never commit your `.env` file to version control**
2. The `.env` file is already in `.gitignore` to prevent accidental commits
3. Use strong, unique passwords for database and JWT secret keys
4. Keep your OpenAI API key secure and never share it publicly

## Getting OpenAI API Key

1. Go to https://platform.openai.com/
2. Sign up or log in to your account
3. Navigate to API Keys section
4. Create a new API key
5. Copy the key and paste it in your `.env` file

## Troubleshooting

### Environment Variables Not Loading
- Make sure the `.env` file is in the root of the backend directory
- Verify the file has no extra spaces or special characters
- Check that the DotNetEnv package is installed

### Database Connection Issues
- Ensure PostgreSQL is running
- Verify database credentials are correct
- Check that the database exists

### OpenAI API Issues
- Verify your API key is correct
- Check your OpenAI account has sufficient credits
- Ensure the model name is valid

## Development vs Production

- **Development**: Uses `DEV_DB_NAME` for database
- **Production**: Uses `DB_NAME` for database
- Set `ASPNETCORE_ENVIRONMENT=Production` for production deployments
- Use different JWT secret keys for different environments 