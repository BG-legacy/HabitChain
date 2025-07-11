using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using HabitChain.Application.Mappings;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace HabitChain.Tests;

public abstract class TestBase
{
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;

    protected TestBase()
    {
        Fixture = new Fixture();
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        Fixture.Customize(new AutoMoqCustomization());

        // Configure AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        
        Mapper = mapperConfig.CreateMapper();
        Fixture.Register(() => Mapper);
    }

    protected Mock<UserManager<User>> CreateMockUserManager()
    {
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object,
            null, null, null, null, null, null, null, null);

        return userManager;
    }

    protected User CreateTestUser(string? id = null, string? email = null, string? username = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Email = email ?? Fixture.Create<string>(),
            UserName = username ?? Fixture.Create<string>(),
            FirstName = Fixture.Create<string>(),
            LastName = Fixture.Create<string>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected Habit CreateTestHabit(string userId, string? name = null, string? description = null)
    {
        return new Habit
        {
            Id = Guid.NewGuid(),
            Name = name ?? Fixture.Create<string>(),
            Description = description ?? Fixture.Create<string>(),
            UserId = userId,
            Frequency = HabitFrequency.Daily,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected Badge CreateTestBadge(string? name = null, string? description = null)
    {
        return new Badge
        {
            Id = Guid.NewGuid(),
            Name = name ?? Fixture.Create<string>(),
            Description = description ?? Fixture.Create<string>(),
            Type = BadgeType.Streak,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
} 