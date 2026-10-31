namespace Lepton.Core.App;

public sealed class AppPaths
{
    public const string DataDirectoryName = "lepton-launcher";
    public const string ConfigFileName = "config.json";
    public const string LogsDirectoryName = "Logs";
    public const string CacheDirectoryName = "Cache";

    public AppPaths(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        DataDirectory = Path.GetFullPath(dataDirectory);
    }

    public static readonly string DefaultDataDirectory = Path.Combine(
        AppContext.BaseDirectory, DataDirectoryName);

    public string DataDirectory { get; }
    public string ConfigFile => Path.Combine(DataDirectory, ConfigFileName);
    public string LogsDirectory => Path.Combine(DataDirectory, LogsDirectoryName);
    public string CacheDirectory => Path.Combine(DataDirectory, CacheDirectoryName);
}
