using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using System.Text;

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
    /// Generates PDF data for user export (basic implementation)
    /// </summary>
    public async Task<byte[]> GeneratePDFAsync(string userId, ExportOptionsDto options)
    {
        // For now, we'll generate a simple text-based PDF
        // In a production environment, you would use a library like iTextSharp or QuestPDF
        
        // Validate options
        if (!options.IsValid(out string validationError))
        {
            throw new ArgumentException(validationError);
        }

        // Get user data
        var data = await GetUserExportDataAsync(userId, options);

        // Generate a simple text report
        var report = new StringBuilder();
        report.AppendLine("HABITCHAIN EXPORT REPORT");
        report.AppendLine("========================");
        report.AppendLine();
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
        report.AppendLine($"User ID: {userId}");
        report.AppendLine($"Date Range: {options.DateRange}");
        report.AppendLine();

        // Summary
        report.AppendLine("SUMMARY");
        report.AppendLine("-------");
        report.AppendLine($"Total Habits: {data.Statistics.TotalHabits}");
        report.AppendLine($"Active Habits: {data.Statistics.ActiveHabits}");
        report.AppendLine($"Total Check-ins: {data.Statistics.TotalCheckIns}");
        report.AppendLine($"Total Badges: {data.Statistics.TotalBadges}");
        report.AppendLine($"Longest Streak: {data.Statistics.LongestStreak} days");
        report.AppendLine($"30-Day Completion Rate: {data.Statistics.CompletionRate30Days:F2}%");
        report.AppendLine();

        // Habits
        if (options.IncludeHabits && data.Habits.Any())
        {
            report.AppendLine("HABITS");
            report.AppendLine("------");
            foreach (var habit in data.Habits)
            {
                report.AppendLine($"• {habit.Name}");
                report.AppendLine($"  Description: {habit.Description ?? "N/A"}");
                report.AppendLine($"  Frequency: {habit.Frequency}");
                report.AppendLine($"  Current Streak: {habit.CurrentStreak} days");
                report.AppendLine($"  Longest Streak: {habit.LongestStreak} days");
                report.AppendLine($"  Status: {(habit.IsActive ? "Active" : "Inactive")}");
                report.AppendLine();
            }
        }

        // For now, return the text as bytes
        // In production, you would generate an actual PDF
        var textBytes = Encoding.UTF8.GetBytes(report.ToString());
        
        // Placeholder for actual PDF generation
        // This would typically involve using a PDF library like:
        // - iTextSharp/iText 7
        // - QuestPDF
        // - PdfSharpCore
        // - etc.
        
        return textBytes;
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