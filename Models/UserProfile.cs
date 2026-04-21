namespace QazaqQuest.Models;

public class UserProfile
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string AvatarUrl { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int CompletedQuests { get; set; }
    public int Achievements { get; set; }
    public int ExperiencePoints { get; set; }
    public int Coins { get; set; }
    public int Level { get; set; }
    public int StartedQuests { get; set; }
    public int RankPosition { get; set; }
}
