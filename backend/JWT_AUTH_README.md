# JWT Authentication with ASP.NET Identity

This document describes the JWT authentication implementation for the HabitChain API using ASP.NET Identity.

## Overview

The authentication system provides:
- User registration and login
- JWT access tokens with refresh token support
- Password management
- Secure API endpoints with role-based authorization
- Token revocation capabilities

## Architecture

### Components

1. **User Entity** (`HabitChain.Domain.Entities.User`)
   - Inherits from `IdentityUser` for built-in authentication features
   - Extended with custom properties like `FirstName`, `LastName`, `ProfilePictureUrl`, etc.
   - Includes navigation properties for habits, check-ins, badges, and refresh tokens

2. **RefreshToken Entity** (`HabitChain.Domain.Entities.RefreshToken`)
   - Stores refresh tokens in the database
   - Includes expiration and revocation tracking
   - Linked to users for token management

3. **Authentication Service** (`HabitChain.Application.Services.AuthService`)
   - Handles user registration, login, and password management
   - Manages refresh token lifecycle
   - Integrates with ASP.NET Identity UserManager and SignInManager

4. **JWT Service** (`HabitChain.Infrastructure.Services.JwtService`)
   - Generates and validates JWT access tokens
   - Manages refresh token creation and validation
   - Configurable token expiration and security settings

5. **Authentication Controller** (`HabitChain.WebAPI.Controllers.AuthController`)
   - Exposes REST endpoints for authentication operations
   - Handles error responses and validation

## Configuration

### JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HabitChainAPI",
    "Audience": "HabitChainClient",
    "ExpireMinutes": 60
  }
}
```

### Identity Configuration (Program.cs)

```csharp
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<HabitChainDbContext>()
.AddDefaultTokenProviders();
```

## API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Login with email/password | No |
| POST | `/api/auth/refresh` | Refresh access token | No |
| POST | `/api/auth/revoke` | Revoke refresh token | Yes |
| POST | `/api/auth/change-password` | Change user password | Yes |
| GET | `/api/auth/me` | Get current user info | Yes |

### Request/Response Examples

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "username",
  "firstName": "John",
  "lastName": "Doe",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

#### Login Response
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "user-id",
    "email": "user@example.com",
    "username": "username",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

## Security Features

### Token Security
- **Access Tokens**: Short-lived (60 minutes by default)
- **Refresh Tokens**: Long-lived (7 days), stored securely in database
- **Token Revocation**: Refresh tokens can be revoked
- **Secure Headers**: HTTPS required in production

### Password Security
- Minimum 6 characters
- Requires uppercase, lowercase, and digit
- Hashed using ASP.NET Identity's secure hashing

### Database Security
- Refresh tokens stored with expiration tracking
- User accounts can be deactivated
- Audit trail with created/updated timestamps

## Usage in Controllers

### Protecting Endpoints
```csharp
[Authorize]
[HttpGet("protected-endpoint")]
public async Task<IActionResult> ProtectedEndpoint()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // Your protected logic here
}
```

### Getting Current User
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value;
var firstName = User.FindFirst("firstName")?.Value;
```

## Database Migrations

After implementing authentication, run migrations to update the database:

```bash
cd backend/HabitChain.Infrastructure
dotnet ef migrations add AddIdentityAndRefreshTokens --startup-project ../HabitChain.WebAPI
dotnet ef database update --startup-project ../HabitChain.WebAPI
```

## Testing

Use the provided `auth-test.http` file to test authentication endpoints:

1. Register a new user
2. Login to get tokens
3. Use the access token to access protected endpoints
4. Refresh tokens when they expire
5. Test password changes and token revocation

## Security Considerations

### Production Deployment
1. **Secret Key**: Use a strong, randomly generated secret key
2. **HTTPS**: Always use HTTPS in production
3. **Token Storage**: Store refresh tokens securely on the client
4. **Rate Limiting**: Implement rate limiting for auth endpoints
5. **Logging**: Log authentication events for security monitoring

### Environment Variables
Store sensitive configuration in environment variables:

```bash
export JWT_SECRET_KEY="your-production-secret-key"
export JWT_ISSUER="your-production-issuer"
export JWT_AUDIENCE="your-production-audience"
```

## Troubleshooting

### Common Issues

1. **Token Validation Fails**
   - Check JWT secret key configuration
   - Verify token hasn't expired
   - Ensure proper Authorization header format: `Bearer <token>`

2. **User Registration Fails**
   - Check password requirements
   - Verify email/username uniqueness
   - Review validation error messages

3. **Database Connection Issues**
   - Verify connection string
   - Ensure database exists and migrations are applied
   - Check PostgreSQL service is running

### Debugging Tips

1. Enable detailed logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"
    }
  }
}
```

2. Use Swagger UI in development to test endpoints interactively
3. Check browser network tab for detailed error responses
4. Review server logs for authentication failures

## Next Steps

1. **Role-Based Authorization**: Implement user roles and permissions
2. **Email Verification**: Add email confirmation for new registrations
3. **Password Reset**: Implement forgot password functionality
4. **Two-Factor Authentication**: Add 2FA support
5. **OAuth Integration**: Support Google/Facebook login
6. **API Rate Limiting**: Implement request throttling
7. **Audit Logging**: Track user actions for security 