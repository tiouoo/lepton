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

        Directory.CreateDirectory(AppPaths.UserDataRootPath);
        Directory.CreateDirectory(AppPaths.TempFolderPath);
        Directory.CreateDirectory(AppPaths.LogFolderPath);
        Directory.CreateDirectory(AppPaths.CacheFolderPath);
        Logger.Initialize(AppPaths.LogFolderPath, "Lepton", BuildInfo.Version);

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
