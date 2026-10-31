using CommunityToolkit.Mvvm.ComponentModel;

namespace Lepton.Core.App.Runtime;

public sealed class Context : ObservableObject
{
    private static DateTime _sessionTimestamp;
    public static DateTime SessionTimestamp => _sessionTimestamp;

    private static Config _config = new();

    private Context()
    {
        _config.PropertyChanged += OnConfigPropertyChanged;
        _sessionTimestamp = DateTime.Now;
    }

    public static Context Instance { get; } = new();
    public static UiProperty UiProperty { get; } = new();

    public static Config Config
    {
        get => Volatile.Read(ref _config);
        internal set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_config, value)) return;
            _config.PropertyChanged -= Instance.OnConfigPropertyChanged;
            Volatile.Write(ref _config, value);
            value.PropertyChanged += Instance.OnConfigPropertyChanged;
            Instance.OnPropertyChanged();
        }
    }

    private void OnConfigPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        => OnPropertyChanged(nameof(Config));
}