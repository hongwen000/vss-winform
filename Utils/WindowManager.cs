using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static vss.Utils.NativeInterop;

namespace vss.Utils
{
    internal class WindowManager
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const uint GA_ROOTOWNER = 3;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNORMAL = 1;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;
        private static readonly nint HWND_TOPMOST = new nint(-1);
        private static readonly nint HWND_NOTOPMOST = new nint(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        public const int WPF_RESTORETOMAXIMIZED = 2;

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


        public static bool WasMaximized(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                NativeInterop.WINDOWPLACEMENT placement = new NativeInterop.WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);
                NativeInterop.GetWindowPlacement(hWnd, ref placement);
                return (placement.flags & WPF_RESTORETOMAXIMIZED) == WPF_RESTORETOMAXIMIZED;
            }
            return false;
        }
        private static int GetShowState(IntPtr hWnd)
        {
            NativeInterop.WINDOWPLACEMENT placement = new NativeInterop.WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            NativeInterop.GetWindowPlacement(hWnd, ref placement);
            return placement.showCmd;
        }
        public static bool IsMinimized(IntPtr hWnd)
        {
            return GetShowState(hWnd) == SW_SHOWMINIMIZED;
        }

        public static void Maximize(IntPtr hWnd)
        {
            NativeInterop.ShowWindow(hWnd, SW_MAXIMIZE);
        }

        public static bool IsMaximized(IntPtr hWnd)
        {
            return GetShowState(hWnd) == SW_MAXIMIZE;
        }

        public static void Restore(IntPtr hWnd)
        {
            NativeInterop.ShowWindow(hWnd, SW_RESTORE);
        }

        public static bool IsNormal(IntPtr hWnd)
        {
            return GetShowState(hWnd) == SW_SHOWNORMAL;
        }
        private static bool HasFocus(IntPtr hWnd)
        {
            return NativeInterop.GetForegroundWindow() == hWnd;
        }

        public bool SetTopWindow(nint hWnd)
        {
            // Check if window is minimized
            if (IsMinimized(hWnd))
            {
                if (WasMaximized(hWnd))
                {
                    Maximize(hWnd);
                }
                else
                {
                    Restore(hWnd);
                }
            }

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