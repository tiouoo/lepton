using System.ComponentModel;
using Avalonia.Threading;
using Lepton.Core.App.Runtime;
using Standard.Logging;
using Standard.Threading;
using Standard.Events;

namespace Lepton.Core.App.Initialization;

internal sealed class ConfigSaver : IDisposable
{
    private readonly string _path;
    private readonly Lock _sync = new();
    private readonly Debouncer _debouncer;
    private string? _pendingJson;
    private volatile bool _disposed;
    private bool _capturing;

    public ConfigSaver(string path)
    {
        _path = path;
        _debouncer = new Debouncer(WritePending, 300);
        Context.Instance.PropertyChanged += OnContextChanged;
    }

    public Exception? SaveException { get; private set; }

    private void OnContextChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Context.Config) || _disposed) return;
        if (Dispatcher.UIThread.CheckAccess()) Capture();
        else Dispatcher.UIThread.Post(() => { if (!_disposed) Capture(); });
    }

    private void Capture()
    {
        if (_capturing) return;
        try
        {
            var json = CaptureSnapshot();
            lock (_sync) _pendingJson = json;
            _debouncer.Invoke();
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
    }

    public void Flush()
    {
        if (_capturing) return;
        var json = CaptureSnapshot();
        lock (_sync) _pendingJson = json;
        WritePending();
    }

    private string CaptureSnapshot()
    {
        _capturing = true;
        try
        {
            ApplicationEvents.RaiseSaveSettings();
            return Context.Config.ToJson();
        }
        finally
        {
            _capturing = false;
        }
    }

    private void WritePending()
    {
        lock (_sync)
        {
            if (_pendingJson is null) return;
            try
            {
                WriteAtomic(_path, _pendingJson);
                _pendingJson = null;
                SaveException = null;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                             or System.Security.SecurityException)
            {
                ReportError(exception);
            }
        }
    }

    private void ReportError(Exception exception)
    {
        SaveException = exception;
        Logger.Error(exception);
    }

    internal static void WriteAtomic(string path, string json)
    {
        var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Context.Instance.PropertyChanged -= OnContextChanged;
        _debouncer.Dispose();
        try { Flush(); }
        catch (Exception exception) { ReportError(exception); }
    }
}
