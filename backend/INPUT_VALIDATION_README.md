# Input Validation Strategy

## Overview

This document explains the comprehensive input validation strategy implemented across the HabitChain backend using DataAnnotations and custom validation attributes. The validation ensures data integrity, security, and user experience by preventing invalid data from entering the system.

## Validation Strategy Summary

### 1. DataAnnotations vs FluentValidation

- **DataAnnotations**: Built-in .NET validation attributes, declarative and easy to use
- **Custom Attributes**: Domain-specific validation rules for HabitChain requirements
- **FluentValidation**: Considered for complex business rules (future enhancement)

### 2. Validation Layers

1. **Client-side validation**: Frontend validation for immediate feedback
2. **Model validation**: DataAnnotations in DTOs
3. **Service layer validation**: Business logic validation
4. **Database constraints**: Final data integrity layer

## Custom Validation Attributes

### 1. NotInFutureAttribute

**Purpose**: Ensures dates are not in the future
**Usage**: Applied to completion dates, check-in dates, etc.

```csharp
[NotInFuture(ErrorMessage = "Date cannot be in the future.")]
public DateTime CompletedAt { get; set; }
```

**Validation Logic**:
- Compares date with `DateTime.UtcNow`
- Returns error if date is in the future
- Allows null values (optional fields)

### 2. DateRangeAttribute

**Purpose**: Ensures dates are within reasonable ranges
**Usage**: Applied to dates that should be within specific time windows

```csharp
[DateRange(30, 1, ErrorMessage = "Date must be within 30 days in the past and 1 day in the future.")]
public DateTime CompletedAt { get; set; }
```

**Parameters**:
- `maxDaysInPast`: Maximum days in the past (default: 365)
- `maxDaysInFuture`: Maximum days in the future (default: 1)

### 3. NoHtmlAttribute

**Purpose**: Prevents HTML and script injection
**Usage**: Applied to user-generated content fields

```csharp
[NoHtml(ErrorMessage = "Content cannot contain HTML or script tags.")]
public string? Notes { get; set; }
```

**Validation Logic**:
- Checks for common HTML/script patterns
- Prevents XSS attacks
- Blocks: `<script>`, `<iframe>`, `javascript:`, etc.

### 4. HexColorAttribute

**Purpose**: Validates hex color codes
**Usage**: Applied to color fields

```csharp
[HexColor(ErrorMessage = "Color must be a valid hex color code (e.g., #FF0000).")]
public string? Color { get; set; }
```

**Validation Logic**:
- Ensures format: `#RRGGBB`
- Case-insensitive hex validation
- Allows null/empty values

### 5. SafeStringAttribute

**Purpose**: Ensures strings contain only safe characters
**Usage**: Applied to names, identifiers, etc.

```csharp
[SafeString(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Name can only contain letters, numbers, spaces, hyphens, and underscores.")]
public string Name { get; set; }
```

**Parameters**:
- `pattern`: Regex pattern for allowed characters
- Default pattern: `^[a-zA-Z0-9\s\-_\.]+$`

## DTO Validation Implementation

### 1. Authentication DTOs

#### LoginDto
```csharp
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
```

#### RegisterDto
```csharp
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

### 2. Habit DTOs

#### CreateHabitDto
```csharp
public class CreateHabitDto
{
    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Habit name must be between 3 and 100 characters.")]
    [SafeString(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Habit name can only contain letters, numbers, spaces, hyphens, and underscores.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [NoHtml(ErrorMessage = "Description cannot contain HTML or script tags.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "User ID is required.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Habit frequency is required.")]
    [EnumDataType(typeof(HabitFrequency), ErrorMessage = "Invalid habit frequency.")]
    public HabitFrequency Frequency { get; set; }

    [HexColor(ErrorMessage = "Color must be a valid hex color code (e.g., #FF0000).")]
    public string? Color { get; set; }

    [StringLength(50, ErrorMessage = "Icon name cannot exceed 50 characters.")]
    [SafeString(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "Icon name can only contain letters, numbers, hyphens, and underscores.")]
    public string? IconName { get; set; }
}
```

### 3. Check-In DTOs

#### CreateCheckInDto
```csharp
public class CreateCheckInDto
{
    [Required(ErrorMessage = "User ID is required.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Habit ID is required.")]
    public Guid HabitId { get; set; }

    [Required(ErrorMessage = "Completion date is required.")]
    [DataType(DataType.DateTime)]
    [NotInFuture(ErrorMessage = "Check-in date cannot be in the future.")]
    [DateRange(30, 1, ErrorMessage = "Check-in date must be within 30 days in the past and 1 day in the future.")]
    public DateTime CompletedAt { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
    [NoHtml(ErrorMessage = "Notes cannot contain HTML or script tags.")]
    public string? Notes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Streak day must be a positive number.")]
    public int StreakDay { get; set; }

    public bool IsManualEntry { get; set; } = false;
}
```

### 4. Encouragement DTOs

#### CreateEncouragementDto
```csharp
public class CreateEncouragementDto
{
    [Required(ErrorMessage = "Sender user ID is required.")]
    public string FromUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient user ID is required.")]
    public string ToUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Encouragement message is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 500 characters.")]
    [NoHtml(ErrorMessage = "Message cannot contain HTML or script tags.")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Encouragement type is required.")]
    [EnumDataType(typeof(EncouragementType), ErrorMessage = "Invalid encouragement type.")]
    public EncouragementType Type { get; set; }

    public Guid? HabitId { get; set; }
}
```

### 5. Badge DTOs

#### CreateBadgeDto
```csharp
public class CreateBadgeDto
{
    [Required(ErrorMessage = "Badge name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Badge name must be between 3 and 100 characters.")]
    [SafeString(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Badge name can only contain letters, numbers, spaces, hyphens, and underscores.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Badge description is required.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters.")]
    [NoHtml(ErrorMessage = "Description cannot contain HTML or script tags.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Badge type is required.")]
    [EnumDataType(typeof(BadgeType), ErrorMessage = "Invalid badge type.")]
    public BadgeType Type { get; set; }

    [StringLength(50, ErrorMessage = "Icon name cannot exceed 50 characters.")]
    [SafeString(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "Icon name can only contain letters, numbers, hyphens, and underscores.")]
    public string? IconName { get; set; }

    public bool IsActive { get; set; } = true;
}
```

## Validation Best Practices

### 1. Security First
- **XSS Prevention**: Use `NoHtmlAttribute` for user-generated content
- **SQL Injection Prevention**: Use parameterized queries (handled by EF Core)
- **Input Sanitization**: Validate and sanitize all inputs

### 2. User Experience
- **Clear Error Messages**: Provide specific, actionable error messages
- **Consistent Validation**: Apply similar rules across similar fields
- **Reasonable Limits**: Set appropriate min/max lengths and ranges

### 3. Performance
- **Efficient Validation**: Use built-in attributes when possible
- **Early Validation**: Validate at the model level before service layer
- **Caching**: Consider caching validation results for complex rules

### 4. Maintainability
- **Custom Attributes**: Create reusable validation attributes
- **Documentation**: Document validation rules and their purposes
- **Testing**: Unit test validation logic

## Validation Error Handling

### 1. Controller Level
```csharp
[HttpPost]
public async Task<ActionResult<HabitDto>> CreateHabit([FromBody] CreateHabitDto createHabitDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    // Process valid data
}
```

### 2. Custom Error Responses
```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState
        .Where(x => x.Value?.Errors.Count > 0)
        .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
        .ToList();
    
    return BadRequest(new { Message = "Validation failed", Errors = errors });
}
```

## Testing Validation

### 1. Unit Tests
```csharp
[Test]
public void CreateHabitDto_WithInvalidName_ShouldFailValidation()
{
    // Arrange
    var dto = new CreateHabitDto { Name = "ab" }; // Too short
    
    // Act
    var results = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), results, true);
    
    // Assert
    Assert.IsFalse(isValid);
    Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Name")));
}
```

### 2. Integration Tests
```csharp
[Test]
public async Task CreateHabit_WithInvalidData_ShouldReturnBadRequest()
{
    // Arrange
    var invalidDto = new CreateHabitDto { Name = "" };
    var client = CreateAuthenticatedClient();
    
    // Act
    var response = await client.PostAsJsonAsync("/api/habits", invalidDto);
    
    // Assert
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
}
```

## Future Enhancements

### 1. FluentValidation Integration
Consider adding FluentValidation for complex business rules:

```csharp
public class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 100)
            .Matches(@"^[a-zA-Z0-9\s\-_]+$");
        
        RuleFor(x => x.Frequency)
            .IsInEnum();
        
        RuleFor(x => x.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}
```

### 2. Async Validation
Implement async validation for database checks:

```csharp
public class UniqueHabitNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Async validation logic
        return ValidationResult.Success;
    }
}
```

### 3. Conditional Validation
Add conditional validation based on other properties:

```csharp
public class ConditionalRequiredAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;
    
    public ConditionalRequiredAttribute(string dependentProperty)
    {
        _dependentProperty = dependentProperty;
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Conditional validation logic
        return ValidationResult.Success;
    }
}
```

## Validation Checklist

- [x] All DTOs have appropriate validation attributes
- [x] Custom validation attributes for domain-specific rules
- [x] Security validation (XSS prevention, SQL injection)
- [x] User experience validation (clear error messages)
- [x] Performance considerations
- [x] Comprehensive documentation
- [x] Unit tests for validation logic
- [ ] Integration tests for validation flow
- [ ] FluentValidation integration (future)
- [ ] Async validation support (future)
- [ ] Conditional validation (future)

## Conclusion

The implemented validation strategy provides comprehensive data validation across the HabitChain API. The combination of built-in DataAnnotations and custom validation attributes ensures data integrity, security, and excellent user experience.

The validation is designed to be:
- **Secure**: Prevents common attack vectors
- **User-friendly**: Provides clear, actionable error messages
- **Maintainable**: Uses reusable validation attributes
- **Performant**: Validates at the appropriate layers
- **Extensible**: Easy to add new validation rules

The strategy can be enhanced with FluentValidation, async validation, and conditional validation as the application grows and requirements become more complex. 