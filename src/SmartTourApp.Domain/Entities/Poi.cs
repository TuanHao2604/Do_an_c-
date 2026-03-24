using NetTopologySuite.Geometries;

namespace SmartTourApp.Domain.Entities;

public class Poi
{
    public Guid Id { get; set; }
    public Point Location { get; set; } = null!; // PostGIS geometry Point, SRID=4326
    public double GeofenceRadius { get; set; }
    public Guid CategoryId { get; set; }
    public string? QrValue { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<PoiContent> Contents { get; set; } = new List<PoiContent>();
    public ICollection<PoiOperatingHours> OperatingHours { get; set; } = new List<PoiOperatingHours>();
    public ICollection<PoiImage> Images { get; set; } = new List<PoiImage>();
    public ICollection<AudioFile> AudioFiles { get; set; } = new List<AudioFile>();
    public ICollection<PoiManager> Managers { get; set; } = new List<PoiManager>();
    public ICollection<VisitLog> VisitLogs { get; set; } = new List<VisitLog>();
    public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
    public ICollection<PoiReview> Reviews { get; set; } = new List<PoiReview>();
}
