using System;
using System.Drawing;
using System.Windows.Forms;

namespace vss
{
    internal class TaskbarManager
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;
        private Form1 mainForm;

        public TaskbarManager(Form1 form)
        {
            mainForm = form;
            RegisterContextMenuStrip();
            CreateTrayIcon();
        }

        private void CreateTrayIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = CreateIcon();
            notifyIcon.Visible = true;
            notifyIcon.Text = "My Switcher Program";
            notifyIcon.DoubleClick += (sender, e) => mainForm.ActivateWindow();
            notifyIcon.ContextMenuStrip = trayMenu;
            mainForm.FormClosed += (sender, e) => notifyIcon.Dispose();
        }

        private void RegisterContextMenuStrip()
        {
            trayMenu = new ContextMenuStrip();
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += ExitItem_Click;
            trayMenu.Items.Add(exitItem);

            ToolStripMenuItem showItem = new ToolStripMenuItem("Show");
            showItem.Click += ShowItem_Click;
            trayMenu.Items.Add(showItem);
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            mainForm.Show();
            mainForm.WindowState = FormWindowState.Normal;
            mainForm.Activate();
        }

        private Icon CreateIcon()
        {
            int size = 16;
            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                int padding = 2;
                int squareSize = size - 2 * padding;
                graphics.FillRectangle(Brushes.Black, padding, padding, squareSize, squareSize);
            }

            IntPtr hIcon = bitmap.GetHicon();
            return Icon.FromHandle(hIcon);
        }
    }
}