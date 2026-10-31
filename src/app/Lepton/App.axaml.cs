using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lepton.Services;
using Lepton.Core.App.Initialization;

namespace Lepton;

public partial class App : Application
{
    public override void Initialize()
    {
        StartupDiagnostics.Measure("加载应用界面资源", () => AvaloniaXamlLoader.Load(this));
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            StartupDiagnostics.Measure("初始化桌面主窗口", () => DesktopInitialization.Initialize(desktop));

        base.OnFrameworkInitializationCompleted();
    }
}
