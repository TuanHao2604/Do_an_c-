namespace SmartTourApp.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Check if user is already logged in
        var token = SecureStorage.GetAsync("auth_token").Result;
        Page startPage = string.IsNullOrWhiteSpace(token)
            ? new LoginPage()
            : new MainPage();

        return new Window(startPage);
    }
}
