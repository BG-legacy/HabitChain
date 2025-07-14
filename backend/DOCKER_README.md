# HabitChain Backend Docker Setup

This guide explains how to run the HabitChain backend using Docker and Docker Compose.

## Prerequisites

- Docker (version 20.10 or later)
- Docker Compose (version 2.0 or later)

## Quick Start

### Development Environment

1. **Clone the repository and navigate to the backend directory:**
   ```bash
   cd backend
   ```

2. **Create environment file:**
   ```bash
   cp env.template .env
   ```
   Edit `.env` file with your configuration values.

3. **Build and start the services:**
   ```bash
   docker-compose up --build
   ```

4. **The application will be available at:**
   - Backend API: http://localhost:8081
   - Swagger UI: http://localhost:8081/swagger (in development)
   - Health Check: http://localhost:8081/health

### Production Environment

1. **Create production environment file:**
   ```bash
   cp env.template .env.prod
   ```
   Edit `.env.prod` with your production values.

2. **Run with production configuration:**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
   ```

## Environment Variables

Create a `.env` file in the backend directory with the following variables:

```env
# Database Configuration
DB_HOST=habitchain-db
DB_NAME=HabitChainDb
DB_USERNAME=postgres
DB_PASSWORD=your_secure_password_here
DB_PORT=5432
DB_SSL_MODE=Prefer

# Development Database (for local development)
DEV_DB_NAME=HabitChainDb_Dev

# JWT Configuration
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=HabitChainAPI
JWT_AUDIENCE=HabitChainClient
JWT_EXPIRE_MINUTES=60

# OpenAI Configuration
OPENAI_API_KEY=your_openai_api_key_here
OPENAI_MODEL=gpt-4o-mini
OPENAI_MAX_TOKENS=1000
OPENAI_TEMPERATURE=0.7

# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
```

## Docker Services

### habitchain-backend
- **Description**: Main .NET 9.0 Web API application
- **Port**: 8081 (mapped to internal 8080)
- **Health Check**: `/health` endpoint
- **Dependencies**: PostgreSQL database

### habitchain-db
- **Description**: PostgreSQL 15 database
- **Port**: 5433 (mapped to internal 5432, exposed only in development)
- **Data Persistence**: Named volume `habitchain_db_data`

## Useful Commands

### Development Commands

```bash
# Start services
docker-compose up

# Start services in background
docker-compose up -d

# Build and start services
docker-compose up --build

# Stop services
docker-compose down

# Stop services and remove volumes
docker-compose down -v

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f habitchain-backend

# Access database
docker-compose exec habitchain-db psql -U postgres -d HabitChainDb_Dev
```

### Production Commands

```bash
# Start production services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# View production logs
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f

# Stop production services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
```

### Database Management

```bash
# Run Entity Framework migrations
docker-compose exec habitchain-backend dotnet ef database update

# Access database directly
docker-compose exec habitchain-db psql -U postgres -d HabitChainDb_Dev

# Backup database
docker-compose exec habitchain-db pg_dump -U postgres HabitChainDb_Dev > backup.sql

# Restore database
docker-compose exec -T habitchain-db psql -U postgres -d HabitChainDb_Dev < backup.sql
```

## Health Checks

The application includes health check endpoints:

- **Basic Health Check**: `GET /health`
- **Detailed Health Check**: `GET /health/detailed`

These endpoints are used by Docker health checks and can be used by monitoring systems.

## Volumes

### Development
- `habitchain_db_data`: PostgreSQL data persistence
- `./logs`: Application logs (mounted from host)

### Production
- `habitchain_db_prod_data`: PostgreSQL data persistence
- `./logs`: Application logs
- `./backups`: Database backups

## Network

All services run on the `habitchain-network` bridge network, allowing them to communicate with each other using service names.

## Troubleshooting

### Common Issues

1. **Database connection issues**:
   - Check if PostgreSQL container is running: `docker-compose ps`
   - Check database logs: `docker-compose logs habitchain-db`
   - Verify environment variables are set correctly

2. **Application startup issues**:
   - Check application logs: `docker-compose logs habitchain-backend`
   - Verify all required environment variables are set
   - Check if database is ready before application starts

3. **Port conflicts**:
   - Backend uses port 8081 externally to avoid conflicts with other services on 8080
   - Database uses port 5433 externally to avoid conflicts with local PostgreSQL on 5432

4. **Permission issues**:
   - Ensure Docker has permission to access the project directory
   - Check if log directories can be created

### Health Check Failures

If health checks are failing:

```bash
# Test health endpoint manually
curl http://localhost:8081/health

# Check application logs
docker-compose logs habitchain-backend

# Check if application is responding
docker-compose exec habitchain-backend curl http://localhost:8080/health
```

## Security Considerations

### Production Security

1. **Use strong passwords** for database and JWT secret
2. **Don't expose database port** in production (commented out in prod config)
3. **Use SSL/TLS** for database connections in production
4. **Limit container resources** (configured in production compose file)
5. **Keep secrets secure** - use Docker secrets or external secret management

### Development Security

- Default credentials are provided for easy development
- Change all default passwords before deploying to production
- OpenAI API key is optional for development but required for AI features

## Monitoring

### Container Monitoring

```bash
# View container stats
docker stats

# View container resource usage
docker-compose top

# Monitor logs in real-time
docker-compose logs -f --tail=100
```

### Application Monitoring

- Health endpoints provide basic application status
- Logs are available in the mounted `./logs` directory
- Use external monitoring tools to track health endpoints

## Backup and Recovery

### Database Backups

```bash
# Create backup
docker-compose exec habitchain-db pg_dump -U postgres HabitChainDb_Dev > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore backup
docker-compose exec -T habitchain-db psql -U postgres -d HabitChainDb_Dev < backup_file.sql
```

### Application Data

- Database data is persisted in Docker volumes
- Configuration is stored in mounted files
- Logs are stored in mounted directories

## Development Workflow

1. **Make code changes** in your IDE
2. **Rebuild container**: `docker-compose up --build`
3. **Test changes** using the API endpoints
4. **View logs**: `docker-compose logs -f habitchain-backend`
5. **Access database** if needed: `docker-compose exec habitchain-db psql -U postgres -d HabitChainDb_Dev`

## Integration with Frontend

The backend API will be available at `http://localhost:8081` for frontend integration. Configure your frontend to use this URL for API calls.

## Next Steps

1. Set up your environment variables
2. Start the development environment
3. Test the API endpoints
4. Configure your frontend to connect to the backend
5. Set up production environment when ready to deploy 