using SmartTourApp.Mobile.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartTourApp.Mobile.Services;

/// <summary>
/// Sync service: pulls changed POIs from server → saves to local SQLite.
/// One-directional sync (server → mobile only).
/// </summary>
public class SyncApiService
{
    private readonly HttpClient _httpClient;
    private readonly LocalDbService _localDb;

    public SyncApiService(HttpClient httpClient, LocalDbService localDb)
    {
        _httpClient = httpClient;
        _localDb = localDb;
    }

    /// <summary>
    /// Pull changed POIs from server since last sync.
    /// </summary>
    public async Task<int> SyncAsync()
    {
        try
        {
            var lastSync = await _localDb.GetLastSyncTimeAsync();
            var url = $"api/sync/pois?lastUpdated={lastSync:o}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var syncResult = JsonSerializer.Deserialize<SyncResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (syncResult?.Pois is null || syncResult.Pois.Count == 0)
                return 0;

            var localPois = syncResult.Pois.Select(p =>
            {
                var viContent = p.Contents?.FirstOrDefault(c => c.LanguageCode == "vi");
                var enContent = p.Contents?.FirstOrDefault(c => c.LanguageCode == "en");
                var thumbnail = p.Images?.FirstOrDefault(i => i.IsThumbnail)?.ImageUrl
                    ?? p.Images?.FirstOrDefault()?.ImageUrl;

                return new LocalPoi
                {
                    Id = p.Id.ToString(),
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    GeofenceRadius = p.GeofenceRadius,
                    QrValue = p.QrValue,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CategoryName = p.CategoryName ?? "",
                    Name = viContent?.Name ?? enContent?.Name ?? "Unknown",
                    NameEn = enContent?.Name,
                    Address = viContent?.Address ?? enContent?.Address,
                    Description = viContent?.Description ?? enContent?.Description,
                    ThumbnailUrl = thumbnail,
                    UpdatedAt = p.UpdatedAt,
                    SyncedAt = DateTime.UtcNow,
                };
            }).ToList();

            await _localDb.SavePoisAsync(localPois);
            return localPois.Count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sync error: {ex.Message}");
            return -1;
        }
    }
}

// ── Response models matching API DTOs ──
public class SyncResponse
{
    public DateTime ServerTime { get; set; }
    public List<SyncPoiItem> Pois { get; set; } = new();
}

public class SyncPoiItem
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double GeofenceRadius { get; set; }
    public string? QrValue { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? CategoryName { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SyncContentItem>? Contents { get; set; }
    public List<SyncImageItem>? Images { get; set; }
}

public class SyncContentItem
{
    public string LanguageCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public string? Description { get; set; }
}

public class SyncImageItem
{
    public string ImageUrl { get; set; } = "";
    public bool IsThumbnail { get; set; }
}
