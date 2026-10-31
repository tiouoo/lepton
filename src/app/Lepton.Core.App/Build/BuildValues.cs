using Standard.Build;

namespace Lepton.Core.App.Build;

public static class BuildValues
{
    private static readonly AssemblyBuildValues Values = new(typeof(BuildValues).Assembly);

    public static string? Get(string key, string? defaultValue = null) =>
        Values.GetValue(key) ?? defaultValue;
}
