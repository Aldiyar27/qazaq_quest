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
    public List<QuestPoint> Points { get; set; } = new();
    public List<Reward> Rewards { get; set; } = new();
}
