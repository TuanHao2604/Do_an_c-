using SQLite;
using SmartTourApp.Mobile.Models;

namespace SmartTourApp.Mobile.Services;

/// <summary>
/// Local SQLite database service — fully offline, no API needed.
/// Seeds mock data for Linh Ung Da Nang on first run.
/// </summary>
public class LocalDbService
{
    private SQLiteAsyncConnection? _db;

    private async Task InitAsync()
    {
        if (_db is not null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "smarttour.db");
        _db = new SQLiteAsyncConnection(dbPath);
        await _db.CreateTableAsync<LocalPoi>();
        await _db.CreateTableAsync<LocalVisitLog>();
        await _db.CreateTableAsync<LocalLocationLog>();
        await _db.CreateTableAsync<LocalTour>();
        await _db.CreateTableAsync<LocalFavorite>();

        await SeedMockDataAsync();
    }

    // ── SEED ──────────────────────────────────────────────────────────
    private async Task SeedMockDataAsync()
    {
        var count = await _db!.Table<LocalPoi>().CountAsync();
        if (count > 0) return;

        var pois = new List<LocalPoi>
        {
            new()
            {
                Id = "poi-chanh-dien",
                Name = "Chánh Điện",
                CategoryName = "Điểm đến",
                Address = "Chùa Linh Ứng, Bãi Bụt, Đà Nẵng",
                Description = "Khu chánh điện khang trang rộng lớn với nhiều tượng Phật bề thế. Đây là trung tâm tâm linh của chùa Linh Ứng.",
                Latitude = 16.0998, Longitude = 108.2775,
                GeofenceRadius = 50, IsActive = true, IsFeatured = true,
                QrValue = "CHANH-DIEN-001",
                UpdatedAt = DateTime.UtcNow, SyncedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = "poi-thu-vien",
                Name = "Thư Viện",
                CategoryName = "Điểm đến",
                Address = "Chùa Linh Ứng, Bãi Bụt, Đà Nẵng",
                Description = "Thư viện lưu trữ nhiều kinh sách quý của Phật giáo, nơi tĩnh lặng thích hợp cho thiền định.",
                Latitude = 16.1005, Longitude = 108.2780,
                GeofenceRadius = 30, IsActive = true,
                QrValue = "THU-VIEN-002",
                UpdatedAt = DateTime.UtcNow, SyncedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = "poi-cong-tam-quan",
                Name = "Cổng Tam Quan",
                CategoryName = "Điểm đến",
                Address = "Chùa Linh Ứng, Bãi Bụt, Đà Nẵng",
                Description = "Cổng vào uy nghi mang đậm kiến trúc Phật giáo truyền thống, đánh dấu ranh giới linh thiêng.",
                Latitude = 16.0985, Longitude = 108.2768,
                GeofenceRadius = 40, IsActive = true, IsFeatured = true,
                QrValue = "TAM-QUAN-003",
                UpdatedAt = DateTime.UtcNow, SyncedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = "poi-tuong-phat",
                Name = "Tượng Phật Quan Âm",
                CategoryName = "Điểm đến",
                Address = "Chùa Linh Ứng, Bãi Bụt, Đà Nẵng",
                Description = "Tượng Phật Bà Quan Âm cao 67m, tượng Phật cao nhất Việt Nam, hướng nhìn ra biển Đông.",
                Latitude = 16.1010, Longitude = 108.2770,
                GeofenceRadius = 100, IsActive = true, IsFeatured = true,
                QrValue = "QUAN-AM-004",
                UpdatedAt = DateTime.UtcNow, SyncedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = "poi-toilet-1",
                Name = "Toilet khu vực A",
                CategoryName = "Toilet",
                Address = "Gần cổng Tam Quan, Chùa Linh Ứng",
                Description = "Nhà vệ sinh công cộng sạch sẽ, phục vụ du khách.",
                Latitude = 16.0990, Longitude = 108.2772,
                GeofenceRadius = 20, IsActive = true,
                UpdatedAt = DateTime.UtcNow, SyncedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = "poi-nghi-chan",
                Name = "Khu nghỉ chân",
                CategoryName = "Nghỉ chân",
                Address = "Bên cạnh vườn cây, Chùa Linh Ứng",
                Description = "Khu vực có ghế đá và mái che cho du khách nghỉ ngơi, ngắm cảnh.",
                Latitude = 16.1002, Longitude = 108.2778,
                GeofenceRadius = 30, IsActive = true,
                UpdatedAt = DateTime.UtcNow, SyncedAt = DateTime.UtcNow,
            },
        };
        await _db.InsertAllAsync(pois);

        var tours = new List<LocalTour>
        {
            new()
            {
                Id = "tour-co-ban",
                Name = "Tour Linh Ứng cơ bản",
                Description = "Tham quan các điểm chính của chùa Linh Ứng: Cổng Tam Quan, Chánh Điện và Tượng Phật Quan Âm.",
                DurationMinutes = 105,
                PoiIds = "poi-cong-tam-quan,poi-chanh-dien,poi-tuong-phat",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = "tour-dao-mat",
                Name = "Tour Dạo mát",
                Description = "Khám phá toàn bộ khuôn viên chùa Linh Ứng bao gồm thư viện và khu nghỉ chân.",
                DurationMinutes = 140,
                PoiIds = "poi-cong-tam-quan,poi-chanh-dien,poi-thu-vien,poi-tuong-phat,poi-nghi-chan",
                CreatedAt = DateTime.UtcNow,
            },
        };
        await _db.InsertAllAsync(tours);

        // Seed some visit logs so stats work
        var visitLogs = new List<LocalVisitLog>
        {
            new() { PoiId = "poi-chanh-dien",     PoiName = "Chánh Điện",        VisitedAt = DateTime.UtcNow.AddHours(-2), NarrationSeconds = 85,  TriggeredByQr = false },
            new() { PoiId = "poi-tuong-phat",     PoiName = "Tượng Phật Quan Âm", VisitedAt = DateTime.UtcNow.AddHours(-1), NarrationSeconds = 120, TriggeredByQr = true  },
            new() { PoiId = "poi-cong-tam-quan",  PoiName = "Cổng Tam Quan",      VisitedAt = DateTime.UtcNow.AddMinutes(-40), NarrationSeconds = 60, TriggeredByQr = false },
            new() { PoiId = "poi-chanh-dien",     PoiName = "Chánh Điện",        VisitedAt = DateTime.UtcNow.AddMinutes(-20), NarrationSeconds = 90,  TriggeredByQr = false },
        };
        await _db.InsertAllAsync(visitLogs);

        // Seed location logs for heatmap
        var locationLogs = new List<LocalLocationLog>
        {
            new() { Latitude = 16.0998, Longitude = 108.2775, CellKey = "16.10,108.28", RouteSessionId = "seed", LoggedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Latitude = 16.0985, Longitude = 108.2768, CellKey = "16.10,108.28", RouteSessionId = "seed", LoggedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Latitude = 16.1010, Longitude = 108.2770, CellKey = "16.10,108.28", RouteSessionId = "seed", LoggedAt = DateTime.UtcNow.AddMinutes(-40) },
            new() { Latitude = 16.1005, Longitude = 108.2780, CellKey = "16.10,108.28", RouteSessionId = "seed", LoggedAt = DateTime.UtcNow.AddMinutes(-20) },
        };
        await _db.InsertAllAsync(locationLogs);
    }

    // ── POI ────────────────────────────────────────────────────────────
    public async Task<List<LocalPoi>> GetAllPoisAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalPoi>().Where(p => p.IsActive).ToListAsync();
    }

    public async Task<List<LocalPoi>> GetPoisByCategoryAsync(string category)
    {
        await InitAsync();
        return await _db!.Table<LocalPoi>()
            .Where(p => p.IsActive && p.CategoryName == category)
            .ToListAsync();
    }

    public async Task<LocalPoi?> GetPoiByIdAsync(string id)
    {
        await InitAsync();
        return await _db!.Table<LocalPoi>().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task SavePoiAsync(LocalPoi poi)
    {
        await InitAsync();
        var existing = await _db!.Table<LocalPoi>().FirstOrDefaultAsync(p => p.Id == poi.Id);
        if (existing is not null)
            await _db.UpdateAsync(poi);
        else
            await _db.InsertAsync(poi);
    }

    public async Task SavePoisAsync(List<LocalPoi> pois)
    {
        await InitAsync();
        foreach (var poi in pois) await SavePoiAsync(poi);
    }

    public async Task<DateTime> GetLastSyncTimeAsync()
    {
        await InitAsync();
        var latest = await _db!.Table<LocalPoi>()
            .OrderByDescending(p => p.SyncedAt)
            .FirstOrDefaultAsync();
        return latest?.SyncedAt ?? DateTime.MinValue;
    }

    public async Task<LocalPoi?> GetPoiByQrValueAsync(string qrValue)
    {
        await InitAsync();
        return await _db!.Table<LocalPoi>().FirstOrDefaultAsync(p => p.QrValue == qrValue);
    }

    // ── TOURS ──────────────────────────────────────────────────────────
    public async Task<List<LocalTour>> GetAllToursAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalTour>().ToListAsync();
    }

    public async Task SaveTourAsync(LocalTour tour)
    {
        await InitAsync();
        var existing = await _db!.Table<LocalTour>().FirstOrDefaultAsync(t => t.Id == tour.Id);
        if (existing is not null)
            await _db.UpdateAsync(tour);
        else
            await _db.InsertAsync(tour);
    }

    // ── FAVORITES ──────────────────────────────────────────────────────
    public async Task<List<LocalFavorite>> GetFavoritesAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalFavorite>().OrderByDescending(f => f.AddedAt).ToListAsync();
    }

    public async Task<bool> IsFavoriteAsync(string poiId)
    {
        await InitAsync();
        var fav = await _db!.Table<LocalFavorite>().FirstOrDefaultAsync(f => f.PoiId == poiId);
        return fav is not null;
    }

    public async Task ToggleFavoriteAsync(LocalPoi poi)
    {
        await InitAsync();
        var existing = await _db!.Table<LocalFavorite>().FirstOrDefaultAsync(f => f.PoiId == poi.Id);
        if (existing is not null)
        {
            await _db.DeleteAsync(existing);
        }
        else
        {
            await _db.InsertAsync(new LocalFavorite
            {
                PoiId = poi.Id,
                PoiName = poi.Name,
                CategoryName = poi.CategoryName,
                Address = poi.Address,
                AddedAt = DateTime.UtcNow,
            });
        }
    }

    public async Task<int> GetFavoritesCountAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalFavorite>().CountAsync();
    }

    public async Task ToggleFavoriteAsync(string poiId)
    {
        await InitAsync();
        var existing = await _db!.Table<LocalFavorite>().FirstOrDefaultAsync(f => f.PoiId == poiId);
        if (existing is not null)
        {
            await _db.DeleteAsync(existing);
        }
        else
        {
            var poi = await GetPoiByIdAsync(poiId);
            if (poi is not null)
            {
                await _db.InsertAsync(new LocalFavorite
                {
                    PoiId = poi.Id,
                    PoiName = poi.Name,
                    CategoryName = poi.CategoryName,
                    Address = poi.Address,
                    AddedAt = DateTime.UtcNow,
                });
            }
        }
    }

    public async Task RemoveFavoriteAsync(string poiId)
    {
        await InitAsync();
        var existing = await _db!.Table<LocalFavorite>().FirstOrDefaultAsync(f => f.PoiId == poiId);
        if (existing is not null)
        {
            await _db.DeleteAsync(existing);
        }
    }

    // ── VISIT & LOCATION LOGS ──────────────────────────────────────────
    public async Task LogVisitAsync(LocalVisitLog log)
    {
        await InitAsync();
        await _db!.InsertAsync(log);
    }

    public async Task LogLocationAsync(LocalLocationLog log)
    {
        await InitAsync();
        await _db!.InsertAsync(log);
    }

    public async Task<List<LocalVisitLog>> GetVisitLogsAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalVisitLog>().OrderByDescending(l => l.VisitedAt).ToListAsync();
    }

    public async Task<int> GetVisitedPlacesCountAsync()
    {
        var logs = await GetVisitLogsAsync();
        return logs.Select(l => l.PoiId).Distinct().Count();
    }

    public async Task<List<LocalLocationLog>> GetLocationLogsAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalLocationLog>().OrderByDescending(l => l.LoggedAt).ToListAsync();
    }

    public async Task<LocalLocationLog?> GetLatestLocationLogAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalLocationLog>()
            .OrderByDescending(l => l.LoggedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TopPoiStat>> GetTopVisitedPoisAsync(int limit)
    {
        var logs = await GetVisitLogsAsync();
        return logs
            .GroupBy(l => new { l.PoiId, l.PoiName })
            .Select(g => new TopPoiStat { PoiId = g.Key.PoiId, PoiName = g.Key.PoiName, VisitCount = g.Count() })
            .OrderByDescending(x => x.VisitCount)
            .Take(limit)
            .ToList();
    }

    public async Task<double> GetAverageNarrationSecondsAsync()
    {
        var logs = await GetVisitLogsAsync();
        var durations = logs
            .Where(l => l.NarrationSeconds is > 0)
            .Select(l => l.NarrationSeconds!.Value)
            .ToList();
        return durations.Count == 0 ? 0 : durations.Average();
    }

    public async Task<List<HeatmapCellStat>> GetHeatmapAsync(int limit)
    {
        var logs = await GetLocationLogsAsync();
        return logs
            .GroupBy(l => l.CellKey)
            .Select(g => new HeatmapCellStat { CellKey = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToList();
    }

    public async Task ClearVisitHistoryAsync()
    {
        await InitAsync();
        await _db!.DeleteAllAsync<LocalVisitLog>();
    }
}
