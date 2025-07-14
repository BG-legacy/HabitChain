#!/bin/bash

# HabitChain Backend Deployment Script
# This script helps deploy the HabitChain backend to production

set -e  # Exit on any error

echo "🚀 Starting HabitChain Backend Deployment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    print_error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

print_status "Docker and Docker Compose are available"

# Check if .env file exists
if [ ! -f ".env" ]; then
    print_warning ".env file not found. Creating from template..."
    cat > .env << EOF
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
OPENAI_API_KEY=
EOF
    print_status ".env file created. Please review and update the values as needed."
fi

# Function to check if container is running
check_container() {
    local container_name=$1
    if docker ps --format "table {{.Names}}" | grep -q "^${container_name}$"; then
        return 0
    else
        return 1
    fi
}

# Function to check if container is healthy
check_health() {
    local container_name=$1
    local max_attempts=30
    local attempt=1
    
    print_status "Checking health of ${container_name}..."
    
    while [ $attempt -le $max_attempts ]; do
        if docker inspect --format='{{.State.Health.Status}}' "${container_name}" 2>/dev/null | grep -q "healthy"; then
            print_status "${container_name} is healthy!"
            return 0
        fi
        
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    print_error "${container_name} failed to become healthy after ${max_attempts} attempts"
    return 1
}

# Stop existing containers if running
print_status "Stopping existing containers..."
docker-compose down 2>/dev/null || true
docker-compose -f docker-compose.prod.yml down 2>/dev/null || true

# Build and start the application
print_status "Building and starting the application..."

# Choose deployment mode
if [ "$1" = "prod" ]; then
    print_status "Deploying in PRODUCTION mode..."
    docker-compose -f docker-compose.prod.yml up --build -d
    CONTAINER_NAME="habitchain-backend-prod"
else
    print_status "Deploying in DEVELOPMENT mode..."
    docker-compose up --build -d
    CONTAINER_NAME="habitchain-backend"
fi

# Wait for container to start
print_status "Waiting for container to start..."
sleep 10

# Check if container is running
if check_container "$CONTAINER_NAME"; then
    print_status "Container $CONTAINER_NAME is running"
else
    print_error "Container $CONTAINER_NAME failed to start"
    docker-compose logs || docker-compose -f docker-compose.prod.yml logs
    exit 1
fi

# Check health
if check_health "$CONTAINER_NAME"; then
    print_status "Application is healthy!"
else
    print_warning "Health check failed, but container is running"
fi

# Test the health endpoint
print_status "Testing health endpoint..."
sleep 5

if curl -f http://localhost:8080/health > /dev/null 2>&1; then
    print_status "Health endpoint is responding correctly"
else
    print_warning "Health endpoint is not responding yet. This might be normal during startup."
fi

# Show container logs
print_status "Recent application logs:"
docker logs --tail 20 "$CONTAINER_NAME"

# Show deployment summary
echo ""
print_status "Deployment Summary:"
echo "======================"
echo "Container Name: $CONTAINER_NAME"
echo "Health Endpoint: http://localhost:8080/health"
echo "API Endpoint: http://localhost:8080"
echo ""

if [ "$1" = "prod" ]; then
    echo "Production deployment completed!"
    echo "To view logs: docker logs -f $CONTAINER_NAME"
    echo "To stop: docker-compose -f docker-compose.prod.yml down"
else
    echo "Development deployment completed!"
    echo "To view logs: docker logs -f $CONTAINER_NAME"
    echo "To stop: docker-compose down"
fi

echo ""
print_status "Deployment completed successfully! 🎉" 