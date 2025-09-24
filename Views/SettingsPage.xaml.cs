using Microsoft.Maui.Storage;
using AiMediaGenerator.Services;
using System.Text;

namespace AiMediaGenerator.Views;

public partial class SettingsPage : ContentPage
{
    readonly ApiService _api;

    public SettingsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ApiUrlEntry.Text = Preferences.Get("ApiBaseUrl", "");
        ApiKeyEntry.Text = Preferences.Get("ApiKey", "");
        NsfwSwitch.IsToggled = Preferences.Get("AllowNSFW", false);
        await LoadCapabilitiesAsync();
    }

    void OnSave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ApiUrlEntry.Text))
        {
            DisplayAlert("Fehler", "Bitte API Base URL eintragen.", "OK");
            return;
        }
        Preferences.Set("ApiBaseUrl", ApiUrlEntry.Text!.Trim());
        Preferences.Set("ApiKey", ApiKeyEntry.Text ?? "");
        Preferences.Set("AllowNSFW", NsfwSwitch.IsToggled);
        StatusLabel.Text = "Gespeichert.";
    }

    async void OnPing(object sender, EventArgs e)
    {
        PingLabel.Text = "Pinge…";
        try
        {
            var ok = await _api.PingAsync();
            PingLabel.Text = ok ? "Cloud erreichbar ✅" : "Cloud nicht erreichbar ❌";
        }
        catch (Exception ex)
        {
            PingLabel.Text = $"Fehler: {ex.Message}";
        }
    }

    async Task LoadCapabilitiesAsync()
    {
        CapsLabel.Text = "Lade Limits…";
        try
        {
            var caps = await _api.GetCapabilitiesAsync();
            if (caps == null)
            {
                CapsLabel.Text = "Keine Limits vom Server geliefert.";
                return;
            }

            var sb = new StringBuilder();
            sb.Append("• Bild: ");
            if (!string.IsNullOrWhiteSpace(caps.MaxImageResolution))
                sb.Append(caps.MaxImageResolution);
            else if (caps.MaxImageWidth.HasValue && caps.MaxImageHeight.HasValue)
                sb.Append($"{caps.MaxImageWidth}x{caps.MaxImageHeight}");
            else sb.Append("—");

            sb.Append("   • Frames: ").Append(caps.MaxFrames?.ToString() ?? "—");
            sb.Append("   • FPS: ").Append(caps.MaxFps?.ToString() ?? "—");
            sb.Append("   • Video: ").Append(caps.MaxVideoSeconds?.ToString() ?? "—").Append("s");
            sb.Append("   • Upload: ").Append(caps.MaxUploadMb?.ToString() ?? "—").Append("MB");

            if (!string.IsNullOrWhiteSpace(caps.Notes))
                sb.Append("\nHinweis: ").Append(caps.Notes);

            CapsLabel.Text = sb.ToString();
        }
        catch (Exception ex)
        {
            CapsLabel.Text = $"Fehler beim Laden: {ex.Message}";
        }
    }

    async void OnRefreshCaps(object sender, EventArgs e)
    {
        await LoadCapabilitiesAsync();
    }
}
