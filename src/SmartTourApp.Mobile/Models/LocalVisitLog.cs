using SQLite;

namespace SmartTourApp.Mobile.Models;

[Table("visit_logs")]
public class LocalVisitLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string PoiId { get; set; } = string.Empty;
    public string PoiName { get; set; } = string.Empty;
    public DateTime VisitedAt { get; set; }
    public double? NarrationSeconds { get; set; }
    public bool TriggeredByQr { get; set; }
}
