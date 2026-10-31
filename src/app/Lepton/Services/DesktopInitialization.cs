using System.ComponentModel;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Lepton.Core.App.Runtime;
using Lepton.Core.App.Initialization;
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
        LeptonWindow window = null!;
        StartupDiagnostics.Measure("创建主窗口", () => window = new LeptonWindow { IsPrimaryWindow = true });

        async Task RequestExitAsync()
        {
            if (exitPending || !active) return;
            exitPending = true;
            try
            {
                Logger.Info("正在请求退出程序，调用退出前检查");
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
                if (!await ApplicationEvents.RaiseExitingAsync() || !active)
                {
                    Logger.Info("退出请求已取消或程序已退出");
                    return;
                }
                exitApproved = true;
                Logger.Info("退出前检查通过，正在关闭桌面程序");
                desktop.TryShutdown();
            }
            catch (Exception exception)
            {
                Logger.Error("退出前检查或关闭程序失败", exception);
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
            StartupDiagnostics.WindowOpened();
            StartupDiagnostics.Measure("调用界面加载后扩展", InitializationEvents.RaiseAfterUiLoad);
        }

        void ApplySettings()
        {
            if (active)
                StartupDiagnostics.Measure("应用界面主题设置", () => ThemeManager.ToggleTheme(Context.Config.Theme));
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
            Logger.Info($"桌面程序已退出，退出代码：{e.ApplicationExitCode}");
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

        StartupDiagnostics.Measure("创建默认标签页", () => window.AddTab(TabService.CreateTab()));
        desktop.MainWindow = window;
        Logger.Info("主窗口初始化完成，等待窗口显示事件");
    }
}
