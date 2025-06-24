using AutoFixture;
using AutoMapper;
using FluentAssertions;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Application.Services;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Tests;
using Moq;
using Xunit;

namespace HabitChain.Tests.Services;

public class CheckInServiceTests : TestBase
{
    private readonly Mock<ICheckInRepository> _mockRepository;
    private readonly CheckInService _service;
    private readonly IFixture _fixture;

    public CheckInServiceTests()
    {
        _mockRepository = new Mock<ICheckInRepository>();
        _service = new CheckInService(_mockRepository.Object, Mapper);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateCheckInAsync_ValidDto_ReturnsCheckInDto()
    {
        // Arrange
        var createDto = _fixture.Create<CreateCheckInDto>();
        var checkIn = _fixture.Create<CheckIn>();
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<CheckIn>()))
            .ReturnsAsync(checkIn);

        // Act
        var result = await _service.CreateCheckInAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CheckInDto>();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<CheckIn>()), Times.Once);
    }

    [Fact]
    public async Task GetCheckInByIdAsync_ValidId_ReturnsCheckInDto()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        var checkIn = _fixture.Create<CheckIn>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync(checkIn);

        // Act
        var result = await _service.GetCheckInByIdAsync(checkInId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CheckInDto>();
        result.Id.Should().Be(checkInId);
    }

    [Fact]
    public async Task GetCheckInByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync((CheckIn?)null);

        // Act
        var result = await _service.GetCheckInByIdAsync(checkInId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCheckInsByUserIdAsync_ValidUserId_ReturnsCheckInDtos()
    {
        // Arrange
        var userId = "test-user-id";
        var checkIns = _fixture.CreateMany<CheckIn>(3).ToList();
        
        _mockRepository.Setup(r => r.GetCheckInsByUserIdAsync(userId))
            .ReturnsAsync(checkIns);

        // Act
        var result = await _service.GetCheckInsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<CheckInDto>();
    }

    [Fact]
    public async Task GetCheckInsByHabitIdAsync_ValidHabitId_ReturnsCheckInDtos()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var checkIns = _fixture.CreateMany<CheckIn>(2).ToList();
        
        _mockRepository.Setup(r => r.GetCheckInsByHabitIdAsync(habitId))
            .ReturnsAsync(checkIns);

        // Act
        var result = await _service.GetCheckInsByHabitIdAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<CheckInDto>();
    }

    [Fact]
    public async Task UpdateCheckInAsync_ValidDto_ReturnsUpdatedCheckInDto()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateCheckInDto>();
        var existingCheckIn = _fixture.Create<CheckIn>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync(existingCheckIn);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CheckIn>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateCheckInAsync(checkInId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CheckInDto>();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<CheckIn>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCheckInAsync_CheckInNotFound_ThrowsException()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateCheckInDto>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync((CheckIn?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdateCheckInAsync(checkInId, updateDto));
    }

    [Fact]
    public async Task DeleteCheckInAsync_ValidId_DeletesCheckIn()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        var checkIn = _fixture.Create<CheckIn>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync(checkIn);
        _mockRepository.Setup(r => r.DeleteAsync(checkInId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteCheckInAsync(checkInId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(checkInId), Times.Once);
    }

    [Fact]
    public async Task DeleteCheckInAsync_CheckInNotFound_ThrowsException()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync((CheckIn?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.DeleteCheckInAsync(checkInId));
    }

    [Fact]
    public async Task GetCheckInsByDateRangeAsync_ValidRange_ReturnsCheckInDtos()
    {
        // Arrange
        var userId = "test-user-id";
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var checkIns = _fixture.CreateMany<CheckIn>(5).ToList();
        
        _mockRepository.Setup(r => r.GetCheckInsForDateRangeAsync(userId, startDate, endDate))
            .ReturnsAsync(checkIns);

        // Act
        var result = await _service.GetCheckInsByDateRangeAsync(userId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().AllBeOfType<CheckInDto>();
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ValidHabitId_ReturnsStreak()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var expectedStreak = 5;
        
        _mockRepository.Setup(r => r.GetCurrentStreakAsync(habitId))
            .ReturnsAsync(expectedStreak);

        // Act
        var result = await _mockRepository.Object.GetCurrentStreakAsync(habitId);

        // Assert
        result.Should().Be(expectedStreak);
    }

    [Fact]
    public async Task HasCheckInForDateAsync_ValidParameters_ReturnsBoolean()
    {
        // Arrange
        var userId = "test-user-id";
        var habitId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var checkIns = _fixture.CreateMany<CheckIn>(2).ToList();
        checkIns[0].HabitId = habitId;
        checkIns[1].HabitId = habitId;
        
        _mockRepository.Setup(r => r.GetCheckInsForDateRangeAsync(userId, date.Date, date.Date.AddDays(1).AddTicks(-1)))
            .ReturnsAsync(checkIns);

        // Act
        var result = await _service.HasCheckInForDateAsync(userId, habitId, date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasCheckInForDateAsync_NoCheckInForDate_ReturnsFalse()
    {
        // Arrange
        var userId = "test-user-id";
        var habitId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var checkIns = new List<CheckIn>(); // Empty list
        
        _mockRepository.Setup(r => r.GetCheckInsForDateRangeAsync(userId, date.Date, date.Date.AddDays(1).AddTicks(-1)))
            .ReturnsAsync(checkIns);

        // Act
        var result = await _service.HasCheckInForDateAsync(userId, habitId, date);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCheckInsByDateRangeAsync_ValidatesDateRange()
    {
        // Arrange
        var userId = "test-user-id";
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1); // End before start

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetCheckInsByDateRangeAsync(userId, startDate, endDate));
    }

    [Fact]
    public async Task GetCheckInsByDateRangeAsync_ValidatesMaxDateRange()
    {
        // Arrange
        var userId = "test-user-id";
        var startDate = DateTime.UtcNow.AddDays(-400); // Too far in past
        var endDate = DateTime.UtcNow;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetCheckInsByDateRangeAsync(userId, startDate, endDate));
    }

    [Fact]
    public async Task CreateCheckInAsync_ValidatesCompletedAtDate()
    {
        // Arrange
        var createDto = _fixture.Create<CreateCheckInDto>();
        createDto.CompletedAt = DateTime.UtcNow.AddDays(1); // Future date

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateCheckInAsync(createDto));
    }

    [Fact]
    public async Task CreateCheckInAsync_ValidatesStreakDay()
    {
        // Arrange
        var createDto = _fixture.Create<CreateCheckInDto>();
        createDto.StreakDay = 0; // Invalid streak day

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateCheckInAsync(createDto));
    }

    [Fact]
    public async Task UpdateCheckInAsync_ValidatesCompletedAtDate()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateCheckInDto>();
        updateDto.CompletedAt = DateTime.UtcNow.AddDays(1); // Future date
        var existingCheckIn = _fixture.Create<CheckIn>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync(existingCheckIn);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateCheckInAsync(checkInId, updateDto));
    }

    [Fact]
    public async Task UpdateCheckInAsync_ValidatesStreakDay()
    {
        // Arrange
        var checkInId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateCheckInDto>();
        updateDto.StreakDay = -1; // Invalid streak day
        var existingCheckIn = _fixture.Create<CheckIn>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(checkInId))
            .ReturnsAsync(existingCheckIn);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateCheckInAsync(checkInId, updateDto));
    }

    [Fact]
    public async Task CreateCheckInAsync_ValidatesNotesLength()
    {
        // Arrange
        var createDto = _fixture.Create<CreateCheckInDto>();
        createDto.Notes = new string('a', 1001); // Too long

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateCheckInAsync(createDto));
    }

    [Fact]
    public async Task CreateCheckInAsync_ValidatesNotesNoHtml()
    {
        // Arrange
        var createDto = _fixture.Create<CreateCheckInDto>();
        createDto.Notes = "<script>alert('xss')</script>"; // Contains HTML

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateCheckInAsync(createDto));
    }
} 