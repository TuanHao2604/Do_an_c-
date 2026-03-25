namespace SmartTourApp.Domain.Entities;

public class PoiRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RequestType { get; set; } = "Create"; // Create, Update
    public string RequestData { get; set; } = "{}"; // JSON with POI fields
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string? AdminNote { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
