using System.Runtime.InteropServices;
using System.Text;

namespace HappreeTool.Windows.WindowsOperators;

public static class ClipboardHelper
{
    private const uint CF_UNICODETEXT = 13;
    private static readonly nint HGlobalNull = nint.Zero;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(nint hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetClipboardData(uint uFormat, nint hMem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GlobalAlloc(uint uFlags, nuint dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GlobalLock(nint hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalUnlock(nint hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GlobalFree(nint hMem);

    private const uint GMEM_MOVEABLE = 0x0002;

    /// <summary>
    /// 写入文本到系统剪贴板
    /// </summary>
    public static void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        RetryOpenClipboard();

        nint hGlobal = HGlobalNull;

        try
        {
            if (!EmptyClipboard())
            {
                ThrowLastWin32Exception("EmptyClipboard failed.");
            }

            byte[] bytes = Encoding.Unicode.GetBytes(text + '\0');

            hGlobal = GlobalAlloc(GMEM_MOVEABLE, (nuint)bytes.Length);

            if (hGlobal == HGlobalNull)
            {
                ThrowLastWin32Exception("GlobalAlloc failed.");
            }

            nint target = GlobalLock(hGlobal);

            if (target == HGlobalNull)
            {
                ThrowLastWin32Exception("GlobalLock failed.");
            }

            try
            {
                Marshal.Copy(bytes, 0, target, bytes.Length);
            }
            finally
            {
                GlobalUnlock(hGlobal);
            }

            if (SetClipboardData(CF_UNICODETEXT, hGlobal) == HGlobalNull)
            {
                ThrowLastWin32Exception("SetClipboardData failed.");
            }

            // 所有权已经转移给系统
            hGlobal = HGlobalNull;
        }
        finally
        {
            if (hGlobal != HGlobalNull)
            {
                GlobalFree(hGlobal);
            }

            CloseClipboard();
        }
    }

    /// <summary>
    /// 从系统剪贴板读取文本
    /// </summary>
    public static string? GetText()
    {
        if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
        {
            return null;
        }

        RetryOpenClipboard();

        try
        {
            nint handle = GetClipboardData(CF_UNICODETEXT);

            if (handle == HGlobalNull)
            {
                return null;
            }

            nint pointer = GlobalLock(handle);

            if (pointer == HGlobalNull)
            {
                return null;
            }

            try
            {
                return Marshal.PtrToStringUni(pointer);
            }
            finally
            {
                GlobalUnlock(handle);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    /// <summary>
    /// 判断剪贴板是否包含文本
    /// </summary>
    public static bool ContainsText()
    {
        return IsClipboardFormatAvailable(CF_UNICODETEXT);
    }

    /// <summary>
    /// 清空剪贴板
    /// </summary>
    public static void Clear()
    {
        RetryOpenClipboard();

        try
        {
            if (!EmptyClipboard())
            {
                ThrowLastWin32Exception("EmptyClipboard failed.");
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static void RetryOpenClipboard(int retryCount = 10, int delayMs = 50)
    {
        for (int i = 0; i < retryCount; i++)
        {
            if (OpenClipboard(nint.Zero))
            {
                return;
            }

            Thread.Sleep(delayMs);
        }

        ThrowLastWin32Exception("OpenClipboard failed.");
    }

    private static void ThrowLastWin32Exception(string message)
    {
        throw new InvalidOperationException(
            $"{message} Win32Error={Marshal.GetLastWin32Error()}");
    }
}