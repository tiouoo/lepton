using Lepton.Views;
using Lepton.Views.Models;
using OvoUi.Tab;

namespace Lepton.Services;

public static class TabService
{
    public static OvoTabService Instance { get; } = new()
    {
        WindowFactory = _ => new LeptonWindow()
    };

    public static OvoTabItem CreateTab() => new("Lepton", new DataPage());
}
