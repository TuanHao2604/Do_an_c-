using SQLite;

namespace SmartTourApp.Mobile.Models;

[Table("tours")]
public class LocalTour
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public string PoiIds { get; set; } = string.Empty;   // comma-separated
    public DateTime CreatedAt { get; set; }
}
