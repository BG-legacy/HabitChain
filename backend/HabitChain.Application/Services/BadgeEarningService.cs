using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using HabitChain.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace HabitChain.Application.Services;

public class BadgeEarningService : IBadgeEarningService
{
    private readonly IBadgeRepository _badgeRepository;
    private readonly IUserBadgeRepository _userBadgeRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly IHabitRepository _habitRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BadgeEarningService> _logger;

    public BadgeEarningService(
        IBadgeRepository badgeRepository,
        IUserBadgeRepository userBadgeRepository,
        ICheckInRepository checkInRepository,
        IHabitRepository habitRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<BadgeEarningService> logger)
    {
        _badgeRepository = badgeRepository;
        _userBadgeRepository = userBadgeRepository;
        _checkInRepository = checkInRepository;
        _habitRepository = habitRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<BadgeDto>> CheckAndAwardBadgesAsync(string userId, Guid? habitId = null)
    {
        var earnedBadges = new List<BadgeDto>();
        
        try
        {
            // Get all active badges
            var allBadges = await _badgeRepository.GetActiveBadgesAsync();
            var userBadges = await _userBadgeRepository.GetUserBadgesAsync(userId);
            var earnedBadgeIds = userBadges.Select(ub => ub.BadgeId).ToHashSet();

            // Get user's habits and check-ins
            var userHabits = await _habitRepository.GetHabitsByUserIdAsync(userId);
            var userCheckIns = await _checkInRepository.GetCheckInsByUserIdAsync(userId);

            foreach (var badge in allBadges)
            {
                // Skip if already earned
                if (earnedBadgeIds.Contains(badge.Id))
                    continue;

                // Skip secret badges unless they should be revealed
                if (badge.IsSecret && !ShouldRevealSecretBadge(badge, userHabits, userCheckIns))
                    continue;

                // Check if user qualifies for this badge
                if (await CheckBadgeEligibilityAsync(badge, userId, userHabits, userCheckIns, habitId))
                {
                    var earnedBadge = await AwardBadgeAsync(userId, badge.Id, habitId);
                    if (earnedBadge != null)
                    {
                        earnedBadges.Add(earnedBadge);
                        _logger.LogInformation($"User {userId} earned badge: {badge.Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking badges for user {userId}");
        }

        return earnedBadges;
    }

    private async Task<bool> CheckBadgeEligibilityAsync(Badge badge, string userId, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns, Guid? habitId)
    {
        return badge.Type switch
        {
            BadgeType.Streak => await CheckStreakBadgeAsync(badge, userHabits, habitId),
            BadgeType.Total => await CheckTotalBadgeAsync(badge, userCheckIns),
            BadgeType.Consistency => await CheckConsistencyBadgeAsync(badge, userHabits, userCheckIns),
            BadgeType.Special => await CheckSpecialBadgeAsync(badge, userCheckIns),
            BadgeType.Milestone => await CheckMilestoneBadgeAsync(badge, userHabits, userCheckIns),
            BadgeType.Social => await CheckSocialBadgeAsync(badge, userId),
            BadgeType.Creation => await CheckCreationBadgeAsync(badge, userHabits),
            BadgeType.TimeBased => await CheckTimeBasedBadgeAsync(badge, userCheckIns),
            BadgeType.Challenge => await CheckChallengeBadgeAsync(badge, userHabits, userCheckIns),
            BadgeType.Seasonal => await CheckSeasonalBadgeAsync(badge, userCheckIns),
            BadgeType.Rarity => await CheckRarityBadgeAsync(badge, userHabits, userCheckIns),
            BadgeType.Chain => await CheckChainBadgeAsync(badge, userHabits, userCheckIns),
            _ => false
        };
    }

    private async Task<bool> CheckStreakBadgeAsync(Badge badge, IEnumerable<Habit> userHabits, Guid? habitId)
    {
        var habitsToCheck = habitId.HasValue 
            ? userHabits.Where(h => h.Id == habitId.Value)
            : userHabits;

        return habitsToCheck.Any(h => h.CurrentStreak >= badge.RequiredValue);
    }

    private async Task<bool> CheckTotalBadgeAsync(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        var totalCheckIns = userCheckIns.Count();
        return totalCheckIns >= badge.RequiredValue;
    }

    private async Task<bool> CheckConsistencyBadgeAsync(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        var activeHabits = userHabits.Where(h => h.IsActive).ToList();
        if (!activeHabits.Any()) return false;

        // Calculate overall consistency rate
        var totalPossible = activeHabits.Sum(h => CalculatePossibleCompletions(h));
        var totalActual = userCheckIns.Count();
        var consistencyRate = totalPossible > 0 ? (double)totalActual / totalPossible * 100 : 0;

        return consistencyRate >= badge.RequiredValue;
    }

    private async Task<bool> CheckSpecialBadgeAsync(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        // Early Bird badge
        if (badge.Name.Contains("Early Bird", StringComparison.OrdinalIgnoreCase))
        {
            var earlyCheckIns = userCheckIns.Where(c => c.CompletedAt.Hour < 8).Count();
            return earlyCheckIns >= badge.RequiredValue;
        }

        // Night Owl badge
        if (badge.Name.Contains("Night Owl", StringComparison.OrdinalIgnoreCase))
        {
            var nightCheckIns = userCheckIns.Where(c => c.CompletedAt.Hour >= 22).Count();
            return nightCheckIns >= badge.RequiredValue;
        }

        // Weekend Warrior badge
        if (badge.Name.Contains("Weekend Warrior", StringComparison.OrdinalIgnoreCase))
        {
            var weekendCheckIns = userCheckIns.Where(c => 
                c.CompletedAt.DayOfWeek == DayOfWeek.Saturday || 
                c.CompletedAt.DayOfWeek == DayOfWeek.Sunday).Count();
            return weekendCheckIns >= badge.RequiredValue;
        }

        return false;
    }

    private async Task<bool> CheckMilestoneBadgeAsync(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        var totalHabits = userHabits.Count();
        var totalCheckIns = userCheckIns.Count();
        var activeHabits = userHabits.Count(h => h.IsActive);

        return badge.RequiredValue switch
        {
            1 => totalHabits >= 1, // First Habit
            5 => totalHabits >= 5, // Habit Collector
            10 => totalHabits >= 10, // Habit Master
            100 => totalCheckIns >= 100, // Centurion
            500 => totalCheckIns >= 500, // Dedication Master
            1000 => totalCheckIns >= 1000, // Legend
            _ => false
        };
    }

    private async Task<bool> CheckSocialBadgeAsync(Badge badge, string userId)
    {
        // For now, return false as social features aren't fully implemented
        // This would check for encouragements sent/received, habit sharing, etc.
        return false;
    }

    private async Task<bool> CheckCreationBadgeAsync(Badge badge, IEnumerable<Habit> userHabits)
    {
        var totalHabits = userHabits.Count();
        return totalHabits >= badge.RequiredValue;
    }

    private async Task<bool> CheckTimeBasedBadgeAsync(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        var now = DateTime.UtcNow;
        var recentCheckIns = userCheckIns.Where(c => c.CompletedAt >= now.AddDays(-7));

        // Daily Check-in Streak
        if (badge.Name.Contains("Daily", StringComparison.OrdinalIgnoreCase))
        {
            var dailyStreak = CalculateDailyStreak(recentCheckIns);
            return dailyStreak >= badge.RequiredValue;
        }

        // Weekly Consistency
        if (badge.Name.Contains("Weekly", StringComparison.OrdinalIgnoreCase))
        {
            var weeklyCheckIns = recentCheckIns.Count();
            return weeklyCheckIns >= badge.RequiredValue;
        }

        return false;
    }

    private async Task<bool> CheckChallengeBadgeAsync(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        // 7-Day Challenge
        if (badge.Name.Contains("7-Day", StringComparison.OrdinalIgnoreCase))
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var weeklyCheckIns = userCheckIns.Where(c => c.CompletedAt >= weekAgo).Count();
            return weeklyCheckIns >= 7;
        }

        // 30-Day Challenge
        if (badge.Name.Contains("30-Day", StringComparison.OrdinalIgnoreCase))
        {
            var monthAgo = DateTime.UtcNow.AddDays(-30);
            var monthlyCheckIns = userCheckIns.Where(c => c.CompletedAt >= monthAgo).Count();
            return monthlyCheckIns >= 30;
        }

        return false;
    }

    private async Task<bool> CheckSeasonalBadgeAsync(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        var now = DateTime.UtcNow;
        var month = now.Month;

        // Spring badge (March-May)
        if (badge.Name.Contains("Spring", StringComparison.OrdinalIgnoreCase))
        {
            var springCheckIns = userCheckIns.Where(c => 
                c.CompletedAt.Month >= 3 && c.CompletedAt.Month <= 5).Count();
            return springCheckIns >= badge.RequiredValue;
        }

        // Summer badge (June-August)
        if (badge.Name.Contains("Summer", StringComparison.OrdinalIgnoreCase))
        {
            var summerCheckIns = userCheckIns.Where(c => 
                c.CompletedAt.Month >= 6 && c.CompletedAt.Month <= 8).Count();
            return summerCheckIns >= badge.RequiredValue;
        }

        // Fall badge (September-November)
        if (badge.Name.Contains("Fall", StringComparison.OrdinalIgnoreCase))
        {
            var fallCheckIns = userCheckIns.Where(c => 
                c.CompletedAt.Month >= 9 && c.CompletedAt.Month <= 11).Count();
            return fallCheckIns >= badge.RequiredValue;
        }

        // Winter badge (December-February)
        if (badge.Name.Contains("Winter", StringComparison.OrdinalIgnoreCase))
        {
            var winterCheckIns = userCheckIns.Where(c => 
                (c.CompletedAt.Month == 12) || 
                (c.CompletedAt.Month >= 1 && c.CompletedAt.Month <= 2)).Count();
            return winterCheckIns >= badge.RequiredValue;
        }

        return false;
    }

    private async Task<bool> CheckRarityBadgeAsync(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        // Perfect Week (7 days in a row with 100% completion)
        if (badge.Name.Contains("Perfect Week", StringComparison.OrdinalIgnoreCase))
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var weeklyCheckIns = userCheckIns.Where(c => c.CompletedAt >= weekAgo).ToList();
            var activeHabits = userHabits.Where(h => h.IsActive).Count();
            
            if (activeHabits == 0) return false;
            
            var perfectDays = 0;
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.UtcNow.AddDays(-i).Date;
                var dayCheckIns = weeklyCheckIns.Where(c => c.CompletedAt.Date == date).Count();
                if (dayCheckIns >= activeHabits)
                    perfectDays++;
            }
            
            return perfectDays >= 7;
        }

        // Streak Master (multiple habits with long streaks)
        if (badge.Name.Contains("Streak Master", StringComparison.OrdinalIgnoreCase))
        {
            var longStreakHabits = userHabits.Where(h => h.CurrentStreak >= 10).Count();
            return longStreakHabits >= 3;
        }

        return false;
    }

    private async Task<bool> CheckChainBadgeAsync(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        // Habit Chain (multiple related habits)
        if (badge.Name.Contains("Chain", StringComparison.OrdinalIgnoreCase))
        {
            var activeHabits = userHabits.Where(h => h.IsActive).Count();
            return activeHabits >= badge.RequiredValue;
        }

        // Category Master (habits in same category)
        if (badge.Name.Contains("Category", StringComparison.OrdinalIgnoreCase))
        {
            var categories = userHabits
                .Where(h => h.IsActive)
                .GroupBy(h => GetHabitCategory(h.Name, h.Description))
                .Count();
            return categories >= badge.RequiredValue;
        }

        return false;
    }

    private bool ShouldRevealSecretBadge(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        // Reveal secret badges based on certain conditions
        if (badge.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase))
        {
            var totalCheckIns = userCheckIns.Count();
            return totalCheckIns >= 50; // Reveal after 50 check-ins
        }

        return false;
    }

    private async Task<BadgeDto?> AwardBadgeAsync(string userId, Guid badgeId, Guid? habitId)
    {
        try
        {
            var userBadge = new UserBadge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BadgeId = badgeId,
                HabitId = habitId,
                EarnedAt = DateTime.UtcNow
            };

            var createdUserBadge = await _userBadgeRepository.AddAsync(userBadge);
            var badge = await _badgeRepository.GetByIdAsync(badgeId);
            
            return badge != null ? _mapper.Map<BadgeDto>(badge) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error awarding badge {badgeId} to user {userId}");
            return null;
        }
    }

    private int CalculatePossibleCompletions(Habit habit)
    {
        var daysSinceCreation = (DateTime.UtcNow - habit.CreatedAt).Days;
        return habit.Frequency switch
        {
            HabitFrequency.Daily => daysSinceCreation,
            HabitFrequency.Weekly => daysSinceCreation / 7,
            HabitFrequency.Monthly => daysSinceCreation / 30,
            HabitFrequency.Custom => daysSinceCreation,
            _ => daysSinceCreation
        };
    }

    private int CalculateDailyStreak(IEnumerable<CheckIn> recentCheckIns)
    {
        var dates = recentCheckIns.Select(c => c.CompletedAt.Date).Distinct().OrderByDescending(d => d).ToList();
        
        if (!dates.Any()) return 0;

        int streak = 0;
        var currentDate = DateTime.UtcNow.Date;

        for (int i = 0; i < 7; i++)
        {
            var checkDate = currentDate.AddDays(-i);
            if (dates.Contains(checkDate))
                streak++;
            else
                break;
        }

        return streak;
    }

    private string GetHabitCategory(string name, string description)
    {
        var text = $"{name} {description}".ToLower();
        
        if (text.Contains("exercise") || text.Contains("workout") || text.Contains("fitness"))
            return "Fitness";
        if (text.Contains("read") || text.Contains("study") || text.Contains("learn"))
            return "Learning";
        if (text.Contains("meditation") || text.Contains("mindfulness") || text.Contains("yoga"))
            return "Wellness";
        if (text.Contains("water") || text.Contains("drink") || text.Contains("hydration"))
            return "Health";
        if (text.Contains("write") || text.Contains("journal") || text.Contains("blog"))
            return "Creativity";
        if (text.Contains("clean") || text.Contains("organize") || text.Contains("declutter"))
            return "Productivity";
        
        return "Other";
    }

    public async Task<List<BadgeDto>> GetUserBadgesWithProgressAsync(string userId)
    {
        var allBadges = await _badgeRepository.GetActiveBadgesAsync();
        var userBadges = await _userBadgeRepository.GetUserBadgesAsync(userId);
        var earnedBadgeIds = userBadges.Select(ub => ub.BadgeId).ToHashSet();
        
        var userHabits = await _habitRepository.GetHabitsByUserIdAsync(userId);
        var userCheckIns = await _checkInRepository.GetCheckInsByUserIdAsync(userId);

        var badgesWithProgress = new List<BadgeDto>();

        foreach (var badge in allBadges)
        {
            var badgeDto = _mapper.Map<BadgeDto>(badge);
            badgeDto.IsEarned = earnedBadgeIds.Contains(badge.Id);
            
            if (!badgeDto.IsEarned)
            {
                var (progress, target) = await CalculateBadgeProgressAsync(badge, userId, userHabits, userCheckIns);
                badgeDto.Progress = progress;
                badgeDto.Target = target;
            }

            badgesWithProgress.Add(badgeDto);
        }

        return badgesWithProgress.OrderBy(b => b.DisplayOrder).ToList();
    }

    private async Task<(int progress, int target)> CalculateBadgeProgressAsync(Badge badge, string userId, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        return badge.Type switch
        {
            BadgeType.Streak => CalculateStreakProgress(badge, userHabits),
            BadgeType.Total => CalculateTotalProgress(badge, userCheckIns),
            BadgeType.Consistency => CalculateConsistencyProgress(badge, userHabits, userCheckIns),
            BadgeType.Special => CalculateSpecialProgress(badge, userCheckIns),
            BadgeType.Milestone => CalculateMilestoneProgress(badge, userHabits, userCheckIns),
            BadgeType.Creation => CalculateCreationProgress(badge, userHabits),
            BadgeType.TimeBased => CalculateTimeBasedProgress(badge, userCheckIns),
            BadgeType.Challenge => CalculateChallengeProgress(badge, userHabits, userCheckIns),
            BadgeType.Seasonal => CalculateSeasonalProgress(badge, userCheckIns),
            BadgeType.Rarity => CalculateRarityProgress(badge, userHabits, userCheckIns),
            BadgeType.Chain => CalculateChainProgress(badge, userHabits, userCheckIns),
            _ => (0, badge.RequiredValue)
        };
    }

    private (int progress, int target) CalculateStreakProgress(Badge badge, IEnumerable<Habit> userHabits)
    {
        var maxStreak = userHabits.Max(h => h.CurrentStreak);
        return (maxStreak, badge.RequiredValue);
    }

    private (int progress, int target) CalculateTotalProgress(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        var totalCheckIns = userCheckIns.Count();
        return (totalCheckIns, badge.RequiredValue);
    }

    private (int progress, int target) CalculateConsistencyProgress(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        var activeHabits = userHabits.Where(h => h.IsActive).ToList();
        if (!activeHabits.Any()) return (0, badge.RequiredValue);

        var totalPossible = activeHabits.Sum(h => CalculatePossibleCompletions(h));
        var totalActual = userCheckIns.Count();
        var consistencyRate = totalPossible > 0 ? (int)((double)totalActual / totalPossible * 100) : 0;

        return (consistencyRate, badge.RequiredValue);
    }

    private (int progress, int target) CalculateSpecialProgress(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        if (badge.Name.Contains("Early Bird", StringComparison.OrdinalIgnoreCase))
        {
            var earlyCheckIns = userCheckIns.Where(c => c.CompletedAt.Hour < 8).Count();
            return (earlyCheckIns, badge.RequiredValue);
        }

        if (badge.Name.Contains("Night Owl", StringComparison.OrdinalIgnoreCase))
        {
            var nightCheckIns = userCheckIns.Where(c => c.CompletedAt.Hour >= 22).Count();
            return (nightCheckIns, badge.RequiredValue);
        }

        return (0, badge.RequiredValue);
    }

    private (int progress, int target) CalculateMilestoneProgress(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        var totalHabits = userHabits.Count();
        var totalCheckIns = userCheckIns.Count();

        return badge.RequiredValue switch
        {
            1 => (totalHabits, 1),
            5 => (totalHabits, 5),
            10 => (totalHabits, 10),
            100 => (totalCheckIns, 100),
            500 => (totalCheckIns, 500),
            1000 => (totalCheckIns, 1000),
            _ => (0, badge.RequiredValue)
        };
    }

    private (int progress, int target) CalculateCreationProgress(Badge badge, IEnumerable<Habit> userHabits)
    {
        var totalHabits = userHabits.Count();
        return (totalHabits, badge.RequiredValue);
    }

    private (int progress, int target) CalculateTimeBasedProgress(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        var now = DateTime.UtcNow;
        var recentCheckIns = userCheckIns.Where(c => c.CompletedAt >= now.AddDays(-7));

        if (badge.Name.Contains("Daily", StringComparison.OrdinalIgnoreCase))
        {
            var dailyStreak = CalculateDailyStreak(recentCheckIns);
            return (dailyStreak, badge.RequiredValue);
        }

        if (badge.Name.Contains("Weekly", StringComparison.OrdinalIgnoreCase))
        {
            var weeklyCheckIns = recentCheckIns.Count();
            return (weeklyCheckIns, badge.RequiredValue);
        }

        return (0, badge.RequiredValue);
    }

    private (int progress, int target) CalculateChallengeProgress(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        if (badge.Name.Contains("7-Day", StringComparison.OrdinalIgnoreCase))
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var weeklyCheckIns = userCheckIns.Where(c => c.CompletedAt >= weekAgo).Count();
            return (weeklyCheckIns, 7);
        }

        if (badge.Name.Contains("30-Day", StringComparison.OrdinalIgnoreCase))
        {
            var monthAgo = DateTime.UtcNow.AddDays(-30);
            var monthlyCheckIns = userCheckIns.Where(c => c.CompletedAt >= monthAgo).Count();
            return (monthlyCheckIns, 30);
        }

        return (0, badge.RequiredValue);
    }

    private (int progress, int target) CalculateSeasonalProgress(Badge badge, IEnumerable<CheckIn> userCheckIns)
    {
        var now = DateTime.UtcNow;
        var month = now.Month;

        if (badge.Name.Contains("Spring", StringComparison.OrdinalIgnoreCase))
        {
            var springCheckIns = userCheckIns.Where(c => 
                c.CompletedAt.Month >= 3 && c.CompletedAt.Month <= 5).Count();
            return (springCheckIns, badge.RequiredValue);
        }

        if (badge.Name.Contains("Summer", StringComparison.OrdinalIgnoreCase))
        {
            var summerCheckIns = userCheckIns.Where(c => 
                c.CompletedAt.Month >= 6 && c.CompletedAt.Month <= 8).Count();
            return (summerCheckIns, badge.RequiredValue);
        }

        if (badge.Name.Contains("Fall", StringComparison.OrdinalIgnoreCase))
        {
            var fallCheckIns = userCheckIns.Where(c => 
                c.CompletedAt.Month >= 9 && c.CompletedAt.Month <= 11).Count();
            return (fallCheckIns, badge.RequiredValue);
        }

        if (badge.Name.Contains("Winter", StringComparison.OrdinalIgnoreCase))
        {
            var winterCheckIns = userCheckIns.Where(c => 
                (c.CompletedAt.Month == 12) || 
                (c.CompletedAt.Month >= 1 && c.CompletedAt.Month <= 2)).Count();
            return (winterCheckIns, badge.RequiredValue);
        }

        return (0, badge.RequiredValue);
    }

    private (int progress, int target) CalculateRarityProgress(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        if (badge.Name.Contains("Perfect Week", StringComparison.OrdinalIgnoreCase))
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var weeklyCheckIns = userCheckIns.Where(c => c.CompletedAt >= weekAgo).ToList();
            var activeHabits = userHabits.Where(h => h.IsActive).Count();
            
            if (activeHabits == 0) return (0, 7);
            
            var perfectDays = 0;
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.UtcNow.AddDays(-i).Date;
                var dayCheckIns = weeklyCheckIns.Where(c => c.CompletedAt.Date == date).Count();
                if (dayCheckIns >= activeHabits)
                    perfectDays++;
            }
            
            return (perfectDays, 7);
        }

        if (badge.Name.Contains("Streak Master", StringComparison.OrdinalIgnoreCase))
        {
            var longStreakHabits = userHabits.Where(h => h.CurrentStreak >= 10).Count();
            return (longStreakHabits, 3);
        }

        return (0, badge.RequiredValue);
    }

    private (int progress, int target) CalculateChainProgress(Badge badge, IEnumerable<Habit> userHabits, IEnumerable<CheckIn> userCheckIns)
    {
        if (badge.Name.Contains("Chain", StringComparison.OrdinalIgnoreCase))
        {
            var activeHabits = userHabits.Where(h => h.IsActive).Count();
            return (activeHabits, badge.RequiredValue);
        }

        if (badge.Name.Contains("Category", StringComparison.OrdinalIgnoreCase))
        {
            var categories = userHabits
                .Where(h => h.IsActive)
                .GroupBy(h => GetHabitCategory(h.Name, h.Description))
                .Count();
            return (categories, badge.RequiredValue);
        }

        return (0, badge.RequiredValue);
    }
} 