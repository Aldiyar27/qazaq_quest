namespace QazaqQuest.Models;

public class QuestPoint
{
    public int Id { get; set; }
    public int QuestId { get; set; }
    public Quest? Quest { get; set; }

    public int? QuestLocationId { get; set; }
    public QuestLocation? Location { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
    public string OptionsSerialized { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; }
    public int Order { get; set; }

    public List<string> Options
    {
        get => string.IsNullOrWhiteSpace(OptionsSerialized)
            ? new List<string>()
            : OptionsSerialized.Split("||", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        set => OptionsSerialized = value == null || value.Count == 0
            ? string.Empty
            : string.Join("||", value.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
    }
}
