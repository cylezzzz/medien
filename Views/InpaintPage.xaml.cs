using AiMediaGenerator.Services;
using Microsoft.Maui.Storage;
using AiMediaGenerator.Models;

namespace AiMediaGenerator.Views;

public partial class InpaintPage : ContentPage
{
    readonly ApiService _api;
    string? _inputPath;
    string? _maskPath;
    Capabilities? _caps;

    public InpaintPage(ApiService api)
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
            var res = _caps.MaxImageResolution;
            if (string.IsNullOrWhiteSpace(res) && _caps.MaxImageWidth.HasValue && _caps.MaxImageHeight.HasValue)
                res = $"{_caps.MaxImageWidth}x{_caps.MaxImageHeight}";
            LimitsLabel.Text = !string.IsNullOrWhiteSpace(res) ? $"Empf. maximale Auflösung: {res}" : "";
        }
        else
        {
            LimitsLabel.Text = "";
        }
    }

    async void OnPickImage(object sender, EventArgs e)
    {
        var file = await FilePicker.PickAsync();
        if (file != null)
        {
            _inputPath = file.FullPath;
            Preview.Source = ImageSource.FromFile(_inputPath);
            StatusLabel.Text = "Bild geladen";
        }
    }

    async void OnPickMask(object sender, EventArgs e)
    {
        var file = await FilePicker.PickAsync();
        if (file != null)
        {
            _maskPath = file.FullPath;
            MaskPreview.Source = ImageSource.FromFile(_maskPath);
            StatusLabel.Text = "Maske geladen";
        }
    }

    async void OnInpaint(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_inputPath) || string.IsNullOrWhiteSpace(_maskPath))
        {
            await DisplayAlert("Fehler", "Bitte Bild und Maske wählen.", "OK");
            return;
        }

        var prompt = PromptEntry.Text ?? "";
        var negative = NegativeEntry.Text ?? "";

        StatusLabel.Text = "Sende an Cloud…";
        try
        {
            var resultUrl = await _api.InpaintAsync(_inputPath, _maskPath, prompt, negative);
            ResultLabel.Text = $"Result: {resultUrl}";
            StatusLabel.Text = "Fertig";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Fehlgeschlagen";
            await DisplayAlert("API Fehler", ex.Message, "OK");
        }
    }
}
