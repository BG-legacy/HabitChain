using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;

namespace HabitChain.Application.Services;

public class UserBadgeService : IUserBadgeService
{
    private readonly IUserBadgeRepository _userBadgeRepository;
    private readonly IMapper _mapper;

    public UserBadgeService(IUserBadgeRepository userBadgeRepository, IMapper mapper)
    {
        _userBadgeRepository = userBadgeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserBadgeDto>> GetUserBadgesByUserIdAsync(string userId)
    {
        var userBadges = await _userBadgeRepository.GetUserBadgesAsync(userId);
        return _mapper.Map<IEnumerable<UserBadgeDto>>(userBadges);
    }

    public async Task<IEnumerable<UserBadgeDto>> GetUserBadgesByHabitIdAsync(Guid habitId)
    {
        var userBadges = await _userBadgeRepository.FindAsync(ub => ub.HabitId == habitId);
        return _mapper.Map<IEnumerable<UserBadgeDto>>(userBadges);
    }

    public async Task<UserBadgeDto?> GetUserBadgeByIdAsync(Guid id)
    {
        var userBadge = await _userBadgeRepository.GetByIdAsync(id);
        return _mapper.Map<UserBadgeDto>(userBadge);
    }

    public async Task<UserBadgeDto> CreateUserBadgeAsync(CreateUserBadgeDto createUserBadgeDto)
    {
        var userBadge = _mapper.Map<UserBadge>(createUserBadgeDto);
        userBadge.EarnedAt = DateTime.UtcNow;
        var createdUserBadge = await _userBadgeRepository.AddAsync(userBadge);
        return _mapper.Map<UserBadgeDto>(createdUserBadge);
    }

    public async Task DeleteUserBadgeAsync(Guid id)
    {
        var userBadge = await _userBadgeRepository.GetByIdAsync(id);
        if (userBadge == null)
        {
            throw new ArgumentException($"UserBadge with ID {id} not found.");
        }

        await _userBadgeRepository.DeleteAsync(id);
    }

    public async Task<bool> HasUserEarnedBadgeAsync(string userId, Guid badgeId)
    {
        var userBadge = await _userBadgeRepository.GetUserBadgeAsync(userId, badgeId);
        return userBadge != null;
    }
} 