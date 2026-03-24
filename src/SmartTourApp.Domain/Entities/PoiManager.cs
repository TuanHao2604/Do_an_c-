namespace SmartTourApp.Domain.Entities;

public class PoiManager
{
    public Guid UserId { get; set; }  // Composite PK
    public Guid PoiId { get; set; }   // Composite PK
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Poi Poi { get; set; } = null!;
}
