using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface IUserBadgeService
{
    Task<IEnumerable<UserBadgeDto>> GetUserBadgesByUserIdAsync(string userId);
    Task<IEnumerable<UserBadgeDto>> GetUserBadgesByHabitIdAsync(Guid habitId);
    Task<UserBadgeDto?> GetUserBadgeByIdAsync(Guid id);
    Task<UserBadgeDto> CreateUserBadgeAsync(CreateUserBadgeDto createUserBadgeDto);
    Task DeleteUserBadgeAsync(Guid id);
    Task<bool> HasUserEarnedBadgeAsync(string userId, Guid badgeId);
} 