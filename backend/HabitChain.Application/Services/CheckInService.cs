using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;

namespace HabitChain.Application.Services;

public class CheckInService : ICheckInService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IHabitRepository _habitRepository;
    private readonly IBadgeEarningService _badgeEarningService;
    private readonly IMapper _mapper;

    public CheckInService(
        ICheckInRepository checkInRepository, 
        IHabitRepository habitRepository, 
        IBadgeEarningService badgeEarningService,
        IMapper mapper)
    {
        _checkInRepository = checkInRepository;
        _habitRepository = habitRepository;
        _badgeEarningService = badgeEarningService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CheckInDto>> GetCheckInsByUserIdAsync(string userId)
    {
        var checkIns = await _checkInRepository.GetCheckInsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<CheckInDto>>(checkIns);
    }

    public async Task<IEnumerable<CheckInDto>> GetCheckInsByHabitIdAsync(Guid habitId)
    {
        var checkIns = await _checkInRepository.GetCheckInsByHabitIdAsync(habitId);
        return _mapper.Map<IEnumerable<CheckInDto>>(checkIns);
    }

    public async Task<IEnumerable<CheckInDto>> GetCheckInsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var checkIns = await _checkInRepository.GetCheckInsForDateRangeAsync(userId, startDate, endDate);
        return _mapper.Map<IEnumerable<CheckInDto>>(checkIns);
    }

    public async Task<CheckInDto?> GetCheckInByIdAsync(Guid id)
    {
        var checkIn = await _checkInRepository.GetByIdAsync(id);
        return _mapper.Map<CheckInDto>(checkIn);
    }

    public async Task<CheckInDto> CreateCheckInAsync(CreateCheckInDto createCheckInDto)
    {
        var checkIn = _mapper.Map<CheckIn>(createCheckInDto);
        
        // Calculate the actual streak based on previous check-ins
        var actualStreak = await CalculateStreakAsync(createCheckInDto.HabitId, createCheckInDto.CompletedAt);
        checkIn.StreakDay = actualStreak;
        
        var createdCheckIn = await _checkInRepository.AddAsync(checkIn);
        
        // Update the habit's streak information and completion tracking
        await UpdateHabitStreakAsync(createCheckInDto.HabitId, actualStreak, createCheckInDto.CompletedAt);
        
        // Check for badge earning opportunities
        await _badgeEarningService.CheckAndAwardBadgesAsync(createCheckInDto.UserId, createCheckInDto.HabitId);
        
        return _mapper.Map<CheckInDto>(createdCheckIn);
    }

    private async Task<int> CalculateStreakAsync(Guid habitId, DateTime completedAt)
    {
        // Get the latest check-in for this habit
        var latestCheckIn = await _checkInRepository.GetLatestCheckInForHabitAsync(habitId);
        
        if (latestCheckIn == null)
        {
            // First check-in for this habit
            return 1;
        }
        
        // Check if the new check-in is on a consecutive day
        var lastCheckInDate = latestCheckIn.CompletedAt.Date;
        var newCheckInDate = completedAt.Date;
        var daysDifference = (newCheckInDate - lastCheckInDate).Days;
        
        if (daysDifference == 1)
        {
            // Consecutive day, increment streak
            return latestCheckIn.StreakDay + 1;
        }
        else if (daysDifference == 0)
        {
            // Same day, keep the same streak (but this shouldn't happen with proper validation)
            return latestCheckIn.StreakDay;
        }
        else
        {
            // Gap in streak, start over
            return 1;
        }
    }

    private async Task UpdateHabitStreakAsync(Guid habitId, int newStreakDay, DateTime completedAt)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit != null)
        {
            habit.CurrentStreak = newStreakDay;
            habit.LongestStreak = Math.Max(habit.LongestStreak, newStreakDay);
            habit.LastCompletedAt = completedAt;
            
            await _habitRepository.UpdateAsync(habit);
        }
    }

    public async Task<CheckInDto> UpdateCheckInAsync(Guid id, CreateCheckInDto updateCheckInDto)
    {
        var existingCheckIn = await _checkInRepository.GetByIdAsync(id);
        if (existingCheckIn == null)
        {
            throw new ArgumentException($"CheckIn with ID {id} not found.");
        }

        _mapper.Map(updateCheckInDto, existingCheckIn);
        await _checkInRepository.UpdateAsync(existingCheckIn);
        return _mapper.Map<CheckInDto>(existingCheckIn);
    }

    public async Task DeleteCheckInAsync(Guid id)
    {
        var checkIn = await _checkInRepository.GetByIdAsync(id);
        if (checkIn == null)
        {
            throw new ArgumentException($"CheckIn with ID {id} not found.");
        }

        await _checkInRepository.DeleteAsync(id);
    }

    public async Task<bool> HasCheckInForDateAsync(string userId, Guid habitId, DateTime date)
    {
        // Ensure we're working with UTC dates for PostgreSQL compatibility
        var utcDate = date.Kind switch
        {
            DateTimeKind.Utc => date.Date,
            DateTimeKind.Local => date.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)
        };

        var startOfDay = utcDate;
        var endOfDay = utcDate.AddDays(1).AddTicks(-1);

        var checkIns = await _checkInRepository.GetCheckInsForDateRangeAsync(userId, startOfDay, endOfDay);
        return checkIns.Any(ci => ci.HabitId == habitId);
    }
} 