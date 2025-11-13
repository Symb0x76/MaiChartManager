namespace MaiChartManager.CLI.Utils;

/// <summary>
/// https://learn.microsoft.com/zh-cn/windows/terminal/tutorials/progress-bar-sequences
/// </summary>
public static class TerminalProgress
{
    public enum Status
    {
        Hidden,
        Default,
        Error,
        Indeterminate,
        Warning,
    }

    public static void Set(Status status = Status.Default, int progress = 0)
    {
        Console.Write($"\e]9;4;{status:D};{progress}\a");
    }

    public static void Set(int progress)
    {
        Set(Status.Default, progress);
    }

    public static void Clear()
    {
        Set(Status.Hidden);
    }
}