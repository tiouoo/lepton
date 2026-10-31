using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lepton.Core.App.Runtime;

public class UiProperty : ObservableObject
{
    private static UiProperty? _instance;

    public UiProperty()
    {
        PropertyChanged += OnPropertyChanged;
    }

    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }
}