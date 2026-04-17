using QazaqQuest.Models;

namespace QazaqQuest.ViewModels;

public class QuestListViewModel
{
    public List<Quest> Quests { get; set; } = new();
    public List<string> AvailableCities { get; set; } = new();
    public List<string> AvailableDifficulties { get; set; } = new();
    public List<string> AvailableTypes { get; set; } = new();
    public string? Difficulty { get; set; }
    public string? Type { get; set; }
    public string? City { get; set; }
    public string? Search { get; set; }
}
