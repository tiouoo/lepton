namespace Lepton.Core.App.Build;

public static class BuildInfo
{
    private static string? Get(string name) => BuildValues.Get($"Lepton.{name}");

    public static string Type => Get(nameof(Type)) ?? "development";
    public static string Version => Get(nameof(Version)) ?? BaseVersion;
    public static string BaseVersion => Get(nameof(BaseVersion)) ?? "1.0.0";
    public static string? BuildTime => Get(nameof(BuildTime));
    public static string? Action => Get(nameof(Action));
    public static string? Commit => Get(nameof(Commit));
    public static string? Timestamp => Get(nameof(Timestamp));
    public static string? VersionTitle => Get(nameof(VersionTitle));
    public static string PackageType => Get(nameof(PackageType)) ?? "development";
}
