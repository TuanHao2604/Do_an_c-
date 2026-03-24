namespace SmartTourApp.Mobile;

public partial class FavoritesPage : ContentPage
{
    public FavoritesPage()
    {
        InitializeComponent();
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
