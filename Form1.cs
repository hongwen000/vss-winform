using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace vss
{
    public partial class Form1 : Form
    {
        private const int HotkeyId = 1;
        private const uint MOD_ALT = 0x0001;
        private const int VK_SPACE = 0x20;

        private string recent = "";
        private List<IntPtr> windowList;
        private bool onlyShowVSCode = true;
        private TaskbarManager taskbarManager;
        private WindowManager windowManager;

        public Form1()
        {
            InitializeComponent();
            windowManager = new WindowManager();
            windowList = windowManager.GetWindows();
            UpdateList();
            CenterToScreen();
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            taskbarManager = new TaskbarManager(this);
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
            if (m.Msg == NativeInterop.WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
            {
                ActivateWindow();
            }
            base.WndProc(ref m);
        }

        private void RegisterHotkey()
        {
            NativeInterop.RegisterHotKey(Handle, HotkeyId, MOD_ALT, VK_SPACE);
        }

        private void UnregisterHotkey()
        {
            NativeInterop.UnregisterHotKey(Handle, HotkeyId);
        }

        private void UpdateWindowList()
        {
            windowList = windowManager.GetWindows();
            UpdateList();
        }



        private void UpdateList()
        {
            string search = textBoxSearch.Text;
            var magicSearches = ScoringLogic.LoadMagicSearches();
            var isVSCode = windowList.Where(hwnd => windowManager.GetWindowText(hwnd).Contains("Visual Studio Code")).ToList();
            var notVSCode = windowList.Except(isVSCode).ToList();

            if (onlyShowVSCode)
            {
                notVSCode.Clear();
            }

            if (string.IsNullOrEmpty(search))
            {
                listBoxWindows.Items.Clear();
                var winTitles = (isVSCode.Concat(notVSCode)).Select(hwnd => windowManager.GetWindowText(hwnd)).Where(title => !string.IsNullOrEmpty(title)).ToList();
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
                if (magicSearches.TryGetValue(search, out string magicSearch))
                {
                    search = magicSearch;
                    isVSCode = isVSCode.Where(hwnd => windowManager.GetWindowText(hwnd).Contains(search)).ToList();
                }

                string[] keywords = Regex.Split(search.Replace('[', ' ').Replace(']', ' '), @"\s+");
                var scoreList = new List<Tuple<int, string>>();

                foreach (IntPtr hwnd in isVSCode)
                {
                    int rcuScore = windowManager.GetWindowText(hwnd) == recent ? 10 : 0;
                    string searchTitle = ScoringLogic.RemoveVSCodePostfix(windowManager.GetWindowText(hwnd));
                    string origTitle = windowManager.GetWindowText(hwnd);
                    int baseScore = keywords.Sum(keyword => Regex.IsMatch(searchTitle, keyword, RegexOptions.IgnoreCase) ? 1 : 0) * 100;
                    int acronymScore = ScoringLogic.GetCharacterMatchScore(search, searchTitle) * 10;
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
                var window = windowList.FirstOrDefault(hwnd => windowManager.GetWindowText(hwnd) == title);
                if (window != IntPtr.Zero)
                {
                    windowManager.SetTopWindow(window);
                    recent = title;
                }
            }
            textBoxSearch.Text = "";
            Hide();
        }

        public void ActivateWindow()
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
    }
}