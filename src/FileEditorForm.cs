using ParBoil.RGGFormats;
using ParLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yarhl.FileSystem;

namespace ParBoil
{
    public partial class FileEditorForm : Form
    {
        public FileEditorForm(Node node)
        {
            InitializeComponent();

            file = node.GetFormatAs<RGGFormat>();
            Text += node.Name;

            file.GenerateControls(Size, ForeColor, EditableColor, BackColor, Mincho);
            Controls.Clear();
            Controls.Add(file.Handle);

            Refresh();
        }

        public RGGFormat file;

        private Color EditableColor = Color.FromArgb(45, 45, 45);

        private Font Mincho = new Font("MS Mincho", 18, FontStyle.Regular);


        private void FileEditorForm_Resize(object sender, EventArgs e)
        {
            file.Resize();
        }

        private void FileEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (file.EditCount > 0 && MessageBox.Show("", "Warning", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                e.Cancel = true;
        }
    }
}
