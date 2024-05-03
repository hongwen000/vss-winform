using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace vss
{
    internal class WindowManager
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const uint GA_ROOTOWNER = 3;
        private const int SW_SHOWNORMAL = 1;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;

        public List<IntPtr> GetWindows()
        {
            return EnumerateWindows()
                .Where(hwnd => IsAltTabWindow(hwnd) && NativeInterop.GetWindowTextLength(hwnd) > 0)
                .ToList();
        }

        private bool IsAltTabWindow(IntPtr hwnd)
        {
            if (!NativeInterop.IsWindowVisible(hwnd)) return false;
            if (NativeInterop.GetAncestor(hwnd, GA_ROOTOWNER) != hwnd) return false;
            int exStyle = NativeInterop.GetWindowLong(hwnd, GWL_EXSTYLE);
            if ((exStyle & WS_EX_TOOLWINDOW) != 0)
                return false;
            if ((exStyle & WS_EX_NOACTIVATE) != 0)
                return false;

            return true;
        }

        private IEnumerable<IntPtr> EnumerateWindows()
        {
            IntPtr hwnd = IntPtr.Zero;
            while ((hwnd = NativeInterop.FindWindowEx(IntPtr.Zero, hwnd, null, null)) != IntPtr.Zero)
            {
                yield return hwnd;
            }
        }

        public bool SetTopWindow(IntPtr hWnd)
        {
            IntPtr hForeWnd = NativeInterop.GetForegroundWindow();
            uint dwForeID = NativeInterop.GetWindowThreadProcessId(hForeWnd, IntPtr.Zero);
            uint dwCurID = NativeInterop.GetCurrentThreadId();
            NativeInterop.AttachThreadInput(dwCurID, dwForeID, true);
            NativeInterop.ShowWindow(hWnd, SW_SHOWNORMAL);
            NativeInterop.SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
            NativeInterop.SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
            NativeInterop.SetForegroundWindow(hWnd);
            NativeInterop.AttachThreadInput(dwCurID, dwForeID, false);
            return true;
        }

        public string GetWindowText(IntPtr hWnd)
        {
            int length = NativeInterop.GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;
            StringBuilder sb = new StringBuilder(length + 1);
            NativeInterop.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}