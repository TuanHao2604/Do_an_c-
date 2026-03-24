namespace SmartTourApp.Domain.Entities;

public class VisitLog
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Guid PoiId { get; set; }
    public string TriggerType { get; set; } = string.Empty; // qr, geofence, manual
    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Device Device { get; set; } = null!;
    public Poi Poi { get; set; } = null!;
}
