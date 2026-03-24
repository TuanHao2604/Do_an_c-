using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface IPoiService
{
    Task<List<PoiDetailDto>> GetAllAsync();
    Task<PoiDetailDto?> GetByIdAsync(Guid id);
    Task<PoiDetailDto> CreateAsync(CreatePoiRequest request);
    Task<PoiDetailDto> UpdateAsync(Guid id, UpdatePoiRequest request);
    Task DeleteAsync(Guid id);
    Task<PoiAudioDto> AddAudioAsync(Guid poiId, string languageCode, string fileUrl, int durationSeconds, int sortOrder, bool isTts);
    Task RemoveAudioAsync(Guid poiId, Guid audioId);
    Task<PoiImageDto> AddImageAsync(Guid poiId, string imageUrl, int sortOrder, bool isThumbnail);
    Task RemoveImageAsync(Guid poiId, Guid imageId);
}

// ── DTOs used by service interface ──
public record PoiDetailDto(
    Guid Id,
    double Latitude,
    double Longitude,
    double GeofenceRadius,
    Guid CategoryId,
    string? CategoryName,
    string? QrValue,
    bool IsActive,
    bool IsFeatured,
    List<PoiContentDto> Contents,
    List<PoiImageDto> Images,
    List<PoiOperatingHoursDto> OperatingHours,
    List<PoiAudioDto> AudioFiles,
    double? DistanceKm = null,
    double? AverageRating = null
);

public record PoiContentDto(Guid Id, string LanguageCode, string Name, string? Address, string? Description, string? OperatingHours);
public record PoiImageDto(Guid Id, string ImageUrl, int SortOrder, bool IsThumbnail);
public record PoiOperatingHoursDto(Guid Id, int DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);
public record PoiAudioDto(Guid Id, string LanguageCode, string FileUrl, int DurationSeconds, int SortOrder, bool IsTts);

public record CreatePoiRequest(
    double Latitude, double Longitude, double GeofenceRadius,
    Guid CategoryId, string? QrValue, bool IsActive, bool IsFeatured,
    List<CreatePoiContentRequest> Contents
);
public record CreatePoiContentRequest(string LanguageCode, string Name, string? Address, string? Description, string? OperatingHours);

public record UpdatePoiRequest(
    double Latitude, double Longitude, double GeofenceRadius,
    Guid CategoryId, string? QrValue, bool IsActive, bool IsFeatured,
    List<CreatePoiContentRequest> Contents
);
