using AiMediaGenerator.Services;
using Microsoft.Maui.Storage;
using AiMediaGenerator.Models;

namespace AiMediaGenerator.Views;

public partial class Img2ImgPage : ContentPage
{
    readonly ApiService _api;
    string? _inputPath;
    Capabilities? _caps;

    public Img2ImgPage(ApiService api)
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
        var file = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Bild auswählen" });
        if (file != null)
        {
            _inputPath = file.FullPath;
            Preview.Source = ImageSource.FromFile(_inputPath);
            StatusLabel.Text = "Bild geladen";
        }
    }

    void OnReset(object sender, EventArgs e)
    {
        _inputPath = null;
        Preview.Source = null;
        PromptEntry.Text = string.Empty;
        NegativeEntry.Text = string.Empty;
        ResultLabel.Text = string.Empty;
        StatusLabel.Text = "Bereit";
    }

    async void OnGenerate(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_inputPath))
        {
            await DisplayAlert("Fehler", "Bitte erst ein Bild wählen.", "OK");
            return;
        }
        var prompt = PromptEntry.Text ?? "";
        var negative = NegativeEntry.Text ?? "";

        StatusLabel.Text = "Sende an Cloud…";
        try
        {
            var resultUrl = await _api.Img2ImgAsync(_inputPath, prompt, negative);
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
