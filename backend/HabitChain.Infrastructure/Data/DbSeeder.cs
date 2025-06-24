using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(HabitChainDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Badges if they don't exist
        if (!await context.Badges.AnyAsync())
        {
            var badges = new List<Badge>
            {
                // Streak Badges
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "First Steps",
                    Description = "Complete your first habit check-in",
                    IconUrl = "/icons/first-steps.svg",
                    Type = BadgeType.Streak,
                    RequiredValue = 1,
                    IsActive = true
                },
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Week Warrior",
                    Description = "Maintain a 7-day streak",
                    IconUrl = "/icons/week-warrior.svg",
                    Type = BadgeType.Streak,
                    RequiredValue = 7,
                    IsActive = true
                },
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Month Master",
                    Description = "Maintain a 30-day streak",
                    IconUrl = "/icons/month-master.svg",
                    Type = BadgeType.Streak,
                    RequiredValue = 30,
                    IsActive = true
                },
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Century Club",
                    Description = "Maintain a 100-day streak",
                    IconUrl = "/icons/century-club.svg",
                    Type = BadgeType.Streak,
                    RequiredValue = 100,
                    IsActive = true
                },

                // Total Badges
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Getting Started",
                    Description = "Complete 10 total check-ins",
                    IconUrl = "/icons/getting-started.svg",
                    Type = BadgeType.Total,
                    RequiredValue = 10,
                    IsActive = true
                },
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Habit Builder",
                    Description = "Complete 50 total check-ins",
                    IconUrl = "/icons/habit-builder.svg",
                    Type = BadgeType.Total,
                    RequiredValue = 50,
                    IsActive = true
                },
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Dedication Master",
                    Description = "Complete 500 total check-ins",
                    IconUrl = "/icons/dedication-master.svg",
                    Type = BadgeType.Total,
                    RequiredValue = 500,
                    IsActive = true
                },

                // Consistency Badges
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Consistent Performer",
                    Description = "Maintain 80% consistency for a month",
                    IconUrl = "/icons/consistent-performer.svg",
                    Type = BadgeType.Consistency,
                    RequiredValue = 80,
                    IsActive = true
                },
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Perfectionist",
                    Description = "Maintain 100% consistency for a week",
                    IconUrl = "/icons/perfectionist.svg",
                    Type = BadgeType.Consistency,
                    RequiredValue = 100,
                    IsActive = true
                },

                // Special Badges
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Early Bird",
                    Description = "Complete 10 check-ins before 8 AM",
                    IconUrl = "/icons/early-bird.svg",
                    Type = BadgeType.Special,
                    RequiredValue = 10,
                    IsActive = true
                },
                new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = "Night Owl",
                    Description = "Complete 10 check-ins after 10 PM",
                    IconUrl = "/icons/night-owl.svg",
                    Type = BadgeType.Special,
                    RequiredValue = 10,
                    IsActive = true
                }
            };

            await context.Badges.AddRangeAsync(badges);
            await context.SaveChangesAsync();
        }

        // Seed test users if they don't exist
        if (!await context.Users.AnyAsync())
        {
            var testUsers = new List<User>
            {
                new User
                {
                    Id = "test-user-1",
                    UserName = "john.doe@test.com",
                    Email = "john.doe@test.com",
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Doe",
                    ProfilePictureUrl = "https://via.placeholder.com/150/4CAF50/FFFFFF?text=JD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = "test-user-2",
                    UserName = "jane.smith@test.com",
                    Email = "jane.smith@test.com",
                    EmailConfirmed = true,
                    FirstName = "Jane",
                    LastName = "Smith",
                    ProfilePictureUrl = "https://via.placeholder.com/150/2196F3/FFFFFF?text=JS",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = "test-user-3",
                    UserName = "mike.wilson@test.com",
                    Email = "mike.wilson@test.com",
                    EmailConfirmed = true,
                    FirstName = "Mike",
                    LastName = "Wilson",
                    ProfilePictureUrl = "https://via.placeholder.com/150/FF9800/FFFFFF?text=MW",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(testUsers);
            await context.SaveChangesAsync();
        }

        // Seed test habits if they don't exist
        if (!await context.Habits.AnyAsync())
        {
            var testHabits = new List<Habit>
            {
                // John's habits
                new Habit
                {
                    Id = Guid.NewGuid(),
                    Name = "Morning Exercise",
                    Description = "30 minutes of cardio and strength training",
                    Frequency = HabitFrequency.Daily,
                    UserId = "test-user-1",
                    IsActive = true,
                    Color = "#4CAF50",
                    IconName = "fitness",
                    CurrentStreak = 15,
                    LongestStreak = 25,
                    LastCompletedAt = DateTime.UtcNow.AddHours(-2),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow
                },
                new Habit
                {
                    Id = Guid.NewGuid(),
                    Name = "Read Books",
                    Description = "Read at least 20 pages daily",
                    Frequency = HabitFrequency.Daily,
                    UserId = "test-user-1",
                    IsActive = true,
                    Color = "#2196F3",
                    IconName = "book",
                    CurrentStreak = 8,
                    LongestStreak = 12,
                    LastCompletedAt = DateTime.UtcNow.AddHours(-4),
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow
                },
                new Habit
                {
                    Id = Guid.NewGuid(),
                    Name = "Meditation",
                    Description = "10 minutes of mindfulness meditation",
                    Frequency = HabitFrequency.Daily,
                    UserId = "test-user-1",
                    IsActive = true,
                    Color = "#9C27B0",
                    IconName = "meditation",
                    CurrentStreak = 22,
                    LongestStreak = 22,
                    LastCompletedAt = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-22),
                    UpdatedAt = DateTime.UtcNow
                },

                // Jane's habits
                new Habit
                {
                    Id = Guid.NewGuid(),
                    Name = "Drink Water",
                    Description = "Drink 8 glasses of water daily",
                    Frequency = HabitFrequency.Daily,
                    UserId = "test-user-2",
                    IsActive = true,
                    Color = "#00BCD4",
                    IconName = "water",
                    CurrentStreak = 5,
                    LongestStreak = 18,
                    LastCompletedAt = DateTime.UtcNow.AddHours(-3),
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow
                },
                new Habit
                {
                    Id = Guid.NewGuid(),
                    Name = "Practice Guitar",
                    Description = "Practice guitar for 30 minutes",
                    Frequency = HabitFrequency.Daily,
                    UserId = "test-user-2",
                    IsActive = true,
                    Color = "#FF9800",
                    IconName = "music",
                    CurrentStreak = 12,
                    LongestStreak = 12,
                    LastCompletedAt = DateTime.UtcNow.AddHours(-6),
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow
                },

                // Mike's habits
                new Habit
                {
                    Id = Guid.NewGuid(),
                    Name = "Coding Practice",
                    Description = "Work on coding projects for 1 hour",
                    Frequency = HabitFrequency.Daily,
                    UserId = "test-user-3",
                    IsActive = true,
                    Color = "#607D8B",
                    IconName = "code",
                    CurrentStreak = 3,
                    LongestStreak = 10,
                    LastCompletedAt = DateTime.UtcNow.AddHours(-8),
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow
                },
                new Habit
                {
                    Id = Guid.NewGuid(),
                    Name = "Weekly Review",
                    Description = "Review goals and plan for next week",
                    Frequency = HabitFrequency.Weekly,
                    UserId = "test-user-3",
                    IsActive = true,
                    Color = "#795548",
                    IconName = "calendar",
                    CurrentStreak = 2,
                    LongestStreak = 4,
                    LastCompletedAt = DateTime.UtcNow.AddDays(-2),
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Habits.AddRangeAsync(testHabits);
            await context.SaveChangesAsync();
        }

        // Seed test habit entries if they don't exist
        if (!await context.HabitEntries.AnyAsync())
        {
            var habits = await context.Habits.ToListAsync();
            var entries = new List<HabitEntry>();

            foreach (var habit in habits)
            {
                // Create entries for the last 30 days
                for (int i = 0; i < 30; i++)
                {
                    var entryDate = DateTime.UtcNow.AddDays(-i);
                    
                    // Skip some days randomly to simulate real usage
                    if (new Random().Next(1, 10) > 2) // 80% completion rate
                    {
                        entries.Add(new HabitEntry
                        {
                            Id = Guid.NewGuid(),
                            HabitId = habit.Id,
                            CompletedAt = entryDate.AddHours(new Random().Next(6, 22)), // Random time during day
                            Notes = i % 5 == 0 ? $"Great progress on day {30 - i}!" : null,
                            CreatedAt = entryDate,
                            UpdatedAt = entryDate
                        });
                    }
                }
            }

            await context.HabitEntries.AddRangeAsync(entries);
            await context.SaveChangesAsync();
        }

        // Seed test check-ins if they don't exist
        if (!await context.CheckIns.AnyAsync())
        {
            var habits = await context.Habits.ToListAsync();
            var checkIns = new List<CheckIn>();

            foreach (var habit in habits)
            {
                // Create check-ins for the last 7 days
                for (int i = 0; i < 7; i++)
                {
                    var checkInDate = DateTime.UtcNow.AddDays(-i);
                    
                    // 90% check-in rate
                    if (new Random().Next(1, 10) > 1)
                    {
                        checkIns.Add(new CheckIn
                        {
                            Id = Guid.NewGuid(),
                            HabitId = habit.Id,
                            UserId = habit.UserId,
                            CompletedAt = checkInDate.AddHours(new Random().Next(6, 22)),
                            Notes = i % 3 == 0 ? $"Feeling good about progress!" : null,
                            StreakDay = i + 1,
                            IsManualEntry = new Random().Next(0, 2) == 1,
                            CreatedAt = checkInDate,
                            UpdatedAt = checkInDate
                        });
                    }
                }
            }

            await context.CheckIns.AddRangeAsync(checkIns);
            await context.SaveChangesAsync();
        }

        // Seed test user badges if they don't exist
        if (!await context.UserBadges.AnyAsync())
        {
            var badges = await context.Badges.ToListAsync();
            var users = await context.Users.ToListAsync();
            var userBadges = new List<UserBadge>();

            foreach (var user in users)
            {
                // Award some badges to each user
                var userBadgeCount = new Random().Next(2, 5);
                var selectedBadges = badges.OrderBy(x => Guid.NewGuid()).Take(userBadgeCount);

                foreach (var badge in selectedBadges)
                {
                    userBadges.Add(new UserBadge
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        BadgeId = badge.Id,
                        EarnedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)),
                        CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)),
                        UpdatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30))
                    });
                }
            }

            await context.UserBadges.AddRangeAsync(userBadges);
            await context.SaveChangesAsync();
        }

        // Seed test encouragements if they don't exist
        if (!await context.Encouragements.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            var encouragements = new List<Encouragement>();

            // Create some encouragements between users
            for (int i = 0; i < 10; i++)
            {
                var sender = users[new Random().Next(users.Count)];
                var receiver = users.FirstOrDefault(u => u.Id != sender.Id);
                
                if (receiver != null)
                {
                    encouragements.Add(new Encouragement
                    {
                        Id = Guid.NewGuid(),
                        FromUserId = sender.Id,
                        ToUserId = receiver.Id,
                        Type = (EncouragementType)new Random().Next(0, 3),
                        Message = GetRandomEncouragementMessage(),
                        IsRead = new Random().Next(0, 2) == 1,
                        CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 15)),
                        UpdatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 15))
                    });
                }
            }

            await context.Encouragements.AddRangeAsync(encouragements);
            await context.SaveChangesAsync();
        }
    }

    private static string GetRandomEncouragementMessage()
    {
        var messages = new[]
        {
            "You're doing amazing! Keep up the great work! 💪",
            "Every step counts towards your goals! 🌟",
            "You've got this! Your consistency is inspiring! ✨",
            "Don't give up! You're making incredible progress! 🎯",
            "Your dedication is truly remarkable! Keep going! 🚀",
            "Small steps lead to big changes! You're on fire! 🔥",
            "Your habit-building journey is inspiring others! 🌈",
            "Remember why you started! You're unstoppable! 💎",
            "Every day is a new opportunity to grow! 🌱",
            "Your future self will thank you for this! 🙏"
        };

        return messages[new Random().Next(messages.Length)];
    }
} 