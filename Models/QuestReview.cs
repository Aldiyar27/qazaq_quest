namespace QazaqQuest.Models;

public class QuestReview
{
    public int Id { get; set; }
    public int QuestId { get; set; }
    public Quest? Quest { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
