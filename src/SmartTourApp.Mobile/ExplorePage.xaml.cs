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
    private LocalPoi? _featuredPoi;
    private IDispatcherTimer? _trackingTimer;
    private string _routeSessionId = string.Empty;
    private bool _useBackgroundService;
    private bool _isAutoPlayEnabled = true;
    private bool _isAudioPlaying;
    private string _selectedCategory = "Tất cả";
    private List<LocalPoi> _allPois = new();

    private static readonly string[] DefaultCategories =
    {
        "Tất cả", "Di tích", "Nhà hàng", "Cà phê", "Khách sạn", "Tham quan"
    };

    public ExplorePage()
    {
        InitializeComponent();

        var services = MauiApplication.Current.Services;
        _localDb = services.GetService<LocalDbService>() ?? new LocalDbService();
        _syncService = services.GetService<SyncApiService>()
            ?? new SyncApiService(new HttpClient { BaseAddress = new Uri("http://10.0.2.2:5001/") }, _localDb);

        BuildCategoryChips();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPoisAsync();
        await RefreshStatsAsync();
        await UpdateHeatmapAsync();
    }

    // ── Category Chips ──────────────────────────────────────────────────

    private void BuildCategoryChips()
    {
        CategoryChips.Children.Clear();
        foreach (var cat in DefaultCategories)
        {
            var isActive = cat == _selectedCategory;
            var btn = new Button
            {
                Text = cat,
                FontSize = 12,
                FontFamily = "UbuntuMedium",
                HeightRequest = 34,
                CornerRadius = 17,
                Padding = new Thickness(14, 0),
                BackgroundColor = isActive
                    ? Color.FromArgb("#0E6EB8")
                    : Color.FromArgb("#140E6EB8"),
                TextColor = isActive
                    ? Colors.White
                    : Color.FromArgb("#0E6EB8"),
            };
            btn.Clicked += OnCategoryClicked;
            CategoryChips.Children.Add(btn);
        }
    }

    private async void OnCategoryClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            _selectedCategory = btn.Text;
            BuildCategoryChips();
            await ApplyFiltersAsync();
        }
    }

    private async Task ApplyFiltersAsync()
    {
        if (_selectedCategory == "Tất cả")
        {
            BindableLayout.SetItemsSource(PoiListLayout, _allPois);
            PoiEmptyView.IsVisible = _allPois.Count == 0;
            PoiListLayout.IsVisible = _allPois.Count > 0;
        }
        else
        {
            var filtered = _allPois
                .Where(p => string.Equals(p.CategoryName, _selectedCategory, StringComparison.OrdinalIgnoreCase))
                .ToList();
            BindableLayout.SetItemsSource(PoiListLayout, filtered);
            PoiEmptyView.IsVisible = filtered.Count == 0;
            PoiListLayout.IsVisible = filtered.Count > 0;
        }

        UpdateFeaturedPoi();
        await Task.CompletedTask;
    }

    // ── Featured POI ────────────────────────────────────────────────────

    private void UpdateFeaturedPoi()
    {
        _featuredPoi = _allPois.FirstOrDefault(p => p.IsFeatured) ?? _allPois.FirstOrDefault();

        if (_featuredPoi != null)
        {
            FeaturedSection.IsVisible = true;
            FeaturedPoiName.Text = _featuredPoi.Name;
            FeaturedPoiCategory.Text = _featuredPoi.CategoryName;
        }
        else
        {
            FeaturedSection.IsVisible = false;
        }
    }

    private async void OnFeaturedPoiTapped(object? sender, TappedEventArgs e)
    {
        if (_featuredPoi is not null)
        {
            await Navigation.PushAsync(new PoiDetailPage(_featuredPoi, _localDb));
        }
    }

    // ── Audio Banner ────────────────────────────────────────────────────

    private void ShowAudioBanner(LocalPoi poi)
    {
        AudioBannerPoiName.Text = poi.Name;
        AudioBanner.IsVisible = true;
        _isAudioPlaying = true;
        AudioPlayPauseIcon.Text = "⏸";
    }

    private void OnAudioPlayPause(object? sender, TappedEventArgs e)
    {
        _isAudioPlaying = !_isAudioPlaying;
        AudioPlayPauseIcon.Text = _isAudioPlaying ? "⏸" : "▶";
    }

    private async void OnAudioReplay(object? sender, TappedEventArgs e)
    {
        if (_nearestPoi is not null)
        {
            await NarratePoiAsync(_nearestPoi, triggeredByQr: false);
        }
    }

    private void OnAudioClose(object? sender, TappedEventArgs e)
    {
        AudioBanner.IsVisible = false;
        _isAudioPlaying = false;
    }

    private void OnAutoPlayToggled(object? sender, ToggledEventArgs e)
    {
        _isAutoPlayEnabled = e.Value;
    }

    // ── Language ─────────────────────────────────────────────────────────

    private async void OnLanguageTapped(object? sender, TappedEventArgs e)
    {
        var action = await DisplayActionSheet("Chọn ngôn ngữ", "Đóng", null,
            "Tiếng Việt", "English", "한국어");
        if (!string.IsNullOrEmpty(action) && action != "Đóng")
        {
            await DisplayAlert("Ngôn ngữ", $"Đã chọn: {action}", "OK");
        }
    }

    // ── Data Loading ────────────────────────────────────────────────────

    private async Task LoadPoisAsync()
    {
        try
        {
            _allPois = await _localDb.GetAllPoisAsync();
            await ApplyFiltersAsync();
            SyncStatusLabel.Text = $"POI: {_allPois.Count} | Sync: {(await _localDb.GetLastSyncTimeAsync()):g}";

            UpdateFeaturedPoi();

            var location = await GetCurrentLocationAsync();
            if (location is not null)
            {
                await UpdateNearestPoiAsync(location, _allPois);
            }
            else
            {
                NearestPoiCard.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            SyncStatusLabel.Text = $"Lỗi: {ex.Message}";
        }
    }

    private async Task RefreshStatsAsync()
    {
        var topVisited = await _localDb.GetTopVisitedPoisAsync(5);
        BindableLayout.SetItemsSource(TopVisitedList, topVisited);

        var avg = await _localDb.GetAverageNarrationSecondsAsync();
        AverageNarrationLabel.Text = avg > 0
            ? $"Thời gian thuyết minh trung bình: {avg:0.#}s"
            : "Thời gian thuyết minh trung bình: --";
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
        NearestPoiDistance.Text = $"Cách: {nearest.Distance:0.0} km";
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
        if (allPois.Count == 0) return;

        var inRange = allPois
            .Select(p => new
            {
                Poi = p,
                DistanceKm = GeoUtils.HaversineKm(location.Latitude, location.Longitude, p.Latitude, p.Longitude)
            })
            .Where(x => x.Poi.GeofenceRadius > 0 && x.DistanceKm * 1000 <= x.Poi.GeofenceRadius)
            .OrderBy(x => x.DistanceKm)
            .FirstOrDefault();

        if (inRange is null) return;

        var lastTime = _lastTrigger.TryGetValue(inRange.Poi.Id, out var value) ? value : DateTime.MinValue;
        if (DateTime.UtcNow - lastTime < TimeSpan.FromMinutes(10)) return;

        _lastTrigger[inRange.Poi.Id] = DateTime.UtcNow;

        if (_isAutoPlayEnabled)
        {
            ShowAudioBanner(inRange.Poi);
            await NarratePoiAsync(inRange.Poi, triggeredByQr: false);
        }
        else
        {
            ShowAudioBanner(inRange.Poi);
            _isAudioPlaying = false;
            AudioPlayPauseIcon.Text = "▶";
        }
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
        if (location is null) return;

        await LogLocationAsync(location);
        await CheckGeofenceAndTriggerAsync(location);
        await UpdateNearestPoiAsync(location);
        await UpdateHeatmapAsync();
    }

    // ── Event Handlers ──────────────────────────────────────────────────

    private async void OnSyncClicked(object? sender, EventArgs e)
    {
        SyncStatusLabel.Text = "Đang đồng bộ...";
        try
        {
            var count = await _syncService.SyncAsync();
            if (count >= 0)
            {
                SyncStatusLabel.Text = $"Đã đồng bộ {count} POI";
                await LoadPoisAsync();
            }
            else
            {
                SyncStatusLabel.Text = "Không thể kết nối server (dùng dữ liệu offline)";
            }
        }
        catch (Exception ex)
        {
            SyncStatusLabel.Text = $"Lỗi sync: {ex.Message}";
        }
    }

    private async void OnNearbyClicked(object? sender, EventArgs e)
    {
        try
        {
            var location = await GetCurrentLocationAsync();
            if (location is null)
            {
                await DisplayAlert("GPS", "Không lấy được vị trí", "OK");
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
            SyncStatusLabel.Text = $"Gần bạn: {nearby.Count} POI (bán kính 5km)";

            await LogLocationAsync(location);
            await CheckGeofenceAndTriggerAsync(location, pois);
            await UpdateNearestPoiAsync(location, pois);
            await UpdateHeatmapAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", ex.Message, "OK");
        }
    }

    private async void OnPoiTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is LocalPoi poi)
        {
            await Navigation.PushAsync(new PoiDetailPage(poi, _localDb));
        }
    }

    private async void OnSearchTapped(object? sender, TappedEventArgs e)
    {
        var query = await DisplayPromptAsync("Tìm kiếm", "Nhập tên điểm đến", "Tìm", "Hủy");
        if (!string.IsNullOrWhiteSpace(query))
        {
            var results = _allPois
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            BindableLayout.SetItemsSource(PoiListLayout, results);
            PoiEmptyView.IsVisible = results.Count == 0;
            PoiListLayout.IsVisible = results.Count > 0;
            SyncStatusLabel.Text = $"Kết quả: {results.Count} POI";
        }
    }

    private async void OnQrClicked(object? sender, EventArgs e)
    {
        var qrValue = await DisplayPromptAsync("Quét QR", "Nhập mã QR", "OK", "Hủy");
        if (string.IsNullOrWhiteSpace(qrValue)) return;

        var poi = await _localDb.GetPoiByQrValueAsync(qrValue.Trim());
        if (poi is null)
        {
            await DisplayAlert("QR", "Không tìm thấy POI từ QR.", "OK");
            return;
        }

        await DisplayAlert("QR", $"Kích hoạt nội dung: {poi.Name}", "OK");
        await NarratePoiAsync(poi, triggeredByQr: true);
    }

    private void OnVoiceSelected(object? sender, EventArgs e)
    {
        var buttons = new[] { VoiceNorthBtn, VoiceCentralBtn, VoiceSouthBtn };
        foreach (var btn in buttons)
        {
            btn.BackgroundColor = Color.FromArgb("#F8FAFD");
            btn.TextColor = Color.FromArgb("#6B7280");
        }
        if (sender is Button selected)
        {
            selected.BackgroundColor = Color.FromArgb("#0E6EB8");
            selected.TextColor = Colors.White;
        }
    }

    private async void OnCreateTourClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Tour", "Tính năng tạo Tour sẽ sớm có.", "OK");
    }

    private void OnTourTapped(object? sender, TappedEventArgs e)
    {
        // Navigate to tour detail
    }

    private async void OnPremiumTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Premium", "Gói Premium sẽ sớm có.", "OK");
    }

    private async void OnNearestNarrateClicked(object? sender, EventArgs e)
    {
        if (_nearestPoi is not null)
        {
            ShowAudioBanner(_nearestPoi);
            await NarratePoiAsync(_nearestPoi, triggeredByQr: false);
        }
    }

    // ── Tracking ────────────────────────────────────────────────────────

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
                await DisplayAlert("GPS", "Cần quyền vị trí nền để theo dõi.", "OK");
                TrackingSwitch.IsToggled = false;
                return;
            }

            _routeSessionId = Guid.NewGuid().ToString("N");
            Preferences.Set("route_session_id", _routeSessionId);
            Preferences.Set("tracking_enabled", true);
            RouteSessionLabel.Text = $"Đang theo dõi (ẩn danh): {_routeSessionId[..6]}";

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
            RouteSessionLabel.Text = "Đã tắt theo dõi";
        }
    }

    private static string BuildCellKey(double lat, double lng)
    {
        var latKey = Math.Round(lat, 2).ToString("0.00", CultureInfo.InvariantCulture);
        var lngKey = Math.Round(lng, 2).ToString("0.00", CultureInfo.InvariantCulture);
        return $"{latKey},{lngKey}";
    }
}
