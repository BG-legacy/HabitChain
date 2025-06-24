using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;

namespace HabitChain.Application.Services;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IMapper _mapper;

    public HabitService(IHabitRepository habitRepository, IMapper mapper)
    {
        _habitRepository = habitRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<HabitDto>> GetHabitsByUserIdAsync(string userId)
    {
        var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<HabitDto>>(habits);
    }

    public async Task<IEnumerable<HabitDto>> GetActiveHabitsByUserIdAsync(string userId)
    {
        var habits = await _habitRepository.GetActiveHabitsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<HabitDto>>(habits);
    }

    public async Task<HabitDto?> GetHabitByIdAsync(Guid id)
    {
        var habit = await _habitRepository.GetByIdAsync(id);
        return habit != null ? _mapper.Map<HabitDto>(habit) : null;
    }

    public async Task<HabitDto> CreateHabitAsync(CreateHabitDto createHabitDto)
    {
        var habit = _mapper.Map<Habit>(createHabitDto);
        habit.Id = Guid.NewGuid();
        
        var createdHabit = await _habitRepository.AddAsync(habit);
        return _mapper.Map<HabitDto>(createdHabit);
    }

    public async Task<HabitDto> UpdateHabitAsync(Guid id, CreateHabitDto updateHabitDto)
    {
        var existingHabit = await _habitRepository.GetByIdAsync(id);
        if (existingHabit == null)
        {
            throw new ArgumentException($"Habit with ID {id} not found.");
        }

        _mapper.Map(updateHabitDto, existingHabit);
        await _habitRepository.UpdateAsync(existingHabit);
        
        return _mapper.Map<HabitDto>(existingHabit);
    }

    public async Task DeleteHabitAsync(Guid id)
    {
        var exists = await _habitRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new ArgumentException($"Habit with ID {id} not found.");
        }

        await _habitRepository.DeleteAsync(id);
    }

    public async Task<HabitDto?> GetHabitWithEntriesAsync(Guid habitId)
    {
        var habit = await _habitRepository.GetHabitWithEntriesAsync(habitId);
        return habit != null ? _mapper.Map<HabitDto>(habit) : null;
    }
} 