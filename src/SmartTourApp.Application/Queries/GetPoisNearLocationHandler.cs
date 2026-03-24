using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NetTopologySuite.Geometries;
using SmartTourApp.Domain.Interfaces;
using System.Text.Json;

namespace SmartTourApp.Application.Queries;

/// <summary>
/// Handler: PostGIS geo-query + Redis cache.
/// </summary>
public class GetPoisNearLocationHandler : IRequestHandler<GetPoisNearLocationQuery, List<PoiDetailDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDistributedCache _cache;

    public GetPoisNearLocationHandler(IAppDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<PoiDetailDto>> Handle(GetPoisNearLocationQuery request, CancellationToken ct)
    {
        // Cache key based on rounded coordinates + radius
        var cacheKey = $"pois:nearby:{request.Latitude:F3}:{request.Longitude:F3}:{request.RadiusKm}";
        
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<PoiDetailDto>>(cached) ?? [];
        }

        // Create user location point (SRID 4326 = WGS84)
        var userLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        var radiusMeters = request.RadiusKm * 1000;

        var pois = await _db.Pois
            .Where(p => p.IsActive && p.Location.IsWithinDistance(userLocation, radiusMeters))
            .Include(p => p.Category)
            .Include(p => p.Contents)
            .Include(p => p.Images)
            .Include(p => p.AudioFiles)
            .Include(p => p.OperatingHours)
            .Include(p => p.Reviews)
            .OrderBy(p => p.Location.Distance(userLocation))
            .Select(p => new PoiDetailDto(
                p.Id,
                p.Location.Y, // Latitude
                p.Location.X, // Longitude
                p.GeofenceRadius,
                p.CategoryId,
                p.Category.Name,
                p.QrValue,
                p.IsActive,
                p.IsFeatured,
                p.Contents.Select(c => new PoiContentDto(
                    c.Id, c.LanguageCode, c.Name, c.Address, c.Description, c.OperatingHours
                )).ToList(),
                p.Images.OrderBy(i => i.SortOrder).Select(i => new PoiImageDto(
                    i.Id, i.ImageUrl, i.SortOrder, i.IsThumbnail
                )).ToList(),
                p.OperatingHours.Select(oh => new PoiOperatingHoursDto(
                    oh.Id, oh.DayOfWeek, oh.OpenTime, oh.CloseTime
                )).ToList(),
                p.AudioFiles.OrderBy(a => a.SortOrder).Select(a => new PoiAudioDto(
                    a.Id, a.LanguageCode, a.FileUrl, a.DurationSeconds, a.SortOrder, a.IsTts
                )).ToList(),
                p.Location.Distance(userLocation) / 1000.0, // DistanceKm
                p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : null
            ))
            .ToListAsync(ct);

        // Cache for 5 minutes
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(pois), options, ct);

        return pois;
    }
}
