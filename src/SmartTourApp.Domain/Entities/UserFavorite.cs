namespace SmartTourApp.Domain.Entities;

public class UserFavorite
{
    public Guid UserId { get; set; }  // Composite PK
    public Guid PoiId { get; set; }   // Composite PK
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Poi Poi { get; set; } = null!;
}
