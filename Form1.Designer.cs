namespace vss
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBoxSearch = new TextBox();
            listBoxWindows = new ListBox();
            configButton = new Button();
            SuspendLayout();
            // 
            // textBoxSearch
            // 
            textBoxSearch.Location = new Point(20, 18);
            textBoxSearch.Margin = new Padding(5);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(601, 26);
            textBoxSearch.TabIndex = 0;
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            textBoxSearch.KeyDown += textBoxSearch_KeyDown;
            // 
            // listBoxWindows
            // 
            listBoxWindows.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            listBoxWindows.FormattingEnabled = true;
            listBoxWindows.ItemHeight = 31;
            listBoxWindows.Location = new Point(20, 58);
            listBoxWindows.Margin = new Padding(5);
            listBoxWindows.Name = "listBoxWindows";
            listBoxWindows.Size = new Size(601, 314);
            listBoxWindows.TabIndex = 1;
            listBoxWindows.KeyDown += listBoxWindows_KeyDown;
            listBoxWindows.MouseDoubleClick += listBoxWindows_MouseDoubleClick;
            // 
            // configButton
            // 
            configButton.Location = new Point(258, 380);
            configButton.Name = "configButton";
            configButton.Size = new Size(112, 34);
            configButton.TabIndex = 2;
            configButton.Text = "Config";
            configButton.UseVisualStyleBackColor = true;
            configButton.Click += configButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(635, 421);
            Controls.Add(configButton);
            Controls.Add(listBoxWindows);
            Controls.Add(textBoxSearch);
            Margin = new Padding(5);
            Name = "Form1";
            Text = "Window Switcher";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ListBox listBoxWindows;
        private Button configButton;
    }
}
