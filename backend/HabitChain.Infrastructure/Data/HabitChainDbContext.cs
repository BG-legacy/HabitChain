using HabitChain.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Data;

public class HabitChainDbContext : IdentityDbContext<User>
{
    public HabitChainDbContext(DbContextOptions<HabitChainDbContext> options) : base(options)
    {
    }

    public DbSet<Habit> Habits { get; set; }
    public DbSet<HabitEntry> HabitEntries { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<Encouragement> Encouragements { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(u => u.IsActive).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.UpdatedAt).IsRequired();

            entity.HasMany(u => u.Habits)
                  .WithOne(h => h.User)
                  .HasForeignKey(h => h.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.CheckIns)
                  .WithOne(c => c.User)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.UserBadges)
                  .WithOne(ub => ub.User)
                  .HasForeignKey(ub => ub.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SentEncouragements)
                  .WithOne(e => e.FromUser)
                  .HasForeignKey(e => e.FromUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ReceivedEncouragements)
                  .WithOne(e => e.ToUser)
                  .HasForeignKey(e => e.ToUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.RefreshTokens)
                  .WithOne(rt => rt.User)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Habit entity
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).IsRequired().HasMaxLength(200);
            entity.Property(h => h.Description).HasMaxLength(1000);
            entity.Property(h => h.UserId).IsRequired();
            entity.Property(h => h.Frequency).IsRequired();
            entity.Property(h => h.IsActive).IsRequired();
            entity.Property(h => h.Color).HasMaxLength(7); // For hex colors
            entity.Property(h => h.IconName).HasMaxLength(50);
            entity.Property(h => h.CurrentStreak).IsRequired();
            entity.Property(h => h.LongestStreak).IsRequired();
            entity.Property(h => h.CreatedAt).IsRequired();
            entity.Property(h => h.UpdatedAt).IsRequired();

            entity.HasMany(h => h.Entries)
                  .WithOne(e => e.Habit)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(h => h.CheckIns)
                  .WithOne(c => c.Habit)
                  .HasForeignKey(c => c.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(h => h.UserBadges)
                  .WithOne(ub => ub.Habit)
                  .HasForeignKey(ub => ub.HabitId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(h => h.Encouragements)
                  .WithOne(e => e.Habit)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure HabitEntry entity (keeping for backward compatibility)
        modelBuilder.Entity<HabitEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HabitId).IsRequired();
            entity.Property(e => e.CompletedAt).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure CheckIn entity
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.UserId).IsRequired();
            entity.Property(c => c.HabitId).IsRequired();
            entity.Property(c => c.CompletedAt).IsRequired();
            entity.Property(c => c.Notes).HasMaxLength(500);
            entity.Property(c => c.StreakDay).IsRequired();
            entity.Property(c => c.IsManualEntry).IsRequired();
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.UpdatedAt).IsRequired();

            entity.HasIndex(c => new { c.UserId, c.HabitId, c.CompletedAt });
        });

        // Configure Badge entity
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Description).IsRequired().HasMaxLength(500);
            entity.Property(b => b.IconUrl).IsRequired().HasMaxLength(500);
            entity.Property(b => b.Type).IsRequired();
            entity.Property(b => b.RequiredValue).IsRequired();
            entity.Property(b => b.IsActive).IsRequired();
            entity.Property(b => b.CreatedAt).IsRequired();
            entity.Property(b => b.UpdatedAt).IsRequired();

            entity.HasMany(b => b.UserBadges)
                  .WithOne(ub => ub.Badge)
                  .HasForeignKey(ub => ub.BadgeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserBadge entity
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(ub => ub.Id);
            entity.Property(ub => ub.UserId).IsRequired();
            entity.Property(ub => ub.BadgeId).IsRequired();
            entity.Property(ub => ub.EarnedAt).IsRequired();
            entity.Property(ub => ub.CreatedAt).IsRequired();
            entity.Property(ub => ub.UpdatedAt).IsRequired();

            entity.HasIndex(ub => new { ub.UserId, ub.BadgeId }).IsUnique();
        });

        // Configure Encouragement entity
        modelBuilder.Entity<Encouragement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromUserId).IsRequired();
            entity.Property(e => e.ToUserId).IsRequired();
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.IsRead).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.ToUserId, e.IsRead });
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.Property(rt => rt.UserId).IsRequired();
            entity.Property(rt => rt.ExpiresAt).IsRequired();
            entity.Property(rt => rt.IsRevoked).IsRequired();
            entity.Property(rt => rt.CreatedByIp).HasMaxLength(45);
            entity.Property(rt => rt.RevokedByIp).HasMaxLength(45);
            entity.Property(rt => rt.CreatedAt).IsRequired();
            entity.Property(rt => rt.UpdatedAt).IsRequired();

            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => new { rt.UserId, rt.IsRevoked });
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
} 