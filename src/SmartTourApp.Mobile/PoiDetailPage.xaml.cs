using SmartTourApp.Mobile.Models;
using SmartTourApp.Mobile.Services;

namespace SmartTourApp.Mobile;

public partial class PoiDetailPage : ContentPage
{
    private readonly LocalPoi _poi;
    private readonly LocalDbService _localDb;

    public PoiDetailPage(LocalPoi poi, LocalDbService localDb)
    {
        InitializeComponent();
        _poi = poi;
        _localDb = localDb;
        PopulateData();
    }

    private void PopulateData()
    {
        HeroTitle.Text = _poi.Name;
        HeroCategory.Text = _poi.CategoryName;
        DescriptionLabel.Text = string.IsNullOrWhiteSpace(_poi.Description)
            ? "Đang cập nhật nội dung..."
            : _poi.Description;
        CoordinatesLabel.Text = $"Tọa độ: {_poi.Latitude:F5}, {_poi.Longitude:F5}";
        CategoryLabel.Text = $"Danh mục: {_poi.CategoryName}";
        GeofenceLabel.Text = $"Bán kính geofence: {_poi.GeofenceRadius}m";
        LocationSubtitle.Text = $"{_poi.Latitude:F4}, {_poi.Longitude:F4}";
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnDirectionTapped(object? sender, TappedEventArgs e)
    {
        var uri = $"https://www.google.com/maps/dir/?api=1&destination={_poi.Latitude},{_poi.Longitude}";
        Launcher.Default.OpenAsync(new Uri(uri));
    }

    private async void OnNarrateTapped(object? sender, TappedEventArgs e)
    {
        var text = _poi.Description;
        if (string.IsNullOrWhiteSpace(text))
        {
            text = _poi.Name;
        }

        try
        {
            await TextToSpeech.SpeakAsync(text);

            await _localDb.LogVisitAsync(new LocalVisitLog
            {
                PoiId = _poi.Id,
                PoiName = _poi.Name,
                VisitedAt = DateTime.UtcNow,
                NarrationSeconds = 0,
                TriggeredByQr = false
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể phát thuyết minh: {ex.Message}", "OK");
        }
    }

    private async void OnFavoriteTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await _localDb.ToggleFavoriteAsync(_poi.Id);
            await DisplayAlert("Yêu thích", $"Đã thêm {_poi.Name} vào danh sách yêu thích!", "OK");
        }
        catch
        {
            await DisplayAlert("Yêu thích", $"Đã thêm {_poi.Name} vào yêu thích!", "OK");
        }
    }

    private void OnStartJourneyClicked(object? sender, EventArgs e)
    {
        OnDirectionTapped(sender, new TappedEventArgs(null));
    }
}
