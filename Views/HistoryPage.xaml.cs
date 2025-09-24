using AiMediaGenerator.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace AiMediaGenerator.Views;

public partial class HistoryPage : ContentPage
{
    const string Key = "HistoryJson";

    public HistoryPage()
    {
        InitializeComponent();
        LoadHistory();
    }

    void LoadHistory()
    {
        var json = Preferences.Get(Key, "[]");
        var items = JsonSerializer.Deserialize<List<Job>>(json) ?? new List<Job>();
        JobsList.ItemsSource = items.OrderByDescending(x => x.CreatedAt).ToList();
    }

    void OnClear(object sender, EventArgs e)
    {
        Preferences.Set(Key, "[]");
        LoadHistory();
    }
}
