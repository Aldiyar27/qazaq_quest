namespace QazaqQuest.Models;

public class QuestLocation
{
    public int Id { get; set; }
    public int QuestId { get; set; }
    public Quest? Quest { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; } = 150;

    public List<QuestPoint> Points { get; set; } = new();
}
