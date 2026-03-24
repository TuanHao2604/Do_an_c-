using SQLite;
using SmartTourApp.Mobile.Models;

namespace SmartTourApp.Mobile.Services;

/// <summary>
/// Local SQLite database service for offline data.
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
    }

    public async Task<List<LocalPoi>> GetAllPoisAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalPoi>().Where(p => p.IsActive).ToListAsync();
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
        foreach (var poi in pois)
        {
            await SavePoiAsync(poi);
        }
    }

    public async Task<DateTime> GetLastSyncTimeAsync()
    {
        await InitAsync();
        var latest = await _db!.Table<LocalPoi>()
            .OrderByDescending(p => p.SyncedAt)
            .FirstOrDefaultAsync();
        return latest?.SyncedAt ?? DateTime.MinValue;
    }

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

    public async Task<LocalPoi?> GetPoiByQrValueAsync(string qrValue)
    {
        await InitAsync();
        return await _db!.Table<LocalPoi>().FirstOrDefaultAsync(p => p.QrValue == qrValue);
    }

    public async Task<List<LocalVisitLog>> GetVisitLogsAsync()
    {
        await InitAsync();
        return await _db!.Table<LocalVisitLog>().OrderByDescending(l => l.VisitedAt).ToListAsync();
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
            .Select(g => new TopPoiStat
            {
                PoiId = g.Key.PoiId,
                PoiName = g.Key.PoiName,
                VisitCount = g.Count()
            })
            .OrderByDescending(x => x.VisitCount)
            .Take(limit)
            .ToList();
    }

    public async Task<double> GetAverageNarrationSecondsAsync()
    {
        var logs = await GetVisitLogsAsync();
        var durations = logs
            .Where(l => l.NarrationSeconds.HasValue && l.NarrationSeconds.Value > 0)
            .Select(l => l.NarrationSeconds!.Value)
            .ToList();
        return durations.Count == 0 ? 0 : durations.Average();
    }

    public async Task<List<HeatmapCellStat>> GetHeatmapAsync(int limit)
    {
        var logs = await GetLocationLogsAsync();
        return logs
            .GroupBy(l => l.CellKey)
            .Select(g => new HeatmapCellStat
            {
                CellKey = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToList();
    }
}
