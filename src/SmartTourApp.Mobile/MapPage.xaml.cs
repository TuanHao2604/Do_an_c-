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

    public MapPage()
    {
        InitializeComponent();
        _localDb = MauiApplication.Current.Services.GetService<LocalDbService>() ?? new LocalDbService();
        PoiMap.PropertyChanged += OnMapPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshHeatmapAsync();
        await LoadMapDataAsync();
    }

    private async Task LoadMapDataAsync()
    {
        _cachedPois.Clear();
        _cachedPois.AddRange(await _localDb.GetAllPoisAsync());

        var last = await _localDb.GetLatestLocationLogAsync();
        _lastLocation = last is null ? null : new Location(last.Latitude, last.Longitude);
        var center = _lastLocation is null
            ? new Location(10.7764, 106.7009)
            : _lastLocation;

        var span = MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(4));
        PoiMap.MoveToRegion(span);
        UpdatePinsAndOverlays(span);
    }

    private async Task RefreshHeatmapAsync()
    {
        _cachedHeatmap.Clear();
        _cachedHeatmap.AddRange(await _localDb.GetHeatmapAsync(12));
        BindableLayout.SetItemsSource(MapHeatmapList, _cachedHeatmap);
        MapHeatmapEmpty.IsVisible = _cachedHeatmap.Count == 0;

        var last = await _localDb.GetLatestLocationLogAsync();
        _lastLocation = last is null ? _lastLocation : new Location(last.Latitude, last.Longitude);
        MapLastLocationLabel.Text = last is null
            ? "Vi tri gan nhat: --"
            : $"Vi tri gan nhat: {last.Latitude:0.0000}, {last.Longitude:0.0000}";

        var span = PoiMap.VisibleRegion;
        if (span is not null)
        {
            UpdatePinsAndOverlays(span);
        }
    }

    private void OnMapPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MapControl.VisibleRegion))
        {
            return;
        }

        var span = PoiMap.VisibleRegion;
        if (span is null)
        {
            return;
        }

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
                    PoiMap.Pins.Add(new Pin
                    {
                        Label = poi.Name,
                        Address = poi.Address,
                        Type = PinType.Place,
                        Location = new Location(poi.Latitude, poi.Longitude)
                    });
                    var singlePin = PoiMap.Pins.Last();
                    singlePin.MarkerClicked += OnPinMarkerClicked;
                    _pinLookup[singlePin] = new ClusterInfo
                    {
                        IsCluster = false,
                        Center = singlePin.Location
                    };
                    continue;
                }

                var nameList = string.Join(", ", cluster.Items.Take(3).Select(x => x.Name));
                PoiMap.Pins.Add(new Pin
                {
                    Label = $"{cluster.Items.Count} POI",
                    Address = nameList,
                    Type = PinType.Generic,
                    Location = new Location(cluster.Lat, cluster.Lng)
                });
                var clusterPin = PoiMap.Pins.Last();
                clusterPin.MarkerClicked += OnPinMarkerClicked;
                _pinLookup[clusterPin] = new ClusterInfo
                {
                    IsCluster = true,
                    Center = clusterPin.Location,
                    Count = cluster.Items.Count
                };
            }
        }

        foreach (var cell in _cachedHeatmap)
        {
            var coord = ParseCellKey(cell.CellKey);
            if (coord is null)
            {
                continue;
            }

            var radius = Math.Min(300, 80 + cell.Count * 30);
            PoiMap.MapElements.Add(new Circle
            {
                Center = new Location(coord.Value.Lat, coord.Value.Lng),
                Radius = new Distance(radius),
                FillColor = Color.FromRgba(243, 194, 68, 60),
                StrokeColor = Color.FromRgba(243, 194, 68, 120),
                StrokeWidth = 1
            });
        }

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
                    FillColor = Color.FromRgba(243, 194, 68, 80),
                    StrokeColor = Color.FromRgba(243, 194, 68, 200),
                    StrokeWidth = 2
                });
            }
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
        if (parts.Length != 2)
        {
            return null;
        }

        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat))
        {
            return null;
        }

        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
        {
            return null;
        }

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

    private void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is not Pin pin || !_pinLookup.TryGetValue(pin, out var info))
        {
            return;
        }

        if (!info.IsCluster)
        {
            return;
        }

        e.HideInfoWindow = true;
        var span = PoiMap.VisibleRegion;
        var currentRadius = span?.Radius.Kilometers ?? 2.0;
        var nextRadius = Math.Max(0.3, currentRadius / 2);
        PoiMap.MoveToRegion(MapSpan.FromCenterAndRadius(info.Center, Distance.FromKilometers(nextRadius)));
    }

    private sealed class ClusterInfo
    {
        public bool IsCluster { get; init; }
        public int Count { get; init; }
        public Location Center { get; init; } = new Location(0, 0);
    }

    private async void OnSearchTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Search", "Tinh nang tim kiem se som co.", "OK");
    }

    private async void OnPremiumTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Premium", "Goi Premium se som co.", "OK");
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
            await DisplayAlert("GPS", "Chua co vi tri gan nhat.", "OK");
            return;
        }

        PoiMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(last.Latitude, last.Longitude), Distance.FromKilometers(2)));
    }
}
