using QazaqQuest.Models;

namespace QazaqQuest.ViewModels;

public class QuestPlayViewModel
{
    public Quest Quest { get; set; } = new();
    public QuestPoint CurrentPoint { get; set; } = new();
    public int CurrentStepIndex { get; set; }
    public int TotalSteps { get; set; }
    public bool IsCompleted { get; set; }
    public bool LocationVerified { get; set; }
    public string? Message { get; set; }
}
