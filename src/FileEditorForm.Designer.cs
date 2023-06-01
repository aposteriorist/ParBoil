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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileEditorForm));
            toolStripControls = new ToolStrip();
            tS_NewVersion = new ToolStripSplitButton();
            toolStripSeparator1 = new ToolStripSeparator();
            tSSB_Include = new ToolStripSplitButton();
            tSMI_Include_Current = new ToolStripMenuItem();
            iSMI_Include_Other = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            tSB_Exclude = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            tSB_Revert = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            tS_VersionSelector = new ToolStripComboBox();
            toolStripControls.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripControls
            // 
            toolStripControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            toolStripControls.Dock = DockStyle.None;
            toolStripControls.GripStyle = ToolStripGripStyle.Hidden;
            toolStripControls.Items.AddRange(new ToolStripItem[] { tS_NewVersion, toolStripSeparator1, tSSB_Include, toolStripSeparator2, tSB_Exclude, toolStripSeparator3, tSB_Revert, toolStripSeparator4, tS_VersionSelector });
            toolStripControls.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripControls.Location = new Point(-1, 1337);
            toolStripControls.Name = "toolStripControls";
            toolStripControls.RenderMode = ToolStripRenderMode.Professional;
            toolStripControls.Size = new Size(574, 25);
            toolStripControls.TabIndex = 0;
            toolStripControls.Text = "toolStrip1";
            // 
            // tS_NewVersion
            // 
            tS_NewVersion.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tS_NewVersion.ForeColor = SystemColors.MenuText;
            tS_NewVersion.Image = (Image)resources.GetObject("tS_NewVersion.Image");
            tS_NewVersion.ImageTransparentColor = Color.Magenta;
            tS_NewVersion.Name = "tS_NewVersion";
            tS_NewVersion.Size = new Size(115, 22);
            tS_NewVersion.Text = "Save New Version";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // tSSB_Include
            // 
            tSSB_Include.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tSSB_Include.DropDownItems.AddRange(new ToolStripItem[] { tSMI_Include_Current, iSMI_Include_Other });
            tSSB_Include.Image = (Image)resources.GetObject("tSSB_Include.Image");
            tSSB_Include.ImageTransparentColor = Color.Magenta;
            tSSB_Include.Name = "tSSB_Include";
            tSSB_Include.Size = new Size(99, 22);
            tSSB_Include.Text = "Include in PAR";
            // 
            // tSMI_Include_Current
            // 
            tSMI_Include_Current.Name = "tSMI_Include_Current";
            tSMI_Include_Current.Size = new Size(169, 22);
            tSMI_Include_Current.Text = "Current Version";
            // 
            // iSMI_Include_Other
            // 
            iSMI_Include_Other.Name = "iSMI_Include_Other";
            iSMI_Include_Other.Size = new Size(169, 22);
            iSMI_Include_Other.Text = "Previous Version...";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // tSB_Exclude
            // 
            tSB_Exclude.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tSB_Exclude.Enabled = false;
            tSB_Exclude.ForeColor = SystemColors.MenuText;
            tSB_Exclude.Image = (Image)resources.GetObject("tSB_Exclude.Image");
            tSB_Exclude.ImageTransparentColor = Color.Magenta;
            tSB_Exclude.Name = "tSB_Exclude";
            tSB_Exclude.Size = new Size(107, 22);
            tSB_Exclude.Text = "Exclude From PAR";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // tSB_Revert
            // 
            tSB_Revert.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tSB_Revert.Enabled = false;
            tSB_Revert.ForeColor = SystemColors.MenuText;
            tSB_Revert.Image = (Image)resources.GetObject("tSB_Revert.Image");
            tSB_Revert.ImageTransparentColor = Color.Magenta;
            tSB_Revert.Name = "tSB_Revert";
            tSB_Revert.Size = new Size(72, 22);
            tSB_Revert.Text = "Revert Edits";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            // 
            // tS_VersionSelector
            // 
            tS_VersionSelector.MaxDropDownItems = 16;
            tS_VersionSelector.Name = "tS_VersionSelector";
            tS_VersionSelector.Size = new Size(121, 25);
            tS_VersionSelector.SelectedIndexChanged += tS_VersionSelector_SelectedIndexChanged;
            // 
            // FileEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.FromArgb(60, 59, 55);
            ClientSize = new Size(1384, 1361);
            Controls.Add(toolStripControls);
            ForeColor = Color.FromArgb(213, 213, 213);
            MaximumSize = new Size(1400, 1600);
            MinimumSize = new Size(1400, 500);
            Name = "FileEditorForm";
            ShowIcon = false;
            Text = "File Editor -- ";
            Activated += FileEditorForm_Activated;
            FormClosing += FileEditorForm_FormClosing;
            Resize += FileEditorForm_Resize;
            toolStripControls.ResumeLayout(false);
            toolStripControls.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStripControls;
        private ToolStripButton tSB_Include;
        private ToolStripButton tSB_Exclude;
        private ToolStripButton tSB_Revert;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSplitButton tSSB_Include;
        private ToolStripMenuItem tSMI_Include_Current;
        private ToolStripMenuItem iSMI_Include_Other;
        private ToolStripComboBox tS_VersionSelector;
        private ToolStripSplitButton tS_NewVersion;
        private ToolStripSeparator toolStripSeparator4;
    }
}