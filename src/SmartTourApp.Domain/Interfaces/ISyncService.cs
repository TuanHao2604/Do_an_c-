namespace SmartTourApp.Domain.Interfaces;

public interface ISyncService
{
    Task<SyncResultDto> GetChangedPoisAsync(DateTime lastUpdated);
}

public record SyncPoiDto(
    Guid Id,
    double Latitude,
    double Longitude,
    double GeofenceRadius,
    string? QrValue,
    bool IsActive,
    bool IsFeatured,
    string CategoryName,
    DateTime UpdatedAt,
    List<PoiContentDto> Contents,
    List<PoiImageDto> Images
);

public record SyncResultDto(
    DateTime ServerTime,
    List<SyncPoiDto> Pois
);
