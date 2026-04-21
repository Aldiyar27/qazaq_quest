namespace QazaqQuest.Models;

public class Friendship
{
    public int Id { get; set; }
    public Guid RequesterId { get; set; }
    public AppUser? Requester { get; set; }
    public Guid AddresseeId { get; set; }
    public AppUser? Addressee { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAtUtc { get; set; }
}
