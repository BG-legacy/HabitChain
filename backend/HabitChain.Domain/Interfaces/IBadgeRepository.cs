using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;

namespace HabitChain.Domain.Interfaces;

public interface IBadgeRepository : IRepository<Badge>
{
    Task<IEnumerable<Badge>> GetActiveBadgesAsync();
    Task<IEnumerable<Badge>> GetBadgesByTypeAsync(BadgeType type);
    Task<IEnumerable<Badge>> GetEligibleBadgesForUserAsync(Guid userId);
} 