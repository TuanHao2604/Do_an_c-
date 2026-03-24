using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class PoiService : IPoiService
{
    private readonly IAppDbContext _db;

    public PoiService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PoiDetailDto>> GetAllAsync()
    {
        return await _db.Pois
            .Include(p => p.Category)
            .Include(p => p.Contents)
            .Include(p => p.Images)
            .Include(p => p.AudioFiles)
            .Include(p => p.OperatingHours)
            .Include(p => p.Reviews)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<PoiDetailDto?> GetByIdAsync(Guid id)
    {
        var p = await _db.Pois
            .Include(p => p.Category)
            .Include(p => p.Contents)
            .Include(p => p.Images)
            .Include(p => p.AudioFiles)
            .Include(p => p.OperatingHours)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id);

        return p is null ? null : MapToDto(p);
    }

    public async Task<PoiDetailDto> CreateAsync(CreatePoiRequest request)
    {
        var poi = new Poi
        {
            Id = Guid.NewGuid(),
            Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 },
            GeofenceRadius = request.GeofenceRadius,
            CategoryId = request.CategoryId,
            QrValue = request.QrValue,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        foreach (var c in request.Contents)
        {
            poi.Contents.Add(new PoiContent
            {
                Id = Guid.NewGuid(),
                LanguageCode = c.LanguageCode,
                Name = c.Name,
                Address = c.Address,
                Description = c.Description,
                OperatingHours = c.OperatingHours,
            });
        }

        _db.Pois.Add(poi);
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(poi.Id))!;
    }

    public async Task<PoiDetailDto> UpdateAsync(Guid id, UpdatePoiRequest request)
    {
        var poi = await _db.Pois
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"POI {id} not found");

        poi.Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        poi.GeofenceRadius = request.GeofenceRadius;
        poi.CategoryId = request.CategoryId;
        poi.QrValue = request.QrValue;
        poi.IsActive = request.IsActive;
        poi.IsFeatured = request.IsFeatured;
        poi.UpdatedAt = DateTime.UtcNow;

        // Replace contents
        _db.PoiContents.RemoveRange(poi.Contents);
        foreach (var c in request.Contents)
        {
            poi.Contents.Add(new PoiContent
            {
                Id = Guid.NewGuid(),
                LanguageCode = c.LanguageCode,
                Name = c.Name,
                Address = c.Address,
                Description = c.Description,
                OperatingHours = c.OperatingHours,
            });
        }

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var poi = await _db.Pois.FindAsync(id)
            ?? throw new KeyNotFoundException($"POI {id} not found");

        _db.Pois.Remove(poi);
        await _db.SaveChangesAsync();
    }

    public async Task<PoiAudioDto> AddAudioAsync(Guid poiId, string languageCode, string fileUrl, int durationSeconds, int sortOrder, bool isTts)
    {
        var audio = new AudioFile
        {
            Id = Guid.NewGuid(),
            PoiId = poiId,
            LanguageCode = languageCode,
            FileUrl = fileUrl,
            DurationSeconds = durationSeconds,
            SortOrder = sortOrder,
            IsTts = isTts
        };
        _db.AudioFiles.Add(audio);
        await _db.SaveChangesAsync();
        return new PoiAudioDto(audio.Id, audio.LanguageCode, audio.FileUrl, audio.DurationSeconds, audio.SortOrder, audio.IsTts);
    }

    public async Task RemoveAudioAsync(Guid poiId, Guid audioId)
    {
        var audio = await _db.AudioFiles.FirstOrDefaultAsync(a => a.PoiId == poiId && a.Id == audioId);
        if (audio is not null)
        {
            _db.AudioFiles.Remove(audio);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<PoiImageDto> AddImageAsync(Guid poiId, string imageUrl, int sortOrder, bool isThumbnail)
    {
        var image = new PoiImage
        {
            Id = Guid.NewGuid(),
            PoiId = poiId,
            ImageUrl = imageUrl,
            SortOrder = sortOrder,
            IsThumbnail = isThumbnail
        };
        _db.PoiImages.Add(image);
        await _db.SaveChangesAsync();
        return new PoiImageDto(image.Id, image.ImageUrl, image.SortOrder, image.IsThumbnail);
    }

    public async Task RemoveImageAsync(Guid poiId, Guid imageId)
    {
        var image = await _db.PoiImages.FirstOrDefaultAsync(i => i.PoiId == poiId && i.Id == imageId);
        if (image is not null)
        {
            _db.PoiImages.Remove(image);
            await _db.SaveChangesAsync();
        }
    }

    private static PoiDetailDto MapToDto(Poi p) => new(
        p.Id,
        p.Location.Y,
        p.Location.X,
        p.GeofenceRadius,
        p.CategoryId,
        p.Category?.Name,
        p.QrValue,
        p.IsActive,
        p.IsFeatured,
        p.Contents.Select(c => new PoiContentDto(c.Id, c.LanguageCode, c.Name, c.Address, c.Description, c.OperatingHours)).ToList(),
        p.Images.OrderBy(i => i.SortOrder).Select(i => new PoiImageDto(i.Id, i.ImageUrl, i.SortOrder, i.IsThumbnail)).ToList(),
        p.OperatingHours.Select(oh => new PoiOperatingHoursDto(oh.Id, oh.DayOfWeek, oh.OpenTime, oh.CloseTime)).ToList(),
        p.AudioFiles.OrderBy(a => a.SortOrder).Select(a => new PoiAudioDto(a.Id, a.LanguageCode, a.FileUrl, a.DurationSeconds, a.SortOrder, a.IsTts)).ToList(),
        null,
        p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : null
    );
}
