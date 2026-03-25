namespace SmartTourApp.Mobile;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập tên đăng nhập và mật khẩu.", "OK");
            return;
        }

        // Simple local validation (no API needed for offline mode)
        await SecureStorage.SetAsync("auth_token", "local_session");
        await SecureStorage.SetAsync("user_name", username);
        await NavigateToMainAsync(username);
    }

    private async void OnGoogleLoginClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await WebAuthenticator.AuthenticateAsync(
                new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                new Uri("smarttour://callback"));

            var token = result?.AccessToken;
            if (!string.IsNullOrWhiteSpace(token))
            {
                await SecureStorage.SetAsync("auth_token", token);
                await SecureStorage.SetAsync("user_name", "Google User");
                await NavigateToMainAsync("Google User");
            }
        }
        catch (TaskCanceledException)
        {
            // User cancelled
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Đăng nhập Google thất bại: {ex.Message}", "OK");
        }
    }

    private async void OnGuestClicked(object? sender, EventArgs e)
    {
        await SecureStorage.SetAsync("auth_token", "guest");
        await SecureStorage.SetAsync("user_name", "Khách");
        await NavigateToMainAsync("Khách");
    }

    private async Task NavigateToMainAsync(string displayName)
    {
        if (Application.Current is not null)
        {
            Application.Current.MainPage = new MainPage();
        }
        await Task.CompletedTask;
    }
}
