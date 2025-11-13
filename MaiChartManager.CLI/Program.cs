using System.Text;
using MaiChartManager.CLI.Commands;
using Spectre.Console.Cli;

Console.OutputEncoding = Encoding.UTF8;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("MaiChartManager CLI");

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