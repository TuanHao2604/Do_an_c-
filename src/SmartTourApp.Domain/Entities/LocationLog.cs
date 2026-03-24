using NetTopologySuite.Geometries;

namespace SmartTourApp.Domain.Entities;

public class LocationLog
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Point Location { get; set; } = null!; // PostGIS geometry Point, SRID=4326
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Device Device { get; set; } = null!;
}
