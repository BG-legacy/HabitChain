using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;

namespace HabitChain.Application.Services;

public class EncouragementService : IEncouragementService
{
    private readonly IEncouragementRepository _encouragementRepository;
    private readonly IMapper _mapper;

    public EncouragementService(IEncouragementRepository encouragementRepository, IMapper mapper)
    {
        _encouragementRepository = encouragementRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EncouragementDto>> GetEncouragementsByUserIdAsync(string userId)
    {
        var encouragements = await _encouragementRepository.GetReceivedEncouragements(userId);
        return _mapper.Map<IEnumerable<EncouragementDto>>(encouragements);
    }

    public async Task<IEnumerable<EncouragementDto>> GetEncouragementsByFromUserIdAsync(string fromUserId)
    {
        var encouragements = await _encouragementRepository.GetSentEncouragements(fromUserId);
        return _mapper.Map<IEnumerable<EncouragementDto>>(encouragements);
    }

    public async Task<IEnumerable<EncouragementDto>> GetEncouragementsByHabitIdAsync(Guid habitId)
    {
        // Since the repository doesn't have a method for habit-specific encouragements,
        // we'll get all encouragements and filter by habit ID
        var allEncouragements = await _encouragementRepository.GetAllAsync();
        var encouragements = allEncouragements.Where(e => e.HabitId == habitId);
        return _mapper.Map<IEnumerable<EncouragementDto>>(encouragements);
    }

    public async Task<IEnumerable<EncouragementDto>> GetUnreadEncouragementsByUserIdAsync(string userId)
    {
        var encouragements = await _encouragementRepository.GetUnreadEncouragements(userId);
        return _mapper.Map<IEnumerable<EncouragementDto>>(encouragements);
    }

    public async Task<EncouragementDto?> GetEncouragementByIdAsync(Guid id)
    {
        var encouragement = await _encouragementRepository.GetByIdAsync(id);
        return _mapper.Map<EncouragementDto>(encouragement);
    }

    public async Task<EncouragementDto> CreateEncouragementAsync(CreateEncouragementDto createEncouragementDto)
    {
        var encouragement = _mapper.Map<Encouragement>(createEncouragementDto);
        var createdEncouragement = await _encouragementRepository.AddAsync(encouragement);
        return _mapper.Map<EncouragementDto>(createdEncouragement);
    }

    public async Task<EncouragementDto> UpdateEncouragementAsync(Guid id, CreateEncouragementDto updateEncouragementDto)
    {
        var existingEncouragement = await _encouragementRepository.GetByIdAsync(id);
        if (existingEncouragement == null)
        {
            throw new ArgumentException($"Encouragement with ID {id} not found.");
        }

        _mapper.Map(updateEncouragementDto, existingEncouragement);
        await _encouragementRepository.UpdateAsync(existingEncouragement);
        return _mapper.Map<EncouragementDto>(existingEncouragement);
    }

    public async Task DeleteEncouragementAsync(Guid id)
    {
        var encouragement = await _encouragementRepository.GetByIdAsync(id);
        if (encouragement == null)
        {
            throw new ArgumentException($"Encouragement with ID {id} not found.");
        }

        await _encouragementRepository.DeleteAsync(id);
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        await _encouragementRepository.MarkAsReadAsync(id);
    }
} 