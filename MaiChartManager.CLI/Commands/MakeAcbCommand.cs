using MaiChartManager.Utils;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MaiChartManager.CLI.Commands;

public class MakeAcbCommand : AsyncCommand<MakeAcbCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<sources>")]
        [Description("要转换的源音频文件")]
        public string[] Sources { get; set; } = [];

        [CommandOption("-O|--output")]
        [Description("输出文件路径（仅单文件时可用）")]
        public string? Output { get; set; }

        [CommandOption("-p|--padding")]
        [Description("音频填充（秒），正数为前置静音，负数为裁剪开头")]
        [DefaultValue(0f)]
        public float Padding { get; set; }

        public override ValidationResult Validate()
        {
            if (Sources.Length == 0)
            {
                return ValidationResult.Error("至少需要一个源文件");
            }

            if (Sources.Length > 1 && !string.IsNullOrEmpty(Output))
            {
                return ValidationResult.Error("多文件转换时不能使用 -O 选项");
            }

            foreach (var source in Sources)
            {
                if (!File.Exists(source))
                {
                    return ValidationResult.Error($"源文件不存在: {source}");
                }
            }

            return ValidationResult.Success();
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (settings.Sources.Length == 1)
            {
                var source = settings.Sources[0];
                var output = settings.Output ?? Path.ChangeExtension(source, null);
                await ConvertSingleFile(source, output, settings);
            }
            else
            {
                await ConvertMultipleFiles(settings);
            }

            AnsiConsole.MarkupLine("[green]✓ 所有转换已成功完成！[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ 错误: {ex.Message}[/]");
            return 1;
        }
    }

    private async Task ConvertSingleFile(string source, string output, Settings settings)
    {
        AnsiConsole.MarkupLine($"[yellow]正在转换:[/] {Path.GetFileName(source)} → {Path.GetFileName(output)}.acb");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"转换 {Path.GetFileName(source)}...", async ctx =>
            {
                await Task.Run(() =>
                {
                    Audio.ConvertToMai(
                        srcPath: source,
                        savePath: output,
                        padding: settings.Padding
                    );
                });
            });

        AnsiConsole.MarkupLine($"[green]✓ 已保存到: {output}[/]");
    }

    private async Task ConvertMultipleFiles(Settings settings)
    {
        AnsiConsole.MarkupLine($"[yellow]正在转换 {settings.Sources.Length} 个文件...[/]");

        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(
                new TaskDescriptionColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                foreach (var source in settings.Sources)
                {
                    var output = Path.ChangeExtension(source, null);
                    var task = ctx.AddTask($"[green]{Path.GetFileName(source)}[/]");
                    task.MaxValue = 100;

                    try
                    {
                        task.StartTask();
                        await Task.Run(() =>
                        {
                            Audio.ConvertToMai(
                                srcPath: source,
                                savePath: output,
                                padding: settings.Padding
                            );
                        });

                        task.StopTask();
                    }
                    catch (Exception ex)
                    {
                        task.Description = $"[red]{Path.GetFileName(source)} - 失败[/]";
                        task.StopTask();
                    }
                }
            });
    }
}