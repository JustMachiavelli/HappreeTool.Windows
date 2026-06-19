using System.Runtime.InteropServices;

namespace HappreeTool.Windows.WindowsOperators
{
    public static class KeyboardSimulator
    {
        // Windows API constants and structures
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public nint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public nint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        public const uint INPUT_MOUSE = 0;
        public const uint INPUT_KEYBOARD = 1;
        public const uint INPUT_HARDWARE = 2;

        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_SCANCODE = 0x0008;

        public const ushort VK_SHIFT = 0x10;
        public const ushort VK_DOWN = 0x28;
        public const ushort VK_ESCAPE = 0x1B;
        public const ushort VK_RIGHT = 0x27;
        public const ushort VK_RETURN = 0x0D;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public static void SendKey(ushort key, bool keyUp = false)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = key;
            inputs[0].u.ki.wScan = 0;
            inputs[0].u.ki.dwFlags = keyUp ? KEYEVENTF_KEYUP : 0;
            inputs[0].u.ki.time = 0;
            inputs[0].u.ki.dwExtraInfo = nint.Zero;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SendChar(char character)
        {
            if (charToScanCode.TryGetValue(character, out ushort scanCode))
            {
                bool shift = char.IsUpper(character) || "~!@#$%^&*()_+{}|:\"<>?".Contains(character);

                if (shift)
                {
                    SendKey(VK_SHIFT);
                }

                INPUT[] inputs = new INPUT[1];
                inputs[0].type = INPUT_KEYBOARD;
                inputs[0].u.ki.wVk = 0;
                inputs[0].u.ki.wScan = scanCode;
                inputs[0].u.ki.dwFlags = KEYEVENTF_SCANCODE;
                inputs[0].u.ki.time = 0;
                inputs[0].u.ki.dwExtraInfo = nint.Zero;

                SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));

                inputs[0].u.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
                SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));

                if (shift)
                {
                    SendKey(VK_SHIFT, true);
                }
            }
            else
            {
                throw new ArgumentException($"Unsupported character: {character}");
            }
        }

        public static void SendText(string text)
        {
            foreach (char ch in text)
            {
                SendChar(ch);
                Thread.Sleep(100); // 等待每个字符输入
            }
        }

        public static void PressKey(ushort key)
        {
            SendKey(key);
            Thread.Sleep(50); // 短暂等待
            SendKey(key, true);
        }

        private static readonly Dictionary<char, ushort> charToScanCode = new Dictionary<char, ushort>
        {
            { '1', 0x02 },
            { '2', 0x03 },
            { '3', 0x04 },
            { '4', 0x05 },
            { '5', 0x06 },
            { '6', 0x07 },
            { '7', 0x08 },
            { '8', 0x09 },
            { '9', 0x0A },
            { '0', 0x0B },
            { 'a', 0x1E },
            { 'b', 0x30 },
            { 'c', 0x2E },
            { 'd', 0x20 },
            { 'e', 0x12 },
            { 'f', 0x21 },
            { 'g', 0x22 },
            { 'h', 0x23 },
            { 'i', 0x17 },
            { 'j', 0x24 },
            { 'k', 0x25 },
            { 'l', 0x26 },
            { 'm', 0x32 },
            { 'n', 0x31 },
            { 'o', 0x18 },
            { 'p', 0x19 },
            { 'q', 0x10 },
            { 'r', 0x13 },
            { 's', 0x1F },
            { 't', 0x14 },
            { 'u', 0x16 },
            { 'v', 0x2F },
            { 'w', 0x11 },
            { 'x', 0x2D },
            { 'y', 0x15 },
            { 'z', 0x2C },
            { 'A', 0x1E },
            { 'B', 0x30 },
            { 'C', 0x2E },
            { 'D', 0x20 },
            { 'E', 0x12 },
            { 'F', 0x21 },
            { 'G', 0x22 },
            { 'H', 0x23 },
            { 'I', 0x17 },
            { 'J', 0x24 },
            { 'K', 0x25 },
            { 'L', 0x26 },
            { 'M', 0x32 },
            { 'N', 0x31 },
            { 'O', 0x18 },
            { 'P', 0x19 },
            { 'Q', 0x10 },
            { 'R', 0x13 },
            { 'S', 0x1F },
            { 'T', 0x14 },
            { 'U', 0x16 },
            { 'V', 0x2F },
            { 'W', 0x11 },
            { 'X', 0x2D },
            { 'Y', 0x15 },
            { 'Z', 0x2C },
            { '-', 0x0C },
            { '=', 0x0D },
            { '[', 0x1A },
            { ']', 0x1B },
            { '\\', 0x2B },
            { ';', 0x27 },
            { '\'', 0x28 },
            { '`', 0x29 },
            { ',', 0x33 },
            { '.', 0x34 },
            { '/', 0x35 },
            { ' ', 0x39 },
            { '!', 0x02 },
            { '@', 0x03 },
            { '#', 0x04 },
            { '$', 0x05 },
            { '%', 0x06 },
            { '^', 0x07 },
            { '&', 0x08 },
            { '*', 0x09 },
            { '(', 0x0A },
            { ')', 0x0B },
            { '_', 0x0C },
            { '+', 0x0D },
            { '{', 0x1A },
            { '}', 0x1B },
            { '|', 0x2B },
            { ':', 0x27 },
            { '"', 0x28 },
            { '~', 0x29 },
            { '<', 0x33 },
            { '>', 0x34 },
            { '?', 0x35 }
        };
        
        public const ushort VK_CONTROL = 0x11;
        public const ushort VK_A = 0x41;
        public const ushort VK_V = 0x56;

        /// <summary>
        /// 输入Ctrl+C这样的组合键
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="key"></param>
        public static void SendHotKey(ushort modifier, ushort key)
        {
            SendKey(modifier);

            Thread.Sleep(50);

            SendKey(key);

            Thread.Sleep(50);

            SendKey(key, true);

            Thread.Sleep(50);

            SendKey(modifier, true);
        }

        public static void Paste()
        {
            SendHotKey(VK_CONTROL, VK_V);
        }

        public static void SelectAll()
        {
            SendHotKey(VK_CONTROL, VK_A);
        }
    }
}
