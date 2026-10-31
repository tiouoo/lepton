using System.ComponentModel;

namespace Lepton.Core.App.Runtime;

public partial class Config
{
    public Config()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }
}