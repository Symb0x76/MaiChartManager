using MaiChartManager.Utils;
using Microsoft.AspNetCore.Mvc;
using Mono.Cecil;
using PeNet;

namespace MaiChartManager.Controllers.Mod;

[ApiController]
[Route("MaiChartManagerServlet/[action]Api")]
public class ManualInstallController(StaticSettings settings, ILogger<InstallationController> logger) : ControllerBase
{
    public record CheckAquaMaiFileResult(bool isValid, string? version = null, AquaMaiSignatureV2.VerifyResult? signature = null, string? buildDate = null);

    [HttpPost]
    public CheckAquaMaiFileResult CheckAquaMaiFile(IFormFile file)
    {
        using var ms = new MemoryStream();
        file.CopyTo(ms);
        try
        {
            var bytes = ms.ToArray();
            var pe = new PeFile(bytes);
            if (!pe.IsDotNet) return new CheckAquaMaiFileResult(false);
            var table = pe.Resources?.VsVersionInfo?.StringFileInfo.StringTable.FirstOrDefault(it => it.ProductName != null);
            if (table?.ProductName != "AquaMai")
            {
                return new CheckAquaMaiFileResult(false);
            }
            var version = table.FileVersion;

            var signature = AquaMaiSignatureV2.VerifySignature(bytes);
            string? buildDate = null;

            ms.Seek(0, SeekOrigin.Begin);
            var cecil = AssemblyDefinition.ReadAssembly(ms);
            if (cecil != null)
            {
                var clas = GetTypeDefinition(cecil, "AquaMai", "BuildInfo");
                var field = clas?.Fields.FirstOrDefault(f => f.Name == "BuildDate");
                if (field is { HasConstant: true })
                {
                    buildDate = field.Constant as string;
                }
            }

            return new CheckAquaMaiFileResult(true, version, signature, buildDate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in CheckAquaMaiFile");
            return new CheckAquaMaiFileResult(false);
        }
    }

    [HttpPost]
    public void InstallAquaMaiFile(IFormFile file)
    {
        using var fs = System.IO.File.Open(ModPaths.AquaMaiDllInstalledPath, FileMode.Create);
        file.CopyTo(fs);
    }

    [NonAction]
    private static TypeDefinition? GetTypeDefinition(AssemblyDefinition assemblyDefinition, string nameSpace, string className)
    {
        // 遍历程序集中的所有模块
        foreach (var module in assemblyDefinition.Modules)
        {
            // 遍历模块中的所有类型
            foreach (var type in module.Types)
            {
                if (type.Name == className && type.Namespace == nameSpace)
                {
                    return type;
                }
            }
        }

        return null;
    }
}