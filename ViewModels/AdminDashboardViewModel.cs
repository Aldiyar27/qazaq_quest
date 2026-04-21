using QazaqQuest.Models;

namespace QazaqQuest.ViewModels;

public class AdminDashboardViewModel
{
    public List<Quest> Quests { get; set; } = new();
    public List<AppUser> TopUsers { get; set; } = new();
    public int TotalCities { get; set; }
    public int FreeCount { get; set; }
    public int PaidCount { get; set; }
    public int TotalPoints { get; set; }
    public int TotalLocations { get; set; }
    public int TotalUsers { get; set; }
    public int CompletedRuns { get; set; }
    public int ActiveRuns { get; set; }
    public int HiddenQuests { get; set; }
}
