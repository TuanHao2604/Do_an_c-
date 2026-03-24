namespace SmartTourApp.Domain.Entities;

public class ServicePackage
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
}
