using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace vss.Utils
{
    internal class WindowManager
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const uint GA_ROOTOWNER = 3;
        private const int SW_SHOW = 5;
        private static readonly nint HWND_TOPMOST = new nint(-1);
        private static readonly nint HWND_NOTOPMOST = new nint(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;

        public List<nint> GetWindows()
        {
            return EnumerateWindows()
                .Where(hwnd => IsAltTabWindow(hwnd) && NativeInterop.GetWindowTextLength(hwnd) > 0)
                .ToList();
        }

        private bool IsAltTabWindow(nint hwnd)
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

        private IEnumerable<nint> EnumerateWindows()
        {
            nint hwnd = nint.Zero;
            while ((hwnd = NativeInterop.FindWindowEx(nint.Zero, hwnd, null, null)) != nint.Zero)
            {
                yield return hwnd;
            }
        }

        public bool SetTopWindow(nint hWnd)
        {
            nint hForeWnd = NativeInterop.GetForegroundWindow();
            uint dwForeID = NativeInterop.GetWindowThreadProcessId(hForeWnd, nint.Zero);
            uint dwCurID = NativeInterop.GetCurrentThreadId();
            NativeInterop.AttachThreadInput(dwCurID, dwForeID, true);
            NativeInterop.BringWindowToTop(hWnd);
            NativeInterop.ShowWindow(hWnd, SW_SHOW);
            //NativeInterop.SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
            //NativeInterop.SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
            NativeInterop.SetForegroundWindow(hWnd);
            NativeInterop.AttachThreadInput(dwCurID, dwForeID, false);
            return true;
        }

        public string GetWindowText(nint hWnd)
        {
            int length = NativeInterop.GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;
            StringBuilder sb = new StringBuilder(length + 1);
            NativeInterop.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}