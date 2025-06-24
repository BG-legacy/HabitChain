using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface IBadgeService
{
    Task<IEnumerable<BadgeDto>> GetAllBadgesAsync();
    Task<IEnumerable<BadgeDto>> GetActiveBadgesAsync();
    Task<BadgeDto?> GetBadgeByIdAsync(Guid id);
    Task<BadgeDto> CreateBadgeAsync(CreateBadgeDto createBadgeDto);
    Task<BadgeDto> UpdateBadgeAsync(Guid id, CreateBadgeDto updateBadgeDto);
    Task DeleteBadgeAsync(Guid id);
} 