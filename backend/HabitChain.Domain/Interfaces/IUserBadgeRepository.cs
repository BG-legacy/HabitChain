using HabitChain.Domain.Entities;

namespace HabitChain.Domain.Interfaces;

public interface IUserBadgeRepository : IRepository<UserBadge>
{
    Task<IEnumerable<UserBadge>> GetUserBadgesAsync(string userId);
    Task<UserBadge?> GetUserBadgeAsync(string userId, Guid badgeId);
    Task<IEnumerable<UserBadge>> GetRecentlyEarnedBadgesAsync(string userId, int count = 5);
} 