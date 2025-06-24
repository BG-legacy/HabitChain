using FluentAssertions;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Application.Services;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using Moq;
using Xunit;

namespace HabitChain.Tests.Services;

public class HabitEntryServiceTests : TestBase
{
    private readonly Mock<IHabitEntryRepository> _mockHabitEntryRepository;
    private readonly HabitEntryService _habitEntryService;

    public HabitEntryServiceTests()
    {
        _mockHabitEntryRepository = new Mock<IHabitEntryRepository>();
        _habitEntryService = new HabitEntryService(_mockHabitEntryRepository.Object, Mapper);
    }

    [Fact]
    public async Task GetEntriesByHabitIdAsync_WithValidHabitId_ReturnsHabitEntryDtos()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var entries = new List<HabitEntry>
        {
            CreateTestHabitEntry(habitId),
            CreateTestHabitEntry(habitId)
        };

        _mockHabitEntryRepository.Setup(x => x.GetEntriesByHabitIdAsync(habitId))
            .ReturnsAsync(entries);

        // Act
        var result = await _habitEntryService.GetEntriesByHabitIdAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(entryDto =>
        {
            entryDto.Should().NotBeNull();
            entryDto.HabitId.Should().Be(habitId);
        });
    }

    [Fact]
    public async Task GetEntriesByHabitIdAsync_WithNoEntries_ReturnsEmptyCollection()
    {
        // Arrange
        var habitId = Guid.NewGuid();

        _mockHabitEntryRepository.Setup(x => x.GetEntriesByHabitIdAsync(habitId))
            .ReturnsAsync(new List<HabitEntry>());

        // Act
        var result = await _habitEntryService.GetEntriesByHabitIdAsync(habitId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEntriesByDateRangeAsync_WithValidDateRange_ReturnsHabitEntryDtos()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var entries = new List<HabitEntry>
        {
            CreateTestHabitEntry(habitId, startDate.AddDays(1)),
            CreateTestHabitEntry(habitId, startDate.AddDays(3))
        };

        _mockHabitEntryRepository.Setup(x => x.GetEntriesByDateRangeAsync(habitId, startDate, endDate))
            .ReturnsAsync(entries);

        // Act
        var result = await _habitEntryService.GetEntriesByDateRangeAsync(habitId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(entryDto =>
        {
            entryDto.Should().NotBeNull();
            entryDto.HabitId.Should().Be(habitId);
            entryDto.CompletedAt.Should().BeOnOrAfter(startDate);
            entryDto.CompletedAt.Should().BeOnOrBefore(endDate);
        });
    }

    [Fact]
    public async Task GetEntryByIdAsync_WithValidId_ReturnsHabitEntryDto()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var entry = CreateTestHabitEntry(Guid.NewGuid());
        entry.Id = entryId;

        _mockHabitEntryRepository.Setup(x => x.GetByIdAsync(entryId))
            .ReturnsAsync(entry);

        // Act
        var result = await _habitEntryService.GetEntryByIdAsync(entryId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entryId);
        result.HabitId.Should().Be(entry.HabitId);
        result.CompletedAt.Should().Be(entry.CompletedAt);
    }

    [Fact]
    public async Task GetEntryByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var entryId = Guid.NewGuid();

        _mockHabitEntryRepository.Setup(x => x.GetByIdAsync(entryId))
            .ReturnsAsync((HabitEntry?)null);

        // Act
        var result = await _habitEntryService.GetEntryByIdAsync(entryId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateEntryAsync_WithValidData_ReturnsCreatedHabitEntryDto()
    {
        // Arrange
        var createEntryDto = new CreateHabitEntryDto
        {
            HabitId = Guid.NewGuid(),
            CompletedAt = DateTime.UtcNow,
            Notes = "Test entry"
        };

        var createdEntry = CreateTestHabitEntry(createEntryDto.HabitId, createEntryDto.CompletedAt);
        createdEntry.Notes = createEntryDto.Notes;

        _mockHabitEntryRepository.Setup(x => x.AddAsync(It.IsAny<HabitEntry>()))
            .ReturnsAsync(createdEntry);

        // Act
        var result = await _habitEntryService.CreateEntryAsync(createEntryDto);

        // Assert
        result.Should().NotBeNull();
        result.HabitId.Should().Be(createEntryDto.HabitId);
        result.CompletedAt.Should().Be(createEntryDto.CompletedAt);
        result.Notes.Should().Be(createEntryDto.Notes);
        result.Id.Should().NotBeEmpty();

        _mockHabitEntryRepository.Verify(x => x.AddAsync(It.Is<HabitEntry>(e =>
            e.HabitId == createEntryDto.HabitId &&
            e.CompletedAt == createEntryDto.CompletedAt &&
            e.Notes == createEntryDto.Notes)), Times.Once);
    }

    [Fact]
    public async Task DeleteEntryAsync_WithValidId_DeletesEntry()
    {
        // Arrange
        var entryId = Guid.NewGuid();

        _mockHabitEntryRepository.Setup(x => x.ExistsAsync(entryId))
            .ReturnsAsync(true);
        _mockHabitEntryRepository.Setup(x => x.DeleteAsync(entryId))
            .Returns(Task.CompletedTask);

        // Act
        await _habitEntryService.DeleteEntryAsync(entryId);

        // Assert
        _mockHabitEntryRepository.Verify(x => x.DeleteAsync(entryId), Times.Once);
    }

    [Fact]
    public async Task DeleteEntryAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var entryId = Guid.NewGuid();

        _mockHabitEntryRepository.Setup(x => x.ExistsAsync(entryId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _habitEntryService.DeleteEntryAsync(entryId));

        exception.Message.Should().Contain($"Habit entry with ID {entryId} not found");
    }

    [Fact]
    public async Task HasEntryForDateAsync_WithExistingEntry_ReturnsTrue()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        _mockHabitEntryRepository.Setup(x => x.HasEntryForDateAsync(habitId, date))
            .ReturnsAsync(true);

        // Act
        var result = await _habitEntryService.HasEntryForDateAsync(habitId, date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasEntryForDateAsync_WithNoEntry_ReturnsFalse()
    {
        // Arrange
        var habitId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        _mockHabitEntryRepository.Setup(x => x.HasEntryForDateAsync(habitId, date))
            .ReturnsAsync(false);

        // Act
        var result = await _habitEntryService.HasEntryForDateAsync(habitId, date);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Test notes")]
    [InlineData("Very long notes with lots of details about the habit completion")]
    public async Task CreateEntryAsync_WithDifferentNotes_CreatesEntryWithCorrectNotes(string notes)
    {
        // Arrange
        var createEntryDto = new CreateHabitEntryDto
        {
            HabitId = Guid.NewGuid(),
            CompletedAt = DateTime.UtcNow,
            Notes = notes
        };

        var createdEntry = CreateTestHabitEntry(createEntryDto.HabitId, createEntryDto.CompletedAt);
        createdEntry.Notes = notes;

        _mockHabitEntryRepository.Setup(x => x.AddAsync(It.IsAny<HabitEntry>()))
            .ReturnsAsync(createdEntry);

        // Act
        var result = await _habitEntryService.CreateEntryAsync(createEntryDto);

        // Assert
        result.Should().NotBeNull();
        result.Notes.Should().Be(notes);

        _mockHabitEntryRepository.Verify(x => x.AddAsync(It.Is<HabitEntry>(e =>
            e.Notes == notes)), Times.Once);
    }

    private HabitEntry CreateTestHabitEntry(Guid habitId, DateTime? completedAt = null)
    {
        return new HabitEntry
        {
            Id = Guid.NewGuid(),
            HabitId = habitId,
            CompletedAt = completedAt ?? DateTime.UtcNow,
            Notes = "Test notes",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
} 