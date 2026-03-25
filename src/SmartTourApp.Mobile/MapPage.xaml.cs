using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using SmartTourApp.Mobile.Models;
using SmartTourApp.Mobile.Services;
using MapControl = Microsoft.Maui.Controls.Maps.Map;

namespace SmartTourApp.Mobile;

public partial class MapPage : ContentPage
{
    private readonly LocalDbService _localDb;
    private readonly List<LocalPoi> _cachedPois = new();
    private readonly List<HeatmapCellStat> _cachedHeatmap = new();
    private Location? _lastLocation;
    private double _lastClusterSize;
    private bool _mapReady;
    private readonly Dictionary<Pin, ClusterInfo> _pinLookup = new();
    private LocalPoi? _selectedPoi;

    public MapPage()
    {
        InitializeComponent();
        _localDb = MauiApplication.Current.Services.GetService<LocalDbService>() ?? new LocalDbService();
        PoiMap.PropertyChanged += OnMapPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMapDataAsync();
    }

    private async Task LoadMapDataAsync()
    {
        _cachedPois.Clear();
        _cachedPois.AddRange(await _localDb.GetAllPoisAsync());
        _cachedHeatmap.Clear();
        _cachedHeatmap.AddRange(await _localDb.GetHeatmapAsync(12));

        var last = await _localDb.GetLatestLocationLogAsync();
        _lastLocation = last is null ? null : new Location(last.Latitude, last.Longitude);
        var center = _lastLocation ?? new Location(10.7764, 106.7009);

        var span = MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(4));
        PoiMap.MoveToRegion(span);
        UpdatePinsAndOverlays(span);
    }

    private void OnMapPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MapControl.VisibleRegion)) return;
        var span = PoiMap.VisibleRegion;
        if (span is null) return;

        var clusterSize = GetClusterSize(span);
        if (!_mapReady || Math.Abs(clusterSize - _lastClusterSize) > 0.0001)
        {
            _lastClusterSize = clusterSize;
            _mapReady = true;
            UpdatePinsAndOverlays(span);
        }
    }

    private void UpdatePinsAndOverlays(MapSpan span)
    {
        PoiMap.Pins.Clear();
        PoiMap.MapElements.Clear();
        _pinLookup.Clear();

        if (_cachedPois.Count > 0)
        {
            var clusterSize = GetClusterSize(span);
            var clusters = _cachedPois
                .GroupBy(p => BuildClusterKey(p.Latitude, p.Longitude, clusterSize))
                .Select(g => new
                {
                    Items = g.ToList(),
                    Lat = g.Average(x => x.Latitude),
                    Lng = g.Average(x => x.Longitude)
                })
                .ToList();

            foreach (var cluster in clusters)
            {
                if (cluster.Items.Count == 1)
                {
                    var poi = cluster.Items[0];
                    var pin = new Pin
                    {
                        Label = poi.Name,
                        Address = poi.CategoryName,
                        Type = PinType.Place,
                        Location = new Location(poi.Latitude, poi.Longitude)
                    };
                    pin.MarkerClicked += OnPinMarkerClicked;
                    PoiMap.Pins.Add(pin);
                    _pinLookup[pin] = new ClusterInfo
                    {
                        IsCluster = false,
                        Center = pin.Location,
                        PoiId = poi.Id,
                    };
                    continue;
                }

                var nameList = string.Join(", ", cluster.Items.Take(3).Select(x => x.Name));
                var clusterPin = new Pin
                {
                    Label = $"{cluster.Items.Count} POI",
                    Address = nameList,
                    Type = PinType.Generic,
                    Location = new Location(cluster.Lat, cluster.Lng)
                };
                clusterPin.MarkerClicked += OnPinMarkerClicked;
                PoiMap.Pins.Add(clusterPin);
                _pinLookup[clusterPin] = new ClusterInfo
                {
                    IsCluster = true,
                    Center = clusterPin.Location,
                    Count = cluster.Items.Count
                };
            }
        }

        // Heatmap circles
        foreach (var cell in _cachedHeatmap)
        {
            var coord = ParseCellKey(cell.CellKey);
            if (coord is null) continue;

            var radius = Math.Min(300, 80 + cell.Count * 30);
            PoiMap.MapElements.Add(new Circle
            {
                Center = new Location(coord.Value.Lat, coord.Value.Lng),
                Radius = new Distance(radius),
                FillColor = Color.FromRgba(14, 110, 184, 40),
                StrokeColor = Color.FromRgba(14, 110, 184, 80),
                StrokeWidth = 1
            });
        }

        // Nearest POI highlight
        if (_lastLocation is not null && _cachedPois.Count > 0)
        {
            var nearest = _cachedPois
                .Select(p => new { Poi = p, Distance = GeoUtils.HaversineKm(_lastLocation.Latitude, _lastLocation.Longitude, p.Latitude, p.Longitude) })
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (nearest is not null)
            {
                PoiMap.MapElements.Add(new Circle
                {
                    Center = new Location(nearest.Poi.Latitude, nearest.Poi.Longitude),
                    Radius = new Distance(80),
                    FillColor = Color.FromRgba(14, 110, 184, 50),
                    StrokeColor = Color.FromRgba(14, 110, 184, 150),
                    StrokeWidth = 2
                });
            }
        }
    }

    private void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is not Pin pin || !_pinLookup.TryGetValue(pin, out var info)) return;

        if (info.IsCluster)
        {
            e.HideInfoWindow = true;
            var span = PoiMap.VisibleRegion;
            var currentRadius = span?.Radius.Kilometers ?? 2.0;
            var nextRadius = Math.Max(0.3, currentRadius / 2);
            PoiMap.MoveToRegion(MapSpan.FromCenterAndRadius(info.Center, Distance.FromKilometers(nextRadius)));
            return;
        }

        // Show preview card for single POI
        e.HideInfoWindow = true;
        _selectedPoi = _cachedPois.FirstOrDefault(p => p.Id == info.PoiId);
        if (_selectedPoi != null)
        {
            PreviewPoiName.Text = _selectedPoi.Name;
            PreviewPoiCategory.Text = _selectedPoi.CategoryName;
            PreviewPoiCoords.Text = $"{_selectedPoi.Latitude:F4}, {_selectedPoi.Longitude:F4}";
            PoiPreviewCard.IsVisible = true;
        }
    }

    private void OnPreviewClose(object? sender, TappedEventArgs e)
    {
        PoiPreviewCard.IsVisible = false;
        _selectedPoi = null;
    }

    private async void OnPreviewDetailClicked(object? sender, EventArgs e)
    {
        if (_selectedPoi != null)
        {
            await Navigation.PushAsync(new PoiDetailPage(_selectedPoi, _localDb));
        }
    }

    private async void OnPreviewListenClicked(object? sender, EventArgs e)
    {
        if (_selectedPoi != null)
        {
            var text = _selectedPoi.Description ?? _selectedPoi.Name;
            await TextToSpeech.SpeakAsync(text);
        }
    }

    private static double GetClusterSize(MapSpan span)
    {
        var degrees = Math.Max(span.LatitudeDegrees, span.LongitudeDegrees);
        return Math.Max(0.0025, degrees / 8);
    }

    private static (double Lat, double Lng)? ParseCellKey(string cellKey)
    {
        var parts = cellKey.Split(',');
        if (parts.Length != 2) return null;
        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)) return null;
        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lng)) return null;
        return (lat, lng);
    }

    private static string BuildClusterKey(double lat, double lng, double clusterSize)
    {
        var latBucket = Math.Floor(lat / clusterSize);
        var lngBucket = Math.Floor(lng / clusterSize);
        var latKey = (latBucket * clusterSize).ToString("0.0000", CultureInfo.InvariantCulture);
        var lngKey = (lngBucket * clusterSize).ToString("0.0000", CultureInfo.InvariantCulture);
        return $"{latKey},{lngKey}";
    }

    private async void OnSearchTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Tìm kiếm", "Tính năng tìm kiếm sẽ sớm có.", "OK");
    }

    private async void OnPremiumTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Premium", "Gói Premium sẽ sớm có.", "OK");
    }

    private async void OnLayersTapped(object? sender, TappedEventArgs e)
    {
        PoiMap.MapType = PoiMap.MapType == MapType.Street ? MapType.Hybrid : MapType.Street;
        await Task.CompletedTask;
    }

    private async void OnTargetTapped(object? sender, TappedEventArgs e)
    {
        var last = await _localDb.GetLatestLocationLogAsync();
        if (last is null)
        {
            await DisplayAlert("GPS", "Chưa có vị trí gần nhất.", "OK");
            return;
        }
        PoiMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(last.Latitude, last.Longitude), Distance.FromKilometers(2)));
    }

    private sealed class ClusterInfo
    {
        public bool IsCluster { get; init; }
        public int Count { get; init; }
        public string PoiId { get; init; } = string.Empty;
        public Location Center { get; init; } = new Location(0, 0);
    }
}
