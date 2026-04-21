using Microsoft.EntityFrameworkCore;
using QazaqQuest.Models;

namespace QazaqQuest.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<QuestPoint> QuestPoints => Set<QuestPoint>();
    public DbSet<QuestLocation> QuestLocations => Set<QuestLocation>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<UserQuestProgress> UserQuestProgresses => Set<UserQuestProgress>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<DailyChallenge> DailyChallenges => Set<DailyChallenge>();
    public DbSet<DailyChallengeProgress> DailyChallengeProgresses => Set<DailyChallengeProgress>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<QuestReview> QuestReviews => Set<QuestReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(30);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Role).IsRequired().HasMaxLength(20);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.PasswordSalt).IsRequired();
            entity.Property(x => x.AvatarUrl).HasMaxLength(500);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Quest>(entity =>
        {
            entity.ToTable("Quests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.Property(x => x.City).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Difficulty).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Type).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.CoverStyle).HasMaxLength(300);
            entity.Property(x => x.Icon).HasMaxLength(20);
        });

        modelBuilder.Entity<QuestLocation>(entity =>
        {
            entity.ToTable("QuestLocations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.HasOne(x => x.Quest)
                .WithMany(x => x.Locations)
                .HasForeignKey(x => x.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestPoint>(entity =>
        {
            entity.ToTable("QuestPoints");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Task).IsRequired();
            entity.Property(x => x.Answer).IsRequired().HasMaxLength(200);
            entity.Property(x => x.TaskType).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Hint).HasMaxLength(500);
            entity.Property(x => x.OptionsSerialized).HasColumnName("Options");
            entity.HasOne(x => x.Quest)
                .WithMany(x => x.Points)
                .HasForeignKey(x => x.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Location)
                .WithMany(x => x.Points)
                .HasForeignKey(x => x.QuestLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.ToTable("Rewards");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.HasOne(x => x.Quest)
                .WithMany(x => x.Rewards)
                .HasForeignKey(x => x.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserQuestProgress>(entity =>
        {
            entity.ToTable("UserQuestProgresses");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.QuestId }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.QuestProgresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Quest).WithMany(x => x.UserProgresses).HasForeignKey(x => x.QuestId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.ToTable("UserAchievements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.Code }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.Achievements).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<DailyChallenge>(entity =>
        {
            entity.ToTable("DailyChallenges");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.Property(x => x.ChallengeType).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<DailyChallengeProgress>(entity =>
        {
            entity.ToTable("DailyChallengeProgresses");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.DailyChallengeId }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.DailyChallengeProgresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.DailyChallenge).WithMany(x => x.Progresses).HasForeignKey(x => x.DailyChallengeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.ToTable("Friendships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(40);
            entity.HasIndex(x => new { x.RequesterId, x.AddresseeId }).IsUnique();
            entity.HasOne(x => x.Requester).WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Addressee).WithMany().HasForeignKey(x => x.AddresseeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MessageText).IsRequired().HasMaxLength(1500);
            entity.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Recipient).WithMany().HasForeignKey(x => x.RecipientId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuestReview>(entity =>
        {
            entity.ToTable("QuestReviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Comment).IsRequired().HasMaxLength(1500);
            entity.HasIndex(x => new { x.UserId, x.QuestId }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.QuestReviews).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Quest).WithMany().HasForeignKey(x => x.QuestId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
