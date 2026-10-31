using System.Diagnostics;
using Standard.Logging;

namespace Lepton.Core.App.Initialization;

public static class StartupDiagnostics
{
    private static Stopwatch? _startupWatch;
    private static double _entryDelayMilliseconds;

    public static void Begin()
    {
        var entryTime = DateTime.UtcNow;
        _startupWatch = Stopwatch.StartNew();
        using var process = Process.GetCurrentProcess();
        _entryDelayMilliseconds = Math.Max(0, (entryTime - process.StartTime.ToUniversalTime()).TotalMilliseconds);
        Logger.Info("程序进程已启动，开始记录启动流程");
    }

    public static void WindowOpened()
    {
        if (_startupWatch is not { IsRunning: true } watch) return;
        watch.Stop();
        Logger.Info($"主窗口已显示，从进程启动到窗口显示共耗时 {_entryDelayMilliseconds + watch.Elapsed.TotalMilliseconds:F2} 毫秒");
    }

    public static void Measure(string operation, Action action)
    {
        var watch = Stopwatch.StartNew();
        Logger.Info($"开始{operation}");
        try
        {
            action();
            watch.Stop();
            Logger.Info($"{operation}完成，耗时 {watch.Elapsed.TotalMilliseconds:F2} 毫秒");
        }
        catch (Exception exception)
        {
            watch.Stop();
            Logger.Error($"{operation}失败，耗时 {watch.Elapsed.TotalMilliseconds:F2} 毫秒", exception);
            throw;
        }
    }
}
