namespace ConsoleFlasher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static class Program
    {
        [DllImport(@"user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport(@"kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(@"user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport(@"user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        // ReSharper disable UnusedMember.Local
        private const uint FLASHW_STOP = 0;

        private const uint FLASHW_CAPTION = 1;
        private const uint FLASHW_TRAY = 2;
        private const uint FLASHW_ALL = 3;
        private const uint FLASHW_TIMER = 4;

        private const uint FLASHW_TIMERNOFG = 12;

        private static int Main()
        {
            var a = GetConsoleWindow();

            var consoleWindowHandles = new HashSet<IntPtr> {a};
            consoleWindowHandles.UnionWith(getEffectiveWindowHandles(a));

            var foregroundHandle = GetForegroundWindow();

            foreach (var consoleWindowHandle in consoleWindowHandles)
            {
                //Console.WriteLine($@"Window '0x{consoleWindowHandle.ToInt64():X8}'.");

                var flags = consoleWindowHandle.Equals(foregroundHandle)
                    // If already in the foreground, blink a few times, then stop.
                    ? FLASHW_ALL
                    // If in the background, blink until user brings to the foreground.
                    : FLASHW_ALL | FLASHW_TIMERNOFG;

                var fInfo = new FLASHWINFO();
                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
                fInfo.hwnd = consoleWindowHandle;
                fInfo.dwFlags = flags;
                fInfo.uCount = 4;
                fInfo.dwTimeout = 0;
                FlashWindowEx(ref fInfo);
            }

            return 0;
        }

        /// <summary>
        /// https://github.com/Eugeny/terminus/issues/1553
        /// </summary>
        private static IEnumerable<IntPtr> getEffectiveWindowHandles(IntPtr hwnd)
        {
            var result = new List<IntPtr>();

            // Get process for window.
            GetWindowThreadProcessId(hwnd, out var processId);

            var process = Process.GetProcessById((int)processId);

            //Console.WriteLine($@"Process '{process.ProcessName}'.");

            try
            {
                var parent = ParentProcessUtilities.GetParentProcess(process.Handle);
                do
                {
                    if (parent == null ||
                        string.Equals(parent.ProcessName, @"Explorer", StringComparison.OrdinalIgnoreCase)) break;
                    result.Add(parent.Handle);

                    // Console.WriteLine($@"Process '{parent.ProcessName}'.");

                    parent = ParentProcessUtilities.GetParentProcess(parent.Handle);
                } while (parent != null && !parent.HasExited &&
                         !string.Equals(parent.ProcessName, @"Explorer", StringComparison.OrdinalIgnoreCase));
            }
            catch (Win32Exception)
            {
                //Console.WriteLine($@"Ignoring error '{x.Message}': {x}");
            }

            return result.ToArray();
        }
    }
}