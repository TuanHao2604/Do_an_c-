namespace SmartTourApp.Domain.Entities;

public class PoiImage
{
    public Guid Id { get; set; }
    public Guid PoiId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsThumbnail { get; set; }

    // Navigation
    public Poi Poi { get; set; } = null!;
}
