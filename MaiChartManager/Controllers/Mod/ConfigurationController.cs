using System.Text.Json;
using System.Text.Json.Serialization;
using AquaMai.Config.HeadlessLoader;
using AquaMai.Config.Interfaces;
using MaiChartManager.Models;
using Microsoft.AspNetCore.Mvc;
using Mono.Cecil;
using YamlDotNet.Serialization;

namespace MaiChartManager.Controllers.Mod;

[ApiController]
[Route("MaiChartManagerServlet/[action]Api")]
public class ConfigurationController(StaticSettings settings, ILogger<ConfigurationController> logger) : ControllerBase
{
    public class UnsupportedConfigApiVersionException() : Exception(Locale.UnsupportedConfigVersion);

    public class ConfigCorruptedException() : Exception(Locale.AquaMaiConfigCorrupted);

    public class AquaMaiNotInstalledException() : Exception(Locale.AquaMaiNotInstalled);

    [NonAction]
    private static void CheckConfigApiVersion(HeadlessConfigInterface configInterface)
    {
        var currentSupportedApiVersion = new Version(1, 1);
        var configApiVersion = new Version(configInterface.ApiVersion);
        if (currentSupportedApiVersion.Major != configApiVersion.Major)
        {
            throw new UnsupportedConfigApiVersionException();
        }

        if (currentSupportedApiVersion.Minor > configApiVersion.Minor)
        {
            throw new UnsupportedConfigApiVersionException();
        }
    }

    [NonAction]
    public static IConfig GetCurrentAquaMaiConfig(bool forceDefault = false)
    {
        if (!System.IO.File.Exists(ModPaths.AquaMaiDllInstalledPath))
        {
            throw new AquaMaiNotInstalledException();
        }

        var configInterface = HeadlessConfigLoader.LoadFromPacked(ModPaths.AquaMaiDllInstalledPath);
        var config = configInterface.CreateConfig();
        CheckConfigApiVersion(configInterface);
        if (System.IO.File.Exists(ModPaths.AquaMaiConfigPath) && !forceDefault)
        {
            try
            {
                var view = configInterface.CreateConfigView(System.IO.File.ReadAllText(ModPaths.AquaMaiConfigPath));
                var migrationManager = configInterface.GetConfigMigrationManager();

                if (migrationManager.GetVersion(view) != migrationManager.LatestVersion)
                {
                    Console.WriteLine("Migrating AquaMai config from {0} to {1}", migrationManager.GetVersion(view), migrationManager.LatestVersion);
                    view = migrationManager.Migrate(view);
                }

                var parser = configInterface.GetConfigParser();
                parser.Parse(config, view);
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法加载 AquaMai 配置");
                Console.WriteLine(ex);
                if (ex.Message.Contains("Could not migrate the config"))
                {
                    // 这个应该是，AquaMai 未安装或需要更新
                    throw;
                }
                // 这个的提示是 AquaMai 配置文件损坏
                throw new ConfigCorruptedException();
            }
        }

        return config;
    }

    [HttpGet]
    public AquaMaiConfigDto.ConfigDto GetAquaMaiConfig(bool forceDefault = false)
    {
        Dictionary<string, string[]>? configSort = null;
        using (var stream = new FileStream(ModPaths.AquaMaiDllInstalledPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var asm = ModuleDefinition.ReadModule(stream);
            var configSortRes = asm.Resources.FirstOrDefault(it => it is EmbeddedResource { Name: "configSort.yaml.compressed" } or EmbeddedResource { Name: "configSort.yaml" });
            if (configSortRes != null)
            {
                var (name, res) = ResourceLoader.LoadResource(configSortRes);
                var deserializer = new DeserializerBuilder().Build();
                var yaml = new StreamReader(res).ReadToEnd();
                configSort = deserializer.Deserialize<Dictionary<string, string[]>>(yaml);
            }
        }
        var config = GetCurrentAquaMaiConfig(forceDefault);
        return new AquaMaiConfigDto.ConfigDto(
            config.ReflectionManager.Sections.Select(section =>
            {
                var entries = section.Entries.Select(entry => new AquaMaiConfigDto.Entry(entry.Path, entry.Name, entry.Attribute, entry.Field.FieldType.FullName ?? entry.Field.FieldType.Name));
                return new AquaMaiConfigDto.Section(section.Path, entries, section.Attribute);
            }),
            config.ReflectionManager.Sections.ToDictionary(section => section.Path, section => config.GetSectionState(section)),
            config.ReflectionManager.Entries.ToDictionary(entry => entry.Path, entry => config.GetEntryState(entry)),
            configSort
        );
    }

    [HttpPut]
    public async Task SetAquaMaiConfig(AquaMaiConfigDto.ConfigSaveDto config)
    {
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var configInterface = HeadlessConfigLoader.LoadFromPacked(ModPaths.AquaMaiDllInstalledPath);
        CheckConfigApiVersion(configInterface);
        var configEdit = configInterface.CreateConfig();

        foreach (var section in configEdit.ReflectionManager.Sections)
        {
            var newState = config.SectionStates[section.Path];
            var oldState = configEdit.GetSectionState(section);
            if (newState.Enabled != oldState.Enabled)
                configEdit.SetSectionEnabled(section, newState.Enabled);

            // 看起来现在如果禁用了 section 并且改了选项，选项会被写到上一个 section 里面
            // 两个 section 有名字一样的选项怕不是会出问题
            if (!newState.Enabled) continue;

            foreach (var entry in section.Entries)
            {
                var newEntryState = config.EntryStates[entry.Path];
                var oldEntryState = configEdit.GetEntryState(entry);
                var newEntryValue = newEntryState.Value.Deserialize(entry.Field.FieldType, jsonOptions);
                if (!oldEntryState.Value.Equals(newEntryValue))
                {
                    configEdit.SetEntryValue(entry, newEntryValue);
                }
                else if (!newEntryState.IsDefault)
                {
                    configEdit.SetEntryValue(entry, newEntryValue);
                }
            }
        }

        var serializer = configInterface.CreateConfigSerializer(new IConfigSerializer.Options()
        {
            Lang = StaticSettings.CurrentLocale.StartsWith("zh") ? "zh" : "en",
            IncludeBanner = true,
        });
        var serializedConfig = serializer.Serialize(configEdit);

        if (System.IO.File.Exists(ModPaths.AquaMaiConfigPath))
        {
            var originalContent = await System.IO.File.ReadAllTextAsync(ModPaths.AquaMaiConfigPath);
            if (originalContent == serializedConfig)
            {
                // 没改动就不写了
                return;
            }
            Directory.CreateDirectory(ModPaths.AquaMaiConfigBackupDirPath);
            var backupPath = Path.Combine(ModPaths.AquaMaiConfigBackupDirPath, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.toml");
            await System.IO.File.WriteAllTextAsync(backupPath, originalContent);
        }

        await System.IO.File.WriteAllTextAsync(Path.Combine(StaticSettings.GamePath, "AquaMai.toml"), serializer.Serialize(configEdit));
        StaticSettings.UpdateAssetPathsFromAquaMaiConfig(configEdit);
        // 可能修改了歌曲封面目录
        settings.ScanMusicList();
    }
}