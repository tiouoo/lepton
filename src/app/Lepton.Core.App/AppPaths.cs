namespace Lepton.Core.App;

public sealed class AppPaths
{
    public static string UserDataRootPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lepton-launcher");

    public static string TempFolderPath => Path.Combine(UserDataRootPath, "temp");
    public static string LogFolderPath => Path.Combine(UserDataRootPath, "logs");
    public static string CacheFolderPath => Path.Combine(UserDataRootPath, "caches");
    public static string UpdateFolderPath => Path.Combine(UserDataRootPath, "updates");
    public static string ConfigDataPath => Path.Combine(UserDataRootPath, "config.lpt");
    public static string AppPathDataPath => Path.Combine(UserDataRootPath, "app.lpt");
    public static string DebugConsoleDataPath => Path.Combine(UserDataRootPath, "debugConsole.lpt");
}