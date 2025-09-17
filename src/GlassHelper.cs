using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GlobalClockApp
{
    public static class GlassHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

        [StructLayout(LayoutKind.Sequential)]
        private struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        public static void EnableGlass(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            var margins = new Margins
            {
                Left = -1,
                Right = -1,
                Top = -1,
                Bottom = -1
            };

            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
    }
}
