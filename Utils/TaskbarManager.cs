using System;
using System.Windows.Forms;

namespace vss.Utils
{
    internal class TaskbarManager
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;
        private Form1 mainForm;
        private TaskbarController controller;

        public TaskbarManager(Form1 form)
        {
            mainForm = form;
            controller = new TaskbarController(form);
            RegisterContextMenuStrip();
            CreateTrayIcon();
        }

        private void CreateTrayIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = controller.CreateIcon();
            notifyIcon.Visible = true;
            notifyIcon.Text = "My Switcher Program";
            notifyIcon.DoubleClick += (sender, e) => controller.ShowMainForm();
            notifyIcon.ContextMenuStrip = trayMenu;
            mainForm.FormClosed += (sender, e) => notifyIcon.Dispose();
        }

        private void RegisterContextMenuStrip()
        {
            trayMenu = new ContextMenuStrip();
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (sender, e) => controller.ExitApplication();
            trayMenu.Items.Add(exitItem);

            ToolStripMenuItem showItem = new ToolStripMenuItem("Show");
            showItem.Click += (sender, e) => controller.ShowMainForm();
            trayMenu.Items.Add(showItem);
        }
    }
}