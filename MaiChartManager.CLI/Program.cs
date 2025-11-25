using System.Text;
using MaiChartManager;
using MaiChartManager.CLI.Commands;
using MaiChartManager.CLI.Utils;
using Spectre.Console.Cli;

Console.OutputEncoding = Encoding.UTF8;
Console.CancelKeyPress += (_, _) => TerminalProgress.Clear();

try
{
    SentrySdk.Init(o =>
    {
        // Tells which project in Sentry to send events to:
        o.Dsn = "https://be7a9ae3a9a88f4660737b25894b3c20@sentry.c5y.moe/3";
        // Set TracesSampleRate to 1.0 to capture 100% of transactions for tracing.
        // We recommend adjusting this value in production.
        o.TracesSampleRate = 0.5;
#if DEBUG
        o.Environment = "development";
#endif
    });

    AppMain.InitConfiguration(true);
    await IapManager.Init();

    if (IapManager.License != IapManager.LicenseStatus.Active)
    {
        Console.WriteLine("命令行工具目前为赞助版功能，请先使用桌面版应用程序解锁");
        return 1;
    }

    var app = new CommandApp();

    app.Configure(config =>
    {
        config.SetApplicationName("mcm");

        config.AddCommand<MakeUsmCommand>("makeusm")
            .WithDescription("将视频文件转换为 USM 格式")
            .WithExample("makeusm", "video.mp4")
            .WithExample("makeusm", "video.mp4", "-O", "output.dat")
            .WithExample("makeusm", "video1.mp4", "video2.mp4", "video3.mp4");

        config.AddCommand<MakeAcbCommand>("makeacb")
            .WithDescription("将音频文件转换为 ACB 格式")
            .WithExample("makeacb", "audio.wav")
            .WithExample("makeacb", "audio.mp3", "-O", "output.acb")
            .WithExample("makeacb", "audio1.wav", "audio2.mp3", "--padding", "0.5");
    });

    return await app.RunAsync(args);
}
catch (Exception e)
{
    SentrySdk.CaptureException(e);
    Console.Error.WriteLine($"发生错误: {e.Message}");
    return 1;
}