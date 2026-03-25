using Microsoft.Extensions.DependencyInjection;
using SmartTourApp.Mobile.Models;
using SmartTourApp.Mobile.Services;

namespace SmartTourApp.Mobile;

public partial class FavoritesPage : ContentPage
{
    private readonly LocalDbService _localDb;

    public FavoritesPage()
    {
        InitializeComponent();
        _localDb = MauiApplication.Current.Services.GetService<LocalDbService>() ?? new LocalDbService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var favorites = await _localDb.GetFavoritesAsync();
            if (favorites.Count > 0)
            {
                BindableLayout.SetItemsSource(FavoritesListLayout, favorites);
                EmptyState.IsVisible = false;
                FavoritesScroll.IsVisible = true;
                FavCountLabel.Text = $"{favorites.Count} mục";
            }
            else
            {
                EmptyState.IsVisible = true;
                FavoritesScroll.IsVisible = false;
                FavCountLabel.Text = "0 mục";
            }
        }
        catch
        {
            EmptyState.IsVisible = true;
            FavoritesScroll.IsVisible = false;
        }
    }

    private async void OnFavoriteTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is LocalFavorite fav)
        {
            var poi = await _localDb.GetPoiByIdAsync(fav.PoiId);
            if (poi != null)
            {
                await Navigation.PushAsync(new PoiDetailPage(poi, _localDb));
            }
        }
    }

    private async void OnRemoveFavoriteTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is LocalFavorite fav)
        {
            var confirm = await DisplayAlert("Xóa", $"Xóa {fav.PoiName} khỏi yêu thích?", "Xóa", "Hủy");
            if (confirm)
            {
                await _localDb.RemoveFavoriteAsync(fav.PoiId);
                await LoadFavoritesAsync();
            }
        }
    }

    private void OnStartExploringClicked(object? sender, EventArgs e)
    {
        if (Parent is TabbedPage tabs)
        {
            var explore = tabs.Children.FirstOrDefault(page => page is ExplorePage);
            if (explore is not null)
            {
                tabs.CurrentPage = explore;
            }
        }
    }
}
