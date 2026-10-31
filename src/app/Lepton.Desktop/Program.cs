using System;
using Avalonia;
using Lepton.Core.App;
using Lepton.Core.App.Initialization;

namespace Lepton.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        using var initialization = new InitializationWorkflow();
        initialization.Initialize();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
