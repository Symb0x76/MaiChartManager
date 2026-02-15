using System.Diagnostics;
using Python.Runtime;

namespace MaiChartManager.WannaCRI;

public static class WannaCRI
{
    private const string DefaultKey = "0x7F4551499DF55E68";

    static WannaCRI()
    {
        Runtime.PythonDLL = Path.Combine(StaticSettings.exeDir, "Python", "python312.dll");
        PythonEngine.PythonHome = Path.Combine(StaticSettings.exeDir, "Python");
        PythonEngine.PythonPath = $"{Path.Combine(StaticSettings.exeDir, "WannaCRI")};{Path.Combine(StaticSettings.exeDir, "Python")}";
    }

    private static void RunWannaCRIWithArgsInCurrentProcess(params string[] args)
    {
        PythonEngine.Initialize();
        try
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                // Hook Popen
                scope.Exec("""
                           import subprocess
                           import os

                           # 保存原始的 Popen 函数
                           _orig_Popen = subprocess.Popen

                           # 定义新的 Popen 函数
                           def _Popen_no_window(*args, **kwargs):
                               # 添加 creationflags 参数，防止弹出 cmd 窗口
                               if os.name == 'nt':  # 仅在 Windows 上设置
                                   kwargs['creationflags'] = subprocess.CREATE_NO_WINDOW
                               return _orig_Popen(*args, **kwargs)

                           # 替换原始 Popen 函数
                           subprocess.Popen = _Popen_no_window
                           """);

                var sys = scope.Import("sys");
                var argv = new PyList();
                argv.Append(new PyString("qwq"));
                foreach (var arg in args)
                {
                    argv.Append(new PyString(arg));
                }

                sys.SetAttr("argv", argv);

                var wannacri = scope.Import("wannacri");
                wannacri.GetAttr("main").Invoke();
            }
        }
        finally
        {
            // 不然的话第二次转换会卡住
            PythonEngine.Shutdown();
        }
    }

    private static void RunWannaCRIWithArgsInHelperProcess(params string[] args)
    {
        var processPath = Application.ExecutablePath;
        if (string.IsNullOrEmpty(processPath))
        {
            processPath = Environment.ProcessPath;
        }

        if (string.IsNullOrEmpty(processPath))
        {
            throw new InvalidOperationException("Cannot locate current executable path.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = processPath,
            WorkingDirectory = StaticSettings.exeDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("--run-wannacri");
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start WannaCRI helper process.");
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var stderr = stderrTask.GetAwaiter().GetResult().Trim();

        if (process.ExitCode != 0)
        {
            throw new Exception($"WannaCRI helper failed with exit code {process.ExitCode}: {stderr}");
        }
    }

    public static int RunHelper(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Missing WannaCRI command.");
            return 2;
        }

        try
        {
            RunWannaCRIWithArgsInCurrentProcess(args);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    public static void CreateUsm(string src, string key = DefaultKey)
    {
        RunWannaCRIWithArgsInHelperProcess("createusm", src, "--key", key, "--ffprobe", Path.Combine(StaticSettings.exeDir, "ffprobe.exe"), "--output", Path.GetDirectoryName(src)!);
    }

    public static void UnpackUsm(string src, string output, string key = DefaultKey)
    {
        RunWannaCRIWithArgsInHelperProcess("extractusm", src, "--key", key, "--output", output);
    }
}
