namespace QazaqQuest.Models;

public class UserAchievement
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RewardPoints { get; set; }
    public DateTime UnlockedAtUtc { get; set; } = DateTime.UtcNow;
}
