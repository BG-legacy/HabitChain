# HabitChain Backend

A .NET 9 Web API backend for the HabitChain habit tracking application, built with Clean Architecture principles and PostgreSQL integration.

## Architecture

The backend follows Clean Architecture with the following layers:

- **HabitChain.Domain**: Core business entities, enums, and interfaces
- **HabitChain.Application**: Application services, DTOs, and business logic
- **HabitChain.Infrastructure**: Data access, repositories, and external services
- **HabitChain.WebAPI**: REST API controllers and configuration

## Database Schema

### PostgreSQL Integration

The application uses PostgreSQL as the primary database with Entity Framework Core and Npgsql provider.

#### Connection Configuration

**Development:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=HabitChainDb_Dev;Username=postgres;Password=postgres"
  }
}
```

**Production:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=HabitChainDb;Username=postgres;Password=postgres"
  }
}
```

### Core Entities

#### Users
- **Id**: Unique identifier (Guid)
- **Email**: User email address (unique)
- **Username**: Display name (unique)
- **FirstName**: User's first name
- **LastName**: User's last name
- **ProfilePictureUrl**: Optional profile image URL
- **LastLoginAt**: Last login timestamp
- **IsActive**: Account status flag
- **CreatedAt/UpdatedAt**: Audit timestamps

#### Habits
- **Id**: Unique identifier (Guid)
- **Name**: Habit name
- **Description**: Optional habit description
- **Frequency**: Habit frequency (Daily, Weekly, Monthly, Custom)
- **UserId**: Foreign key to Users
- **IsActive**: Whether habit is currently active
- **Color**: UI customization color (hex)
- **IconName**: UI icon identifier
- **CurrentStreak**: Current consecutive completion streak
- **LongestStreak**: Longest streak achieved
- **LastCompletedAt**: Last completion timestamp

#### CheckIns
- **Id**: Unique identifier (Guid)
- **UserId**: Foreign key to Users
- **HabitId**: Foreign key to Habits
- **CompletedAt**: When the habit was completed
- **Notes**: Optional completion notes
- **StreakDay**: Day number in current streak
- **IsManualEntry**: Whether manually entered or automatic

#### Badges
- **Id**: Unique identifier (Guid)
- **Name**: Badge name
- **Description**: Badge description
- **IconUrl**: Badge icon URL
- **Type**: Badge type (Streak, Total, Consistency, Special)
- **RequiredValue**: Value needed to earn badge
- **IsActive**: Whether badge is currently available

#### UserBadges
- **Id**: Unique identifier (Guid)
- **UserId**: Foreign key to Users
- **BadgeId**: Foreign key to Badges
- **EarnedAt**: When badge was earned
- **HabitId**: Optional habit that earned the badge

#### Encouragements
- **Id**: Unique identifier (Guid)
- **FromUserId**: User sending encouragement
- **ToUserId**: User receiving encouragement
- **Message**: Encouragement message
- **Type**: Encouragement type (General, Milestone, Streak, Comeback, Achievement)
- **HabitId**: Optional related habit
- **IsRead**: Whether message has been read

### Enums

#### HabitFrequency
- Daily = 1
- Weekly = 2
- Monthly = 3
- Custom = 4

#### BadgeType
- Streak = 1 (For consecutive days)
- Total = 2 (For total check-ins)
- Consistency = 3 (For percentage-based achievements)
- Special = 4 (For special achievements)

#### EncouragementType
- General = 1 (General encouragement)
- Milestone = 2 (For reaching a milestone)
- Streak = 3 (For maintaining a streak)
- Comeback = 4 (For getting back on track)
- Achievement = 5 (For earning a badge)

## Getting Started

### Prerequisites

- .NET 9 SDK
- PostgreSQL 12+
- Entity Framework Core Tools

### Setup

1. **Install EF Core Tools** (if not already installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. **Update Connection String**:
   Update the connection string in `appsettings.json` or `appsettings.Development.json` with your PostgreSQL credentials.

3. **Create Database**:
   ```bash
   # Navigate to the backend directory
   cd backend
   
   # Create/update the database
   dotnet ef database update --project HabitChain.Infrastructure --startup-project HabitChain.WebAPI
   ```

4. **Run the Application**:
   ```bash
   dotnet run --project HabitChain.WebAPI
   ```

### Database Migrations

To create a new migration:
```bash
dotnet ef migrations add MigrationName --project HabitChain.Infrastructure --startup-project HabitChain.WebAPI
```

To update the database:
```bash
dotnet ef database update --project HabitChain.Infrastructure --startup-project HabitChain.WebAPI
```

To remove the last migration:
```bash
dotnet ef migrations remove --project HabitChain.Infrastructure --startup-project HabitChain.WebAPI
```

## API Endpoints

### Habits
- `GET /api/habits/user/{userId}` - Get all habits for a user
- `GET /api/habits/user/{userId}/active` - Get active habits for a user
- `GET /api/habits/{id}` - Get habit by ID
- `GET /api/habits/{id}/with-entries` - Get habit with entries
- `POST /api/habits` - Create new habit
- `PUT /api/habits/{id}` - Update habit
- `DELETE /api/habits/{id}` - Delete habit

### Future Endpoints (To be implemented)
- Users management
- Check-ins tracking
- Badge system
- Encouragements/social features

## Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 9.0.5**: ORM
- **Npgsql 9.0.4**: PostgreSQL provider for EF Core
- **PostgreSQL**: Primary database
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation

## Development Notes

- All entities inherit from `BaseEntity` which provides `Id`, `CreatedAt`, and `UpdatedAt` properties
- Automatic timestamp management is handled in the `DbContext`
- Unique constraints are enforced on User email and username
- Foreign key relationships use `DeleteBehavior.Cascade` for owned entities and `DeleteBehavior.Restrict` for cross-references
- All IDs use `Guid` for better distributed system compatibility

## Contributing

1. Follow Clean Architecture principles
2. Use Entity Framework migrations for database changes
3. Implement proper error handling and validation
4. Add unit tests for new features
5. Update this README when adding new features or endpoints 