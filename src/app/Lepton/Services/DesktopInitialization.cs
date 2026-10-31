using System.ComponentModel;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Lepton.Core.App.Runtime;
using Lepton.Views;
using OvoUi.Common.Theme;
using Standard.Events;
using Standard.Logging;

namespace Lepton.Services;

internal static class DesktopInitialization
{
    public static void Initialize(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var active = true;
        var exitPending = false;
        var exitApproved = false;
        var window = new LeptonWindow { IsPrimaryWindow = true };

        async Task RequestExitAsync()
        {
            if (exitPending || !active) return;
            exitPending = true;
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
                if (!await ApplicationEvents.RaiseExitingAsync() || !active) return;
                exitApproved = true;
                desktop.TryShutdown();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            finally
            {
                exitApproved = false;
                exitPending = false;
            }
        }

        async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            if (exitApproved || e.Cancel) return;
            e.Cancel = true;
            await RequestExitAsync();
        }

        async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            if (exitApproved || e.Cancel) return;
            e.Cancel = true;
            await RequestExitAsync();
        }

        void OnWindowOpened(object? sender, EventArgs e)
        {
            window.Opened -= OnWindowOpened;
            InitializationEvents.RaiseAfterUiLoad();
        }

        void ApplySettings()
        {
            if (active) ThemeManager.ToggleTheme(Context.Config.Theme);
        }

        void OnContextChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Context.Config)) return;
            if (Dispatcher.UIThread.CheckAccess()) ApplySettings();
            else Dispatcher.UIThread.Post(ApplySettings);
        }

        void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            active = false;
            Context.Instance.PropertyChanged -= OnContextChanged;
            desktop.Exit -= OnExit;
            desktop.ShutdownRequested -= OnShutdownRequested;
            window.Closing -= OnWindowClosing;
            window.Opened -= OnWindowOpened;
        }

        Context.Instance.PropertyChanged += OnContextChanged;
        desktop.Exit += OnExit;
        desktop.ShutdownRequested += OnShutdownRequested;
        window.Closing += OnWindowClosing;
        window.Opened += OnWindowOpened;
        ApplySettings();

        window.AddTab(TabService.CreateTab());
        desktop.MainWindow = window;
    }
}
