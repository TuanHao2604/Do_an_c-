namespace SmartTourApp.Domain.Interfaces;

public interface IAiGuideService
{
    Task<string> GetGuideDescriptionAsync(Guid poiId, string languageCode = "vi");
}
