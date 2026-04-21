namespace QazaqQuest.Models;

public class DailyChallengeProgress
{
    public int Id { get; set; }
    public int DailyChallengeId { get; set; }
    public DailyChallenge? DailyChallenge { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public int CurrentValue { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRewardClaimed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
