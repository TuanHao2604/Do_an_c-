namespace SmartTourApp.Mobile.Models;

public class TopPoiStat
{
    public string PoiId { get; set; } = string.Empty;
    public string PoiName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
}

public class HeatmapCellStat
{
    public string CellKey { get; set; } = string.Empty;
    public int Count { get; set; }
}
