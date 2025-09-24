using AiMediaGenerator.Views;

namespace AiMediaGenerator;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    void OnImg2Img(object sender, EventArgs e) => Shell.Current.GoToAsync(nameof(Img2ImgPage));
    void OnInpaint(object sender, EventArgs e) => Shell.Current.GoToAsync(nameof(InpaintPage));
    void OnVideo(object sender, EventArgs e) => Shell.Current.GoToAsync(nameof(VideoPage));
    void OnHistory(object sender, EventArgs e) => Shell.Current.GoToAsync(nameof(HistoryPage));
}
