using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using SmartTourApp.Mobile.Models;
using SmartTourApp.Mobile.Services;

namespace SmartTourApp.Mobile;

public partial class ExplorePage : ContentPage
{
    private readonly LocalDbService _localDb;
    private readonly SyncApiService _syncService;
    private readonly Dictionary<string, DateTime> _lastTrigger = new();
    private LocalPoi? _nearestPoi;
    private IDispatcherTimer? _trackingTimer;
    private string _routeSessionId = string.Empty;
    private bool _useBackgroundService;

    public ExplorePage()
    {
        InitializeComponent();

        var services = MauiApplication.Current.Services;
        _localDb = services.GetService<LocalDbService>() ?? new LocalDbService();
        _syncService = services.GetService<SyncApiService>()
            ?? new SyncApiService(new HttpClient { BaseAddress = new Uri("http://10.0.2.2:5001/") }, _localDb);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPoisAsync();
        await RefreshStatsAsync();
        await UpdateHeatmapAsync();
    }

    private async Task LoadPoisAsync()
    {
        try
        {
            var pois = await _localDb.GetAllPoisAsync();
            BindableLayout.SetItemsSource(PoiListLayout, pois);
            PoiEmptyView.IsVisible = pois.Count == 0;
            PoiListLayout.IsVisible = pois.Count > 0;
            SyncStatusLabel.Text = $"POI: {pois.Count} | Sync: {(await _localDb.GetLastSyncTimeAsync()):g}";

            var location = await GetCurrentLocationAsync();
            if (location is not null)
            {
                await UpdateNearestPoiAsync(location, pois);
            }
            else
            {
                NearestPoiCard.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            SyncStatusLabel.Text = $"Loi: {ex.Message}";
        }
    }

    private async Task RefreshStatsAsync()
    {
        var topVisited = await _localDb.GetTopVisitedPoisAsync(5);
        BindableLayout.SetItemsSource(TopVisitedList, topVisited);

        var avg = await _localDb.GetAverageNarrationSecondsAsync();
        AverageNarrationLabel.Text = avg > 0
            ? $"Thoi gian thuyet minh trung binh: {avg:0.#}s"
            : "Thoi gian thuyet minh trung binh: --";
    }

    private async Task UpdateHeatmapAsync()
    {
        var heatmap = await _localDb.GetHeatmapAsync(6);
        BindableLayout.SetItemsSource(HeatmapList, heatmap);
        HeatmapEmptyLabel.IsVisible = heatmap.Count == 0;
    }

    private async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            if (status != PermissionStatus.Granted)
            {
                return null;
            }

            return await Geolocation.GetLastKnownLocationAsync()
                   ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
        }
        catch
        {
            return null;
        }
    }

    private async Task UpdateNearestPoiAsync(Location location, IReadOnlyList<LocalPoi>? pois = null)
    {
        var allPois = pois ?? await _localDb.GetAllPoisAsync();
        if (allPois.Count == 0)
        {
            NearestPoiCard.IsVisible = false;
            return;
        }

        var nearest = allPois
            .Select(p => new { Poi = p, Distance = GeoUtils.HaversineKm(location.Latitude, location.Longitude, p.Latitude, p.Longitude) })
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        if (nearest is null)
        {
            NearestPoiCard.IsVisible = false;
            return;
        }

        _nearestPoi = nearest.Poi;
        NearestPoiName.Text = nearest.Poi.Name;
        NearestPoiDistance.Text = $"Cach: {nearest.Distance:0.0} km";
        NearestPoiCard.IsVisible = true;
    }

    private async Task LogLocationAsync(Location location)
    {
        var cellKey = BuildCellKey(location.Latitude, location.Longitude);
        var log = new LocalLocationLog
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            CellKey = cellKey,
            RouteSessionId = _routeSessionId,
            LoggedAt = DateTime.UtcNow
        };
        await _localDb.LogLocationAsync(log);
    }

    private async Task CheckGeofenceAndTriggerAsync(Location location, IReadOnlyList<LocalPoi>? pois = null)
    {
        var allPois = pois ?? await _localDb.GetAllPoisAsync();
        if (allPois.Count == 0)
        {
            return;
        }

        var inRange = allPois
            .Select(p => new
            {
                Poi = p,
                DistanceKm = GeoUtils.HaversineKm(location.Latitude, location.Longitude, p.Latitude, p.Longitude)
            })
            .Where(x => x.Poi.GeofenceRadius > 0 && x.DistanceKm * 1000 <= x.Poi.GeofenceRadius)
            .OrderBy(x => x.DistanceKm)
            .FirstOrDefault();

        if (inRange is null)
        {
            return;
        }

        var lastTime = _lastTrigger.TryGetValue(inRange.Poi.Id, out var value) ? value : DateTime.MinValue;
        if (DateTime.UtcNow - lastTime < TimeSpan.FromMinutes(10))
        {
            return;
        }

        _lastTrigger[inRange.Poi.Id] = DateTime.UtcNow;
        await DisplayAlert("Da den POI", $"Ban da vao khu vuc: {inRange.Poi.Name}", "OK");
        await NarratePoiAsync(inRange.Poi, triggeredByQr: false);
    }

    private async Task NarratePoiAsync(LocalPoi poi, bool triggeredByQr)
    {
        var text = poi.Description;
        if (string.IsNullOrWhiteSpace(text))
        {
            text = poi.Name;
        }

        var sw = Stopwatch.StartNew();
        await TextToSpeech.SpeakAsync(text);
        sw.Stop();

        await _localDb.LogVisitAsync(new LocalVisitLog
        {
            PoiId = poi.Id,
            PoiName = poi.Name,
            VisitedAt = DateTime.UtcNow,
            NarrationSeconds = sw.Elapsed.TotalSeconds,
            TriggeredByQr = triggeredByQr
        });

        await RefreshStatsAsync();
    }

    private async Task CaptureAndProcessLocationAsync()
    {
        var location = await GetCurrentLocationAsync();
        if (location is null)
        {
            return;
        }

        await LogLocationAsync(location);
        await CheckGeofenceAndTriggerAsync(location);
        await UpdateNearestPoiAsync(location);
        await UpdateHeatmapAsync();
    }

    private async void OnSyncClicked(object? sender, EventArgs e)
    {
        SyncStatusLabel.Text = "Dang dong bo...";

        try
        {
            var count = await _syncService.SyncAsync();
            if (count >= 0)
            {
                SyncStatusLabel.Text = $"Da dong bo {count} POI";
                await LoadPoisAsync();
            }
            else
            {
                SyncStatusLabel.Text = "Khong the ket noi server (dung du lieu offline)";
            }
        }
        catch (Exception ex)
        {
            SyncStatusLabel.Text = $"Loi sync: {ex.Message}";
        }
    }

    private async void OnNearbyClicked(object? sender, EventArgs e)
    {
        try
        {
            var location = await GetCurrentLocationAsync();
            if (location is null)
            {
                await DisplayAlert("GPS", "Khong lay duoc vi tri", "OK");
                return;
            }

            var pois = await _localDb.GetAllPoisAsync();
            var nearby = pois
                .Select(p => new { Poi = p, Distance = GeoUtils.HaversineKm(location.Latitude, location.Longitude, p.Latitude, p.Longitude) })
                .Where(x => x.Distance <= 5)
                .OrderBy(x => x.Distance)
                .Select(x => x.Poi)
                .ToList();

            BindableLayout.SetItemsSource(PoiListLayout, nearby);
            PoiEmptyView.IsVisible = nearby.Count == 0;
            PoiListLayout.IsVisible = nearby.Count > 0;
            SyncStatusLabel.Text = $"Gan ban: {nearby.Count} POI (ban kinh 5km)";

            await LogLocationAsync(location);
            await CheckGeofenceAndTriggerAsync(location, pois);
            await UpdateNearestPoiAsync(location, pois);
            await UpdateHeatmapAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Loi", ex.Message, "OK");
        }
    }

    private async void OnPoiTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is LocalPoi poi)
        {
            var action = await DisplayActionSheet(
                poi.Name,
                "Dong",
                null,
                "Nghe thuyet minh",
                "Danh dau da den");

            if (action == "Nghe thuyet minh")
            {
                await NarratePoiAsync(poi, triggeredByQr: false);
            }
            else if (action == "Danh dau da den")
            {
                await _localDb.LogVisitAsync(new LocalVisitLog
                {
                    PoiId = poi.Id,
                    PoiName = poi.Name,
                    VisitedAt = DateTime.UtcNow,
                    NarrationSeconds = null,
                    TriggeredByQr = false
                });
                await RefreshStatsAsync();
            }
        }
    }

    private async void OnSearchTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Search", "Tinh nang tim kiem se som co.", "OK");
    }

    private async void OnPremiumTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Premium", "Goi Premium se som co.", "OK");
    }

    private async void OnQrClicked(object? sender, EventArgs e)
    {
        var qrValue = await DisplayPromptAsync("Quet QR", "Nhap ma QR", "OK", "Huy");
        if (string.IsNullOrWhiteSpace(qrValue))
        {
            return;
        }

        var poi = await _localDb.GetPoiByQrValueAsync(qrValue.Trim());
        if (poi is null)
        {
            await DisplayAlert("QR", "Khong tim thay POI tu QR.", "OK");
            return;
        }

        await DisplayAlert("QR", $"Kich hoat noi dung: {poi.Name}", "OK");
        await NarratePoiAsync(poi, triggeredByQr: true);
    }

    private async Task RefreshFromLogsAsync()
    {
        await RefreshStatsAsync();
        await UpdateHeatmapAsync();
        var latest = await _localDb.GetLatestLocationLogAsync();
        if (latest is not null)
        {
            await UpdateNearestPoiAsync(new Location(latest.Latitude, latest.Longitude));
        }
    }

    private void StartTrackingTimer()
    {
        _trackingTimer = Dispatcher.CreateTimer();
        _trackingTimer.Interval = TimeSpan.FromSeconds(20);
        _trackingTimer.Tick += async (_, _) =>
        {
            if (_useBackgroundService)
            {
                await RefreshFromLogsAsync();
            }
            else
            {
                await CaptureAndProcessLocationAsync();
            }
        };
        _trackingTimer.Start();
    }

    private void StopTrackingTimer()
    {
        _trackingTimer?.Stop();
        _trackingTimer = null;
    }

    private static bool TryStartBackgroundService()
    {
#if ANDROID
        SmartTourApp.Mobile.Platforms.Android.Services.LocationForegroundServiceStarter.Start();
        return true;
#else
        return false;
#endif
    }

    private static void StopBackgroundService()
    {
#if ANDROID
        SmartTourApp.Mobile.Platforms.Android.Services.LocationForegroundServiceStarter.Stop();
#endif
    }

    private async void OnTrackingToggled(object? sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            }
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("GPS", "Can quyen vi tri nen de theo doi nen.", "OK");
                TrackingSwitch.IsToggled = false;
                return;
            }

            _routeSessionId = Guid.NewGuid().ToString("N");
            Preferences.Set("route_session_id", _routeSessionId);
            Preferences.Set("tracking_enabled", true);
            RouteSessionLabel.Text = $"Dang theo doi (an danh): {_routeSessionId[..6]}";

            _useBackgroundService = TryStartBackgroundService();
            StopTrackingTimer();
            StartTrackingTimer();

            if (_useBackgroundService)
            {
                await RefreshFromLogsAsync();
            }
            else
            {
                await CaptureAndProcessLocationAsync();
            }
        }
        else
        {
            Preferences.Set("route_session_id", string.Empty);
            Preferences.Set("tracking_enabled", false);
            _useBackgroundService = false;
            StopBackgroundService();
            StopTrackingTimer();
            RouteSessionLabel.Text = "Da tat theo doi";
        }
    }

    private async void OnNearestNarrateClicked(object? sender, EventArgs e)
    {
        if (_nearestPoi is not null)
        {
            await NarratePoiAsync(_nearestPoi, triggeredByQr: false);
        }
    }

    private static string BuildCellKey(double lat, double lng)
    {
        var latKey = Math.Round(lat, 2).ToString("0.00", CultureInfo.InvariantCulture);
        var lngKey = Math.Round(lng, 2).ToString("0.00", CultureInfo.InvariantCulture);
        return $"{latKey},{lngKey}";
    }

}
