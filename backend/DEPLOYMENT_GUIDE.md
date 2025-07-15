# HabitChain Backend Deployment Guide

## 🚀 **URGENT: Deploy Performance Optimizations**

Your backend optimizations are ready but need to be deployed to production. Here's how:

### Quick Deployment to Render

1. **Commit your changes**:
   ```bash
   cd backend
   git add .
   git commit -m "Performance optimizations: faster registration, reduced seeding"
   git push origin main
   ```

2. **Trigger Render deployment**:
   - Go to https://render.com/dashboard
   - Find your HabitChain backend service
   - Click "Deploy Latest Commit" or it should auto-deploy

3. **Monitor deployment**:
   - Check build logs for "Production mode: Optimized startup"
   - Should see "Badges already exist. Ready for requests." in logs

### Expected Performance Improvements
- **Registration**: 30+ seconds → 2-5 seconds
- **Cold start**: 30+ seconds → 5-10 seconds
- **Password hashing**: 10x faster (1,000 vs 10,000 iterations)

---

## Overview
This guide helps you deploy the HabitChain backend API to production, addressing the common issues encountered during deployment.

## Issues Fixed

### 1. Database Connection Issues
- **Problem**: Npgsql errors with Supabase connection pooling
- **Solution**: 
  - Disabled multiplexing via connection string for better transaction supportgit 
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
  - Disabled multiplexing in connection string (Multiplexing=false)
  - Added retry logic for database seeding
  - Improved error handling in DbSeeder

### 4. Duplicate Key Issues
- **Problem**: Database seeding fails with duplicate key violations
- **Solution**:
  - Used deterministic GUIDs for all seed data
  - Added proper duplicate key error handling
  - Improved seeding checks and retry logic

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

#### Using the Deploy Script:
```bash
cd backend
./deploy.sh        # For development
./deploy.sh prod   # For production
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
Host={DB_HOST};Database={DB_NAME};Username={DB_USERNAME};Password={DB_PASSWORD};Port={DB_PORT};SSL Mode={DB_SSL_MODE};Trust Server Certificate={DB_TRUST_SERVER_CERTIFICATE};Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Timeout=30;CommandTimeout=30;InternalCommandTimeout=60;Multiplexing=false
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
- Connection string now includes `Multiplexing=false` to prevent issues

#### 2. HTTPS Certificate Errors
**Symptoms**: `Unable to configure HTTPS endpoint. No server certificate was specified`

**Solutions**:
- The application now gracefully handles missing certificates
- Production deployment uses HTTP only
- HTTPS redirection is disabled when no certificate is available

#### 3. Transaction Errors
**Symptoms**: `In multiplexing mode, transactions must be started with BeginTransaction`

**Solutions**:
- Multiplexing is now disabled in the connection string (`Multiplexing=false`)
- Database seeding includes retry logic
- Improved error handling in the seeding process

#### 4. Duplicate Key Errors
**Symptoms**: `duplicate key value violates unique constraint "PK_CheckIns"`

**Solutions**:
- Seeding now uses deterministic GUIDs to prevent duplicates
- Added specific handling for duplicate key errors
- Application continues normally if data already exists
- Seeding is idempotent and safe to run multiple times

#### 5. Application Startup Failures
**Symptoms**: Application fails to start or crashes immediately

**Solutions**:
- Check all required environment variables are set
- Verify database connectivity
- Review application logs for specific error messages
- Database seeding failures no longer prevent app startup

### Logging

The application logs important events to the console:

- Database connection attempts
- Seeding progress and errors (including duplicate key handling)
- Application startup status
- Health check results
- Environment information

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
- **Multiplexing**: Disabled for better transaction support

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
- [ ] Database seeding completed without errors

## Support

If you encounter issues:

1. Check the application logs for specific error messages
2. Verify all environment variables are set correctly
3. Test database connectivity independently
4. Review the troubleshooting section above
5. Check the health endpoint for application status
6. Use the deployment script (`./deploy.sh`) for automated deployment

## Files Modified for Deployment

- `Program.cs`: Updated with better error handling, HTTPS configuration, and improved seeding
- `Dockerfile`: Optimized for production deployment
- `docker-compose.prod.yml`: Production-specific configuration
- `appsettings.Production.json`: Production settings with multiplexing disabled
- `DbSeeder.cs`: Improved error handling, deterministic GUIDs, and transaction support
- `deploy.sh`: Automated deployment script with health checks

## Deployment Notes

- The application is now resilient to database seeding issues
- Duplicate key errors are handled gracefully and don't prevent startup
- All seed data uses deterministic GUIDs for consistency
- Multiplexing is disabled to prevent transaction conflicts
- The application will continue to start even if seeding fails 