using CommunityToolkit.Mvvm.ComponentModel;

namespace Lepton.Core.App.Runtime;

public sealed class Context : ObservableObject
{
    private Context() { }

    public static Context Instance { get; } = new();

    public static UiProperty UiProperty { get; } = new();

    public static Config Config
    {
        get => Volatile.Read(ref field);
        internal set
        {
            ArgumentNullException.ThrowIfNull(value);
            Volatile.Write(ref field, value);
        }
    } = new();
}
