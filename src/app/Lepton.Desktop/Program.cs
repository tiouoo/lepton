using System;
using System.Text;
using Avalonia;
using Lepton.Core.App.Initialization;
using Standard.Logging;

namespace Lepton.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Logger.Fatal($"程序发生未处理异常，即将终止：{e.IsTerminating}{Environment.NewLine}{e.ExceptionObject}");
        try
        {
            StartupDiagnostics.Begin();
            using var initialization = new InitializationWorkflow();
            initialization.Initialize();
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, e) =>
                Logger.Error("后台任务发生未观察到的异常", e.Exception);
            Logger.Info("正在启动桌面界面框架，等待主窗口显示");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            Logger.Info("桌面界面运行结束，正在保存设置并释放初始化资源");
        }
        catch (Exception exception)
        {
            Logger.Fatal("程序启动或运行失败", exception);
            throw;
        }
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
