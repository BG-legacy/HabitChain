using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Application.Services;
using HabitChain.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace HabitChain.Tests.Services;

public class AuthServiceTests : TestBase
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserManager = CreateMockUserManager();
        _mockJwtService = Fixture.Freeze<Mock<IJwtService>>();
        _authService = new AuthService(_mockUserManager.Object, _mockJwtService.Object, Mapper);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = CreateTestUser();
        var loginDto = new LoginDto { Email = user.Email, Password = "password123" };
        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(true);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockJwtService.Setup(x => x.GenerateAccessToken(user))
            .Returns(accessToken);
        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);
        _mockJwtService.Setup(x => x.GetTokenExpiration())
            .Returns(expiresAt);
        _mockJwtService.Setup(x => x.SaveRefreshTokenAsync(user.Id, refreshToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.ExpiresAt.Should().Be(expiresAt);
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(user.Email);
        result.User.Username.Should().Be(user.UserName);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "invalid@email.com", Password = "password123" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsActive = false;
        var loginDto = new LoginDto { Email = user.Email, Password = "password123" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = CreateTestUser();
        var loginDto = new LoginDto { Email = user.Email, Password = "wrongpassword" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
            ConfirmPassword = "password123"
        };
        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns(accessToken);
        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);
        _mockJwtService.Setup(x => x.GetTokenExpiration())
            .Returns(expiresAt);
        _mockJwtService.Setup(x => x.SaveRefreshTokenAsync(It.IsAny<string>(), refreshToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.ExpiresAt.Should().Be(expiresAt);
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(registerDto.Email);
        result.User.Username.Should().Be(registerDto.Username);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = CreateTestUser();
        var registerDto = new RegisterDto
        {
            Email = existingUser.Email,
            Username = "newuser",
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _authService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = CreateTestUser();
        var registerDto = new RegisterDto
        {
            Email = "new@example.com",
            Username = existingUser.UserName,
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _authService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task RegisterAsync_WithFailedUserCreation_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
            ConfirmPassword = "password123"
        };
        var errors = new[] { new IdentityError { Description = "Password too weak" } };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _authService.RegisterAsync(registerDto));
        exception.Message.Should().Contain("Failed to create user");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAuthResponse()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = "valid_refresh_token";
        var newAccessToken = "new_access_token";
        var newRefreshToken = "new_refresh_token";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        user.RefreshTokens = new List<RefreshToken>
        {
            new RefreshToken
            {
                Token = refreshToken,
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            }
        };

        _mockUserManager.Setup(x => x.Users)
            .Returns(new List<User> { user }.AsQueryable());
        _mockJwtService.Setup(x => x.ValidateRefreshTokenAsync(refreshToken, user.Id))
            .ReturnsAsync(true);
        _mockJwtService.Setup(x => x.RevokeRefreshTokenAsync(refreshToken))
            .ReturnsAsync(true);
        _mockJwtService.Setup(x => x.GenerateAccessToken(user))
            .Returns(newAccessToken);
        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);
        _mockJwtService.Setup(x => x.GetTokenExpiration())
            .Returns(expiresAt);
        _mockJwtService.Setup(x => x.SaveRefreshTokenAsync(user.Id, newRefreshToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(newAccessToken);
        result.RefreshToken.Should().Be(newRefreshToken);
        result.ExpiresAt.Should().Be(expiresAt);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var refreshToken = "invalid_token";

        _mockUserManager.Setup(x => x.Users)
            .Returns(new List<User>().AsQueryable());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _authService.RefreshTokenAsync(refreshToken));
    }

    [Fact]
    public async Task RevokeTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var refreshToken = "valid_token";

        _mockJwtService.Setup(x => x.RevokeRefreshTokenAsync(refreshToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RevokeTokenAsync(refreshToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword",
            ConfirmNewPassword = "newpassword"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.ChangePasswordAsync(user.Id, changePasswordDto);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidUser_ReturnsFalse()
    {
        // Arrange
        var userId = "invalid_user_id";
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword",
            ConfirmNewPassword = "newpassword"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUserDto()
    {
        // Arrange
        var user = CreateTestUser();

        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.UserName);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = "invalid_user_id";

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLastLoginAsync_WithValidUser_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser();

        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.UpdateLastLoginAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        _mockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.LastLoginAt.HasValue && u.UpdatedAt > user.UpdatedAt)), Times.Once);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_WithInvalidUser_ReturnsFalse()
    {
        // Arrange
        var userId = "invalid_user_id";

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.UpdateLastLoginAsync(userId);

        // Assert
        result.Should().BeFalse();
    }
} 