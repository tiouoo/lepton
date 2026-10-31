using CommunityToolkit.Mvvm.ComponentModel;

namespace Lepton.Core.App.Runtime;

public sealed class UiProperty : ObservableObject
{
    internal UiProperty() { }

    public bool IsInitialized
    {
        get;
        internal set => SetProperty(ref field, value);
    }
}
