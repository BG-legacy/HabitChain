using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;

namespace HabitChain.Application.Services;

public class CheckInService : ICheckInService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IMapper _mapper;

    public CheckInService(ICheckInRepository checkInRepository, IMapper mapper)
    {
        _checkInRepository = checkInRepository;
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
        var createdCheckIn = await _checkInRepository.AddAsync(checkIn);
        return _mapper.Map<CheckInDto>(createdCheckIn);
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
        var checkIns = await _checkInRepository.GetCheckInsForDateRangeAsync(userId, date.Date, date.Date.AddDays(1).AddTicks(-1));
        return checkIns.Any(ci => ci.HabitId == habitId);
    }
} 