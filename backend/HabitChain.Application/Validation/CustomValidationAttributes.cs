using System.ComponentModel.DataAnnotations;

namespace HabitChain.Application.Validation;

/// <summary>
/// Custom validation attribute to ensure a date is not in the future
/// </summary>
public class NotInFutureAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is DateTime date)
        {
            if (date > DateTime.UtcNow)
            {
                return new ValidationResult(ErrorMessage ?? "Date cannot be in the future.");
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Custom validation attribute to ensure a date is within a reasonable range
/// </summary>
public class DateRangeAttribute : ValidationAttribute
{
    private readonly int _maxDaysInPast;
    private readonly int _maxDaysInFuture;

    public DateRangeAttribute(int maxDaysInPast = 365, int maxDaysInFuture = 1)
    {
        _maxDaysInPast = maxDaysInPast;
        _maxDaysInFuture = maxDaysInFuture;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is DateTime date)
        {
            var now = DateTime.UtcNow;
            var minDate = now.AddDays(-_maxDaysInPast);
            var maxDate = now.AddDays(_maxDaysInFuture);

            if (date < minDate || date > maxDate)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"Date must be within {_maxDaysInPast} days in the past and {_maxDaysInFuture} days in the future.");
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Custom validation attribute to ensure a string contains no HTML or script tags
/// </summary>
public class NoHtmlAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is string str)
        {
            // Check for common HTML/script patterns
            var htmlPatterns = new[] { "<script", "<iframe", "<object", "<embed", "javascript:", "onclick", "onload", "onerror" };
            
            foreach (var pattern in htmlPatterns)
            {
                if (str.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResult(ErrorMessage ?? "Content cannot contain HTML or script tags.");
                }
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Custom validation attribute to ensure a string is a valid hex color code
/// </summary>
public class HexColorAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                // Check if it's a valid hex color (#RRGGBB)
                if (!System.Text.RegularExpressions.Regex.IsMatch(str, @"^#[0-9A-Fa-f]{6}$"))
                {
                    return new ValidationResult(ErrorMessage ?? "Color must be a valid hex color code (e.g., #FF0000).");
                }
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Custom validation attribute to ensure a string contains only safe characters
/// </summary>
public class SafeStringAttribute : ValidationAttribute
{
    private readonly string _pattern;

    public SafeStringAttribute(string pattern = @"^[a-zA-Z0-9\s\-_\.]+$")
    {
        _pattern = pattern;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(str, _pattern))
                {
                    return new ValidationResult(ErrorMessage ?? "String contains invalid characters.");
                }
            }
        }

        return ValidationResult.Success;
    }
} 