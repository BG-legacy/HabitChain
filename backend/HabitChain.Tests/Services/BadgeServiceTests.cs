using FluentAssertions;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Application.Services;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;
using HabitChain.Domain.Interfaces;
using Moq;
using Xunit;

namespace HabitChain.Tests.Services;

public class BadgeServiceTests : TestBase
{
    private readonly Mock<IBadgeRepository> _mockBadgeRepository;
    private readonly BadgeService _badgeService;

    public BadgeServiceTests()
    {
        _mockBadgeRepository = new Mock<IBadgeRepository>();
        _badgeService = new BadgeService(_mockBadgeRepository.Object, Mapper);
    }

    [Fact]
    public async Task GetAllBadgesAsync_WithValidData_ReturnsAllBadgeDtos()
    {
        // Arrange
        var badges = new List<Badge>
        {
            CreateTestBadge("Badge 1", "Description 1"),
            CreateTestBadge("Badge 2", "Description 2"),
            CreateTestBadge("Badge 3", "Description 3")
        };

        _mockBadgeRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(badges);

        // Act
        var result = await _badgeService.GetAllBadgesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(badgeDto =>
        {
            badgeDto.Should().NotBeNull();
            badgeDto.Name.Should().NotBeNullOrEmpty();
            badgeDto.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task GetAllBadgesAsync_WithNoBadges_ReturnsEmptyCollection()
    {
        // Arrange
        _mockBadgeRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Badge>());

        // Act
        var result = await _badgeService.GetAllBadgesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBadgesAsync_WithValidData_ReturnsActiveBadgeDtos()
    {
        // Arrange
        var badges = new List<Badge>
        {
            CreateTestBadge("Active Badge 1", "Description 1"),
            CreateTestBadge("Active Badge 2", "Description 2")
        };

        _mockBadgeRepository.Setup(x => x.GetActiveBadgesAsync())
            .ReturnsAsync(badges);

        // Act
        var result = await _badgeService.GetActiveBadgesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(badgeDto =>
        {
            badgeDto.Should().NotBeNull();
            badgeDto.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task GetBadgeByIdAsync_WithValidId_ReturnsBadgeDto()
    {
        // Arrange
        var badgeId = Guid.NewGuid();
        var badge = CreateTestBadge("Test Badge", "Test Description");
        badge.Id = badgeId;

        _mockBadgeRepository.Setup(x => x.GetByIdAsync(badgeId))
            .ReturnsAsync(badge);

        // Act
        var result = await _badgeService.GetBadgeByIdAsync(badgeId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(badgeId);
        result.Name.Should().Be("Test Badge");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetBadgeByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var badgeId = Guid.NewGuid();

        _mockBadgeRepository.Setup(x => x.GetByIdAsync(badgeId))
            .ReturnsAsync((Badge?)null);

        // Act
        var result = await _badgeService.GetBadgeByIdAsync(badgeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateBadgeAsync_WithValidData_ReturnsCreatedBadgeDto()
    {
        // Arrange
        var createBadgeDto = new CreateBadgeDto
        {
            Name = "New Badge",
            Description = "New Description",
            Type = BadgeType.Streak
        };

        var createdBadge = CreateTestBadge(createBadgeDto.Name, createBadgeDto.Description);
        createdBadge.Type = createBadgeDto.Type;

        _mockBadgeRepository.Setup(x => x.AddAsync(It.IsAny<Badge>()))
            .ReturnsAsync(createdBadge);

        // Act
        var result = await _badgeService.CreateBadgeAsync(createBadgeDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createBadgeDto.Name);
        result.Description.Should().Be(createBadgeDto.Description);
        result.Type.Should().Be(createBadgeDto.Type);
        result.Id.Should().NotBeEmpty();

        _mockBadgeRepository.Verify(x => x.AddAsync(It.Is<Badge>(b =>
            b.Name == createBadgeDto.Name &&
            b.Description == createBadgeDto.Description &&
            b.Type == createBadgeDto.Type)), Times.Once);
    }

    [Fact]
    public async Task UpdateBadgeAsync_WithValidIdAndData_ReturnsUpdatedBadgeDto()
    {
        // Arrange
        var badgeId = Guid.NewGuid();
        var existingBadge = CreateTestBadge("Old Name", "Old Description");
        existingBadge.Id = badgeId;

        var updateBadgeDto = new CreateBadgeDto
        {
            Name = "Updated Badge",
            Description = "Updated Description",
            Type = BadgeType.Total
        };

        _mockBadgeRepository.Setup(x => x.GetByIdAsync(badgeId))
            .ReturnsAsync(existingBadge);
        _mockBadgeRepository.Setup(x => x.UpdateAsync(It.IsAny<Badge>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _badgeService.UpdateBadgeAsync(badgeId, updateBadgeDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(badgeId);
        result.Name.Should().Be(updateBadgeDto.Name);
        result.Description.Should().Be(updateBadgeDto.Description);
        result.Type.Should().Be(updateBadgeDto.Type);

        _mockBadgeRepository.Verify(x => x.UpdateAsync(It.Is<Badge>(b =>
            b.Id == badgeId &&
            b.Name == updateBadgeDto.Name &&
            b.Description == updateBadgeDto.Description &&
            b.Type == updateBadgeDto.Type)), Times.Once);
    }

    [Fact]
    public async Task UpdateBadgeAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var badgeId = Guid.NewGuid();
        var updateBadgeDto = new CreateBadgeDto
        {
            Name = "Updated Badge",
            Description = "Updated Description",
            Type = BadgeType.Streak
        };

        _mockBadgeRepository.Setup(x => x.GetByIdAsync(badgeId))
            .ReturnsAsync((Badge?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _badgeService.UpdateBadgeAsync(badgeId, updateBadgeDto));

        exception.Message.Should().Contain($"Badge with ID {badgeId} not found");
    }

    [Fact]
    public async Task DeleteBadgeAsync_WithValidId_DeletesBadge()
    {
        // Arrange
        var badgeId = Guid.NewGuid();
        var badge = CreateTestBadge("Test Badge", "Test Description");
        badge.Id = badgeId;

        _mockBadgeRepository.Setup(x => x.GetByIdAsync(badgeId))
            .ReturnsAsync(badge);
        _mockBadgeRepository.Setup(x => x.DeleteAsync(badgeId))
            .Returns(Task.CompletedTask);

        // Act
        await _badgeService.DeleteBadgeAsync(badgeId);

        // Assert
        _mockBadgeRepository.Verify(x => x.DeleteAsync(badgeId), Times.Once);
    }

    [Fact]
    public async Task DeleteBadgeAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var badgeId = Guid.NewGuid();

        _mockBadgeRepository.Setup(x => x.GetByIdAsync(badgeId))
            .ReturnsAsync((Badge?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _badgeService.DeleteBadgeAsync(badgeId));

        exception.Message.Should().Contain($"Badge with ID {badgeId} not found");
    }

    [Theory]
    [InlineData(BadgeType.Streak)]
    [InlineData(BadgeType.Total)]
    [InlineData(BadgeType.Consistency)]
    [InlineData(BadgeType.Special)]
    public async Task CreateBadgeAsync_WithDifferentBadgeTypes_CreatesBadgeWithCorrectType(BadgeType badgeType)
    {
        // Arrange
        var createBadgeDto = new CreateBadgeDto
        {
            Name = "Test Badge",
            Description = "Test Description",
            Type = badgeType
        };

        var createdBadge = CreateTestBadge(createBadgeDto.Name, createBadgeDto.Description);
        createdBadge.Type = badgeType;

        _mockBadgeRepository.Setup(x => x.AddAsync(It.IsAny<Badge>()))
            .ReturnsAsync(createdBadge);

        // Act
        var result = await _badgeService.CreateBadgeAsync(createBadgeDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(badgeType);

        _mockBadgeRepository.Verify(x => x.AddAsync(It.Is<Badge>(b =>
            b.Type == badgeType)), Times.Once);
    }

    [Fact]
    public async Task GetAllBadgesAsync_WithInactiveBadges_IncludesInactiveBadges()
    {
        // Arrange
        var activeBadge = CreateTestBadge("Active Badge", "Active Description");
        var inactiveBadge = CreateTestBadge("Inactive Badge", "Inactive Description");
        inactiveBadge.IsActive = false;

        var badges = new List<Badge> { activeBadge, inactiveBadge };

        _mockBadgeRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(badges);

        // Act
        var result = await _badgeService.GetAllBadgesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.IsActive == true);
        result.Should().Contain(b => b.IsActive == false);
    }

    [Fact]
    public async Task GetActiveBadgesAsync_WithInactiveBadges_ExcludesInactiveBadges()
    {
        // Arrange
        var activeBadge = CreateTestBadge("Active Badge", "Active Description");
        var inactiveBadge = CreateTestBadge("Inactive Badge", "Inactive Description");
        inactiveBadge.IsActive = false;

        var badges = new List<Badge> { activeBadge, inactiveBadge };

        _mockBadgeRepository.Setup(x => x.GetActiveBadgesAsync())
            .ReturnsAsync(badges.Where(b => b.IsActive).ToList());

        // Act
        var result = await _badgeService.GetActiveBadgesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().AllSatisfy(b => b.IsActive.Should().BeTrue());
    }
} 