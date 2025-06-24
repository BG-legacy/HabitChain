using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface IEncouragementService
{
    Task<IEnumerable<EncouragementDto>> GetEncouragementsByUserIdAsync(string userId);
    Task<IEnumerable<EncouragementDto>> GetEncouragementsByFromUserIdAsync(string fromUserId);
    Task<IEnumerable<EncouragementDto>> GetEncouragementsByHabitIdAsync(Guid habitId);
    Task<IEnumerable<EncouragementDto>> GetUnreadEncouragementsByUserIdAsync(string userId);
    Task<EncouragementDto?> GetEncouragementByIdAsync(Guid id);
    Task<EncouragementDto> CreateEncouragementAsync(CreateEncouragementDto createEncouragementDto);
    Task<EncouragementDto> UpdateEncouragementAsync(Guid id, CreateEncouragementDto updateEncouragementDto);
    Task DeleteEncouragementAsync(Guid id);
    Task MarkAsReadAsync(Guid id);
} 