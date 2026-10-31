using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lepton.Core.App.Runtime;

public class Context : ObservableObject
{
    private static Context? _instance;
    private static Config? _config;

    public static Context Instance
    {
        get { return _instance ??= new Context(); }
    }

    public Context()
    {
        PropertyChanged += OnPropertyChanged;
        _config.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    public static Config Config
    {
        get => _config ?? throw new InvalidOperationException("Configuration has not been initialized.");
        set
        {
            if (ReferenceEquals(_config, value)) return;
            if (_instance is not null && _config is not null)
                _config.PropertyChanged -= _instance.OnPropertyChanged;

            _config = value;

            if (_instance is not null)
                _config.PropertyChanged += _instance.OnPropertyChanged;
        }
    }
}