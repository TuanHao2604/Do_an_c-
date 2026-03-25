using SQLite;

namespace SmartTourApp.Mobile.Models;

[Table("favorites")]
public class LocalFavorite
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string PoiId { get; set; } = string.Empty;
    public string PoiName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? Address { get; set; }
    public DateTime AddedAt { get; set; }
}
