namespace SmartTourApp.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public string DeviceUuid { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // android, ios
    public string? DeviceToken { get; set; }
    public string? DeviceModel { get; set; }
    public Guid? UserId { get; set; }
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public User? User { get; set; }
    public ICollection<VisitLog> VisitLogs { get; set; } = new List<VisitLog>();
    public ICollection<LocationLog> LocationLogs { get; set; } = new List<LocationLog>();
}
