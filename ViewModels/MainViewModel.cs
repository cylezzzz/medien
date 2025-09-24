using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AiMediaGenerator.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    bool _isBusy;
    string _status = "Bereit";

    public bool IsBusy
    {
        get => _isBusy;
        set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(); } }
    }

    public string Status
    {
        get => _status;
        set { if (_status != value) { _status = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
