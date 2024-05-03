using System;
using System.Drawing;
using System.Windows.Forms;

namespace vss.Utils
{
    internal class TaskbarController
    {
        private Form1 view;

        public TaskbarController(Form1 view)
        {
            this.view = view;
        }

        public void ShowMainForm()
        {
            view.Show();
            view.WindowState = FormWindowState.Normal;
            view.Activate();
        }

        public void ExitApplication()
        {
            Application.Exit();
        }

        public Icon CreateIcon()
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

            nint hIcon = bitmap.GetHicon();
            return Icon.FromHandle(hIcon);
        }
    }
}