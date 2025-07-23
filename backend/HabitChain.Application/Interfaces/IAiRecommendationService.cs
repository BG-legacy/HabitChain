using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface IAiRecommendationService
{
    Task<List<HabitRecommendationDto>> GetHabitRecommendationsAsync(string userId);
    Task<UserHabitAnalysisDto> GetUserHabitAnalysisAsync(string userId);
    Task<string> GetPersonalizedMotivationAsync(string userId);
    Task<List<HabitRecommendationDto>> GetComplementaryHabitsAsync(string userId, Guid habitId);
    Task<HabitDto> CreateHabitFromRecommendationAsync(string userId, HabitRecommendationDto recommendation);
} 