using AiMediaGenerator.Services;
using Microsoft.Maui.Storage;
using AiMediaGenerator.Models;

namespace AiMediaGenerator.Views;

public partial class VideoPage : ContentPage
{
    readonly ApiService _api;
    string? _inputPath;
    Capabilities? _caps;

    public VideoPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _caps = _api.GetCachedCapabilities();
        if (_caps != null)
        {
            var parts = new List<string>();
            if (_caps.MaxFrames.HasValue) parts.Add($"max Frames: {_caps.MaxFrames}");
            if (_caps.MaxFps.HasValue) parts.Add($"max FPS: {_caps.MaxFps}");
            if (_caps.MaxVideoSeconds.HasValue) parts.Add($"max Dauer: {_caps.MaxVideoSeconds}s");
            LimitsLabel.Text = parts.Count > 0 ? string.Join(" · ", parts) : "";
        }
        else
        {
            LimitsLabel.Text = "";
        }
    }

    async void OnPickImage(object sender, EventArgs e)
    {
        var f = await FilePicker.PickAsync();
        if (f != null)
        {
            _inputPath = f.FullPath;
            Preview.Source = ImageSource.FromFile(_inputPath);
            StatusLabel.Text = "Bild geladen";
        }
    }

    async void OnGenerateVideo(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_inputPath))
        {
            await DisplayAlert("Fehler", "Bitte zuerst ein Startbild wählen.", "OK");
            return;
        }
        var prompt = PromptEntry.Text ?? "";
        int.TryParse(FramesEntry.Text, out var frames);
        int.TryParse(FpsEntry.Text, out var fps);
        if (frames <= 0) frames = 24;
        if (fps <= 0) fps = 8;

        bool adjusted = false;
        if (_caps?.MaxFrames is int maxFrames && frames > maxFrames) { frames = maxFrames; adjusted = true; }
        if (_caps?.MaxFps is int maxFps && fps > maxFps) { fps = maxFps; adjusted = true; }

        StatusLabel.Text = adjusted ? "Werte an Server-Limits angepasst…" : "Sende an Cloud…";

        try
        {
            var resultUrl = await _api.VideoAsync(_inputPath, prompt, frames, fps);
            ResultLabel.Text = $"Result: {resultUrl}";
            StatusLabel.Text = adjusted ? "Fertig (angepasst)" : "Fertig";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Fehlgeschlagen";
            await DisplayAlert("API Fehler", ex.Message, "OK");
        }
    }
}
