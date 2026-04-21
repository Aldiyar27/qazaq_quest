namespace QazaqQuest.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int ExperiencePoints { get; set; }
    public int Coins { get; set; }
    public int Level { get; set; } = 1;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<UserQuestProgress> QuestProgresses { get; set; } = new();
    public List<UserAchievement> Achievements { get; set; } = new();
}
