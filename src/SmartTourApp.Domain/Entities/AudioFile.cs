namespace SmartTourApp.Domain.Entities;

public class AudioFile
{
    public Guid Id { get; set; }
    public Guid PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int SortOrder { get; set; }
    public bool IsTts { get; set; }

    // Navigation
    public Poi Poi { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
