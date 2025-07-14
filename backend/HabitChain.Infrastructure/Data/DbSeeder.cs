using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(HabitChainDbContext context)
    {
        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Badges if they don't exist
            if (!await context.Badges.AnyAsync())
            {
                await SeedBadgesAsync(context);
            }

            // Seed test users if they don't exist
            if (!await context.Users.AnyAsync())
            {
                await SeedTestUsersAsync(context);
            }

            // Seed test habits if they don't exist
            if (!await context.Habits.AnyAsync())
            {
                await SeedTestHabitsAsync(context);
            }

            // Seed test check-ins if they don't exist
            if (!await context.CheckIns.AnyAsync())
            {
                await SeedTestCheckInsAsync(context);
            }

            // Seed test encouragements if they don't exist
            if (!await context.Encouragements.AnyAsync())
            {
                await SeedTestEncouragementsAsync(context);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during database seeding: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw; // Re-throw to let the calling code handle it
        }
    }

    private static async Task SeedBadgesAsync(HabitChainDbContext context)
    {
        var badges = new List<Badge>
        {
            // 🎯 Milestone Badges
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "First Steps",
                Description = "Create your very first habit",
                IconUrl = "/icons/first-steps.svg",
                Emoji = "🎯",
                Type = BadgeType.Milestone,
                Category = "milestone",
                Rarity = "common",
                RequiredValue = 1,
                IsActive = true,
                ColorTheme = "#28a745",
                IsSecret = false,
                DisplayOrder = 1
            },
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Habit Collector",
                Description = "Create 5 different habits",
                IconUrl = "/icons/habit-collector.svg",
                Emoji = "📚",
                Type = BadgeType.Milestone,
                Category = "milestone",
                Rarity = "rare",
                RequiredValue = 5,
                IsActive = true,
                ColorTheme = "#007bff",
                IsSecret = false,
                DisplayOrder = 2
            },
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Habit Master",
                Description = "Create 10 different habits",
                IconUrl = "/icons/habit-master.svg",
                Emoji = "👑",
                Type = BadgeType.Milestone,
                Category = "milestone",
                Rarity = "epic",
                RequiredValue = 10,
                IsActive = true,
                ColorTheme = "#6f42c1",
                IsSecret = false,
                DisplayOrder = 3
            },

            // 🔥 Streak Badges
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Week Warrior",
                Description = "Maintain a 7-day streak",
                IconUrl = "/icons/week-warrior.svg",
                Emoji = "🔥",
                Type = BadgeType.Streak,
                Category = "streak",
                Rarity = "common",
                RequiredValue = 7,
                IsActive = true,
                ColorTheme = "#fd7e14",
                IsSecret = false,
                DisplayOrder = 4
            },
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Month Master",
                Description = "Maintain a 30-day streak",
                IconUrl = "/icons/month-master.svg",
                Emoji = "🏆",
                Type = BadgeType.Streak,
                Category = "streak",
                Rarity = "rare",
                RequiredValue = 30,
                IsActive = true,
                ColorTheme = "#007bff",
                IsSecret = false,
                DisplayOrder = 5
            },
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Century Club",
                Description = "Maintain a 100-day streak",
                IconUrl = "/icons/century-club.svg",
                Emoji = "💎",
                Type = BadgeType.Streak,
                Category = "streak",
                Rarity = "legendary",
                RequiredValue = 100,
                IsActive = true,
                ColorTheme = "#fd7e14",
                IsSecret = false,
                DisplayOrder = 6
            },

            // 📊 Total Badges
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Getting Started",
                Description = "Complete 10 total check-ins",
                IconUrl = "/icons/getting-started.svg",
                Emoji = "📊",
                Type = BadgeType.Total,
                Category = "milestone",
                Rarity = "common",
                RequiredValue = 10,
                IsActive = true,
                ColorTheme = "#28a745",
                IsSecret = false,
                DisplayOrder = 7
            },
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Habit Builder",
                Description = "Complete 50 total check-ins",
                IconUrl = "/icons/habit-builder.svg",
                Emoji = "🏗️",
                Type = BadgeType.Total,
                Category = "milestone",
                Rarity = "rare",
                RequiredValue = 50,
                IsActive = true,
                ColorTheme = "#007bff",
                IsSecret = false,
                DisplayOrder = 8
            },
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Dedication Master",
                Description = "Complete 500 total check-ins",
                IconUrl = "/icons/dedication-master.svg",
                Emoji = "🎖️",
                Type = BadgeType.Total,
                Category = "milestone",
                Rarity = "epic",
                RequiredValue = 500,
                IsActive = true,
                ColorTheme = "#6f42c1",
                IsSecret = false,
                DisplayOrder = 9
            },
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Legend",
                Description = "Complete 1000 total check-ins",
                IconUrl = "/icons/legend.svg",
                Emoji = "🌟",
                Type = BadgeType.Total,
                Category = "milestone",
                Rarity = "legendary",
                RequiredValue = 1000,
                IsActive = true,
                ColorTheme = "#fd7e14",
                IsSecret = false,
                DisplayOrder = 10
            },

            // ⏰ Time-based Badges
            new Badge
            {
                Id = Guid.NewGuid(),
                Name = "Early Bird",
                Description = "Complete 10 check-ins before 8 AM",
                IconUrl = "/icons/early-bird.svg",
                Emoji = "🌅",
                Type = BadgeType.TimeBased,
                Category = "time",
                Rarity = "rare",
                RequiredValue = 10,
                IsActive = true,
                ColorTheme = "#ffc107",
                IsSecret = false,
                DisplayOrder = 11
            }
        };

        await context.Badges.AddRangeAsync(badges);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTestUsersAsync(HabitChainDbContext context)
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
                ProfilePictureUrl = "https://via.placeholder.com/150/007BFF/FFFFFF?text=JD",
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
                ProfilePictureUrl = "https://via.placeholder.com/150/28A745/FFFFFF?text=JS",
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

    private static async Task SeedTestHabitsAsync(HabitChainDbContext context)
    {
        var testHabits = new List<Habit>
        {
            // John's habits
            new Habit
            {
                Id = Guid.NewGuid(),
                UserId = "test-user-1",
                Name = "Morning Exercise",
                Description = "30 minutes of cardio and strength training",
                Frequency = HabitFrequency.Daily,
                TargetDays = 7,
                IsActive = true,
                CurrentStreak = 5,
                LongestStreak = 12,
                LastCompletedAt = DateTime.UtcNow.AddHours(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow
            },
            new Habit
            {
                Id = Guid.NewGuid(),
                UserId = "test-user-1",
                Name = "Read 30 Minutes",
                Description = "Read a book for 30 minutes before bed",
                Frequency = HabitFrequency.Daily,
                TargetDays = 7,
                IsActive = true,
                CurrentStreak = 3,
                LongestStreak = 8,
                LastCompletedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow
            },

            // Jane's habits
            new Habit
            {
                Id = Guid.NewGuid(),
                UserId = "test-user-2",
                Name = "Meditation",
                Description = "10 minutes of mindfulness meditation",
                Frequency = HabitFrequency.Daily,
                TargetDays = 7,
                IsActive = true,
                CurrentStreak = 7,
                LongestStreak = 7,
                LastCompletedAt = DateTime.UtcNow.AddHours(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow
            },
            new Habit
            {
                Id = Guid.NewGuid(),
                UserId = "test-user-2",
                Name = "Drink Water",
                Description = "Drink 8 glasses of water daily",
                Frequency = HabitFrequency.Daily,
                TargetDays = 7,
                IsActive = true,
                CurrentStreak = 2,
                LongestStreak = 5,
                LastCompletedAt = DateTime.UtcNow.AddHours(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow
            },

            // Mike's habits
            new Habit
            {
                Id = Guid.NewGuid(),
                UserId = "test-user-3",
                Name = "Write Journal",
                Description = "Write in journal for 15 minutes",
                Frequency = HabitFrequency.Daily,
                TargetDays = 7,
                IsActive = true,
                CurrentStreak = 1,
                LongestStreak = 3,
                LastCompletedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.Habits.AddRangeAsync(testHabits);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTestCheckInsAsync(HabitChainDbContext context)
    {
        var testCheckIns = new List<CheckIn>();
        var habits = await context.Habits.ToListAsync();

        foreach (var habit in habits)
        {
            // Generate some check-ins for the past week
            for (int i = 0; i < 7; i++)
            {
                var checkInDate = DateTime.UtcNow.AddDays(-i);
                if (i < habit.CurrentStreak) // Only create check-ins for the current streak
                {
                    testCheckIns.Add(new CheckIn
                    {
                        Id = Guid.NewGuid(),
                        UserId = habit.UserId,
                        HabitId = habit.Id,
                        CompletedAt = checkInDate,
                        StreakDay = habit.CurrentStreak - i,
                        CreatedAt = checkInDate,
                        UpdatedAt = checkInDate
                    });
                }
            }
        }

        await context.CheckIns.AddRangeAsync(testCheckIns);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTestEncouragementsAsync(HabitChainDbContext context)
    {
        var encouragements = new List<Encouragement>
        {
            new Encouragement
            {
                Id = Guid.NewGuid(),
                FromUserId = "test-user-1",
                ToUserId = "test-user-2",
                HabitId = context.Habits.First(h => h.UserId == "test-user-2").Id,
                Message = "Great job with your meditation habit! Keep it up!",
                Type = EncouragementType.Support,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            },
            new Encouragement
            {
                Id = Guid.NewGuid(),
                FromUserId = "test-user-2",
                ToUserId = "test-user-1",
                HabitId = context.Habits.First(h => h.UserId == "test-user-1").Id,
                Message = "Your exercise routine is inspiring! You're doing amazing!",
                Type = EncouragementType.Motivation,
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.Encouragements.AddRangeAsync(encouragements);
        await context.SaveChangesAsync();
    }

    private static string GetRandomEncouragementMessage()
    {
        var messages = new[]
        {
            "You're doing great! Keep up the amazing work!",
            "Every step counts towards your goals!",
            "You're building a better version of yourself!",
            "Consistency is the key to success!",
            "Your dedication is inspiring!",
            "You're making progress every day!",
            "Keep pushing forward, you've got this!",
            "Your habits are shaping your future!",
            "Small actions lead to big changes!",
            "You're creating positive change in your life!"
        };

        var random = new Random();
        return messages[random.Next(messages.Length)];
    }
} 