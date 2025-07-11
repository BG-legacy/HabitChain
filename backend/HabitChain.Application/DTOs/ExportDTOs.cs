using System.ComponentModel.DataAnnotations;

namespace HabitChain.Application.DTOs;

/// <summary>
/// Export options for configuring what data to export and how
/// </summary>
public class ExportOptionsDto
{
    [Required(ErrorMessage = "Export format is required.")]
    [RegularExpression("^(CSV|PDF)$", ErrorMessage = "Export format must be either 'CSV' or 'PDF'.")]
    public string Format { get; set; } = "CSV";

    [Required(ErrorMessage = "Date range is required.")]
    [RegularExpression("^(all|month|week|custom)$", ErrorMessage = "Date range must be 'all', 'month', 'week', or 'custom'.")]
    public string DateRange { get; set; } = "all";

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IncludeHabits { get; set; } = true;
    public bool IncludeCheckIns { get; set; } = true;
    public bool IncludeBadges { get; set; } = true;
    public bool IncludeEncouragements { get; set; } = false;
    public bool IncludeStats { get; set; } = true;

    /// <summary>
    /// Validates the export options
    /// </summary>
    public bool IsValid(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Validate custom date range
        if (DateRange == "custom")
        {
            if (!StartDate.HasValue && !EndDate.HasValue)
            {
                errorMessage = "Start date or end date is required for custom date range.";
                return false;
            }

            if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            {
                errorMessage = "Start date cannot be after end date.";
                return false;
            }

            // Limit date range to prevent abuse (max 2 years)
            if (StartDate.HasValue && EndDate.HasValue)
            {
                var maxRange = TimeSpan.FromDays(730); // 2 years
                if (EndDate.Value - StartDate.Value > maxRange)
                {
                    errorMessage = "Date range cannot exceed 2 years.";
                    return false;
                }
            }
        }

        // At least one data type must be selected
        if (!IncludeHabits && !IncludeCheckIns && !IncludeBadges && !IncludeEncouragements)
        {
            errorMessage = "At least one data type must be selected for export.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the computed date range based on the DateRange setting
    /// </summary>
    public (DateTime? start, DateTime? end) GetDateRange()
    {
        var now = DateTime.UtcNow;

        return DateRange switch
        {
            "week" => (now.AddDays(-7), now),
            "month" => (now.AddDays(-30), now),
            "custom" => (StartDate, EndDate),
            _ => (null, null) // "all"
        };
    }
}

/// <summary>
/// Preview data for an export operation
/// </summary>
public class ExportPreviewDto
{
    public int HabitsCount { get; set; }
    public int CheckInsCount { get; set; }
    public int BadgesCount { get; set; }
    public int EncouragementsCount { get; set; }
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
    public string EstimatedFileSize { get; set; } = "Unknown";
    public ExportStatsDto? Statistics { get; set; }
}

/// <summary>
/// Statistical summary for export data
/// </summary>
public class ExportStatsDto
{
    public int TotalHabits { get; set; }
    public int ActiveHabits { get; set; }
    public int TotalCheckIns { get; set; }
    public int TotalBadges { get; set; }
    public int TotalEncouragements { get; set; }
    public int LongestStreak { get; set; }
    public int TotalCurrentStreaks { get; set; }
    public decimal CompletionRate30Days { get; set; }
    public DateTime? FirstHabitCreated { get; set; }
    public DateTime? LastCheckIn { get; set; }
    public int DaysTracking { get; set; }
}

/// <summary>
/// Available export formats information
/// </summary>
public class ExportFormatsDto
{
    public List<ExportFormatInfo> Formats { get; set; } = new();
}

/// <summary>
/// Information about a specific export format
/// </summary>
public class ExportFormatInfo
{
    public string Format { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
}

/// <summary>
/// Export data container for internal use
/// </summary>
public class ExportDataDto
{
    public List<HabitDto> Habits { get; set; } = new();
    public List<CheckInDto> CheckIns { get; set; } = new();
    public List<UserBadgeDto> Badges { get; set; } = new();
    public List<EncouragementDto> Encouragements { get; set; } = new();
    public ExportStatsDto Statistics { get; set; } = new();
}

/// <summary>
/// Request model for export operations
/// </summary>
public class ExportRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public ExportOptionsDto Options { get; set; } = new();
    public ExportDataDto? Data { get; set; }
} 