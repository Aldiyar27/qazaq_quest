using QazaqQuest.Models;

namespace QazaqQuest.ViewModels;

public class LeaderboardViewModel
{
    public List<AppUser> TopUsers { get; set; } = new();
    public AppUser? CurrentUser { get; set; }
    public int CurrentUserPosition { get; set; }
    public bool IsCurrentUserHiddenFromLeaderboard { get; set; }
}
