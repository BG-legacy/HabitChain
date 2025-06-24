# Development Database Management Script for HabitChain
# This script helps manage the development database

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("reset", "seed", "status", "help")]
    [string]$Action = "help"
)

$DatabaseName = "HabitChainDb_Dev"
$ConnectionString = "Host=localhost;Database=$DatabaseName;Username=postgres;Password=postgres"

function Show-Help {
    Write-Host "HabitChain Development Database Management" -ForegroundColor Green
    Write-Host "=============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\dev-db.ps1 [-Action <action>]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Actions:" -ForegroundColor Yellow
    Write-Host "  reset  - Drop and recreate the development database" -ForegroundColor White
    Write-Host "  seed   - Seed the database with test data (if empty)" -ForegroundColor White
    Write-Host "  status - Show database connection status" -ForegroundColor White
    Write-Host "  help   - Show this help message (default)" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\dev-db.ps1 reset" -ForegroundColor White
    Write-Host "  .\dev-db.ps1 seed" -ForegroundColor White
    Write-Host "  .\dev-db.ps1 status" -ForegroundColor White
}

function Test-PostgreSQLConnection {
    try {
        $result = psql -h localhost -U postgres -d postgres -c "SELECT 1;" 2>$null
        return $LASTEXITCODE -eq 0
    }
    catch {
        return $false
    }
}

function Test-DatabaseExists {
    param([string]$DbName)
    try {
        $result = psql -h localhost -U postgres -d postgres -c "SELECT 1 FROM pg_database WHERE datname = '$DbName';" 2>$null
        return $result -match "1 row"
    }
    catch {
        return $false
    }
}

function Reset-Database {
    Write-Host "Resetting development database..." -ForegroundColor Yellow
    
    if (-not (Test-PostgreSQLConnection)) {
        Write-Host "Error: Cannot connect to PostgreSQL. Make sure PostgreSQL is running." -ForegroundColor Red
        return
    }
    
    if (Test-DatabaseExists $DatabaseName) {
        Write-Host "Dropping existing database..." -ForegroundColor Yellow
        psql -h localhost -U postgres -d postgres -c "DROP DATABASE IF EXISTS `"$DatabaseName`";" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Database dropped successfully." -ForegroundColor Green
        } else {
            Write-Host "Error dropping database." -ForegroundColor Red
            return
        }
    }
    
    Write-Host "Creating new database..." -ForegroundColor Yellow
    psql -h localhost -U postgres -d postgres -c "CREATE DATABASE `"$DatabaseName`";" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database created successfully." -ForegroundColor Green
        Write-Host "Run the application to seed the database with test data." -ForegroundColor Cyan
    } else {
        Write-Host "Error creating database." -ForegroundColor Red
    }
}

function Show-DatabaseStatus {
    Write-Host "Database Status Check" -ForegroundColor Yellow
    Write-Host "====================" -ForegroundColor Yellow
    
    if (-not (Test-PostgreSQLConnection)) {
        Write-Host "❌ PostgreSQL connection failed" -ForegroundColor Red
        Write-Host "   Make sure PostgreSQL is running on localhost:5432" -ForegroundColor Red
        return
    }
    
    Write-Host "✅ PostgreSQL connection successful" -ForegroundColor Green
    
    if (Test-DatabaseExists $DatabaseName) {
        Write-Host "✅ Database '$DatabaseName' exists" -ForegroundColor Green
        
        # Check if tables exist
        try {
            $result = psql -h localhost -U postgres -d $DatabaseName -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" 2>$null
            if ($result -match "\d+") {
                $tableCount = [regex]::Match($result, "\d+").Value
                Write-Host "   Tables found: $tableCount" -ForegroundColor Cyan
            }
        }
        catch {
            Write-Host "   Could not check table count" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ Database '$DatabaseName' does not exist" -ForegroundColor Red
        Write-Host "   Run '.\dev-db.ps1 reset' to create it" -ForegroundColor Cyan
    }
}

function Seed-Database {
    Write-Host "Seeding database..." -ForegroundColor Yellow
    Write-Host "Note: Database seeding happens automatically when the application starts." -ForegroundColor Cyan
    Write-Host "To seed the database, run the application in development mode." -ForegroundColor Cyan
}

# Main script logic
switch ($Action.ToLower()) {
    "reset" { Reset-Database }
    "seed" { Seed-Database }
    "status" { Show-DatabaseStatus }
    "help" { Show-Help }
    default { Show-Help }
} 