using AiMediaGenerator.Services;
using AiMediaGenerator.ViewModels;
using AiMediaGenerator.Views;

namespace AiMediaGenerator;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<MainViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<Img2ImgPage>();
        builder.Services.AddTransient<InpaintPage>();
        builder.Services.AddTransient<VideoPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
