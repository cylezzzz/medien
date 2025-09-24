using AiMediaGenerator.Views;

namespace AiMediaGenerator;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(Img2ImgPage), typeof(Img2ImgPage));
        Routing.RegisterRoute(nameof(InpaintPage), typeof(InpaintPage));
        Routing.RegisterRoute(nameof(VideoPage), typeof(VideoPage));
        Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
