namespace QazaqQuest.Models;

public class UserQuestProgress
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public int QuestId { get; set; }
    public Quest? Quest { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsHiddenUnlocked { get; set; }
    public bool IsRewardClaimed { get; set; }
    public int AttemptsCount { get; set; }
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? LastPlayedAtUtc { get; set; }
}
