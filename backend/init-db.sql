-- HabitChain Database Initialization Script
-- This script runs when the PostgreSQL container is first created

-- Connect to the database
\c HabitChainDb_Dev;

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Note: Entity Framework will handle table creation via migrations
-- This script is mainly for database initialization and extensions 