using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface IBadgeEarningService
{
    Task<List<BadgeDto>> CheckAndAwardBadgesAsync(string userId, Guid? habitId = null);
    Task<List<BadgeDto>> GetUserBadgesWithProgressAsync(string userId);
} 