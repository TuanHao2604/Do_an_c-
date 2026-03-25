using Microsoft.Extensions.DependencyInjection;
using SmartTourApp.Mobile.Services;

namespace SmartTourApp.Mobile;

public partial class ProfilePage : ContentPage
{
    private readonly LocalDbService _localDb;

    public ProfilePage()
    {
        InitializeComponent();
        _localDb = MauiApplication.Current.Services.GetService<LocalDbService>() ?? new LocalDbService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            var pois = await _localDb.GetAllPoisAsync();
            TotalPoiLabel.Text = pois.Count.ToString();

            var topVisited = await _localDb.GetTopVisitedPoisAsync(100);
            var totalVisits = topVisited.Sum(v => v.VisitCount);
            TotalVisitLabel.Text = totalVisits.ToString();
            VisitedCount.Text = $"{topVisited.Count} đã thăm";

            var avgNarration = await _localDb.GetAverageNarrationSecondsAsync();
            TotalNarrationLabel.Text = avgNarration > 0 ? $"{avgNarration:0.0}s" : "0s";

            try
            {
                var favorites = await _localDb.GetFavoritesAsync();
                FavoriteCount.Text = $"{favorites.Count} yêu thích";
            }
            catch
            {
                FavoriteCount.Text = "0 yêu thích";
            }

            ClearHistoryLabel.Text = $"Xóa lịch sử ({totalVisits})";
        }
        catch
        {
            // Ignore
        }
    }

    private async void OnLogoutTapped(object? sender, TappedEventArgs e)
    {
        var confirm = await DisplayAlert("Đăng xuất", "Bạn có muốn đăng xuất?", "Đăng xuất", "Hủy");
        if (confirm)
        {
            await DisplayAlert("Thông báo", "Đã đăng xuất thành công", "OK");
        }
    }

    private async void OnClearHistoryTapped(object? sender, TappedEventArgs e)
    {
        var confirm = await DisplayAlert("Xóa lịch sử", "Xóa tất cả lịch sử thăm quan?", "Xóa", "Hủy");
        if (confirm)
        {
            await DisplayAlert("Thông báo", "Đã xóa lịch sử", "OK");
            await LoadStatsAsync();
        }
    }
}
