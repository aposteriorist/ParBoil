namespace ParBoil
{
    partial class FileEditorForm
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
            mainPanel = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.AutoScroll = true;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Margin = new Padding(0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(1384, 1362);
            mainPanel.TabIndex = 0;
            // 
            // FileEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.FromArgb(60, 59, 55);
            ClientSize = new Size(1384, 1361);
            Controls.Add(mainPanel);
            ForeColor = Color.FromArgb(213, 213, 213);
            MaximumSize = new Size(1400, 1600);
            MinimumSize = new Size(1400, 500);
            Name = "FileEditorForm";
            ShowIcon = false;
            Text = "File Editor -- ";
            Resize += FileEditorForm_Resize;
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel mainPanel;
    }
}