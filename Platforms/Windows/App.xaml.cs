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
        // Default Settings, only if not set
        if (!Preferences.ContainsKey("ApiBaseUrl"))
            Preferences.Set("ApiBaseUrl", "https://your-cloud-api.example.com"); // Passe in Settings an
        if (!Preferences.ContainsKey("ApiKey"))
            Preferences.Set("ApiKey", string.Empty);
    }
}
