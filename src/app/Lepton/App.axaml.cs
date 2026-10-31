using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lepton.Views;

namespace Lepton;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new LeptonWindow { IsPrimaryWindow = true };
            window.AddTab(Services.TabService.CreateTab());
            desktop.MainWindow = window;
        }
        base.OnFrameworkInitializationCompleted();
    }
}
