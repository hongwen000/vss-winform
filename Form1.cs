using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace vss
{
    public partial class Form1 : Form
    {
        private const int HotkeyId = 1;
        private const uint MOD_ALT = 0x0001;
        private const int VK_SPACE = 0x20;

        private string recent = "";
        private List<IntPtr> windowList;
        private List<string> customTerms = new List<string> { "android-kernel-sunfish-msm-4.14-android11-qpr3", "yzy-r743", "r743", "ubuntu", "aosp_host_working_dir", "aosp", "apk", "androidtools", "vss" };
        private bool onlyShowVSCode = true;

        public Form1()
        {
            InitializeComponent();
            windowList = GetWindows();
            UpdateList();
            CenterToScreen();
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterHotkey();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            UnregisterHotkey();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
            {
                ActivateWindow();
            }
            base.WndProc(ref m);
        }

        private void RegisterHotkey()
        {
            RegisterHotKey(Handle, HotkeyId, MOD_ALT, VK_SPACE);
        }

        private void UnregisterHotkey()
        {
            UnregisterHotKey(Handle, HotkeyId);
        }

        private List<IntPtr> GetWindows()
        {
            return EnumerateWindows()
                .Where(hwnd => IsAltTabWindow(hwnd) && GetWindowText(hwnd).Length > 0)
                .ToList();
        }

        private void UpdateWindowList()
        {
            windowList = GetWindows();
            UpdateList();
        }

        private void UpdateList()
        {
            string search = textBoxSearch.Text;

            var isVSCode = windowList.Where(hwnd => GetWindowText(hwnd).Contains("Visual Studio Code")).ToList();
            var notVSCode = windowList.Except(isVSCode).ToList();

            if (onlyShowVSCode)
            {
                notVSCode.Clear();
            }

            if (string.IsNullOrEmpty(search))
            {
                listBoxWindows.Items.Clear();
                var winTitles = (isVSCode.Concat(notVSCode)).Select(hwnd => GetWindowText(hwnd)).Where(title => !string.IsNullOrEmpty(title)).ToList();
                if (winTitles.Contains(recent))
                {
                    listBoxWindows.Items.Add(recent);
                }
                foreach (string winTitle in winTitles)
                {
                    if (winTitle != recent)
                    {
                        listBoxWindows.Items.Add(winTitle);
                    }
                }
            }
            else
            {
                string[] keywords = Regex.Split(search.Replace('[', ' ').Replace(']', ' '), @"\s+");
                var scoreList = new List<Tuple<int, string>>();

                foreach (IntPtr hwnd in isVSCode)
                {
                    int rcuScore = GetWindowText(hwnd) == recent ? 10 : 0;
                    string searchTitle = RemoveVSCodePostfix(GetWindowText(hwnd));
                    string origTitle = GetWindowText(hwnd);
                    int baseScore = keywords.Sum(keyword => Regex.IsMatch(searchTitle, keyword, RegexOptions.IgnoreCase) ? 1 : 0) * 100;
                    int acronymScore = GetAcronymScore(search, searchTitle) * 10;
                    int score = baseScore + acronymScore + rcuScore;
                    scoreList.Add(new Tuple<int, string>(score, origTitle));
                }

                scoreList.Sort((x, y) => y.Item1.CompareTo(x.Item1));

                listBoxWindows.Items.Clear();
                foreach (var item in scoreList)
                {
                    if (item.Item1 > 0)
                    {
                        listBoxWindows.Items.Add(item.Item2);
                    }
                }
            }

            if (listBoxWindows.Items.Count > 0)
            {
                listBoxWindows.SelectedIndex = 0;
            }
        }

        private void SwitchWindow(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < listBoxWindows.Items.Count)
            {
                string title = listBoxWindows.Items[selectedIndex].ToString();
                var window = windowList.FirstOrDefault(hwnd => GetWindowText(hwnd) == title);
                if (window != IntPtr.Zero)
                {
                    SetForegroundWindow(window);
                    recent = title;
                }
            }
            textBoxSearch.Text = "";
            Hide();
        }

        private void ActivateWindow()
        {
            UpdateWindowList();
            textBoxSearch.Text = "";
            Show();
            Activate();
            textBoxSearch.Focus();
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SwitchWindow(listBoxWindows.SelectedIndex);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (listBoxWindows.SelectedIndex > 0)
                {
                    listBoxWindows.SelectedIndex--;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (listBoxWindows.SelectedIndex < listBoxWindows.Items.Count - 1)
                {
                    listBoxWindows.SelectedIndex++;
                }
            }
        }

        private void listBoxWindows_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SwitchWindow(listBoxWindows.SelectedIndex);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
        }

        private void listBoxWindows_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SwitchWindow(listBoxWindows.SelectedIndex);
        }

        private int GetAcronymScore(string search, string title)
        {
            string[] words = title.Split(' ');
            title = string.Join(" ", words);
            words = title.Split(' ').Where(x => x.Length > 0).ToArray();
            var mapAcronymToWord = words.Select(word => new { Key = word[0], Value = word }).ToList();
            char[] acronym = words.Select(word => word[0]).ToArray();

            int matchedLength = search.Intersect(acronym).Count();
            return matchedLength;
        }

        private string RemoveVSCodePostfix(string title)
        {
            string[] parts = title.Split(new[] { " - " }, StringSplitOptions.None);
            if (parts.Length > 3)
            {
                parts = parts.Take(parts.Length - 2).ToArray();
            }
            else
            {
                parts = parts.Take(parts.Length - 1).ToArray();
            }
            return string.Join(" - ", parts);
        }

        #region Native Methods

        private const int WM_HOTKEY = 0x0312;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        private static string GetWindowText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetAncestor", SetLastError = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const uint GA_ROOTOWNER = 3;

        private bool IsAltTabWindow(IntPtr hwnd)
        {
            if (!IsWindowVisible(hwnd)) return false;
            if (GetAncestor(hwnd, GA_ROOTOWNER) != hwnd) return false;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            if ((exStyle & WS_EX_TOOLWINDOW) != 0)
                return false;
            if ((exStyle & WS_EX_NOACTIVATE) != 0)
                return false;

            return true;
        }

        private IEnumerable<IntPtr> EnumerateWindows()
        {
            IntPtr hwnd = IntPtr.Zero;
            while ((hwnd = FindWindowEx(IntPtr.Zero, hwnd, null, null)) != IntPtr.Zero)
            {
                yield return hwnd;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        #endregion
    }
}