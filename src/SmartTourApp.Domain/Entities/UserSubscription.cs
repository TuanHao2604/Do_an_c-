namespace SmartTourApp.Domain.Entities;

public class UserSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "active"; // active, expired, cancelled

    // Navigation
    public User User { get; set; } = null!;
    public ServicePackage Package { get; set; } = null!;
}
