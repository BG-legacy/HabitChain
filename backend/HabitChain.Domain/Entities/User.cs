using Microsoft.AspNetCore.Identity;

namespace HabitChain.Domain.Entities;

// This class represents me as a user in the system. It inherits from IdentityUser for authentication.
public class User : IdentityUser
{
    // This is my first name.
    public string FirstName { get; set; } = string.Empty;
    // This is my last name.
    public string LastName { get; set; } = string.Empty;
    // This is an optional link to my profile picture.
    public string? ProfilePictureUrl { get; set; }
    // This tells me the last time I logged in.
    public DateTime? LastLoginAt { get; set; }
    // I use this to mark if my account is active.
    public bool IsActive { get; set; } = true;
    // Timestamps from BaseEntity functionality
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties help me link myself to other things in the system.
    // These are all the habits I have.
    public ICollection<Habit> Habits { get; set; } = new List<Habit>();
    // These are all the check-ins I've made.
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    // These are all the badges I've earned.
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    // These are all the encouragements I've sent to others.
    public ICollection<Encouragement> SentEncouragements { get; set; } = new List<Encouragement>();
    // These are all the encouragements I've received from others.
    public ICollection<Encouragement> ReceivedEncouragements { get; set; } = new List<Encouragement>();
    // These are all my refresh tokens.
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
} 