using ParBoil.RGGFormats;
using ParLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParBoil
{
    public partial class FileEditorForm : Form
    {
        public FileEditorForm(string project, Node node)
        {
            InitializeComponent();

            file = node.GetFormatAs<RGGFormat>();
            Text += node.Name;
            name = node.Name;

            path = project + node.Path[1..];

            if (WorkingEnvironmentExists())
            {
                LoadWorkingEnvironment();
            }
            else
            {
                file.LoadFromBin();
                CreateWorkingEnvironment();
            }

            file.GenerateControls(Size, ForeColor, EditableColor, BackColor, Mincho);
            Controls.Clear();
            Controls.Add(file.Handle);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            

            Refresh();
        }

        private string path;
        private string name;
        private RGGFormat file;

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

        private void FileToJSON(string jsoname)
        {
            if (!File.Exists(jsoname))
                File.WriteAllBytes($"{path}\\{jsoname}.json", file.ToJSON());
        }

        private void LoadWorkingEnvironment()
        {
            using var json = DataStreamFactory.FromFile(path, FileOpenMode.Read);

            file.LoadFromJSON(json);
        }

        private void CreateWorkingEnvironment()
        {
            FileToJSON("orig");
            //FileToJSON("current");
            File.Copy($"{path}\\orig.json", $"{path}\\current.json");
        }

        private bool WorkingEnvironmentExists()
        {
            return File.Exists($"{path}\\orig.json") && File.Exists($"{path}\\current.json");
        }
    }
}
