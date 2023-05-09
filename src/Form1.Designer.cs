namespace ParBoil
{
    partial class mainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStripMain = new MenuStrip();
            tSMI_File = new ToolStripMenuItem();
            tSMI_openPAR = new ToolStripMenuItem();
            tSMI_Help = new ToolStripMenuItem();
            tSMI_About = new ToolStripMenuItem();
            treeViewPar = new TreeView();
            labelFileName = new Label();
            menuStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripMain
            // 
            menuStripMain.BackColor = Color.FromArgb(60, 59, 55);
            menuStripMain.GripMargin = new Padding(0);
            menuStripMain.Items.AddRange(new ToolStripItem[] { tSMI_File, tSMI_Help });
            menuStripMain.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            menuStripMain.Location = new Point(0, 0);
            menuStripMain.Name = "menuStripMain";
            menuStripMain.RenderMode = ToolStripRenderMode.System;
            menuStripMain.Size = new Size(784, 24);
            menuStripMain.TabIndex = 1;
            menuStripMain.Text = "Main Menu";
            // 
            // tSMI_File
            // 
            tSMI_File.DropDownItems.AddRange(new ToolStripItem[] { tSMI_openPAR });
            tSMI_File.ForeColor = Color.FromArgb(213, 213, 213);
            tSMI_File.Name = "tSMI_File";
            tSMI_File.Size = new Size(37, 20);
            tSMI_File.Text = "File";
            // 
            // tSMI_openPAR
            // 
            tSMI_openPAR.BackColor = Color.FromArgb(87, 87, 87);
            tSMI_openPAR.ForeColor = SystemColors.ControlText;
            tSMI_openPAR.Name = "tSMI_openPAR";
            tSMI_openPAR.Size = new Size(136, 22);
            tSMI_openPAR.Text = "Open PAR...";
            tSMI_openPAR.Click += tSMI_openPAR_Click;
            // 
            // tSMI_Help
            // 
            tSMI_Help.DropDownItems.AddRange(new ToolStripItem[] { tSMI_About });
            tSMI_Help.ForeColor = Color.FromArgb(213, 213, 213);
            tSMI_Help.Name = "tSMI_Help";
            tSMI_Help.Size = new Size(44, 20);
            tSMI_Help.Text = "Help";
            // 
            // tSMI_About
            // 
            tSMI_About.Name = "tSMI_About";
            tSMI_About.Size = new Size(116, 22);
            tSMI_About.Text = "About...";
            // 
            // treeViewPar
            // 
            treeViewPar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeViewPar.BackColor = Color.FromArgb(39, 38, 35);
            treeViewPar.BorderStyle = BorderStyle.FixedSingle;
            treeViewPar.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
            treeViewPar.ForeColor = Color.FromArgb(213, 213, 213);
            treeViewPar.Indent = 12;
            treeViewPar.LineColor = Color.FromArgb(213, 213, 213);
            treeViewPar.Location = new Point(12, 49);
            treeViewPar.Name = "treeViewPar";
            treeViewPar.PathSeparator = "/";
            treeViewPar.Size = new Size(500, 600);
            treeViewPar.TabIndex = 2;
            // 
            // labelFileName
            // 
            labelFileName.AutoSize = true;
            labelFileName.Location = new Point(12, 31);
            labelFileName.Name = "labelFileName";
            labelFileName.Size = new Size(81, 15);
            labelFileName.TabIndex = 3;
            labelFileName.Text = "No file loaded";
            // 
            // mainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(60, 59, 55);
            ClientSize = new Size(784, 661);
            Controls.Add(labelFileName);
            Controls.Add(treeViewPar);
            Controls.Add(menuStripMain);
            ForeColor = Color.FromArgb(213, 213, 213);
            MainMenuStrip = menuStripMain;
            Name = "mainForm";
            ShowIcon = false;
            Text = "ParBoil";
            FormClosing += mainForm_FormClosing;
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip menuStripMain;
        private ToolStripMenuItem tSMI_File;
        private ToolStripMenuItem tSMI_Help;
        private ToolStripMenuItem tSMI_About;
        private ToolStripMenuItem tSMI_openPAR;
        private TreeView treeViewPar;
        private Label labelFileName;
    }
}