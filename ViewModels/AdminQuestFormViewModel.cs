using QazaqQuest.Models;

namespace QazaqQuest.ViewModels;

public class AdminQuestFormViewModel
{
    public Quest Quest { get; set; } = new();
    public List<QuestPoint> Points { get; set; } = new();
    public List<QuestLocation> Locations { get; set; } = new();
    public List<Reward> Rewards { get; set; } = new();
}
