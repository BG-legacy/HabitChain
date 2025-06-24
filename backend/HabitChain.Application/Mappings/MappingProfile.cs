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
        CreateMap<Habit, HabitDto>();
        CreateMap<CreateHabitDto, Habit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CurrentStreak, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.LongestStreak, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.LastCompletedAt, opt => opt.Ignore())
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
        CreateMap<Badge, BadgeDto>();
        CreateMap<CreateBadgeDto, Badge>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconName ?? string.Empty))
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
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.CheckIns, opt => opt.Ignore())
            .ForMember(dest => dest.UserBadges, opt => opt.Ignore())
            .ForMember(dest => dest.Encouragements, opt => opt.Ignore());

        CreateMap<HabitEntryDto, HabitEntry>()
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        CreateMap<CheckInDto, CheckIn>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());

        CreateMap<EncouragementDto, Encouragement>()
            .ForMember(dest => dest.FromUser, opt => opt.Ignore())
            .ForMember(dest => dest.ToUser, opt => opt.Ignore())
            .ForMember(dest => dest.Habit, opt => opt.Ignore());
    }
} 