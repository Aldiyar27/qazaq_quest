namespace QazaqQuest.Models;

public class QuestPoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; }
    public int Order { get; set; }
}
