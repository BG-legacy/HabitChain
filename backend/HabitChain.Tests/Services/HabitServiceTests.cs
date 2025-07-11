using AutoFixture;
using FluentAssertions;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Application.Services;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using HabitChain.Domain.Interfaces;
using HabitChain.Tests;
using Moq;
using Xunit;

namespace HabitChain.Tests.Services;

public class HabitServiceTests : TestBase
{
    private readonly Mock<IHabitRepository> _mockHabitRepository;
    private readonly Mock<ICheckInRepository> _mockCheckInRepository;
    private readonly Mock<IHabitEntryRepository> _mockHabitEntryRepository;
    private readonly Mock<IBadgeEarningService> _mockBadgeEarningService;
    private readonly HabitService _habitService;
    private readonly IFixture _fixture;

    public HabitServiceTests()
    {
        _mockHabitRepository = new Mock<IHabitRepository>();
        _mockCheckInRepository = new Mock<ICheckInRepository>();
        _mockHabitEntryRepository = new Mock<IHabitEntryRepository>();
        _mockBadgeEarningService = new Mock<IBadgeEarningService>();
        _habitService = new HabitService(_mockHabitRepository.Object, _mockCheckInRepository.Object, _mockHabitEntryRepository.Object, _mockBadgeEarningService.Object, Mapper);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetHabitsByUserIdAsync_WithValidUserId_ReturnsHabitDtos()
    {
        // Arrange
        var userId = "user123";
        var habits = new List<Habit>
        {
            CreateTestHabit(userId, "Habit 1", "Description 1"),
            CreateTestHabit(userId, "Habit 2", "Description 2")
        };

        _mockHabitRepository.Setup(x => x.GetHabitsByUserIdAsync(userId))
            .ReturnsAsync(habits);

        // Act
        var result = await _habitService.GetHabitsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(habitDto =>
        {
            habitDto.Should().NotBeNull();
            habitDto.Name.Should().NotBeNullOrEmpty();
            habitDto.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task GetHabitsByUserIdAsync_WithNoHabits_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = "user123";

        _mockHabitRepository.Setup(x => x.GetHabitsByUserIdAsync(userId))
            .ReturnsAsync(new List<Habit>());

        // Act
        var result = await _habitService.GetHabitsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveHabitsByUserIdAsync_WithValidUserId_ReturnsActiveHabitDtos()
    {
        // Arrange
        var userId = "user123";
        var habits = new List<Habit>
        {
            CreateTestHabit(userId, "Active Habit 1", "Description 1"),
            CreateTestHabit(userId, "Active Habit 2", "Description 2")
        };

        _mockHabitRepository.Setup(x => x.GetActiveHabitsByUserIdAsync(userId))
            .ReturnsAsync(habits);

        // Act
        var result = await _habitService.GetActiveHabitsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(habitDto =>
        {
            habitDto.Should().NotBeNull();
            habitDto.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task GetHabitByIdAsync_WithValidId_ReturnsHabitDto()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var habit = CreateTestHabit("user123", "Test Habit", "Test Description");
        habit.Id = habitId;

        _mockHabitRepository.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(habit);

        // Act
        var result = await _habitService.GetHabitByIdAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(habitId);
        result.Name.Should().Be("Test Habit");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetHabitByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var habitId = Guid.NewGuid();

        _mockHabitRepository.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync((Habit?)null);

        // Act
        var result = await _habitService.GetHabitByIdAsync(habitId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateHabitAsync_WithValidData_ReturnsCreatedHabitDto()
    {
        // Arrange
        var createHabitDto = new CreateHabitDto
        {
            Name = "New Habit",
            Description = "New Description",
            UserId = "user123",
            Frequency = HabitFrequency.Daily
        };

        var createdHabit = CreateTestHabit(createHabitDto.UserId, createHabitDto.Name, createHabitDto.Description);
        createdHabit.Frequency = createHabitDto.Frequency;

        _mockHabitRepository.Setup(x => x.AddAsync(It.IsAny<Habit>()))
            .ReturnsAsync(createdHabit);

        // Act
        var result = await _habitService.CreateHabitAsync(createHabitDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createHabitDto.Name);
        result.Description.Should().Be(createHabitDto.Description);
        result.UserId.Should().Be(createHabitDto.UserId);
        result.Frequency.Should().Be(createHabitDto.Frequency);
        result.Id.Should().NotBeEmpty();

        _mockHabitRepository.Verify(x => x.AddAsync(It.Is<Habit>(h =>
            h.Name == createHabitDto.Name &&
            h.Description == createHabitDto.Description &&
            h.UserId == createHabitDto.UserId &&
            h.Frequency == createHabitDto.Frequency)), Times.Once);
    }

    [Fact]
    public async Task UpdateHabitAsync_WithValidIdAndData_ReturnsUpdatedHabitDto()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var existingHabit = CreateTestHabit("user123", "Old Name", "Old Description");
        existingHabit.Id = habitId;

        var updateHabitDto = new CreateHabitDto
        {
            Name = "Updated Habit",
            Description = "Updated Description",
            UserId = "user123",
            Frequency = HabitFrequency.Weekly
        };

        _mockHabitRepository.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(existingHabit);
        _mockHabitRepository.Setup(x => x.UpdateAsync(It.IsAny<Habit>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _habitService.UpdateHabitAsync(habitId, updateHabitDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(habitId);
        result.Name.Should().Be(updateHabitDto.Name);
        result.Description.Should().Be(updateHabitDto.Description);
        result.Frequency.Should().Be(updateHabitDto.Frequency);

        _mockHabitRepository.Verify(x => x.UpdateAsync(It.Is<Habit>(h =>
            h.Id == habitId &&
            h.Name == updateHabitDto.Name &&
            h.Description == updateHabitDto.Description &&
            h.Frequency == updateHabitDto.Frequency)), Times.Once);
    }

    [Fact]
    public async Task UpdateHabitAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var updateHabitDto = new CreateHabitDto
        {
            Name = "Updated Habit",
            Description = "Updated Description",
            UserId = "user123",
            Frequency = HabitFrequency.Daily
        };

        _mockHabitRepository.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync((Habit?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _habitService.UpdateHabitAsync(habitId, updateHabitDto));

        exception.Message.Should().Contain($"Habit with ID {habitId} not found");
    }

    [Fact]
    public async Task DeleteHabitAsync_WithValidId_DeletesHabit()
    {
        // Arrange
        var habitId = Guid.NewGuid();

        _mockHabitRepository.Setup(x => x.ExistsAsync(habitId))
            .ReturnsAsync(true);
        _mockHabitRepository.Setup(x => x.DeleteAsync(habitId))
            .Returns(Task.CompletedTask);

        // Act
        await _habitService.DeleteHabitAsync(habitId);

        // Assert
        _mockHabitRepository.Verify(x => x.DeleteAsync(habitId), Times.Once);
    }

    [Fact]
    public async Task DeleteHabitAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var habitId = Guid.NewGuid();

        _mockHabitRepository.Setup(x => x.ExistsAsync(habitId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _habitService.DeleteHabitAsync(habitId));

        exception.Message.Should().Contain($"Habit with ID {habitId} not found");
    }

    [Fact]
    public async Task GetHabitWithEntriesAsync_WithValidId_ReturnsHabitWithEntries()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var habit = CreateTestHabit("user123", "Test Habit", "Test Description");
        habit.Id = habitId;
        habit.Entries = new List<HabitEntry>
        {
            new HabitEntry
            {
                Id = Guid.NewGuid(),
                HabitId = habitId,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockHabitRepository.Setup(x => x.GetHabitWithEntriesAsync(habitId))
            .ReturnsAsync(habit);

        // Act
        var result = await _habitService.GetHabitWithEntriesAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(habitId);
        result.Name.Should().Be("Test Habit");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetHabitWithEntriesAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var habitId = Guid.NewGuid();

        _mockHabitRepository.Setup(x => x.GetHabitWithEntriesAsync(habitId))
            .ReturnsAsync((Habit?)null);

        // Act
        var result = await _habitService.GetHabitWithEntriesAsync(habitId);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(HabitFrequency.Daily)]
    [InlineData(HabitFrequency.Weekly)]
    [InlineData(HabitFrequency.Monthly)]
    public async Task CreateHabitAsync_WithDifferentFrequencies_CreatesHabitWithCorrectFrequency(HabitFrequency frequency)
    {
        // Arrange
        var createHabitDto = new CreateHabitDto
        {
            Name = "Test Habit",
            Description = "Test Description",
            UserId = "user123",
            Frequency = frequency
        };

        var createdHabit = CreateTestHabit(createHabitDto.UserId, createHabitDto.Name, createHabitDto.Description);
        createdHabit.Frequency = frequency;

        _mockHabitRepository.Setup(x => x.AddAsync(It.IsAny<Habit>()))
            .ReturnsAsync(createdHabit);

        // Act
        var result = await _habitService.CreateHabitAsync(createHabitDto);

        // Assert
        result.Should().NotBeNull();
        result.Frequency.Should().Be(frequency);

        _mockHabitRepository.Verify(x => x.AddAsync(It.Is<Habit>(h =>
            h.Frequency == frequency)), Times.Once);
    }
} 