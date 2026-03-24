using SQLite;

namespace SmartTourApp.Mobile.Models;

/// <summary>
/// Local SQLite model for offline POI data.
/// </summary>
[Table("pois")]
public class LocalPoi
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double GeofenceRadius { get; set; }
    public string? QrValue { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;               // Vietnamese name
    public string? NameEn { get; set; }                              // English name
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime SyncedAt { get; set; }
}
