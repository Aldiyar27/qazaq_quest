using QazaqQuest.Models;

namespace QazaqQuest.ViewModels;

public class DailyChallengeIndexViewModel
{
    public AppUser CurrentUser { get; set; } = new();
    public List<DailyChallenge> Challenges { get; set; } = new();
    public Dictionary<int, DailyChallengeProgress> ProgressByChallengeId { get; set; } = new();
}
