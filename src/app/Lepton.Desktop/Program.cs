using System;
using System.Text;
using Avalonia;
using Lepton.Core.App.Initialization;

namespace Lepton.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
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
