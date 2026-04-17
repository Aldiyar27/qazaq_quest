namespace QazaqQuest.Models;

public class UserProfile
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public int TotalPoints { get; set; }
    public int CompletedQuests { get; set; }
    public int Achievements { get; set; }
}
