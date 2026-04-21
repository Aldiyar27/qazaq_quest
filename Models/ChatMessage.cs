namespace QazaqQuest.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public Guid SenderId { get; set; }
    public AppUser? Sender { get; set; }
    public Guid RecipientId { get; set; }
    public AppUser? Recipient { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}
