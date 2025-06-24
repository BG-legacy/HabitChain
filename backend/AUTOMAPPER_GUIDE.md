# AutoMapper Guide for HabitChain Backend

This guide explains how to use AutoMapper for mapping between Domain Entities and DTOs in the HabitChain backend.

## Overview

AutoMapper is configured in `HabitChain.Application/Mappings/MappingProfile.cs` and is registered in `Program.cs`. It automatically handles the conversion between your domain entities and DTOs.

## Current Mappings

### User Mappings
- `User` → `UserDto`
- Maps `UserName` to `Username` property
- Maps `Id` from IdentityUser to `Id` property

### Habit Mappings
- `Habit` → `HabitDto`
- `CreateHabitDto` → `Habit` (for creation)
- `HabitDto` → `Habit` (for updates)

### HabitEntry Mappings
- `HabitEntry` → `HabitEntryDto`
- `CreateHabitEntryDto` → `HabitEntry`

### Badge Mappings
- `Badge` → `BadgeDto`
- `CreateBadgeDto` → `Badge`

### UserBadge Mappings
- `UserBadge` → `UserBadgeDto` (includes navigation properties)
- `CreateUserBadgeDto` → `UserBadge`

### CheckIn Mappings
- `CheckIn` → `CheckInDto` (includes Habit navigation property)
- `CreateCheckInDto` → `CheckIn`

### Encouragement Mappings
- `Encouragement` → `EncouragementDto` (includes User and Habit navigation properties)
- `CreateEncouragementDto` → `Encouragement`

## Usage Examples

### Basic Mapping in Services

```csharp
public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IMapper _mapper;

    public HabitService(IHabitRepository habitRepository, IMapper mapper)
    {
        _habitRepository = habitRepository;
        _mapper = mapper;
    }

    // Map single entity to DTO
    public async Task<HabitDto?> GetHabitByIdAsync(Guid id)
    {
        var habit = await _habitRepository.GetByIdAsync(id);
        return habit != null ? _mapper.Map<HabitDto>(habit) : null;
    }

    // Map collection of entities to DTOs
    public async Task<IEnumerable<HabitDto>> GetHabitsByUserIdAsync(string userId)
    {
        var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<HabitDto>>(habits);
    }

    // Map DTO to entity for creation
    public async Task<HabitDto> CreateHabitAsync(CreateHabitDto createHabitDto)
    {
        var habit = _mapper.Map<Habit>(createHabitDto);
        habit.Id = Guid.NewGuid();
        
        var createdHabit = await _habitRepository.AddAsync(habit);
        return _mapper.Map<HabitDto>(createdHabit);
    }

    // Map DTO to existing entity for updates
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
}
```

### Navigation Properties

AutoMapper automatically handles navigation properties when they are included in the mapping configuration:

```csharp
// In MappingProfile.cs
CreateMap<CheckIn, CheckInDto>()
    .ForMember(dest => dest.Habit, opt => opt.MapFrom(src => src.Habit));

// In your service
public async Task<CheckInDto?> GetCheckInWithHabitAsync(Guid checkInId)
{
    var checkIn = await _checkInRepository.GetCheckInWithHabitAsync(checkInId);
    return checkIn != null ? _mapper.Map<CheckInDto>(checkIn) : null;
}
```

### Complex Mappings

For complex scenarios, you can use custom mapping logic:

```csharp
// In MappingProfile.cs
CreateMap<User, UserDto>()
    .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
```

## Best Practices

### 1. Ignore Navigation Properties in Create Mappings
When mapping from DTOs to entities for creation, always ignore navigation properties:

```csharp
CreateMap<CreateHabitDto, Habit>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.Entries, opt => opt.Ignore())
    .ForMember(dest => dest.User, opt => opt.Ignore());
```

### 2. Handle Null Values
Always check for null before mapping:

```csharp
public async Task<HabitDto?> GetHabitByIdAsync(Guid id)
{
    var habit = await _habitRepository.GetByIdAsync(id);
    return habit != null ? _mapper.Map<HabitDto>(habit) : null;
}
```

### 3. Use Explicit Mapping for Updates
For updates, map to existing entities rather than creating new ones:

```csharp
_mapper.Map(updateDto, existingEntity);
```

### 4. Include Navigation Properties When Needed
Only include navigation properties in mappings when they're actually needed:

```csharp
// Only include if you need the full user data
CreateMap<Encouragement, EncouragementDto>()
    .ForMember(dest => dest.FromUser, opt => opt.MapFrom(src => src.FromUser))
    .ForMember(dest => dest.ToUser, opt => opt.MapFrom(src => src.ToUser));
```

## Adding New Mappings

When adding new entities and DTOs:

1. Create the DTO classes in `HabitChain.Application/DTOs/`
2. Add mappings to `MappingProfile.cs`
3. Include both entity-to-DTO and DTO-to-entity mappings
4. Remember to ignore navigation properties in create mappings
5. Test the mappings in your services

## Common Patterns

### Create Pattern
```csharp
var entity = _mapper.Map<Entity>(createDto);
entity.Id = Guid.NewGuid();
var created = await _repository.AddAsync(entity);
return _mapper.Map<EntityDto>(created);
```

### Update Pattern
```csharp
var existing = await _repository.GetByIdAsync(id);
if (existing == null) throw new NotFoundException();
_mapper.Map(updateDto, existing);
await _repository.UpdateAsync(existing);
return _mapper.Map<EntityDto>(existing);
```

### Get Pattern
```csharp
var entity = await _repository.GetByIdAsync(id);
return entity != null ? _mapper.Map<EntityDto>(entity) : null;
```

### List Pattern
```csharp
var entities = await _repository.GetAllAsync();
return _mapper.Map<IEnumerable<EntityDto>>(entities);
```

## Troubleshooting

### Common Issues

1. **Navigation Properties Not Mapping**: Ensure you've included the navigation property in the mapping configuration
2. **Circular References**: Use `.Ignore()` for navigation properties that could cause circular references
3. **Null Reference Exceptions**: Always check for null before mapping
4. **Missing Properties**: Ensure all required properties are mapped or ignored

### Debugging Mappings

You can enable AutoMapper validation in development:

```csharp
// In Program.cs (development only)
if (app.Environment.IsDevelopment())
{
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}
```

This will throw an exception if any mappings are misconfigured. 