using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Data;

public static class DbSeeder
{
    // Use static GUIDs to ensure consistency across runs
    private static readonly Guid Badge1Id = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Badge2Id = new Guid("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Badge3Id = new Guid("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Badge4Id = new Guid("44444444-4444-4444-4444-444444444444");
    private static readonly Guid Badge5Id = new Guid("55555555-5555-5555-5555-555555555555");
    private static readonly Guid Badge6Id = new Guid("66666666-6666-6666-6666-666666666666");
    private static readonly Guid Badge7Id = new Guid("77777777-7777-7777-7777-777777777777");
    private static readonly Guid Badge8Id = new Guid("88888888-8888-8888-8888-888888888888");
    private static readonly Guid Badge9Id = new Guid("99999999-9999-9999-9999-999999999999");
    private static readonly Guid Badge10Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Badge11Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    // Static GUIDs for habits
    private static readonly Guid Habit1Id = new Guid("11111111-1111-1111-1111-111111111001");
    private static readonly Guid Habit2Id = new Guid("22222222-2222-2222-2222-222222222002");
    private static readonly Guid Habit3Id = new Guid("33333333-3333-3333-3333-333333333003");
    private static readonly Guid Habit4Id = new Guid("44444444-4444-4444-4444-444444444004");
    private static readonly Guid Habit5Id = new Guid("55555555-5555-5555-5555-555555555005");

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
                Id = Badge1Id,
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
                Id = Badge2Id,
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
                Id = Badge3Id,
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
                Id = Badge4Id,
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
                Id = Badge5Id,
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
                Id = Badge6Id,
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
                Id = Badge7Id,
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
                Id = Badge8Id,
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
                Id = Badge9Id,
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
                Id = Badge10Id,
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
                Id = Badge11Id,
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
                Id = Habit1Id,
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
                Id = Habit2Id,
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
                Id = Habit3Id,
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
                Id = Habit4Id,
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
                Id = Habit5Id,
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
        
        // Use deterministic IDs for check-ins to avoid duplicates
        var checkInIdCounter = 1;

        // Create check-ins for Habit1 (Morning Exercise - 5 day streak)
        for (int i = 0; i < 5; i++)
        {
            var checkInDate = DateTime.UtcNow.AddDays(-i);
            testCheckIns.Add(new CheckIn
            {
                Id = new Guid($"10000000-0000-0000-0000-{checkInIdCounter:D12}"),
                UserId = "test-user-1",
                HabitId = Habit1Id,
                CompletedAt = checkInDate,
                StreakDay = 5 - i,
                CreatedAt = checkInDate,
                UpdatedAt = checkInDate
            });
            checkInIdCounter++;
        }

        // Create check-ins for Habit2 (Read 30 Minutes - 3 day streak)
        for (int i = 0; i < 3; i++)
        {
            var checkInDate = DateTime.UtcNow.AddDays(-i);
            testCheckIns.Add(new CheckIn
            {
                Id = new Guid($"10000000-0000-0000-0000-{checkInIdCounter:D12}"),
                UserId = "test-user-1",
                HabitId = Habit2Id,
                CompletedAt = checkInDate,
                StreakDay = 3 - i,
                CreatedAt = checkInDate,
                UpdatedAt = checkInDate
            });
            checkInIdCounter++;
        }

        // Create check-ins for Habit3 (Meditation - 7 day streak)
        for (int i = 0; i < 7; i++)
        {
            var checkInDate = DateTime.UtcNow.AddDays(-i);
            testCheckIns.Add(new CheckIn
            {
                Id = new Guid($"10000000-0000-0000-0000-{checkInIdCounter:D12}"),
                UserId = "test-user-2",
                HabitId = Habit3Id,
                CompletedAt = checkInDate,
                StreakDay = 7 - i,
                CreatedAt = checkInDate,
                UpdatedAt = checkInDate
            });
            checkInIdCounter++;
        }

        // Create check-ins for Habit4 (Drink Water - 2 day streak)
        for (int i = 0; i < 2; i++)
        {
            var checkInDate = DateTime.UtcNow.AddDays(-i);
            testCheckIns.Add(new CheckIn
            {
                Id = new Guid($"10000000-0000-0000-0000-{checkInIdCounter:D12}"),
                UserId = "test-user-2",
                HabitId = Habit4Id,
                CompletedAt = checkInDate,
                StreakDay = 2 - i,
                CreatedAt = checkInDate,
                UpdatedAt = checkInDate
            });
            checkInIdCounter++;
        }

        // Create check-ins for Habit5 (Write Journal - 1 day streak)
        var journalCheckInDate = DateTime.UtcNow;
        testCheckIns.Add(new CheckIn
        {
            Id = new Guid($"10000000-0000-0000-0000-{checkInIdCounter:D12}"),
            UserId = "test-user-3",
            HabitId = Habit5Id,
            CompletedAt = journalCheckInDate,
            StreakDay = 1,
            CreatedAt = journalCheckInDate,
            UpdatedAt = journalCheckInDate
        });

        await context.CheckIns.AddRangeAsync(testCheckIns);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTestEncouragementsAsync(HabitChainDbContext context)
    {
        var encouragements = new List<Encouragement>
        {
            new Encouragement
            {
                Id = new Guid("20000000-0000-0000-0000-000000000001"),
                FromUserId = "test-user-1",
                ToUserId = "test-user-2",
                HabitId = Habit3Id, // Meditation habit
                Message = "Great job with your meditation habit! Keep it up!",
                Type = EncouragementType.Support,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            },
            new Encouragement
            {
                Id = new Guid("20000000-0000-0000-0000-000000000002"),
                FromUserId = "test-user-2",
                ToUserId = "test-user-1",
                HabitId = Habit1Id, // Morning Exercise habit
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