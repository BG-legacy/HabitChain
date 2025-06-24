using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Repositories;

public class BadgeRepository : Repository<Badge>, IBadgeRepository
{
    public BadgeRepository(HabitChainDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Badge>> GetActiveBadgesAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive)
            .OrderBy(b => b.Type)
            .ThenBy(b => b.RequiredValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<Badge>> GetBadgesByTypeAsync(BadgeType type)
    {
        return await _dbSet
            .Where(b => b.Type == type && b.IsActive)
            .OrderBy(b => b.RequiredValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<Badge>> GetEligibleBadgesForUserAsync(Guid userId)
    {
        // This is a simplified implementation
        // In a real application, you'd implement complex logic to determine eligibility
        return await _dbSet
            .Where(b => b.IsActive)
            .ToListAsync();
    }
} 