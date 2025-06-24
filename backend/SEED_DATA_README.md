# Seed Data for Development and Testing

This document describes the seed data setup for the HabitChain backend application during development and testing.

## Overview

The application includes comprehensive seed data that is automatically loaded when the application starts in development mode. This seed data includes:

- **Badges**: Pre-defined achievement badges for different types of accomplishments
- **Test Users**: Sample users with realistic profile information
- **Test Habits**: Various habits with different frequencies and progress levels
- **Test Habit Entries**: Historical completion data for the last 30 days
- **Test Check-ins**: Recent check-in data with mood and notes
- **Test User Badges**: Awarded badges for test users
- **Test Encouragements**: Sample encouragement messages between users

## Configuration

### Development Settings

The seed data is configured in `appsettings.Development.json`:

```json
{
  "SeedData": {
    "Enabled": true,
    "TestUsers": {
      "Enabled": true,
      "Count": 3
    },
    "TestHabits": {
      "Enabled": true,
      "PerUser": 3
    },
    "TestEntries": {
      "Enabled": true,
      "DaysBack": 30,
      "CompletionRate": 0.8
    },
    "TestCheckIns": {
      "Enabled": true,
      "DaysBack": 7,
      "CheckInRate": 0.9
    }
  }
}
```

### Production Settings

In production, seed data is disabled by default. Only badges are seeded as they are required for the application to function properly.

## Test Users

The following test users are created:

| Email | Name | Profile Picture | Created |
|-------|------|-----------------|---------|
| john.doe@test.com | John Doe | Green placeholder | 30 days ago |
| jane.smith@test.com | Jane Smith | Blue placeholder | 25 days ago |
| mike.wilson@test.com | Mike Wilson | Orange placeholder | 20 days ago |

**Note**: These users are created without passwords since they're for testing purposes only. In a real application, users would register through the authentication endpoints.

## Test Habits

Each test user has multiple habits with realistic data:

### John Doe's Habits
- **Morning Exercise**: Daily, 15-day streak, green color
- **Read Books**: Daily, 8-day streak, blue color  
- **Meditation**: Daily, 22-day streak, purple color

### Jane Smith's Habits
- **Drink Water**: Daily, 5-day streak, cyan color
- **Practice Guitar**: Daily, 12-day streak, orange color

### Mike Wilson's Habits
- **Coding Practice**: Daily, 3-day streak, blue-grey color
- **Weekly Review**: Weekly, 2-week streak, brown color

## Badges

The following badge types are seeded:

### Streak Badges
- First Steps (1 day)
- Week Warrior (7 days)
- Month Master (30 days)
- Century Club (100 days)

### Total Badges
- Getting Started (10 total)
- Habit Builder (50 total)
- Dedication Master (500 total)

### Consistency Badges
- Consistent Performer (80% consistency)
- Perfectionist (100% consistency)

### Special Badges
- Early Bird (10 early check-ins)
- Night Owl (10 late check-ins)

## Habit Entries

For each habit, entries are created for the last 30 days with:
- 80% completion rate (simulating realistic usage)
- Random completion times between 6 AM and 10 PM
- Occasional notes on milestone days
- Proper streak day tracking

## Check-ins

Recent check-ins are created for each habit with:
- 90% check-in rate for the last 7 days
- Random completion times
- Occasional notes
- Proper streak day tracking
- Mix of manual and automatic entries

## User Badges

Each test user is awarded 2-4 random badges to simulate achievement progression.

## Encouragements

Sample encouragement messages are created between users with:
- Various encouragement types
- Realistic messages with emojis
- Mix of read and unread status
- Different creation dates

## Usage

### Development

1. Ensure you're running in development mode
2. Start the application - seed data will be automatically loaded
3. Use the test user emails for API testing (no passwords required)

### Testing

1. The seed data provides a consistent baseline for testing
2. All test users have realistic data that can be used for UI testing
3. Badge progression can be tested with the existing user data

### API Testing

You can use the following endpoints with the test data:

```http
# Get all habits for a user
GET /api/habits?userId=test-user-1

# Get user badges
GET /api/userbadges?userId=test-user-1

# Get habit entries
GET /api/habitentries?habitId={habitId}

# Get check-ins
GET /api/checkins?habitId={habitId}
```

## Customization

To customize the seed data:

1. Modify the `DbSeeder.cs` file in `HabitChain.Infrastructure/Data/`
2. Update the configuration in `appsettings.Development.json`
3. Restart the application to see changes

## Database Reset

To reset the database and re-seed:

1. Delete the database: `dropdb HabitChainDb_Dev`
2. Restart the application - it will recreate and seed the database

## Notes

- Seed data is only created if the corresponding tables are empty
- This prevents duplicate data when restarting the application
- In production, only badges are seeded automatically
- Test users are created without password hashing for simplicity
- All timestamps use UTC to avoid timezone issues 