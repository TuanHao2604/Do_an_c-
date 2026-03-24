namespace SmartTourApp.Domain.Entities;

public class PoiReview
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PoiId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Poi Poi { get; set; } = null!;
}
