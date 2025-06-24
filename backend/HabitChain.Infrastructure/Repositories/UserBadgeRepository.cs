using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Repositories;

public class UserBadgeRepository : Repository<UserBadge>, IUserBadgeRepository
{
    public UserBadgeRepository(HabitChainDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserBadge>> GetUserBadgesAsync(string userId)
    {
        return await _dbSet
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .Include(ub => ub.Habit)
            .OrderByDescending(ub => ub.EarnedAt)
            .ToListAsync();
    }

    public async Task<UserBadge?> GetUserBadgeAsync(string userId, Guid badgeId)
    {
        return await _dbSet
            .Include(ub => ub.Badge)
            .Include(ub => ub.Habit)
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);
    }

    public async Task<IEnumerable<UserBadge>> GetRecentlyEarnedBadgesAsync(string userId, int count = 5)
    {
        return await _dbSet
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .Include(ub => ub.Habit)
            .OrderByDescending(ub => ub.EarnedAt)
            .Take(count)
            .ToListAsync();
    }
} 