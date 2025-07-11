using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Domain.Entities;

namespace HabitChain.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // Habit mappings
        CreateMap<Habit, HabitDto>()
            .ForMember(dest => dest.TotalCheckIns, opt => opt.MapFrom(src => src.CheckIns.Count))
            .ForMember(dest => dest.CompletionRate, opt => opt.Ignore())
            .ForMember(dest => dest.WeeklyCompletionRate, opt => opt.Ignore())
            .ForMember(dest => dest.MonthlyCompletionRate, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPossibleCompletions, opt => opt.Ignore())
            .ForMember(dest => dest.TotalActualCompletions, opt => opt.Ignore());
        CreateMap<CreateHabitDto, Habit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CurrentStreak, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.LongestStreak, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.LastCompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TargetDays, opt => opt.MapFrom(src => 1))
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.CheckIns, opt => opt.Ignore())
            .ForMember(dest => dest.UserBadges, opt => opt.Ignore())
            .ForMember(dest => dest.Encouragements, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // HabitEntry mappings
        CreateMap<HabitEntry, HabitEntryDto>();
        CreateMap<CreateHabitEntryDto, HabitEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        // Badge mappings
        CreateMap<Badge, BadgeDto>()
            .ForMember(dest => dest.IsEarned, opt => opt.Ignore())
            .ForMember(dest => dest.Progress, opt => opt.Ignore())
            .ForMember(dest => dest.Target, opt => opt.Ignore())
            .ForMember(dest => dest.EarnedAt, opt => opt.Ignore())
            .ForMember(dest => dest.HabitId, opt => opt.Ignore());
        CreateMap<CreateBadgeDto, Badge>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconName ?? string.Empty))
            .ForMember(dest => dest.Emoji, opt => opt.MapFrom(src => "🏆"))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => "general"))
            .ForMember(dest => dest.Rarity, opt => opt.MapFrom(src => "common"))
            .ForMember(dest => dest.ColorTheme, opt => opt.MapFrom(src => "#667eea"))
            .ForMember(dest => dest.IsSecret, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.RequiredValue, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.UserBadges, opt => opt.Ignore());

        // UserBadge mappings
        CreateMap<UserBadge, UserBadgeDto>()
            .ForMember(dest => dest.Badge, opt => opt.MapFrom(src => src.Badge))
            .ForMember(dest => dest.Habit, opt => opt.MapFrom(src => src.Habit));
            
        CreateMap<CreateUserBadgeDto, UserBadge>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.HabitId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Badge, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        // CheckIn mappings
        CreateMap<CheckIn, CheckInDto>()
            .ForMember(dest => dest.Habit, opt => opt.MapFrom(src => src.Habit));
            
        CreateMap<CreateCheckInDto, CheckIn>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        // Encouragement mappings
        CreateMap<Encouragement, EncouragementDto>()
            .ForMember(dest => dest.FromUser, opt => opt.MapFrom(src => src.FromUser))
            .ForMember(dest => dest.ToUser, opt => opt.MapFrom(src => src.ToUser))
            .ForMember(dest => dest.Habit, opt => opt.MapFrom(src => src.Habit));
            
        CreateMap<CreateEncouragementDto, Encouragement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsRead, opt => opt.Ignore())
            .ForMember(dest => dest.FromUser, opt => opt.Ignore())
            .ForMember(dest => dest.ToUser, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        // Reverse mappings for update scenarios (if needed)
        CreateMap<HabitDto, Habit>()
            .ForMember(dest => dest.TargetDays, opt => opt.MapFrom(src => 1))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.CheckIns, opt => opt.Ignore())
            .ForMember(dest => dest.UserBadges, opt => opt.Ignore())
            .ForMember(dest => dest.Encouragements, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore())
            .ForMember(dest => dest.Frequency, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Color, opt => opt.Ignore())
            .ForMember(dest => dest.IconName, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentStreak, opt => opt.Ignore())
            .ForMember(dest => dest.LongestStreak, opt => opt.Ignore())
            .ForMember(dest => dest.LastCompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<HabitEntryDto, HabitEntry>()
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        CreateMap<CheckInDto, CheckIn>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        CreateMap<EncouragementDto, Encouragement>()
            .ForMember(dest => dest.FromUser, opt => opt.Ignore())
            .ForMember(dest => dest.ToUser, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        // AI Recommendation DTOs mappings
        CreateMap<Habit, HabitSummaryDto>()
            .ForMember(dest => dest.TotalCheckIns, opt => opt.MapFrom(src => src.CheckIns.Count))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => DetermineHabitCategory(src.Name, src.Description)));

        CreateMap<CheckIn, CheckInSummaryDto>()
            .ForMember(dest => dest.HabitName, opt => opt.MapFrom(src => src.Habit.Name));
    }

    private static string DetermineHabitCategory(string name, string? description)
    {
        var fullText = $"{name} {description}".ToLower();
        
        if (fullText.Contains("water") || fullText.Contains("hydrat") || fullText.Contains("drink"))
            return "Health";
        if (fullText.Contains("exercise") || fullText.Contains("workout") || fullText.Contains("run") || fullText.Contains("walk") || fullText.Contains("gym"))
            return "Fitness";
        if (fullText.Contains("meditat") || fullText.Contains("mindful") || fullText.Contains("breathe"))
            return "Mindfulness";
        if (fullText.Contains("read") || fullText.Contains("book") || fullText.Contains("learn") || fullText.Contains("study"))
            return "Learning";
        if (fullText.Contains("sleep") || fullText.Contains("rest") || fullText.Contains("bed"))
            return "Sleep";
        if (fullText.Contains("eat") || fullText.Contains("meal") || fullText.Contains("nutrition") || fullText.Contains("diet"))
            return "Nutrition";
        if (fullText.Contains("work") || fullText.Contains("produc") || fullText.Contains("task") || fullText.Contains("focus"))
            return "Productivity";
        if (fullText.Contains("social") || fullText.Contains("friend") || fullText.Contains("family") || fullText.Contains("connect"))
            return "Social";
        if (fullText.Contains("money") || fullText.Contains("budget") || fullText.Contains("save") || fullText.Contains("financ"))
            return "Finance";
        if (fullText.Contains("clean") || fullText.Contains("organiz") || fullText.Contains("tidy"))
            return "Organization";
        
        return "General";
    }
} 