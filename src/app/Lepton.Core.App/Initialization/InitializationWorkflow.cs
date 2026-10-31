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
    
    public void Initialize(AppPaths paths, Action<Config>? checkConfiguration = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException("Startup has already been initialized.");
        ArgumentNullException.ThrowIfNull(paths);

        Directory.CreateDirectory(paths.DataDirectory);
        Directory.CreateDirectory(paths.LogsDirectory);
        Directory.CreateDirectory(paths.CacheDirectory);
        Logger.Initialize(paths.LogsDirectory, "Lepton", BuildInfo.Version);

        if (!File.Exists(paths.ConfigFile))
            ConfigSaver.WriteAtomic(paths.ConfigFile, new Config().ToJson());

        InitializationEvents.RaiseBeforeSettingsLoad();
        Context.Config.Initialize(paths.ConfigFile);
        if (Context.Config.LoadException is { } loadException) Logger.Error(loadException);
        foreach (var (path, exception) in Context.Config.LoadErrors)
            Logger.Error($"{path}: {exception}");

        checkConfiguration?.Invoke(Context.Config);

        if (Context.Config.LoadException is not null || Context.Config.LoadErrors.Count > 0)
            File.Copy(paths.ConfigFile, paths.ConfigFile + "." +
                DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "." + Guid.NewGuid().ToString("N") + ".bak");

        _configSaver = new ConfigSaver(paths.ConfigFile);
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
