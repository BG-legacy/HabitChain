using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;

namespace HabitChain.Application.Services;

public class HabitEntryService : IHabitEntryService
{
    private readonly IHabitEntryRepository _habitEntryRepository;
    private readonly IMapper _mapper;

    public HabitEntryService(IHabitEntryRepository habitEntryRepository, IMapper mapper)
    {
        _habitEntryRepository = habitEntryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<HabitEntryDto>> GetEntriesByHabitIdAsync(Guid habitId)
    {
        var entries = await _habitEntryRepository.GetEntriesByHabitIdAsync(habitId);
        return _mapper.Map<IEnumerable<HabitEntryDto>>(entries);
    }

    public async Task<IEnumerable<HabitEntryDto>> GetEntriesByDateRangeAsync(Guid habitId, DateTime startDate, DateTime endDate)
    {
        var entries = await _habitEntryRepository.GetEntriesByDateRangeAsync(habitId, startDate, endDate);
        return _mapper.Map<IEnumerable<HabitEntryDto>>(entries);
    }

    public async Task<HabitEntryDto?> GetEntryByIdAsync(Guid id)
    {
        var entry = await _habitEntryRepository.GetByIdAsync(id);
        return entry != null ? _mapper.Map<HabitEntryDto>(entry) : null;
    }

    public async Task<HabitEntryDto> CreateEntryAsync(CreateHabitEntryDto createEntryDto)
    {
        var entry = _mapper.Map<HabitEntry>(createEntryDto);
        entry.Id = Guid.NewGuid();
        
        var createdEntry = await _habitEntryRepository.AddAsync(entry);
        return _mapper.Map<HabitEntryDto>(createdEntry);
    }

    public async Task DeleteEntryAsync(Guid id)
    {
        var exists = await _habitEntryRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new ArgumentException($"Habit entry with ID {id} not found.");
        }

        await _habitEntryRepository.DeleteAsync(id);
    }

    public async Task<bool> HasEntryForDateAsync(Guid habitId, DateTime date)
    {
        return await _habitEntryRepository.HasEntryForDateAsync(habitId, date);
    }
} 