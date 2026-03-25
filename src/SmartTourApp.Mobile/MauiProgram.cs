using SmartTourApp.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SmartTourApp.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Ubuntu-R.ttf", "UbuntuRegular");
                fonts.AddFont("Ubuntu-M.ttf", "UbuntuMedium");
                fonts.AddFont("Ubuntu-B.ttf", "UbuntuBold");
            });

        // ── Services ──
        builder.Services.AddSingleton<LocalDbService>();

        builder.Services.AddHttpClient<SyncApiService>(client =>
        {
            // Change this to your API URL
            client.BaseAddress = new Uri("http://10.0.2.2:5001/"); // Android emulator → host
        });

        // ── Pages ──
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ExplorePage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<ProfilePage>();

        return builder.Build();
    }
}
