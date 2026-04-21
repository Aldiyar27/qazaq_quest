using QazaqQuest.Models;

namespace QazaqQuest.ViewModels;

public class SocialHubViewModel
{
    public AppUser CurrentUser { get; set; } = new();
    public List<AppUser> SuggestedUsers { get; set; } = new();
    public List<Friendship> Friends { get; set; } = new();
    public List<Friendship> IncomingRequests { get; set; } = new();
    public List<Friendship> OutgoingRequests { get; set; } = new();
    public Guid? SelectedFriendId { get; set; }
    public AppUser? SelectedFriend { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
}
