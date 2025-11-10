namespace MaiChartManager.Controllers.Mod;

public static class ModPaths
{
    public static string AquaMaiConfigPath => Path.Combine(StaticSettings.GamePath, "AquaMai.toml");
    public static string AquaMaiConfigBackupDirPath => Path.Combine(StaticSettings.GamePath, "AquaMai.toml.bak");
    public static string AquaMaiDllInstalledPath => Path.Combine(StaticSettings.GamePath, @"Mods\AquaMai.dll");
    public static string AquaMaiDllBuiltinPath => Path.Combine(StaticSettings.exeDir, "AquaMai.dll");
}