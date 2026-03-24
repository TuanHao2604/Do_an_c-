namespace SmartTourApp.Domain.Entities;

public class PoiContent
{
    public Guid Id { get; set; }
    public Guid PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? OperatingHours { get; set; }

    // Navigation
    public Poi Poi { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
