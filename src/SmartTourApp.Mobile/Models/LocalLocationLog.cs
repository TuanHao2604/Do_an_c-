using SQLite;

namespace SmartTourApp.Mobile.Models;

[Table("location_logs")]
public class LocalLocationLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CellKey { get; set; } = string.Empty;
    public string RouteSessionId { get; set; } = string.Empty;
    public DateTime LoggedAt { get; set; }
}
