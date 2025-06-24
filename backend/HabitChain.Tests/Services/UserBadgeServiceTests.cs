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
using System.Linq.Expressions;
using Xunit;

namespace HabitChain.Tests.Services;

public class UserBadgeServiceTests : TestBase
{
    private readonly Mock<IUserBadgeRepository> _mockRepository;
    private readonly UserBadgeService _service;
    private readonly IFixture _fixture;

    public UserBadgeServiceTests()
    {
        _mockRepository = new Mock<IUserBadgeRepository>();
        _service = new UserBadgeService(_mockRepository.Object, Mapper);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateUserBadgeAsync_ValidDto_ReturnsUserBadgeDto()
    {
        // Arrange
        var createDto = _fixture.Create<CreateUserBadgeDto>();
        var userBadge = _fixture.Create<UserBadge>();
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserBadge>()))
            .ReturnsAsync(userBadge);

        // Act
        var result = await _service.CreateUserBadgeAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UserBadgeDto>();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<UserBadge>()), Times.Once);
    }

    [Fact]
    public async Task GetUserBadgeByIdAsync_ValidId_ReturnsUserBadgeDto()
    {
        // Arrange
        var userBadgeId = Guid.NewGuid();
        var userBadge = _fixture.Create<UserBadge>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(userBadgeId))
            .ReturnsAsync(userBadge);

        // Act
        var result = await _service.GetUserBadgeByIdAsync(userBadgeId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UserBadgeDto>();
        result.Id.Should().Be(userBadgeId);
    }

    [Fact]
    public async Task GetUserBadgeByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var userBadgeId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(userBadgeId))
            .ReturnsAsync((UserBadge?)null);

        // Act
        var result = await _service.GetUserBadgeByIdAsync(userBadgeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserBadgesByUserIdAsync_ValidUserId_ReturnsUserBadgeDtos()
    {
        // Arrange
        var userId = "test-user-id";
        var userBadges = _fixture.CreateMany<UserBadge>(3).ToList();
        
        _mockRepository.Setup(r => r.GetUserBadgesAsync(userId))
            .ReturnsAsync(userBadges);

        // Act
        var result = await _service.GetUserBadgesByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<UserBadgeDto>();
    }

    [Fact]
    public async Task GetUserBadgesByHabitIdAsync_ValidHabitId_ReturnsUserBadgeDtos()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var userBadges = _fixture.CreateMany<UserBadge>(2).ToList();
        
        _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserBadge, bool>>>()))
            .ReturnsAsync(userBadges);

        // Act
        var result = await _service.GetUserBadgesByHabitIdAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<UserBadgeDto>();
    }

    [Fact]
    public async Task DeleteUserBadgeAsync_ValidId_DeletesUserBadge()
    {
        // Arrange
        var userBadgeId = Guid.NewGuid();
        var userBadge = _fixture.Create<UserBadge>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(userBadgeId))
            .ReturnsAsync(userBadge);
        _mockRepository.Setup(r => r.DeleteAsync(userBadgeId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteUserBadgeAsync(userBadgeId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(userBadgeId), Times.Once);
    }

    [Fact]
    public async Task DeleteUserBadgeAsync_UserBadgeNotFound_ThrowsException()
    {
        // Arrange
        var userBadgeId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(userBadgeId))
            .ReturnsAsync((UserBadge?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.DeleteUserBadgeAsync(userBadgeId));
    }

    [Fact]
    public async Task HasUserEarnedBadgeAsync_WithEarnedBadge_ReturnsTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var badgeId = Guid.NewGuid();
        var userBadge = _fixture.Create<UserBadge>();
        
        _mockRepository.Setup(r => r.GetUserBadgeAsync(userId, badgeId))
            .ReturnsAsync(userBadge);

        // Act
        var result = await _service.HasUserEarnedBadgeAsync(userId, badgeId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasUserEarnedBadgeAsync_WithNotEarnedBadge_ReturnsFalse()
    {
        // Arrange
        var userId = "test-user-id";
        var badgeId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetUserBadgeAsync(userId, badgeId))
            .ReturnsAsync((UserBadge?)null);

        // Act
        var result = await _service.HasUserEarnedBadgeAsync(userId, badgeId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateUserBadgeAsync_SetsEarnedAtToCurrentTime()
    {
        // Arrange
        var createDto = _fixture.Create<CreateUserBadgeDto>();
        var userBadge = _fixture.Create<UserBadge>();
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserBadge>()))
            .ReturnsAsync(userBadge);

        // Act
        var result = await _service.CreateUserBadgeAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.EarnedAt.Should().NotBe(default(DateTime));
        _mockRepository.Verify(r => r.AddAsync(It.Is<UserBadge>(ub => 
            ub.EarnedAt != default(DateTime))), Times.Once);
    }

    [Theory]
    [InlineData("user1", "user2")]
    [InlineData("user123", "user456")]
    [InlineData("test-user", "another-user")]
    public async Task GetUserBadgesByUserIdAsync_WithDifferentUsers_ReturnsCorrectUserBadges(string userId1, string userId2)
    {
        // Arrange
        var userBadges1 = _fixture.CreateMany<UserBadge>(2).ToList();
        var userBadges2 = _fixture.CreateMany<UserBadge>(1).ToList();
        
        _mockRepository.Setup(r => r.GetUserBadgesAsync(userId1))
            .ReturnsAsync(userBadges1);
        _mockRepository.Setup(r => r.GetUserBadgesAsync(userId2))
            .ReturnsAsync(userBadges2);

        // Act
        var result1 = await _service.GetUserBadgesByUserIdAsync(userId1);
        var result2 = await _service.GetUserBadgesByUserIdAsync(userId2);

        // Assert
        result1.Should().HaveCount(2);
        result2.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserBadgesByHabitIdAsync_WithNoUserBadges_ReturnsEmptyCollection()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserBadge, bool>>>()))
            .ReturnsAsync(new List<UserBadge>());

        // Act
        var result = await _service.GetUserBadgesByHabitIdAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserBadgesByUserIdAsync_WithNoUserBadges_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = "test-user-id";
        
        _mockRepository.Setup(r => r.GetUserBadgesAsync(userId))
            .ReturnsAsync(new List<UserBadge>());

        // Act
        var result = await _service.GetUserBadgesByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
} 