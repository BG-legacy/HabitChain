using AutoFixture;
using AutoMapper;
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

public class EncouragementServiceTests : TestBase
{
    private readonly Mock<IEncouragementRepository> _mockRepository;
    private readonly EncouragementService _service;
    private readonly IFixture _fixture;

    public EncouragementServiceTests()
    {
        _mockRepository = new Mock<IEncouragementRepository>();
        _service = new EncouragementService(_mockRepository.Object, Mapper);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidDto_ReturnsEncouragementDto()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        var encouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Encouragement>()))
            .ReturnsAsync(encouragement);

        // Act
        var result = await _service.CreateEncouragementAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EncouragementDto>();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Encouragement>()), Times.Once);
    }

    [Fact]
    public async Task GetEncouragementByIdAsync_ValidId_ReturnsEncouragementDto()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var encouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync(encouragement);

        // Act
        var result = await _service.GetEncouragementByIdAsync(encouragementId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EncouragementDto>();
        result.Id.Should().Be(encouragementId);
    }

    [Fact]
    public async Task GetEncouragementByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync((Encouragement?)null);

        // Act
        var result = await _service.GetEncouragementByIdAsync(encouragementId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEncouragementsByUserIdAsync_ValidUserId_ReturnsEncouragementDtos()
    {
        // Arrange
        var userId = "test-user-id";
        var encouragements = _fixture.CreateMany<Encouragement>(3).ToList();
        
        _mockRepository.Setup(r => r.GetReceivedEncouragements(userId))
            .ReturnsAsync(encouragements);

        // Act
        var result = await _service.GetEncouragementsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<EncouragementDto>();
    }

    [Fact]
    public async Task GetEncouragementsByFromUserIdAsync_ValidUserId_ReturnsEncouragementDtos()
    {
        // Arrange
        var fromUserId = "test-user-id";
        var encouragements = _fixture.CreateMany<Encouragement>(3).ToList();
        
        _mockRepository.Setup(r => r.GetSentEncouragements(fromUserId))
            .ReturnsAsync(encouragements);

        // Act
        var result = await _service.GetEncouragementsByFromUserIdAsync(fromUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<EncouragementDto>();
    }

    [Fact]
    public async Task GetEncouragementsByHabitIdAsync_ValidHabitId_ReturnsEncouragementDtos()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var encouragements = _fixture.CreateMany<Encouragement>(2).ToList();
        
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(encouragements);

        // Act
        var result = await _service.GetEncouragementsByHabitIdAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<EncouragementDto>();
    }

    [Fact]
    public async Task GetUnreadEncouragementsByUserIdAsync_ValidUserId_ReturnsUnreadEncouragements()
    {
        // Arrange
        var userId = "test-user-id";
        var encouragements = _fixture.CreateMany<Encouragement>(2).ToList();
        encouragements.ForEach(e => e.IsRead = false);
        
        _mockRepository.Setup(r => r.GetUnreadEncouragements(userId))
            .ReturnsAsync(encouragements);

        // Act
        var result = await _service.GetUnreadEncouragementsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<EncouragementDto>();
        result.Should().AllSatisfy(e => e.IsRead.Should().BeFalse());
    }

    [Fact]
    public async Task UpdateEncouragementAsync_ValidDto_ReturnsUpdatedEncouragementDto()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateEncouragementDto>();
        var existingEncouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync(existingEncouragement);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Encouragement>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateEncouragementAsync(encouragementId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EncouragementDto>();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Encouragement>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEncouragementAsync_EncouragementNotFound_ThrowsException()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateEncouragementDto>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync((Encouragement?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdateEncouragementAsync(encouragementId, updateDto));
    }

    [Fact]
    public async Task DeleteEncouragementAsync_ValidId_DeletesEncouragement()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var encouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync(encouragement);
        _mockRepository.Setup(r => r.DeleteAsync(encouragementId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteEncouragementAsync(encouragementId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(encouragementId), Times.Once);
    }

    [Fact]
    public async Task DeleteEncouragementAsync_EncouragementNotFound_ThrowsException()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync((Encouragement?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.DeleteEncouragementAsync(encouragementId));
    }

    [Fact]
    public async Task MarkAsReadAsync_ValidId_MarksEncouragementAsRead()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.MarkAsReadAsync(encouragementId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarkAsReadAsync(encouragementId);

        // Assert
        _mockRepository.Verify(r => r.MarkAsReadAsync(encouragementId), Times.Once);
    }

    [Theory]
    [InlineData(EncouragementType.General)]
    [InlineData(EncouragementType.Milestone)]
    [InlineData(EncouragementType.Streak)]
    [InlineData(EncouragementType.Comeback)]
    [InlineData(EncouragementType.Achievement)]
    public async Task CreateEncouragementAsync_WithDifferentEncouragementTypes_CreatesEncouragementWithCorrectType(EncouragementType encouragementType)
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.Type = encouragementType;
        var encouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Encouragement>()))
            .ReturnsAsync(encouragement);

        // Act
        var result = await _service.CreateEncouragementAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(encouragementType);
        _mockRepository.Verify(r => r.AddAsync(It.Is<Encouragement>(e => e.Type == encouragementType)), Times.Once);
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidatesMessageLength()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.Message = new string('a', 501); // Too long
        createDto.Type = EncouragementType.General;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateEncouragementAsync(createDto));
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidatesMessageNotEmpty()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.Message = string.Empty; // Empty message
        createDto.Type = EncouragementType.General;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateEncouragementAsync(createDto));
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidatesMessageNoHtml()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.Message = "<script>alert('xss')</script>"; // Contains HTML
        createDto.Type = EncouragementType.General;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateEncouragementAsync(createDto));
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidatesEncouragementType()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.Type = (EncouragementType)999; // Invalid enum value

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateEncouragementAsync(createDto));
    }

    [Fact]
    public async Task UpdateEncouragementAsync_ValidatesMessageLength()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateEncouragementDto>();
        updateDto.Message = new string('a', 501); // Too long
        updateDto.Type = EncouragementType.General;
        var existingEncouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync(existingEncouragement);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateEncouragementAsync(encouragementId, updateDto));
    }

    [Fact]
    public async Task UpdateEncouragementAsync_ValidatesMessageNotEmpty()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateEncouragementDto>();
        updateDto.Message = string.Empty; // Empty message
        updateDto.Type = EncouragementType.General;
        var existingEncouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync(existingEncouragement);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateEncouragementAsync(encouragementId, updateDto));
    }

    [Fact]
    public async Task UpdateEncouragementAsync_ValidatesMessageNoHtml()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateEncouragementDto>();
        updateDto.Message = "<script>alert('xss')</script>"; // Contains HTML
        updateDto.Type = EncouragementType.General;
        var existingEncouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync(existingEncouragement);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateEncouragementAsync(encouragementId, updateDto));
    }

    [Fact]
    public async Task UpdateEncouragementAsync_ValidatesEncouragementType()
    {
        // Arrange
        var encouragementId = Guid.NewGuid();
        var updateDto = _fixture.Create<CreateEncouragementDto>();
        updateDto.Type = (EncouragementType)999; // Invalid enum value
        var existingEncouragement = _fixture.Create<Encouragement>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(encouragementId))
            .ReturnsAsync(existingEncouragement);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.UpdateEncouragementAsync(encouragementId, updateDto));
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidatesFromUserIdNotEmpty()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.FromUserId = string.Empty; // Empty sender ID
        createDto.Type = EncouragementType.General;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateEncouragementAsync(createDto));
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidatesToUserIdNotEmpty()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.ToUserId = string.Empty; // Empty recipient ID
        createDto.Type = EncouragementType.General;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateEncouragementAsync(createDto));
    }

    [Fact]
    public async Task CreateEncouragementAsync_ValidatesFromUserIdNotEqualToToUserId()
    {
        // Arrange
        var createDto = _fixture.Create<CreateEncouragementDto>();
        createDto.FromUserId = "same-user-id";
        createDto.ToUserId = "same-user-id"; // Same user
        createDto.Type = EncouragementType.General;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateEncouragementAsync(createDto));
    }
} 