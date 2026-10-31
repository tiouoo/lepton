using Lepton.Core.App.Build;
using Lepton.Core.App.Runtime;
using Standard.Events;
using Standard.Logging;

namespace Lepton.Core.App.Initialization;

public sealed class InitializationWorkflow : IDisposable
{
    private ConfigSaver? _configSaver;
    private bool _initialized;
    private bool _disposed;

    public Exception? SaveException => _configSaver?.SaveException;
    
    public void Initialize()
    {
        LoggerInitialize();
        
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException("Startup has already been initialized.");

        Directory.CreateDirectory(AppPaths.UserDataRootPath);
        Directory.CreateDirectory(AppPaths.TempFolderPath);
        Directory.CreateDirectory(AppPaths.LogFolderPath);
        Directory.CreateDirectory(AppPaths.CacheFolderPath);

        if (!File.Exists(AppPaths.ConfigDataPath))
            ConfigSaver.WriteAtomic(AppPaths.ConfigDataPath, new Config().ToJson());

        InitializationEvents.RaiseBeforeSettingsLoad();
        Context.Config.Initialize(AppPaths.ConfigDataPath);
        if (Context.Config.LoadException is { } loadException) Logger.Error(loadException);
        foreach (var (path, exception) in Context.Config.LoadErrors)
            Logger.Error($"{path}: {exception}");
        
        if (Context.Config.LoadException is not null || Context.Config.LoadErrors.Count > 0)
            File.Copy(AppPaths.ConfigDataPath, AppPaths.ConfigDataPath + "." +
                DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "." + Guid.NewGuid().ToString("N") + ".bak");

        _configSaver = new ConfigSaver(AppPaths.ConfigDataPath);
        _initialized = true;
        Context.UiProperty.IsInitialized = true;
        InitializationEvents.RaiseBeforeUiLoad();
    }

    private static void LoggerInitialize()
    {
        Logger.Initialize(AppPaths.LogFolderPath, "Lepton Launcher", BuildInfo.Version, 10,false, () =>
        {
            Logger.Append($"Lepton 日志头 / {Context.SessionTimestamp:O}");
            Logger.Append($"    Lepton 版本: {BuildInfo.Version}");
            Logger.Append($"    操作系统: {Environment.OSVersion}");
            Logger.Append($"    运行时 (.NET) : {Environment.Version}");
            Logger.Append($"    处理器架构: {(Environment.Is64BitProcess ? "x64" : "x86")} /" +
                          $" {Environment.ProcessorCount} cores ");
            Logger.Append($"    机器名 (主机名): {Environment.MachineName}");
            Logger.Append($"    当前用户: {Environment.UserDomainName}/{Environment.UserName}");
            Logger.Append($"    是否交互式用户会话: {Environment.UserInteractive}");
            var workingSetMb = Environment.WorkingSet / (1024 * 1024);
            var gcMemory = GC.GetGCMemoryInfo();
            var totalRamMb = gcMemory.TotalAvailableMemoryBytes > 0 ? gcMemory.TotalAvailableMemoryBytes / (1024 * 1024) : -1;
            Logger.Append($"    进程内存 (WorkingSet): {workingSetMb} MB");
            if (totalRamMb > 0)
            {
                Logger.Append($"    总可用内存: {totalRamMb} MB");
            }
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            Logger.Append($"    系统运行时长: {uptime.Days}天 {uptime.Hours}小时 {uptime.Minutes}分钟 {uptime.Seconds}秒");
            Logger.Append($"    时区: {TimeZoneInfo.Local.DisplayName} ({TimeZoneInfo.Local.Id})");
            Logger.Append($"    当前区域/语言: {System.Globalization.CultureInfo.CurrentCulture.Name}");
            Logger.Append($"    当前工作目录: {Environment.CurrentDirectory}");
            Logger.Append($"    命令行参数: {Environment.CommandLine}");
            Logger.Append($"");
            Logger.Append($"");
        });
        
    }

    public void FlushConfig()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _configSaver?.Flush();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _configSaver?.Dispose();
        Context.UiProperty.IsInitialized = false;
    }
}
