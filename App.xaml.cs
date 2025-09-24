using Microsoft.Maui.Storage;

namespace AiMediaGenerator;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();
        if (!Preferences.ContainsKey("ApiBaseUrl"))
            Preferences.Set("ApiBaseUrl", "https://your-cloud-api.example.com");
        if (!Preferences.ContainsKey("ApiKey"))
            Preferences.Set("ApiKey", string.Empty);
        if (!Preferences.ContainsKey("AllowNSFW"))
            Preferences.Set("AllowNSFW", false);
    }
}
