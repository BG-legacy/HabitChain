using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;

namespace HabitChain.Application.Services;

/// <summary>
/// Service for handling data export operations
/// </summary>
public class ExportService : IExportService
{
    private readonly IHabitService _habitService;
    private readonly ICheckInService _checkInService;
    private readonly IBadgeEarningService _badgeEarningService;
    private readonly IUserBadgeService _userBadgeService;
    private readonly IEncouragementService _encouragementService;
    private readonly IMapper _mapper;

    public ExportService(
        IHabitService habitService,
        ICheckInService checkInService,
        IBadgeEarningService badgeEarningService,
        IUserBadgeService userBadgeService,
        IEncouragementService encouragementService,
        IMapper mapper)
    {
        _habitService = habitService;
        _checkInService = checkInService;
        _badgeEarningService = badgeEarningService;
        _userBadgeService = userBadgeService;
        _encouragementService = encouragementService;
        _mapper = mapper;
    }

    /// <summary>
    /// Generates CSV data for user export
    /// </summary>
    public async Task<string> GenerateCSVAsync(string userId, ExportOptionsDto options)
    {
        // Validate options
        if (!options.IsValid(out string validationError))
        {
            throw new ArgumentException(validationError);
        }

        // Get user data
        var data = await GetUserExportDataAsync(userId, options);

        // Generate CSV content
        var csv = new StringBuilder();
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");

        // Add header
        csv.AppendLine("# HabitChain Export");
        csv.AppendLine($"# Generated: {timestamp}");
        csv.AppendLine($"# Format: {options.Format}");
        csv.AppendLine($"# Date Range: {options.DateRange}");
        csv.AppendLine($"# User ID: {userId}");
        csv.AppendLine();

        // Export habits
        if (options.IncludeHabits && data.Habits.Any())
        {
            csv.AppendLine("HABITS");
            csv.AppendLine("ID,Name,Description,Frequency,Created Date,Current Streak,Longest Streak,Color,Is Active");
            
            foreach (var habit in data.Habits)
            {
                var row = new[]
                {
                    EscapeCsvField(habit.Id.ToString()),
                    EscapeCsvField(habit.Name ?? ""),
                    EscapeCsvField(habit.Description ?? ""),
                    EscapeCsvField(habit.Frequency.ToString()),
                    EscapeCsvField(habit.CreatedAt.ToString("yyyy-MM-dd")),
                    EscapeCsvField(habit.CurrentStreak.ToString()),
                    EscapeCsvField(habit.LongestStreak.ToString()),
                    EscapeCsvField(habit.Color ?? ""),
                    EscapeCsvField(habit.IsActive ? "Yes" : "No")
                };
                csv.AppendLine(string.Join(",", row));
            }
            csv.AppendLine();
        }

        // Export check-ins
        if (options.IncludeCheckIns && data.CheckIns.Any())
        {
            csv.AppendLine("CHECK-INS");
            csv.AppendLine("ID,Date,Habit ID,Habit Name,Completed At,Notes,Streak Day,Is Manual Entry");
            
            foreach (var checkIn in data.CheckIns)
            {
                var habit = data.Habits.FirstOrDefault(h => h.Id == checkIn.HabitId);
                var row = new[]
                {
                    EscapeCsvField(checkIn.Id.ToString()),
                    EscapeCsvField(checkIn.CompletedAt.ToString("yyyy-MM-dd")),
                    EscapeCsvField(checkIn.HabitId.ToString()),
                    EscapeCsvField(habit?.Name ?? "Unknown Habit"),
                    EscapeCsvField(checkIn.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    EscapeCsvField(checkIn.Notes ?? ""),
                    EscapeCsvField(checkIn.StreakDay.ToString()),
                    EscapeCsvField(checkIn.IsManualEntry ? "Yes" : "No")
                };
                csv.AppendLine(string.Join(",", row));
            }
            csv.AppendLine();
        }

        // Export badges
        if (options.IncludeBadges && data.Badges.Any())
        {
            csv.AppendLine("BADGES");
            csv.AppendLine("ID,Badge Name,Description,Earned At,Habit ID,Badge Type");
            
            foreach (var badge in data.Badges)
            {
                var row = new[]
                {
                    EscapeCsvField(badge.Id.ToString()),
                    EscapeCsvField(badge.Badge?.Name ?? ""),
                    EscapeCsvField(badge.Badge?.Description ?? ""),
                    EscapeCsvField(badge.EarnedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    EscapeCsvField(badge.HabitId?.ToString() ?? ""),
                    EscapeCsvField(badge.Badge?.Type.ToString() ?? "")
                };
                csv.AppendLine(string.Join(",", row));
            }
            csv.AppendLine();
        }

        // Export encouragements
        if (options.IncludeEncouragements && data.Encouragements.Any())
        {
            csv.AppendLine("ENCOURAGEMENTS");
            csv.AppendLine("ID,Date,From User,To User,Message,Type,Habit ID,Is Read");
            
            foreach (var encouragement in data.Encouragements)
            {
                var row = new[]
                {
                    EscapeCsvField(encouragement.Id.ToString()),
                    EscapeCsvField(encouragement.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    EscapeCsvField(encouragement.FromUser?.FirstName ?? encouragement.FromUserId ?? ""),
                    EscapeCsvField(encouragement.ToUser?.FirstName ?? encouragement.ToUserId ?? ""),
                    EscapeCsvField(encouragement.Message ?? ""),
                    EscapeCsvField(encouragement.Type.ToString()),
                    EscapeCsvField(encouragement.HabitId?.ToString() ?? ""),
                    EscapeCsvField(encouragement.IsRead ? "Yes" : "No")
                };
                csv.AppendLine(string.Join(",", row));
            }
            csv.AppendLine();
        }

        // Export statistics
        if (options.IncludeStats)
        {
            csv.AppendLine("SUMMARY STATISTICS");
            csv.AppendLine("Metric,Value");
            
            var stats = data.Statistics;
            var statRows = new[]
            {
                ("Total Habits", stats.TotalHabits.ToString()),
                ("Active Habits", stats.ActiveHabits.ToString()),
                ("Total Check-ins", stats.TotalCheckIns.ToString()),
                ("Total Badges Earned", stats.TotalBadges.ToString()),
                ("Total Encouragements", stats.TotalEncouragements.ToString()),
                ("Longest Streak", stats.LongestStreak.ToString()),
                ("Total Current Streaks", stats.TotalCurrentStreaks.ToString()),
                ("30-Day Completion Rate", $"{stats.CompletionRate30Days:F2}%"),
                ("First Habit Created", stats.FirstHabitCreated?.ToString("yyyy-MM-dd") ?? "N/A"),
                ("Last Check-in", stats.LastCheckIn?.ToString("yyyy-MM-dd") ?? "N/A"),
                ("Days Tracking", stats.DaysTracking.ToString()),
                ("Export Date", DateTime.UtcNow.ToString("yyyy-MM-dd"))
            };

            foreach (var (metric, value) in statRows)
            {
                csv.AppendLine($"{EscapeCsvField(metric)},{EscapeCsvField(value)}");
            }
        }

        return csv.ToString();
    }

    /// <summary>
    /// Generates PDF data for user export using QuestPDF
    /// </summary>
    public async Task<byte[]> GeneratePDFAsync(string userId, ExportOptionsDto options)
    {
        // Validate options
        if (!options.IsValid(out string validationError))
        {
            throw new ArgumentException(validationError);
        }

        // Get user data
        var data = await GetUserExportDataAsync(userId, options);

        // Generate PDF using QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(container => ComposeContent(container, data, options));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("HABITCHAIN EXPORT REPORT").FontSize(20).Bold();
                column.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}").FontSize(10).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(50).Height(50).Background(Colors.Blue.Lighten1).AlignCenter().AlignMiddle().Text("📊").FontSize(24);
        });
    }

    private void ComposeContent(IContainer container, ExportDataDto data, ExportOptionsDto options)
    {
        container.Column(column =>
        {
            // Summary Section
            column.Item().Element(container => ComposeSummarySection(container, data));

            // Habits Section
            if (options.IncludeHabits && data.Habits.Any())
            {
                column.Item().Element(container => ComposeHabitsSection(container, data.Habits));
            }

            // Check-ins Section
            if (options.IncludeCheckIns && data.CheckIns.Any())
            {
                column.Item().Element(container => ComposeCheckInsSection(container, data.CheckIns));
            }

            // Badges Section
            if (options.IncludeBadges && data.Badges.Any())
            {
                column.Item().Element(container => ComposeBadgesSection(container, data.Badges));
            }

            // Encouragements Section
            if (options.IncludeEncouragements && data.Encouragements.Any())
            {
                column.Item().Element(container => ComposeEncouragementsSection(container, data.Encouragements));
            }
        });
    }

    private void ComposeSummarySection(IContainer container, ExportDataDto data)
    {
        var stats = data.Statistics;
        
        container.Column(column =>
        {
            column.Item().Text("SUMMARY STATISTICS").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
            column.Item().Height(10);
            
            column.Item().Grid(grid =>
            {
                grid.Columns(2);
                grid.Item().Text("Total Habits:").Bold();
                grid.Item().Text(stats.TotalHabits.ToString());
                
                grid.Item().Text("Active Habits:").Bold();
                grid.Item().Text(stats.ActiveHabits.ToString());
                
                grid.Item().Text("Total Check-ins:").Bold();
                grid.Item().Text(stats.TotalCheckIns.ToString());
                
                grid.Item().Text("Total Badges:").Bold();
                grid.Item().Text(stats.TotalBadges.ToString());
                
                grid.Item().Text("Longest Streak:").Bold();
                grid.Item().Text($"{stats.LongestStreak} days");
                
                grid.Item().Text("30-Day Completion Rate:").Bold();
                grid.Item().Text($"{stats.CompletionRate30Days:F1}%");
            });
            
            column.Item().Height(20);
        });
    }

    private void ComposeHabitsSection(IContainer container, IEnumerable<HabitDto> habits)
    {
        container.Column(column =>
        {
            column.Item().Text("HABITS").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
            column.Item().Height(10);
            
            foreach (var habit in habits)
            {
                column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(habitColumn =>
                {
                    habitColumn.Item().Text(habit.Name).FontSize(12).Bold();
                    habitColumn.Item().Text($"Description: {habit.Description ?? "N/A"}").FontSize(9);
                    habitColumn.Item().Text($"Frequency: {habit.Frequency}").FontSize(9);
                    habitColumn.Item().Text($"Current Streak: {habit.CurrentStreak} days").FontSize(9);
                    habitColumn.Item().Text($"Longest Streak: {habit.LongestStreak} days").FontSize(9);
                    habitColumn.Item().Text($"Status: {(habit.IsActive ? "Active" : "Inactive")}").FontSize(9);
                });
                column.Item().Height(5);
            }
        });
    }

    private void ComposeCheckInsSection(IContainer container, IEnumerable<CheckInDto> checkIns)
    {
        container.Column(column =>
        {
            column.Item().Text("RECENT CHECK-INS").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
            column.Item().Height(10);
            
            column.Item().Table(table =>
            {
                table.Header(header =>
                {
                    header.Cell().Text("Date").Bold();
                    header.Cell().Text("Habit").Bold();
                    header.Cell().Text("Notes").Bold();
                });

                foreach (var checkIn in checkIns.Take(50)) // Limit to 50 most recent
                {
                    table.Cell().Text(checkIn.CompletedAt.ToString("yyyy-MM-dd"));
                    table.Cell().Text(checkIn.Habit.Name);
                    table.Cell().Text(checkIn.Notes ?? "");
                }
            });
        });
    }

    private void ComposeBadgesSection(IContainer container, IEnumerable<UserBadgeDto> userBadges)
    {
        container.Column(column =>
        {
            column.Item().Text("EARNED BADGES").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
            column.Item().Height(10);
            
            column.Item().Grid(grid =>
            {
                grid.Columns(3);
                
                foreach (var userBadge in userBadges)
                {
                    var badge = userBadge.Badge;
                    grid.Item().Background(Colors.Grey.Lighten3).Padding(8).Column(badgeColumn =>
                    {
                        badgeColumn.Item().Text(badge.Emoji).FontSize(16);
                        badgeColumn.Item().Text(badge.Name).FontSize(10).Bold();
                        badgeColumn.Item().Text(badge.Description).FontSize(8);
                        badgeColumn.Item().Text($"Earned: {userBadge.EarnedAt.ToString("yyyy-MM-dd")}").FontSize(8);
                    });
                }
            });
        });
    }

    private void ComposeEncouragementsSection(IContainer container, IEnumerable<EncouragementDto> encouragements)
    {
        container.Column(column =>
        {
            column.Item().Text("ENCOURAGEMENTS").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
            column.Item().Height(10);
            
            foreach (var encouragement in encouragements)
            {
                column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(encouragementColumn =>
                {
                    encouragementColumn.Item().Text(encouragement.Message).FontSize(10);
                    encouragementColumn.Item().Text($"Type: {encouragement.Type}").FontSize(8);
                    encouragementColumn.Item().Text($"Created: {encouragement.CreatedAt:yyyy-MM-dd}").FontSize(8);
                });
                column.Item().Height(5);
            }
        });
    }

    /// <summary>
    /// Gets a preview of what would be included in an export
    /// </summary>
    public async Task<ExportPreviewDto> GetExportPreviewAsync(string userId, ExportOptionsDto options)
    {
        // Validate options
        if (!options.IsValid(out string validationError))
        {
            throw new ArgumentException(validationError);
        }

        // Get basic counts without full data (for performance)
        var (startDate, endDate) = options.GetDateRange();
        
        var preview = new ExportPreviewDto
        {
            DateRangeStart = startDate,
            DateRangeEnd = endDate
        };

        // Get counts
        if (options.IncludeHabits)
        {
            var habits = await _habitService.GetHabitsByUserIdAsync(userId);
            preview.HabitsCount = habits.Count();
        }

        if (options.IncludeCheckIns)
        {
            IEnumerable<CheckInDto> checkIns;
            if (startDate.HasValue || endDate.HasValue)
            {
                checkIns = await _checkInService.GetCheckInsByDateRangeAsync(
                    userId, 
                    startDate ?? DateTime.MinValue, 
                    endDate ?? DateTime.MaxValue);
            }
            else
            {
                checkIns = await _checkInService.GetCheckInsByUserIdAsync(userId);
            }
            preview.CheckInsCount = checkIns.Count();
        }

        if (options.IncludeBadges)
        {
            var badges = await _userBadgeService.GetUserBadgesByUserIdAsync(userId);
            
            // Filter by date range if specified
            if (startDate.HasValue || endDate.HasValue)
            {
                badges = badges.Where(b =>
                {
                    if (startDate.HasValue && b.EarnedAt < startDate) return false;
                    if (endDate.HasValue && b.EarnedAt > endDate) return false;
                    return true;
                });
            }
            
            preview.BadgesCount = badges.Count();
        }

        if (options.IncludeEncouragements)
        {
            var received = await _encouragementService.GetEncouragementsByUserIdAsync(userId);
            var sent = await _encouragementService.GetEncouragementsByFromUserIdAsync(userId);
            
            // Combine and deduplicate
            var combined = received.Concat(sent).GroupBy(e => e.Id).Select(g => g.First());
            
            // Filter by date range if specified
            if (startDate.HasValue || endDate.HasValue)
            {
                combined = combined.Where(e =>
                {
                    if (startDate.HasValue && e.CreatedAt < startDate) return false;
                    if (endDate.HasValue && e.CreatedAt > endDate) return false;
                    return true;
                });
            }
            
            preview.EncouragementsCount = combined.Count();
        }

        // Get quick data for file size estimation
        var quickData = new ExportDataDto
        {
            Habits = options.IncludeHabits ? new List<HabitDto>(new HabitDto[preview.HabitsCount]) : new(),
            CheckIns = options.IncludeCheckIns ? new List<CheckInDto>(new CheckInDto[preview.CheckInsCount]) : new(),
            Badges = options.IncludeBadges ? new List<UserBadgeDto>(new UserBadgeDto[preview.BadgesCount]) : new(),
            Encouragements = options.IncludeEncouragements ? new List<EncouragementDto>(new EncouragementDto[preview.EncouragementsCount]) : new()
        };

        preview.EstimatedFileSize = EstimateFileSize(quickData, options);

        return preview;
    }

    /// <summary>
    /// Gets user data for export based on options
    /// </summary>
    public async Task<ExportDataDto> GetUserExportDataAsync(string userId, ExportOptionsDto options)
    {
        var data = new ExportDataDto();
        var (startDate, endDate) = options.GetDateRange();

        // Fetch habits
        if (options.IncludeHabits)
        {
            var habits = await _habitService.GetHabitsByUserIdAsync(userId);
            data.Habits = habits.ToList();
        }

        // Fetch check-ins
        if (options.IncludeCheckIns)
        {
            IEnumerable<CheckInDto> checkIns;
            if (startDate.HasValue || endDate.HasValue)
            {
                checkIns = await _checkInService.GetCheckInsByDateRangeAsync(
                    userId, 
                    startDate ?? DateTime.MinValue, 
                    endDate ?? DateTime.MaxValue);
            }
            else
            {
                checkIns = await _checkInService.GetCheckInsByUserIdAsync(userId);
            }
            data.CheckIns = checkIns.ToList();
        }

        // Fetch badges
        if (options.IncludeBadges)
        {
            var badges = await _userBadgeService.GetUserBadgesByUserIdAsync(userId);
            
            // Filter by date range if specified
            if (startDate.HasValue || endDate.HasValue)
            {
                badges = badges.Where(b =>
                {
                    if (startDate.HasValue && b.EarnedAt < startDate) return false;
                    if (endDate.HasValue && b.EarnedAt > endDate) return false;
                    return true;
                });
            }
            
            data.Badges = badges.ToList();
        }

        // Fetch encouragements
        if (options.IncludeEncouragements)
        {
            var received = await _encouragementService.GetEncouragementsByUserIdAsync(userId);
            var sent = await _encouragementService.GetEncouragementsByFromUserIdAsync(userId);
            
            // Combine and deduplicate
            var combined = received.Concat(sent).GroupBy(e => e.Id).Select(g => g.First());
            
            // Filter by date range if specified
            if (startDate.HasValue || endDate.HasValue)
            {
                combined = combined.Where(e =>
                {
                    if (startDate.HasValue && e.CreatedAt < startDate) return false;
                    if (endDate.HasValue && e.CreatedAt > endDate) return false;
                    return true;
                });
            }
            
            data.Encouragements = combined.ToList();
        }

        // Calculate statistics
        data.Statistics = CalculateStatistics(data, userId);

        return data;
    }

    /// <summary>
    /// Escapes CSV field content
    /// </summary>
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "\"\"";

        // Escape quotes and wrap in quotes if necessary
        if (field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r'))
        {
            field = field.Replace("\"", "\"\"");
            return $"\"{field}\"";
        }

        return field;
    }

    /// <summary>
    /// Estimates the file size for export
    /// </summary>
    private string EstimateFileSize(ExportDataDto data, ExportOptionsDto options)
    {
        // Rough estimation based on data counts
        long estimatedBytes = 0;

        if (options.IncludeHabits)
            estimatedBytes += data.Habits.Count * 150; // ~150 bytes per habit

        if (options.IncludeCheckIns)
            estimatedBytes += data.CheckIns.Count * 100; // ~100 bytes per check-in

        if (options.IncludeBadges)
            estimatedBytes += data.Badges.Count * 80; // ~80 bytes per badge

        if (options.IncludeEncouragements)
            estimatedBytes += data.Encouragements.Count * 120; // ~120 bytes per encouragement

        if (options.IncludeStats)
            estimatedBytes += 500; // ~500 bytes for statistics

        // Add overhead
        estimatedBytes += 1000; // Header and formatting overhead

        return FormatFileSize(estimatedBytes);
    }

    /// <summary>
    /// Formats file size in human-readable format
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Calculates statistics for the export data
    /// </summary>
    private ExportStatsDto CalculateStatistics(ExportDataDto data, string userId)
    {
        var stats = new ExportStatsDto
        {
            TotalHabits = data.Habits.Count,
            ActiveHabits = data.Habits.Count(h => h.IsActive),
            TotalCheckIns = data.CheckIns.Count,
            TotalBadges = data.Badges.Count,
            TotalEncouragements = data.Encouragements.Count
        };

        // Calculate streaks
        if (data.Habits.Any())
        {
            stats.LongestStreak = data.Habits.Max(h => h.LongestStreak);
            stats.TotalCurrentStreaks = data.Habits.Sum(h => h.CurrentStreak);
            stats.FirstHabitCreated = data.Habits.Min(h => h.CreatedAt);
        }
        else
        {
            stats.LongestStreak = 0;
            stats.TotalCurrentStreaks = 0;
            stats.FirstHabitCreated = null;
        }

        // Calculate completion rate for last 30 days
        if (data.CheckIns.Any())
        {
            stats.LastCheckIn = data.CheckIns.Max(c => c.CompletedAt);
            
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentCheckIns = data.CheckIns.Where(c => c.CompletedAt >= thirtyDaysAgo).Count();
            var activeHabits = stats.ActiveHabits;
            
            if (activeHabits > 0)
            {
                var expectedCheckIns = activeHabits * 30; // Assuming daily frequency for simplicity
                stats.CompletionRate30Days = Math.Round((decimal)recentCheckIns / expectedCheckIns * 100, 2);
            }
            else
            {
                stats.CompletionRate30Days = 0;
            }
        }
        else
        {
            stats.LastCheckIn = null;
            stats.CompletionRate30Days = 0;
        }

        // Calculate days tracking
        if (stats.FirstHabitCreated.HasValue)
        {
            stats.DaysTracking = (int)(DateTime.UtcNow - stats.FirstHabitCreated.Value).TotalDays;
        }
        else
        {
            stats.DaysTracking = 0;
        }

        return stats;
    }
} 