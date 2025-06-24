using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;

namespace HabitChain.Application.Services;

public class BadgeService : IBadgeService
{
    private readonly IBadgeRepository _badgeRepository;
    private readonly IMapper _mapper;

    public BadgeService(IBadgeRepository badgeRepository, IMapper mapper)
    {
        _badgeRepository = badgeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BadgeDto>> GetAllBadgesAsync()
    {
        var badges = await _badgeRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BadgeDto>>(badges);
    }

    public async Task<IEnumerable<BadgeDto>> GetActiveBadgesAsync()
    {
        var badges = await _badgeRepository.GetActiveBadgesAsync();
        return _mapper.Map<IEnumerable<BadgeDto>>(badges);
    }

    public async Task<BadgeDto?> GetBadgeByIdAsync(Guid id)
    {
        var badge = await _badgeRepository.GetByIdAsync(id);
        return _mapper.Map<BadgeDto>(badge);
    }

    public async Task<BadgeDto> CreateBadgeAsync(CreateBadgeDto createBadgeDto)
    {
        var badge = _mapper.Map<Badge>(createBadgeDto);
        var createdBadge = await _badgeRepository.AddAsync(badge);
        return _mapper.Map<BadgeDto>(createdBadge);
    }

    public async Task<BadgeDto> UpdateBadgeAsync(Guid id, CreateBadgeDto updateBadgeDto)
    {
        var existingBadge = await _badgeRepository.GetByIdAsync(id);
        if (existingBadge == null)
        {
            throw new ArgumentException($"Badge with ID {id} not found.");
        }

        _mapper.Map(updateBadgeDto, existingBadge);
        await _badgeRepository.UpdateAsync(existingBadge);
        return _mapper.Map<BadgeDto>(existingBadge);
    }

    public async Task DeleteBadgeAsync(Guid id)
    {
        var badge = await _badgeRepository.GetByIdAsync(id);
        if (badge == null)
        {
            throw new ArgumentException($"Badge with ID {id} not found.");
        }

        await _badgeRepository.DeleteAsync(id);
    }
} 