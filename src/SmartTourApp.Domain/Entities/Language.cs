namespace SmartTourApp.Domain.Entities;

public class Language
{
    public string Code { get; set; } = string.Empty; // PK, e.g. "vi", "en"
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    // Navigation
    public ICollection<PoiContent> PoiContents { get; set; } = new List<PoiContent>();
    public ICollection<AudioFile> AudioFiles { get; set; } = new List<AudioFile>();
}
