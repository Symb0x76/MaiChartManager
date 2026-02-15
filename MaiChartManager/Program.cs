using System.Runtime.InteropServices;
using SingleInstanceCore;
using WannaCriRunner = MaiChartManager.WannaCRI.WannaCRI;

namespace MaiChartManager;

public static partial class Program
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial void SetConsoleOutputCP(uint wCodePageID);

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main(string[] args)
    {
        SetConsoleOutputCP(65001);

        if (args.Length > 0 && args[0].Equals("--run-wannacri", StringComparison.OrdinalIgnoreCase))
        {
            Environment.ExitCode = WannaCriRunner.RunHelper(args[1..]);
            return;
        }

        var app = new AppMain();

        var isFirstInstance = app.InitializeAsFirstInstance("MaiChartManager");
        if (isFirstInstance)
        {
            try
            {
                app.Run();
            }
            finally
            {
                SingleInstance.Cleanup();
            }
        }
    }
}
