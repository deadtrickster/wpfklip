using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfKlip.Core.Win.Structs;

namespace WpfKlip.Core.Win
{
        public delegate int FONTENUMPROC(ENUMLOGFONTEX f, int lpntme, int FontType, int lParam);
        public delegate IntPtr WndProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        public class Helpers
        {
            public static string GetWindowText(IntPtr hWnd)
            {
                // Allocate correct string length first
                int length = User32.GetWindowTextLength(hWnd);
                StringBuilder sb = new StringBuilder(length + 1);
                User32.GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
        }
}
