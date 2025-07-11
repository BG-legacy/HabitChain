using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Domain.Enums;

namespace HabitChain.Application.Services;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly IHabitEntryRepository _habitEntryRepository;
    private readonly IBadgeEarningService _badgeEarningService;
    private readonly IMapper _mapper;

    public HabitService(
        IHabitRepository habitRepository, 
        ICheckInRepository checkInRepository, 
        IHabitEntryRepository habitEntryRepository, 
        IBadgeEarningService badgeEarningService,
        IMapper mapper)
    {
        _habitRepository = habitRepository;
        _checkInRepository = checkInRepository;
        _habitEntryRepository = habitEntryRepository;
        _badgeEarningService = badgeEarningService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<HabitDto>> GetHabitsByUserIdAsync(string userId)
    {
        var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);
        
        // Debug logging
        foreach (var habit in habits)
        {
            Console.WriteLine($"Habit: {habit.Name}, CheckIns Count: {habit.CheckIns?.Count ?? 0}");
        }
        
        var result = _mapper.Map<IEnumerable<HabitDto>>(habits);
        
        // OPTIMIZED: Batch fetch all habit entries for all habits in a single query
        var habitIds = habits.Select(h => h.Id).ToList();
        var allEntries = await _habitEntryRepository.GetEntriesByHabitIdsAsync(habitIds);
        var allEntriesGrouped = allEntries.GroupBy(e => e.HabitId).ToDictionary(g => g.Key, g => g.AsEnumerable());
        
        // Calculate completion rates for each habit using the pre-fetched data
        var habitsWithCompletionRates = new List<HabitDto>();
        foreach (var habitDto in result)
        {
            var habit = habits.First(h => h.Id == habitDto.Id);
            var entries = allEntriesGrouped.GetValueOrDefault(habitDto.Id, Enumerable.Empty<HabitEntry>());
            
            // Use the optimized overload that doesn't make additional DB calls
            var completionRates = CalculateCompletionRatesFromData(habit, entries);
            habitDto.CompletionRate = completionRates.OverallCompletionRate;
            habitDto.WeeklyCompletionRate = completionRates.WeeklyCompletionRate;
            habitDto.MonthlyCompletionRate = completionRates.MonthlyCompletionRate;
            habitDto.TotalPossibleCompletions = completionRates.TotalPossibleCompletions;
            habitDto.TotalActualCompletions = completionRates.TotalActualCompletions;
            habitsWithCompletionRates.Add(habitDto);
        }
        
        // Debug logging for DTOs
        foreach (var habitDto in habitsWithCompletionRates)
        {
            Console.WriteLine($"HabitDto: {habitDto.Name}, TotalCheckIns: {habitDto.TotalCheckIns}, CompletionRate: {habitDto.CompletionRate}%");
        }
        
        return habitsWithCompletionRates;
    }

    public async Task<IEnumerable<HabitDto>> GetActiveHabitsByUserIdAsync(string userId)
    {
        var habits = await _habitRepository.GetActiveHabitsByUserIdAsync(userId);
        var result = _mapper.Map<IEnumerable<HabitDto>>(habits);
        
        // OPTIMIZED: Batch fetch all habit entries for all habits in a single query
        var habitIds = habits.Select(h => h.Id).ToList();
        var allEntries = await _habitEntryRepository.GetEntriesByHabitIdsAsync(habitIds);
        var allEntriesGrouped = allEntries.GroupBy(e => e.HabitId).ToDictionary(g => g.Key, g => g.AsEnumerable());
        
        // Calculate completion rates for each habit using the pre-fetched data
        var habitsWithCompletionRates = new List<HabitDto>();
        foreach (var habitDto in result)
        {
            var habit = habits.First(h => h.Id == habitDto.Id);
            var entries = allEntriesGrouped.GetValueOrDefault(habitDto.Id, Enumerable.Empty<HabitEntry>());
            
            var completionRates = CalculateCompletionRatesFromData(habit, entries);
            habitDto.CompletionRate = completionRates.OverallCompletionRate;
            habitDto.WeeklyCompletionRate = completionRates.WeeklyCompletionRate;
            habitDto.MonthlyCompletionRate = completionRates.MonthlyCompletionRate;
            habitDto.TotalPossibleCompletions = completionRates.TotalPossibleCompletions;
            habitDto.TotalActualCompletions = completionRates.TotalActualCompletions;
            habitsWithCompletionRates.Add(habitDto);
        }
        
        return habitsWithCompletionRates;
    }

    public async Task<HabitDto?> GetHabitByIdAsync(Guid id)
    {
        var habit = await _habitRepository.GetByIdAsync(id);
        if (habit == null) return null;
        
        var habitDto = _mapper.Map<HabitDto>(habit);
        var completionRates = await CalculateCompletionRatesAsync(id);
        habitDto.CompletionRate = completionRates.OverallCompletionRate;
        habitDto.WeeklyCompletionRate = completionRates.WeeklyCompletionRate;
        habitDto.MonthlyCompletionRate = completionRates.MonthlyCompletionRate;
        habitDto.TotalPossibleCompletions = completionRates.TotalPossibleCompletions;
        habitDto.TotalActualCompletions = completionRates.TotalActualCompletions;
        
        return habitDto;
    }

    // Lightweight version for ownership verification
    public async Task<HabitDto?> GetHabitByIdLightweightAsync(Guid id)
    {
        var habit = await _habitRepository.GetByIdAsync(id);
        if (habit == null) return null;
        
        return _mapper.Map<HabitDto>(habit);
    }

    public async Task<HabitDto> CreateHabitAsync(CreateHabitDto createHabitDto)
    {
        var habit = _mapper.Map<Habit>(createHabitDto);
        habit.Id = Guid.NewGuid();
        
        var createdHabit = await _habitRepository.AddAsync(habit);
        
        // Check for badge earning opportunities when a new habit is created
        await _badgeEarningService.CheckAndAwardBadgesAsync(createHabitDto.UserId, createdHabit.Id);
        
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
        if (habit == null) return null;
        
        var habitDto = _mapper.Map<HabitDto>(habit);
        var completionRates = await CalculateCompletionRatesAsync(habitId);
        habitDto.CompletionRate = completionRates.OverallCompletionRate;
        habitDto.WeeklyCompletionRate = completionRates.WeeklyCompletionRate;
        habitDto.MonthlyCompletionRate = completionRates.MonthlyCompletionRate;
        habitDto.TotalPossibleCompletions = completionRates.TotalPossibleCompletions;
        habitDto.TotalActualCompletions = completionRates.TotalActualCompletions;
        
        return habitDto;
    }

    public async Task<UserCompletionRateDto> GetUserCompletionRatesAsync(string userId)
    {
        var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);
        var completionRates = new List<CompletionRateDto>();
        
        double overallCompletionRate = 0;
        double weeklyCompletionRate = 0;
        double monthlyCompletionRate = 0;
        int totalHabits = habits.Count();
        int activeHabits = habits.Count(h => h.IsActive);
        int completedHabitsToday = 0;
        
        // OPTIMIZED: Batch fetch all habit entries for all habits in a single query
        var habitIds = habits.Select(h => h.Id).ToList();
        var allEntries = await _habitEntryRepository.GetEntriesByHabitIdsAsync(habitIds);
        var allEntriesGrouped = allEntries.GroupBy(e => e.HabitId).ToDictionary(g => g.Key, g => g.AsEnumerable());
        
        foreach (var habit in habits)
        {
            var entries = allEntriesGrouped.GetValueOrDefault(habit.Id, Enumerable.Empty<HabitEntry>());
            var rates = CalculateCompletionRatesFromData(habit, entries);
            
            var habitCompletionRate = new CompletionRateDto
            {
                HabitId = habit.Id,
                HabitName = habit.Name,
                OverallCompletionRate = rates.OverallCompletionRate,
                WeeklyCompletionRate = rates.WeeklyCompletionRate,
                MonthlyCompletionRate = rates.MonthlyCompletionRate,
                TotalPossibleCompletions = rates.TotalPossibleCompletions,
                TotalActualCompletions = rates.TotalActualCompletions,
                CurrentStreak = habit.CurrentStreak,
                LongestStreak = habit.LongestStreak,
                LastCompletedAt = habit.LastCompletedAt,
                IsActive = habit.IsActive
            };
            
            completionRates.Add(habitCompletionRate);
            
            // Check if habit was completed today
            if (habit.LastCompletedAt?.Date == DateTime.UtcNow.Date)
            {
                completedHabitsToday++;
            }
        }
        
        // Calculate overall rates
        if (activeHabits > 0)
        {
            overallCompletionRate = completionRates.Where(c => c.IsActive).Average(c => c.OverallCompletionRate);
            weeklyCompletionRate = completionRates.Where(c => c.IsActive).Average(c => c.WeeklyCompletionRate);
            monthlyCompletionRate = completionRates.Where(c => c.IsActive).Average(c => c.MonthlyCompletionRate);
        }
        
        return new UserCompletionRateDto
        {
            UserId = userId,
            OverallCompletionRate = Math.Round(overallCompletionRate, 2),
            WeeklyCompletionRate = Math.Round(weeklyCompletionRate, 2),
            MonthlyCompletionRate = Math.Round(monthlyCompletionRate, 2),
            TotalHabits = totalHabits,
            ActiveHabits = activeHabits,
            CompletedHabitsToday = completedHabitsToday,
            HabitCompletionRates = completionRates
        };
    }

    // OPTIMIZED: New method that works with in-memory data instead of making DB calls
    private (double OverallCompletionRate, double WeeklyCompletionRate, double MonthlyCompletionRate, int TotalPossibleCompletions, int TotalActualCompletions) CalculateCompletionRatesFromData(Habit habit, IEnumerable<HabitEntry> entries)
    {
        var totalActualCompletions = entries.Count();
        
        // Calculate total possible completions based on habit creation date and frequency
        var totalPossibleCompletions = CalculateTotalPossibleCompletions(habit.CreatedAt, habit.Frequency);
        
        // Calculate overall completion rate
        var overallCompletionRate = totalPossibleCompletions > 0 ? 
            (double)totalActualCompletions / totalPossibleCompletions * 100 : 0;
        
        // Calculate weekly completion rate (last 7 days)
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var weeklyEntries = entries.Where(e => e.CompletedAt >= weekAgo).Count();
        var weeklyPossible = CalculatePossibleCompletionsInPeriod(weekAgo, DateTime.UtcNow, habit.Frequency);
        var weeklyCompletionRate = weeklyPossible > 0 ? (double)weeklyEntries / weeklyPossible * 100 : 0;
        
        // Calculate monthly completion rate (last 30 days)
        var monthAgo = DateTime.UtcNow.AddDays(-30);
        var monthlyEntries = entries.Where(e => e.CompletedAt >= monthAgo).Count();
        var monthlyPossible = CalculatePossibleCompletionsInPeriod(monthAgo, DateTime.UtcNow, habit.Frequency);
        var monthlyCompletionRate = monthlyPossible > 0 ? (double)monthlyEntries / monthlyPossible * 100 : 0;
        
        return (
            Math.Round(overallCompletionRate, 2),
            Math.Round(weeklyCompletionRate, 2),
            Math.Round(monthlyCompletionRate, 2),
            totalPossibleCompletions,
            totalActualCompletions
        );
    }

    // LEGACY: Keep original method for backwards compatibility with single habit operations
    private async Task<(double OverallCompletionRate, double WeeklyCompletionRate, double MonthlyCompletionRate, int TotalPossibleCompletions, int TotalActualCompletions)> CalculateCompletionRatesAsync(Guid habitId)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null)
        {
            return (0, 0, 0, 0, 0);
        }

        // Use HabitEntry records for completion rate calculations
        var entries = await _habitEntryRepository.GetEntriesByHabitIdAsync(habitId);
        
        return CalculateCompletionRatesFromData(habit, entries);
    }

    private int CalculateTotalPossibleCompletions(DateTime habitCreatedAt, HabitFrequency frequency)
    {
        var daysSinceCreation = (DateTime.UtcNow - habitCreatedAt).Days;
        
        return frequency switch
        {
            HabitFrequency.Daily => daysSinceCreation,
            HabitFrequency.Weekly => daysSinceCreation / 7,
            HabitFrequency.Monthly => daysSinceCreation / 30,
            HabitFrequency.Custom => daysSinceCreation, // Assuming daily for custom
            _ => daysSinceCreation
        };
    }

    private int CalculatePossibleCompletionsInPeriod(DateTime startDate, DateTime endDate, HabitFrequency frequency)
    {
        var daysInPeriod = (endDate - startDate).Days;
        
        return frequency switch
        {
            HabitFrequency.Daily => daysInPeriod,
            HabitFrequency.Weekly => daysInPeriod / 7,
            HabitFrequency.Monthly => daysInPeriod / 30,
            HabitFrequency.Custom => daysInPeriod, // Assuming daily for custom
            _ => daysInPeriod
        };
    }

    public async Task UpdateHabitCompletionAsync(Guid habitId, DateTime completedAt)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) return;

        // Calculate streak based on HabitEntry records
        var currentStreak = await CalculateHabitEntryStreakAsync(habitId, completedAt);
        
        // Update habit properties
        habit.CurrentStreak = currentStreak;
        habit.LongestStreak = Math.Max(habit.LongestStreak, currentStreak);
        habit.LastCompletedAt = completedAt;
        
        await _habitRepository.UpdateAsync(habit);
    }

    private async Task<int> CalculateHabitEntryStreakAsync(Guid habitId, DateTime completedAt)
    {
        // Get all entries for this habit, ordered by date
        var entries = await _habitEntryRepository.GetEntriesByHabitIdAsync(habitId);
        var entryDates = entries.Select(e => e.CompletedAt.Date).Distinct().OrderByDescending(d => d).ToList();
        
        // Include the current completion date
        var currentDate = completedAt.Date;
        if (!entryDates.Contains(currentDate))
        {
            entryDates = entryDates.Prepend(currentDate).OrderByDescending(d => d).ToList();
        }

        // Calculate consecutive days
        int streak = 0;
        DateTime? previousDate = null;

        foreach (var date in entryDates)
        {
            if (previousDate == null)
            {
                // First date
                streak = 1;
                previousDate = date;
            }
            else if ((previousDate.Value - date).Days == 1)
            {
                // Consecutive day
                streak++;
                previousDate = date;
            }
            else
            {
                // Break in streak
                break;
            }
        }

        return streak;
    }
} 