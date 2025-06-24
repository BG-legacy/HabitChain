#!/bin/bash

# Development Database Management Script for HabitChain
# This script helps manage the development database

DATABASE_NAME="HabitChainDb_Dev"
DB_USER="postgres"
DB_HOST="localhost"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

show_help() {
    echo -e "${GREEN}HabitChain Development Database Management${NC}"
    echo -e "${GREEN}=============================================${NC}"
    echo ""
    echo -e "${YELLOW}Usage: ./dev-db.sh [action]${NC}"
    echo ""
    echo -e "${YELLOW}Actions:${NC}"
    echo -e "${WHITE}  reset  - Drop and recreate the development database${NC}"
    echo -e "${WHITE}  seed   - Seed the database with test data (if empty)${NC}"
    echo -e "${WHITE}  status - Show database connection status${NC}"
    echo -e "${WHITE}  help   - Show this help message (default)${NC}"
    echo ""
    echo -e "${YELLOW}Examples:${NC}"
    echo -e "${WHITE}  ./dev-db.sh reset${NC}"
    echo -e "${WHITE}  ./dev-db.sh seed${NC}"
    echo -e "${WHITE}  ./dev-db.sh status${NC}"
}

test_postgresql_connection() {
    if psql -h "$DB_HOST" -U "$DB_USER" -d postgres -c "SELECT 1;" >/dev/null 2>&1; then
        return 0
    else
        return 1
    fi
}

test_database_exists() {
    local db_name="$1"
    if psql -h "$DB_HOST" -U "$DB_USER" -d postgres -c "SELECT 1 FROM pg_database WHERE datname = '$db_name';" 2>/dev/null | grep -q "1 row"; then
        return 0
    else
        return 1
    fi
}

reset_database() {
    echo -e "${YELLOW}Resetting development database...${NC}"
    
    if ! test_postgresql_connection; then
        echo -e "${RED}Error: Cannot connect to PostgreSQL. Make sure PostgreSQL is running.${NC}"
        return 1
    fi
    
    if test_database_exists "$DATABASE_NAME"; then
        echo -e "${YELLOW}Dropping existing database...${NC}"
        if psql -h "$DB_HOST" -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS \"$DATABASE_NAME\";" >/dev/null 2>&1; then
            echo -e "${GREEN}Database dropped successfully.${NC}"
        else
            echo -e "${RED}Error dropping database.${NC}"
            return 1
        fi
    fi
    
    echo -e "${YELLOW}Creating new database...${NC}"
    if psql -h "$DB_HOST" -U "$DB_USER" -d postgres -c "CREATE DATABASE \"$DATABASE_NAME\";" >/dev/null 2>&1; then
        echo -e "${GREEN}Database created successfully.${NC}"
        echo -e "${CYAN}Run the application to seed the database with test data.${NC}"
    else
        echo -e "${RED}Error creating database.${NC}"
        return 1
    fi
}

show_database_status() {
    echo -e "${YELLOW}Database Status Check${NC}"
    echo -e "${YELLOW}====================${NC}"
    
    if ! test_postgresql_connection; then
        echo -e "${RED}❌ PostgreSQL connection failed${NC}"
        echo -e "${RED}   Make sure PostgreSQL is running on localhost:5432${NC}"
        return 1
    fi
    
    echo -e "${GREEN}✅ PostgreSQL connection successful${NC}"
    
    if test_database_exists "$DATABASE_NAME"; then
        echo -e "${GREEN}✅ Database '$DATABASE_NAME' exists${NC}"
        
        # Check if tables exist
        table_count=$(psql -h "$DB_HOST" -U "$DB_USER" -d "$DATABASE_NAME" -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" 2>/dev/null | grep -E '^[0-9]+$' | head -1)
        if [ -n "$table_count" ]; then
            echo -e "${CYAN}   Tables found: $table_count${NC}"
        else
            echo -e "${YELLOW}   Could not check table count${NC}"
        fi
    else
        echo -e "${RED}❌ Database '$DATABASE_NAME' does not exist${NC}"
        echo -e "${CYAN}   Run './dev-db.sh reset' to create it${NC}"
    fi
}

seed_database() {
    echo -e "${YELLOW}Seeding database...${NC}"
    echo -e "${CYAN}Note: Database seeding happens automatically when the application starts.${NC}"
    echo -e "${CYAN}To seed the database, run the application in development mode.${NC}"
}

# Main script logic
case "${1:-help}" in
    reset)
        reset_database
        ;;
    seed)
        seed_database
        ;;
    status)
        show_database_status
        ;;
    help|*)
        show_help
        ;;
esac 