# HabitChain Backend Deployment Guide

## Overview
This guide helps you deploy the HabitChain backend API to production, addressing the common issues encountered during deployment.

## Issues Fixed

### 1. Database Connection Issues
- **Problem**: Npgsql errors with Supabase connection pooling
- **Solution**: 
  - Disabled multiplexing for better transaction support
  - Added connection resiliency with retry logic
  - Improved connection string configuration

### 2. HTTPS Certificate Issues
- **Problem**: Missing developer certificate in production
- **Solution**:
  - Configured HTTP-only deployment for production
  - Added graceful HTTPS redirection handling
  - Updated Kestrel configuration

### 3. Transaction Issues
- **Problem**: Multiplexing mode conflicts with database seeding
- **Solution**:
  - Disabled multiplexing in Entity Framework configuration
  - Added retry logic for database seeding
  - Improved error handling in DbSeeder

## Deployment Options

### Option 1: Docker Compose (Recommended)

#### For Development:
```bash
cd backend
docker-compose up --build
```

#### For Production:
```bash
cd backend
docker-compose -f docker-compose.prod.yml up --build -d
```

### Option 2: Direct Docker Build

```bash
cd backend
docker build -t habitchain-backend .
docker run -p 8080:8080 --env-file .env habitchain-backend
```

### Option 3: Render.com Deployment

1. **Connect your repository** to Render
2. **Create a new Web Service**
3. **Configure the service**:
   - **Build Command**: `cd backend && dotnet publish -c Release -o ./publish`
   - **Start Command**: `cd backend/publish && dotnet HabitChain.WebAPI.dll`
   - **Environment**: `Docker`

4. **Set Environment Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   DB_HOST=aws-0-us-east-2.pooler.supabase.com
   DB_NAME=postgres
   DB_USERNAME=postgres.ekmnikbqyhgiomqkgcet
   DB_PASSWORD=postgres
   DB_PORT=6543
   DB_SSL_MODE=Require
   DB_TRUST_SERVER_CERTIFICATE=true
   JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
   JWT_ISSUER=HabitChainAPI
   JWT_AUDIENCE=HabitChainClient
   ```

## Environment Configuration

### Required Environment Variables

```bash
# Database Configuration
DB_HOST=aws-0-us-east-2.pooler.supabase.com
DB_NAME=postgres
DB_USERNAME=postgres.ekmnikbqyhgiomqkgcet
DB_PASSWORD=postgres
DB_PORT=6543
DB_SSL_MODE=Require
DB_TRUST_SERVER_CERTIFICATE=true

# JWT Configuration
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=HabitChainAPI
JWT_AUDIENCE=HabitChainClient
JWT_EXPIRE_MINUTES=60

# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Optional: OpenAI Configuration
OPENAI_API_KEY=your_openai_api_key_here
```

### Database Connection String Format

The application automatically constructs the connection string from environment variables:

```
Host={DB_HOST};Database={DB_NAME};Username={DB_USERNAME};Password={DB_PASSWORD};Port={DB_PORT};SSL Mode={DB_SSL_MODE};Trust Server Certificate={DB_TRUST_SERVER_CERTIFICATE};Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Timeout=30;CommandTimeout=30;InternalCommandTimeout=60
```

## Health Checks

The application includes a health check endpoint:

```bash
curl http://localhost:8080/health
```

Expected response: `"Healthy"`

## Troubleshooting

### Common Issues and Solutions

#### 1. Database Connection Errors
**Symptoms**: `NpgsqlException: Received backend message NoData while expecting ParseCompleteMessage`

**Solutions**:
- Ensure all database environment variables are set correctly
- Check that the Supabase connection is active
- Verify SSL configuration is correct

#### 2. HTTPS Certificate Errors
**Symptoms**: `Unable to configure HTTPS endpoint. No server certificate was specified`

**Solutions**:
- The application now gracefully handles missing certificates
- Production deployment uses HTTP only
- HTTPS redirection is disabled when no certificate is available

#### 3. Transaction Errors
**Symptoms**: `In multiplexing mode, transactions must be started with BeginTransaction`

**Solutions**:
- Multiplexing is now disabled in the Entity Framework configuration
- Database seeding includes retry logic
- Improved error handling in the seeding process

#### 4. Application Startup Failures
**Symptoms**: Application fails to start or crashes immediately

**Solutions**:
- Check all required environment variables are set
- Verify database connectivity
- Review application logs for specific error messages

### Logging

The application logs important events to the console:

- Database connection attempts
- Seeding progress and errors
- Application startup status
- Health check results

### Monitoring

Monitor the application using:

1. **Health Check Endpoint**: `GET /health`
2. **Application Logs**: Check console output for errors
3. **Database Connectivity**: Monitor connection pool status

## Performance Optimization

### Database Connection Pooling
- **MinPoolSize**: 1
- **MaxPoolSize**: 20
- **ConnectionIdleLifetime**: 300 seconds
- **ConnectionPruningInterval**: 10 seconds

### Kestrel Configuration
- **MaxConcurrentConnections**: 100
- **MaxRequestBodySize**: 50MB
- **RequestHeadersTimeout**: 30 seconds
- **KeepAliveTimeout**: 2 seconds

## Security Considerations

1. **JWT Secret**: Use a strong, unique secret key in production
2. **Database Credentials**: Store securely and rotate regularly
3. **Environment Variables**: Never commit secrets to version control
4. **HTTPS**: Configure SSL/TLS for production traffic

## Deployment Checklist

- [ ] All environment variables configured
- [ ] Database connection tested
- [ ] JWT secret key set
- [ ] Health check endpoint responding
- [ ] Application logs reviewed
- [ ] SSL/TLS configured (if required)
- [ ] Monitoring and alerting set up

## Support

If you encounter issues:

1. Check the application logs for specific error messages
2. Verify all environment variables are set correctly
3. Test database connectivity independently
4. Review the troubleshooting section above
5. Check the health endpoint for application status

## Files Modified for Deployment

- `Program.cs`: Updated with better error handling and HTTPS configuration
- `Dockerfile`: Optimized for production deployment
- `docker-compose.prod.yml`: Production-specific configuration
- `appsettings.Production.json`: Production settings
- `DbSeeder.cs`: Improved error handling and transaction support 