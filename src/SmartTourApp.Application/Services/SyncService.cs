using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Interfaces;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class SyncService : ISyncService
{
    private readonly IAppDbContext _db;

    public SyncService(IAppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all POIs that have been updated since lastUpdated (server → mobile sync).
    /// </summary>
    public async Task<SyncResultDto> GetChangedPoisAsync(DateTime lastUpdated)
    {
        var pois = await _db.Pois
            .Where(p => p.UpdatedAt > lastUpdated)
            .Include(p => p.Category)
            .Include(p => p.Contents)
            .Include(p => p.Images)
            .OrderBy(p => p.UpdatedAt)
            .Select(p => new SyncPoiDto(
                p.Id,
                p.Location.Y,
                p.Location.X,
                p.GeofenceRadius,
                p.QrValue,
                p.IsActive,
                p.IsFeatured,
                p.Category.Name,
                p.UpdatedAt,
                p.Contents.Select(c => new PoiContentDto(
                    c.Id, c.LanguageCode, c.Name, c.Address, c.Description, c.OperatingHours
                )).ToList(),
                p.Images.OrderBy(i => i.SortOrder).Select(i => new PoiImageDto(
                    i.Id, i.ImageUrl, i.SortOrder, i.IsThumbnail
                )).ToList()
            ))
            .ToListAsync();

        return new SyncResultDto(DateTime.UtcNow, pois);
    }
}
