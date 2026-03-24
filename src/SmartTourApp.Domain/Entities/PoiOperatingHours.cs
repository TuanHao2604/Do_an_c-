namespace SmartTourApp.Domain.Entities;

public class PoiOperatingHours
{
    public Guid Id { get; set; }
    public Guid PoiId { get; set; }
    public int DayOfWeek { get; set; } // 0=Sunday, 6=Saturday
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }

    // Navigation
    public Poi Poi { get; set; } = null!;
}
