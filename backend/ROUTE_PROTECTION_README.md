# Route Protection and Authorization Strategy

## Overview

This document explains the comprehensive route protection and authorization strategy implemented across all controllers in the HabitChain WebAPI. The implementation ensures data security, privacy, and proper access control for all endpoints.

## Authorization Strategy Summary

### 1. Authentication vs Authorization

- **Authentication**: Verifies who the user is (JWT token validation)
- **Authorization**: Determines what the user can access (role-based and ownership-based)

### 2. JWT Token Structure

All protected endpoints require a valid JWT token in the Authorization header:
```
Authorization: Bearer {jwt_token}
```

The JWT token contains user claims including:
- `ClaimTypes.NameIdentifier`: User ID
- `ClaimTypes.Email`: User email
- `ClaimTypes.Name`: User name
- Custom claims for roles and permissions

## Controller-Specific Authorization Strategies

### 1. AuthController

**Strategy**: Mixed public and protected endpoints

#### Public Endpoints (No [Authorize] required):
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User authentication
- `POST /api/auth/refresh` - Token refresh

#### Protected Endpoints ([Authorize] required):
- `POST /api/auth/revoke` - Token revocation
- `POST /api/auth/change-password` - Password changes
- `GET /api/auth/me` - Current user information

**Security Features**:
- User ID extracted from JWT token claims
- Users can only access their own authentication data
- Password changes require current password verification
- Token revocation ensures proper session management

### 2. HabitsController

**Strategy**: All endpoints protected with [Authorize]

**Security Features**:
- User ID extracted from JWT token for all operations
- Users can only access their own habits
- Habit ownership verification before any modifications
- User ID automatically set from JWT token in creation/update operations

**Key Implementation**:
```csharp
[Authorize] // Protect all endpoints in this controller
public class HabitsController : ControllerBase
{
    // User ID extracted from JWT token
    var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Ownership verification
    if (authenticatedUserId != userId)
    {
        return Unauthorized(new { message = "You can only access your own habits." });
    }
}
```

### 3. HabitEntriesController

**Strategy**: All endpoints protected with [Authorize]

**Security Features**:
- User ID extracted from JWT token for all operations
- Users can only access entries for their own habits
- Date validation to prevent abuse (reasonable date ranges)
- Service layer ownership verification

**Key Implementation**:
```csharp
[Authorize] // Protect all endpoints in this controller
public class HabitEntriesController : ControllerBase
{
    // Date validation to prevent abuse
    var maxDateRange = TimeSpan.FromDays(365);
    if (endDate - startDate > maxDateRange)
    {
        return BadRequest(new { message = "Date range cannot exceed 365 days." });
    }
}
```

### 4. CheckInsController

**Strategy**: All endpoints protected with [Authorize]

**Security Features**:
- User ID extracted from JWT token for all operations
- Users can only access check-ins for their own habits
- Date validation and range limiting
- Service layer ownership verification

**Key Implementation**:
```csharp
[Authorize] // Protect all endpoints in this controller
public class CheckInsController : ControllerBase
{
    // User ownership verification
    if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
    {
        return Unauthorized(new { message = "You can only access your own check-ins." });
    }
}
```

### 5. EncouragementsController

**Strategy**: All endpoints protected with [Authorize]

**Security Features**:
- Users can only access encouragements they've sent or received
- Sender/recipient verification for different operations
- Read status updates restricted to recipients
- Permission verification for sending encouragements

**Key Implementation**:
```csharp
[Authorize] // Protect all endpoints in this controller
public class EncouragementsController : ControllerBase
{
    // Sender/recipient verification
    if (encouragement.UserId != authenticatedUserId && 
        encouragement.FromUserId != authenticatedUserId)
    {
        return Unauthorized(new { message = "You can only access encouragements you've sent or received." });
    }
}
```

### 6. BadgesController

**Strategy**: Mixed public read access and protected admin operations

#### Public Endpoints (No [Authorize] required):
- `GET /api/badges` - Get all badges
- `GET /api/badges/active` - Get active badges
- `GET /api/badges/{id}` - Get badge by ID

#### Protected Endpoints ([Authorize] required):
- `POST /api/badges` - Create new badge (admin only)
- `PUT /api/badges/{id}` - Update badge (admin only)
- `DELETE /api/badges/{id}` - Delete badge (admin only)

**Security Features**:
- Badge viewing is public to encourage user engagement
- Badge management restricted to administrators
- Admin role verification in service layer (TODO)

### 7. UserBadgesController

**Strategy**: All endpoints protected with [Authorize]

**Security Features**:
- Users can only access their own earned badges
- Badge earning status is private to the user
- Manual badge assignment restricted to admins
- Ownership verification for all operations

## Security Implementation Details

### 1. JWT Token Extraction

All protected controllers extract the user ID from JWT token claims:

```csharp
var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (string.IsNullOrEmpty(authenticatedUserId))
{
    return Unauthorized(new { message = "User not authenticated." });
}
```

### 2. Ownership Verification

Controllers verify that users can only access their own data:

```csharp
// Ensure user can only access their own data
if (authenticatedUserId != userId)
{
    return Unauthorized(new { message = "You can only access your own data." });
}
```

### 3. Input Validation

Controllers implement input validation to prevent abuse:

```csharp
// Date range validation
if (endDate < startDate)
{
    return BadRequest(new { message = "End date must be after start date." });
}

// Range limiting
var maxDateRange = TimeSpan.FromDays(365);
if (endDate - startDate > maxDateRange)
{
    return BadRequest(new { message = "Date range cannot exceed 365 days." });
}
```

### 4. Service Layer Security

Controllers delegate ownership verification to service layers:

```csharp
// The service layer should verify habit ownership before returning data
var data = await _service.GetDataAsync(id);
```

## Best Practices Implemented

### 1. Defense in Depth
- Controller-level authorization with [Authorize] attributes
- Service layer ownership verification
- Input validation and sanitization
- Date range limiting to prevent abuse

### 2. Principle of Least Privilege
- Users can only access their own data
- Admin operations restricted to administrators
- Public endpoints limited to non-sensitive data

### 3. Secure by Default
- All endpoints protected unless explicitly marked as public
- User ID automatically set from JWT token
- Ownership verification on all data access

### 4. Input Validation
- Model state validation
- Date range validation
- User ID verification against JWT claims

## Future Enhancements

### 1. Role-Based Authorization
Implement role-based authorization for admin operations:

```csharp
[Authorize(Roles = "Admin")]
public async Task<ActionResult<BadgeDto>> CreateBadge([FromBody] CreateBadgeDto dto)
```

### 2. Policy-Based Authorization
Create custom authorization policies for complex scenarios:

```csharp
[Authorize(Policy = "CanManageBadges")]
public async Task<ActionResult<BadgeDto>> CreateBadge([FromBody] CreateBadgeDto dto)
```

### 3. Audit Logging
Implement comprehensive audit logging for all data modifications:

```csharp
// Log all data access and modifications
_logger.LogInformation("User {UserId} accessed habit {HabitId}", authenticatedUserId, habitId);
```

### 4. Rate Limiting
Implement rate limiting to prevent abuse:

```csharp
[EnableRateLimiting("fixed")]
public class HabitsController : ControllerBase
```

## Testing Authorization

### 1. Unit Tests
Test authorization logic in isolation:

```csharp
[Test]
public void GetHabitsByUserId_WithDifferentUserId_ReturnsUnauthorized()
{
    // Arrange
    var controller = new HabitsController(mockService);
    controller.ControllerContext = CreateControllerContext("user1");
    
    // Act
    var result = controller.GetHabitsByUserId("user2");
    
    // Assert
    Assert.IsInstanceOf<UnauthorizedResult>(result.Result);
}
```

### 2. Integration Tests
Test complete authorization flow:

```csharp
[Test]
public async Task GetHabits_WithValidToken_ReturnsUserHabits()
{
    // Arrange
    var token = GenerateJwtToken("user1");
    var client = CreateAuthenticatedClient(token);
    
    // Act
    var response = await client.GetAsync("/api/habits/user/user1");
    
    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
}
```

## Security Checklist

- [x] All sensitive endpoints protected with [Authorize]
- [x] User ID extracted from JWT token claims
- [x] Ownership verification implemented
- [x] Input validation and sanitization
- [x] Date range limiting
- [x] Comprehensive error handling
- [x] Detailed security documentation
- [ ] Role-based authorization (TODO)
- [ ] Policy-based authorization (TODO)
- [ ] Audit logging (TODO)
- [ ] Rate limiting (TODO)

## Conclusion

The implemented authorization strategy provides comprehensive security for the HabitChain API while maintaining usability and performance. The multi-layered approach ensures that users can only access their own data while providing appropriate public access to non-sensitive information.

The strategy is designed to be scalable and can be enhanced with role-based authorization, policy-based authorization, and additional security features as the application grows. 