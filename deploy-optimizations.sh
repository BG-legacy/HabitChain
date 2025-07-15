#!/bin/bash

# HabitChain Performance Optimizations Deployment Script
# This script commits and deploys the performance optimizations

set -e  # Exit on any error

echo "🚀 Deploying HabitChain Performance Optimizations..."

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

# Check if we're in the right directory
if [ ! -f "backend/HabitChain.sln" ]; then
    print_error "Please run this script from the HabitChain root directory"
    exit 1
fi

print_status "Checking git status..."
git status --porcelain

print_status "Adding changes to git..."
git add .

print_status "Committing performance optimizations..."
git commit -m "🚀 Performance optimizations: 
- Reduced password hashing from 10,000 to 1,000 iterations (10x faster)
- Optimized registration database queries (single query vs multiple)
- Disabled heavy seeding in production (faster cold starts)
- Fixed frontend token refresh for auth endpoints
- Added loading states and better error handling

Expected improvements:
- Registration: 30+ seconds → 2-5 seconds
- Cold start: 30+ seconds → 5-10 seconds
- Password hashing: 10x faster"

print_status "Pushing to main branch..."
git push origin main

print_status "✅ Deployment complete!"
print_warning "Monitor your Render deployment at: https://render.com/dashboard"
print_warning "Look for 'Production mode: Optimized startup' in the build logs"

echo ""
echo "🎯 Expected performance improvements:"
echo "  • Registration: 30+ seconds → 2-5 seconds"
echo "  • Cold start: 30+ seconds → 5-10 seconds"
echo "  • Password hashing: 10x faster"
echo ""
echo "🔍 After deployment, test registration again - it should be significantly faster!" 