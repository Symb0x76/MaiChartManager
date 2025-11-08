using Vanara.Windows.Shell;

namespace MaiChartManager.Utils;

public static class WinUtils
{
    public static void SetTaskbarProgress(ulong progress, ulong total = 100)
    {
        if (AppMain.BrowserWin == null) return;
        try
        {
            TaskbarList.SetProgressState(AppMain.BrowserWin.Handle, TaskbarButtonProgressState.Normal);
            TaskbarList.SetProgressValue(AppMain.BrowserWin.Handle, progress, total);
        }
        catch
        {
            // ignored
        }
    }

    public static void ClearTaskbarProgress()
    {
        if (AppMain.BrowserWin == null) return;
        try
        {
            TaskbarList.SetProgressState(AppMain.BrowserWin.Handle, TaskbarButtonProgressState.None);
        }
        catch
        {
            // ignored
        }
    }

    public static void SetTaskbarProgressIndeterminate()
    {
        if (AppMain.BrowserWin == null) return;
        try
        {
            TaskbarList.SetProgressState(AppMain.BrowserWin.Handle, TaskbarButtonProgressState.Indeterminate);
        }
        catch
        {
            // ignored
        }
    }

    public static DialogResult ShowDialog(CommonDialog dialog)
    {
        if (AppMain.BrowserWin == null) return DialogResult.Cancel;
        return AppMain.BrowserWin.Invoke(() => dialog.ShowDialog(AppMain.BrowserWin));
    }
}