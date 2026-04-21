namespace QazaqQuest.Models;

public class DailyChallenge
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ChallengeType { get; set; } = string.Empty;
    public int TargetValue { get; set; }
    public int ExperienceReward { get; set; }
    public int CoinsReward { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ActiveDateUtc { get; set; } = DateTime.UtcNow.Date;

    public List<DailyChallengeProgress> Progresses { get; set; } = new();
}
