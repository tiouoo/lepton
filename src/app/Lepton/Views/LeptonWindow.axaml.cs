using OvoUi.Tab;

namespace Lepton.Views;

public partial class LeptonWindow : OvoTabWindow
{
    public LeptonWindow() : base(Services.TabService.Instance)
    {
        InitializeComponent();
        NewTabFactory = Services.TabService.CreateTab;
    }
}
