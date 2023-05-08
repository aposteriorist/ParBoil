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
            listView1 = new ListView();
            fileNumber = new ColumnHeader();
            fileName = new ColumnHeader();
            menuStripMain = new MenuStrip();
            tSMI_File = new ToolStripMenuItem();
            tSMI_openPAR = new ToolStripMenuItem();
            tSMI_Help = new ToolStripMenuItem();
            tSMI_About = new ToolStripMenuItem();
            treeView1 = new TreeView();
            menuStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listView1.BackColor = Color.FromArgb(39, 38, 35);
            listView1.BorderStyle = BorderStyle.FixedSingle;
            listView1.Columns.AddRange(new ColumnHeader[] { fileNumber, fileName });
            listView1.ForeColor = Color.FromArgb(213, 213, 213);
            listView1.Location = new Point(0, 27);
            listView1.Name = "listView1";
            listView1.Size = new Size(500, 600);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.List;
            // 
            // fileNumber
            // 
            fileNumber.Text = "#";
            // 
            // fileName
            // 
            fileName.Text = "File Name";
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
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            treeView1.BackColor = Color.FromArgb(39, 38, 35);
            treeView1.BorderStyle = BorderStyle.FixedSingle;
            treeView1.ForeColor = Color.FromArgb(213, 213, 213);
            treeView1.LineColor = Color.FromArgb(213, 213, 213);
            treeView1.Location = new Point(509, 34);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(265, 301);
            treeView1.TabIndex = 2;
            // 
            // mainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(60, 59, 55);
            ClientSize = new Size(784, 661);
            Controls.Add(treeView1);
            Controls.Add(listView1);
            Controls.Add(menuStripMain);
            ForeColor = Color.FromArgb(213, 213, 213);
            MainMenuStrip = menuStripMain;
            Name = "mainForm";
            ShowIcon = false;
            Text = "ParBoil";
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listView1;
        private MenuStrip menuStripMain;
        private ToolStripMenuItem tSMI_File;
        private ToolStripMenuItem tSMI_Help;
        private ToolStripMenuItem tSMI_About;
        private ToolStripMenuItem tSMI_openPAR;
        private ColumnHeader fileNumber;
        private ColumnHeader fileName;
        private TreeView treeView1;
    }
}