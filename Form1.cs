using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Diagnostics;
using vss.Utils;

namespace vss
{
    public partial class Form1 : Form
    {
        private const int HotkeyId = 1;
        private const uint MOD_ALT = 0x0001;
        private const int VK_SPACE = 0x20;

        private MainController controller;
        private TaskbarManager taskbarManager;

        public string SearchText => textBoxSearch.Text;

        public Form1()
        {
            InitializeComponent();
            CenterToScreen();
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            taskbarManager = new TaskbarManager(this);
            controller = new MainController(this);
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
                controller.ActivateWindow();
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

        public void ClearWindowList()
        {
            listBoxWindows.Items.Clear();
        }

        public void AddWindowToList(string windowTitle)
        {
            listBoxWindows.Items.Add(windowTitle);
        }

        public void SelectFirstWindowInList()
        {
            if (listBoxWindows.Items.Count > 0)
            {
                listBoxWindows.SelectedIndex = 0;
            }
        }

        public void ClearSearchText()
        {
            textBoxSearch.Text = "";
        }

        public void HideForm()
        {
            Hide();
        }

        public void ShowForm()
        {
            Show();
            Activate();
            textBoxSearch.Focus();
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            controller.UpdateList();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.SwitchWindow(listBoxWindows.SelectedIndex, listBoxWindows.GetItemText(listBoxWindows.SelectedItem));
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideForm();
                e.Handled = true;
                e.SuppressKeyPress = true;
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
                controller.SwitchWindow(listBoxWindows.SelectedIndex, listBoxWindows.GetItemText(listBoxWindows.SelectedItem));
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideForm();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void listBoxWindows_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            controller.SwitchWindow(listBoxWindows.SelectedIndex, listBoxWindows.GetItemText(listBoxWindows.SelectedItem));
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            Process.Start("explorer.exe", configPath);
        }
    }
}