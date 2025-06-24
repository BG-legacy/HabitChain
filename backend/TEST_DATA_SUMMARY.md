# Test Data Summary

Quick reference for the seed data available in development mode.

## Test Users

| ID | Email | Name | Created |
|----|-------|------|---------|
| test-user-1 | john.doe@test.com | John Doe | 30 days ago |
| test-user-2 | jane.smith@test.com | Jane Smith | 25 days ago |
| test-user-3 | mike.wilson@test.com | Mike Wilson | 20 days ago |

## Test Habits

### John Doe (test-user-1)
- **Morning Exercise**: Daily, 15-day streak, green (#4CAF50)
- **Read Books**: Daily, 8-day streak, blue (#2196F3)
- **Meditation**: Daily, 22-day streak, purple (#9C27B0)

### Jane Smith (test-user-2)
- **Drink Water**: Daily, 5-day streak, cyan (#00BCD4)
- **Practice Guitar**: Daily, 12-day streak, orange (#FF9800)

### Mike Wilson (test-user-3)
- **Coding Practice**: Daily, 3-day streak, blue-grey (#607D8B)
- **Weekly Review**: Weekly, 2-week streak, brown (#795548)

## Badges Available

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

## Sample Data Volumes

- **Habit Entries**: ~30 days per habit (80% completion rate)
- **Check-ins**: ~7 days per habit (90% check-in rate)
- **User Badges**: 2-4 random badges per user
- **Encouragements**: 10 sample messages between users

## Quick Commands

```bash
# Check database status
./dev-db.sh status

# Reset database
./dev-db.sh reset

# Start application (seeds automatically)
dotnet run --project HabitChain.WebAPI
```

## API Testing Examples

```http
# Get all habits for John
GET /api/habits?userId=test-user-1

# Get John's badges
GET /api/userbadges?userId=test-user-1

# Get habit entries for a specific habit
GET /api/habitentries?habitId={habitId}

# Get check-ins for a habit
GET /api/checkins?habitId={habitId}

# Get encouragements for a user
GET /api/encouragements?userId=test-user-1
```

## Notes

- All test users have `EmailConfirmed = true`
- No passwords are set (for testing purposes only)
- All timestamps are in UTC
- Data is only seeded if tables are empty
- Production mode only seeds badges 