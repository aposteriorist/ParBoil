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
            tS_SaveVersion = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            tS_Include = new ToolStripSplitButton();
            tS_Include_CurrentVersion = new ToolStripMenuItem();
            tS_Include_ChooseVersion = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            tS_Exclude = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            tS_Revert = new ToolStripButton();
            tS_VersionSelector = new ToolStripComboBox();
            tS_NewVersion_AutoName = new ToolStripMenuItem();
            tS_NewVersion_ChooseName = new ToolStripMenuItem();
            toolStripControls.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripControls
            // 
            toolStripControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            toolStripControls.Dock = DockStyle.None;
            toolStripControls.GripStyle = ToolStripGripStyle.Hidden;
            toolStripControls.Items.AddRange(new ToolStripItem[] { tS_NewVersion, toolStripSeparator1, tS_SaveVersion, toolStripSeparator2, tS_Include, toolStripSeparator3, tS_Exclude, toolStripSeparator4, tS_Revert, tS_VersionSelector });
            toolStripControls.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripControls.Location = new Point(-1, 1337);
            toolStripControls.Name = "toolStripControls";
            toolStripControls.RenderMode = ToolStripRenderMode.Professional;
            toolStripControls.Size = new Size(623, 25);
            toolStripControls.TabIndex = 0;
            toolStripControls.Text = "toolStrip1";
            // 
            // tS_NewVersion
            // 
            tS_NewVersion.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tS_NewVersion.DropDownItems.AddRange(new ToolStripItem[] { tS_NewVersion_AutoName, tS_NewVersion_ChooseName });
            tS_NewVersion.Enabled = false;
            tS_NewVersion.ForeColor = SystemColors.MenuText;
            tS_NewVersion.Image = (Image)resources.GetObject("tS_NewVersion.Image");
            tS_NewVersion.ImageTransparentColor = Color.Magenta;
            tS_NewVersion.Name = "tS_NewVersion";
            tS_NewVersion.Size = new Size(88, 22);
            tS_NewVersion.Text = "New Version";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // tS_SaveVersion
            // 
            tS_SaveVersion.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tS_SaveVersion.Enabled = false;
            tS_SaveVersion.ForeColor = SystemColors.MenuText;
            tS_SaveVersion.Image = (Image)resources.GetObject("tS_SaveVersion.Image");
            tS_SaveVersion.ImageTransparentColor = Color.Magenta;
            tS_SaveVersion.Name = "tS_SaveVersion";
            tS_SaveVersion.Size = new Size(76, 22);
            tS_SaveVersion.Text = "Save Version";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // tS_Include
            // 
            tS_Include.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tS_Include.DropDownItems.AddRange(new ToolStripItem[] { tS_Include_CurrentVersion, tS_Include_ChooseVersion });
            tS_Include.Enabled = false;
            tS_Include.ForeColor = SystemColors.MenuText;
            tS_Include.Image = (Image)resources.GetObject("tS_Include.Image");
            tS_Include.ImageTransparentColor = Color.Magenta;
            tS_Include.Name = "tS_Include";
            tS_Include.Size = new Size(99, 22);
            tS_Include.Text = "Include in PAR";
            // 
            // tS_Include_CurrentVersion
            // 
            tS_Include_CurrentVersion.Name = "tS_Include_CurrentVersion";
            tS_Include_CurrentVersion.Size = new Size(180, 22);
            tS_Include_CurrentVersion.Text = "Current Version";
            // 
            // tS_Include_ChooseVersion
            // 
            tS_Include_ChooseVersion.Name = "tS_Include_ChooseVersion";
            tS_Include_ChooseVersion.Size = new Size(180, 22);
            tS_Include_ChooseVersion.Text = "Select Version...";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // tS_Exclude
            // 
            tS_Exclude.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tS_Exclude.Enabled = false;
            tS_Exclude.ForeColor = SystemColors.MenuText;
            tS_Exclude.Image = (Image)resources.GetObject("tS_Exclude.Image");
            tS_Exclude.ImageTransparentColor = Color.Magenta;
            tS_Exclude.Name = "tS_Exclude";
            tS_Exclude.Size = new Size(107, 22);
            tS_Exclude.Text = "Exclude From PAR";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            // 
            // tS_Revert
            // 
            tS_Revert.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tS_Revert.Enabled = false;
            tS_Revert.ForeColor = SystemColors.MenuText;
            tS_Revert.Image = (Image)resources.GetObject("tS_Revert.Image");
            tS_Revert.ImageTransparentColor = Color.Magenta;
            tS_Revert.Name = "tS_Revert";
            tS_Revert.Size = new Size(72, 22);
            tS_Revert.Text = "Revert Edits";
            // 
            // tS_VersionSelector
            // 
            tS_VersionSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            tS_VersionSelector.Enabled = false;
            tS_VersionSelector.MaxDropDownItems = 16;
            tS_VersionSelector.Name = "tS_VersionSelector";
            tS_VersionSelector.Size = new Size(121, 25);
            tS_VersionSelector.SelectedIndexChanged += tS_VersionSelector_SelectedIndexChanged;
            // 
            // tS_NewVersion_AutoName
            // 
            tS_NewVersion_AutoName.Name = "tS_NewVersion_AutoName";
            tS_NewVersion_AutoName.Size = new Size(180, 22);
            tS_NewVersion_AutoName.Text = "Automatic Name";
            // 
            // tS_NewVersion_ChooseName
            // 
            tS_NewVersion_ChooseName.Name = "tS_NewVersion_ChooseName";
            tS_NewVersion_ChooseName.Size = new Size(180, 22);
            tS_NewVersion_ChooseName.Text = "With Name...";
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
        private ToolStripButton tS_Exclude;
        private ToolStripButton tS_Revert;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSplitButton tS_Include;
        private ToolStripMenuItem tS_Include_CurrentVersion;
        private ToolStripMenuItem tS_Include_ChooseVersion;
        private ToolStripComboBox tS_VersionSelector;
        private ToolStripSplitButton tS_NewVersion;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton tS_SaveVersion;
        private ToolStripMenuItem tS_NewVersion_AutoName;
        private ToolStripMenuItem tS_NewVersion_ChooseName;
    }
}