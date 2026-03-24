namespace SmartTourApp.Domain.Entities;

public class HeatmapCell
{
    public Guid Id { get; set; }
    public double GridLat { get; set; }
    public double GridLng { get; set; }
    public DateTime HourBucket { get; set; }
    public int HitCount { get; set; }
}
