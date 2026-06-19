using System.Runtime.InteropServices;

namespace HappreeTool.Windows.WindowsOperators
{
    /// <summary>
    /// 提供将指定窗口切换到前台（激活窗口）的功能。
    /// </summary>
    public static class ForegroundHelper
    {
        /// <summary>
        /// 根据窗口标题查找窗口并尝试将其切换为前台程序。
        /// </summary>
        /// <param name="windowTitle">窗口标题，如“同花顺远航版”。</param>
        /// <returns>是否切换成功。若找不到窗口，返回 false。</returns>
        public static bool ActivateWindowByTitle(string windowTitle)
        {
            nint hWnd = FindWindow(null, windowTitle);
            if (hWnd == nint.Zero)
                return false;

            return SetForegroundWindow(hWnd);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);
    }
}
