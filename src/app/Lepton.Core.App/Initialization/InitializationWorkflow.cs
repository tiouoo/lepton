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
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException("Startup has already been initialized.");

        StartupDiagnostics.Measure("初始化日志系统", LoggerInitialize);
        StartupDiagnostics.Measure("启动初始化", () =>
        {
            StartupDiagnostics.Measure("初始化目录和配置文件", () =>
            {
                foreach (var path in new[] { AppPaths.UserDataRootPath, AppPaths.TempFolderPath,
                             AppPaths.LogFolderPath, AppPaths.CacheFolderPath })
                {
                    Logger.Info($"正在检查并创建目录：{path}");
                    Directory.CreateDirectory(path);
                }

                if (!File.Exists(AppPaths.ConfigDataPath))
                {
                    Logger.Info($"配置文件不存在，正在创建默认配置：{AppPaths.ConfigDataPath}");
                    ConfigSaver.WriteAtomic(AppPaths.ConfigDataPath, new Config().ToJson());
                }
                else Logger.Info($"发现已有配置文件：{AppPaths.ConfigDataPath}");
            });

            StartupDiagnostics.Measure("调用设置加载前扩展", InitializationEvents.RaiseBeforeSettingsLoad);
            StartupDiagnostics.Measure("加载设置", () =>
            {
                Context.Config.Initialize(AppPaths.ConfigDataPath);
                if (Context.Config.LoadException is { } loadException)
                    Logger.Error("设置文件加载失败，将保留当前设置", loadException);
                foreach (var (path, exception) in Context.Config.LoadErrors)
                    Logger.Error($"设置项加载失败，将使用默认值：{path}", exception);
                if (Context.Config.LoadException is not null || Context.Config.LoadErrors.Count > 0)
                {
                    StartupDiagnostics.Measure("备份异常配置文件", () =>
                    {
                        var backupPath = AppPaths.ConfigDataPath + "." +
                            DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "." + Guid.NewGuid().ToString("N") + ".bak";
                        File.Copy(AppPaths.ConfigDataPath, backupPath);
                        Logger.Warning($"异常配置已备份至：{backupPath}");
                    });
                    Logger.Warning("设置加载结束，存在异常，已使用可用设置继续启动");
                }
                else Logger.Info("设置加载成功");
            });

            _configSaver = new ConfigSaver(AppPaths.ConfigDataPath);
            Logger.Info("设置自动保存监听已启用，修改后延迟 300 毫秒合并保存");
            _initialized = true;
            Context.UiProperty.IsInitialized = true;
            StartupDiagnostics.Measure("调用界面加载前扩展", InitializationEvents.RaiseBeforeUiLoad);
        });
    }

    private static void LoggerInitialize()
    {
        Logger.Initialize(new LoggerOptions
        {
            LogDirectory = AppPaths.LogFolderPath,
            AppName = "Lepton Launcher",
            Version = BuildInfo.Version,
            LogFileCount = 10,
            AppendHeader = false,
            ShowCallSource = true,
            HeaderAction = () =>
            {
                Logger.Append(@"  _               _                _                           _               ");
                Logger.Append(@" | |    ___ _ __ | |_ ___  _ __   | |    __ _ _   _ _ __   ___| |__   ___ _ __ ");
                Logger.Append(@" | |   / _ \ '_ \| __/ _ \| '_ \  | |   / _` | | | | '_ \ / __| '_ \ / _ \ '__|");
                Logger.Append(@" | |__|  __/ |_) | || (_) | | | | | |__| (_| | |_| | | | | (__| | | |  __/ |   ");
                Logger.Append(@" |_____\___| .__/ \__\___/|_| |_| |_____\__,_|\__,_|_| |_|\___|_| |_|\___|_|   ");
                Logger.Append(@"           |_|                                                                 ");
                Logger.Append(@"");
                Logger.Append($"Lepton 日志头 / {Context.SessionTimestamp:O}");
                Logger.Append($"    Lepton 版本: {BuildInfo.Version}");
                Logger.Append($"    操作系统: {Environment.OSVersion}");
                Logger.Append($"    运行时 (.NET) : {Environment.Version}");
                Logger.Append($"    处理器架构: {(Environment.Is64BitProcess ? "x64" : "x86")} /" +
                              $" {Environment.ProcessorCount} 核心 ");
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
                }
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
