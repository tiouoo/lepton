using CommunityToolkit.Mvvm.ComponentModel;
using OvoUi.Common.Theme;

namespace Lepton.Core.App.Runtime;

public partial class Config : ObservableObject
{
    [ObservableProperty] public partial Themes Theme { get; set; } = Themes.System;
}