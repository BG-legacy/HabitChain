# HabitChain Backend Makefile
# Simplifies Docker commands for development and production

.PHONY: help build up down logs clean dev prod test health backup restore

# Default target
help:
	@echo "HabitChain Backend Docker Commands"
	@echo "=================================="
	@echo "Development Commands:"
	@echo "  make dev          - Start development environment"
	@echo "  make build        - Build Docker images"
	@echo "  make up           - Start services in background"
	@echo "  make down         - Stop services"
	@echo "  make logs         - View all logs"
	@echo "  make logs-api     - View API logs"
	@echo "  make logs-db      - View database logs"
	@echo "  make shell        - Access API container shell"
	@echo "  make db-shell     - Access database shell"
	@echo ""
	@echo "Production Commands:"
	@echo "  make prod         - Start production environment"
	@echo "  make prod-down    - Stop production environment"
	@echo "  make prod-logs    - View production logs"
	@echo ""
	@echo "Database Commands:"
	@echo "  make migrate      - Run database migrations"
	@echo "  make backup       - Create database backup"
	@echo "  make restore      - Restore database (set FILE=backup.sql)"
	@echo ""
	@echo "Utility Commands:"
	@echo "  make health       - Check application health"
	@echo "  make clean        - Clean up containers and volumes"
	@echo "  make test         - Run tests locally"
	@echo "  make setup        - Initial setup (copy env.template)"

# Development environment
dev:
	@echo "Starting development environment..."
	docker-compose up --build

# Build images
build:
	@echo "Building Docker images..."
	docker-compose build

# Start services in background
up:
	@echo "Starting services in background..."
	docker-compose up -d

# Stop services
down:
	@echo "Stopping services..."
	docker-compose down

# View logs
logs:
	docker-compose logs -f

# View API logs
logs-api:
	docker-compose logs -f habitchain-backend

# View database logs
logs-db:
	docker-compose logs -f habitchain-db

# Access API container shell
shell:
	docker-compose exec habitchain-backend /bin/bash

# Access database shell
db-shell:
	docker-compose exec habitchain-db psql -U postgres -d HabitChainDb_Dev

# Production environment
prod:
	@echo "Starting production environment..."
	docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Stop production environment
prod-down:
	@echo "Stopping production environment..."
	docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# View production logs
prod-logs:
	docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f

# Run database migrations
migrate:
	@echo "Running database migrations..."
	docker-compose exec habitchain-backend dotnet ef database update

# Create database backup
backup:
	@echo "Creating database backup..."
	docker-compose exec habitchain-db pg_dump -U postgres HabitChainDb_Dev > backup_$(shell date +%Y%m%d_%H%M%S).sql
	@echo "Backup created: backup_$(shell date +%Y%m%d_%H%M%S).sql"

# Restore database
restore:
	@if [ -z "$(FILE)" ]; then \
		echo "Usage: make restore FILE=backup.sql"; \
		exit 1; \
	fi
	@echo "Restoring database from $(FILE)..."
	docker-compose exec -T habitchain-db psql -U postgres -d HabitChainDb_Dev < $(FILE)

# Check application health
health:
	@echo "Checking application health..."
	@curl -f http://localhost:8081/health || echo "Health check failed"

# Clean up containers and volumes
clean:
	@echo "Cleaning up containers and volumes..."
	docker-compose down -v
	docker system prune -f

# Run tests (run locally, not in container)
test:
	@echo "Running tests locally..."
	dotnet test

# Initial setup
setup:
	@echo "Setting up environment..."
	@if [ ! -f .env ]; then \
		cp env.template .env; \
		echo "Created .env file from template. Please edit it with your configuration."; \
	else \
		echo ".env file already exists."; \
	fi
	@mkdir -p logs
	@echo "Created logs directory."
	@echo "Setup complete! Run 'make dev' to start the development environment."

# Quick development workflow
quick-dev: setup dev

# Full clean and rebuild
rebuild:
	@echo "Full rebuild..."
	docker-compose down -v
	docker-compose build --no-cache
	docker-compose up -d

# Show container status
status:
	@echo "Container status:"
	docker-compose ps

# Show resource usage
stats:
	@echo "Resource usage:"
	docker stats --no-stream 