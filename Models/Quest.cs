namespace QazaqQuest.Models;

public class Quest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string RouteLength { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CoverStyle { get; set; } = "linear-gradient(135deg, #114b5f 0%, #1a936f 100%)";
    public string Icon { get; set; } = "🧭";
    public string Language { get; set; } = "RU / KZ / EN";
    public string Partner { get; set; } = string.Empty;
    public string Bonus { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public bool IsTimed { get; set; }
    public bool IsCoop { get; set; }
    public bool IsFeatured { get; set; }
    public int UnlockLevel { get; set; } = 1;
    public int TimeLimitMinutes { get; set; }
    public int ExperienceReward { get; set; } = 100;
    public int CoinsReward { get; set; } = 25;
    public List<QuestPoint> Points { get; set; } = new();
    public List<QuestLocation> Locations { get; set; } = new();
    public List<Reward> Rewards { get; set; } = new();
    public List<UserQuestProgress> UserProgresses { get; set; } = new();
}
