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

            this.project = project;

            path = project + node.Path[1..];

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Directory.SetCurrentDirectory(path);


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

            Refresh();
        }

        private string project;
        private string path;
        private string name;
        private RGGFormat file;

        private string original = "orig.json";
        private string current  = "current.json";

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

            uint count = 1;
            // Write to the JSON on close. For now, make it version files automatically.
            foreach (string file in Directory.GetFiles(path))
                if (file[^12..].StartsWith("ver"))
                    count++;

            File.Move(current, String.Format("ver{0:D4}.json", count), false);
            WriteFileAsJSON(current);

            // We should also copy the MSG fields back into the stream, then get that stream back into the node.
            // Assuming that doesn't already occur when we edit the file's stream, but I don't think it will.

            // When that's done, generate a makeshift dropdown version selector. Just put it on a little generated form, for now.
            // The generated form should move when the editor moves, and close when it closes.

            Directory.SetCurrentDirectory(project);
        }

        private void WriteFileAsJSON(string jsoname)
        {
            if (!File.Exists(jsoname))
                file.ToJSONStream().WriteTo(jsoname);

        }

        private void LoadWorkingEnvironment()
        {
            using var json = DataStreamFactory.FromFile(current, FileOpenMode.Read);

            file.LoadFromJSON(json);
        }

        private void CreateWorkingEnvironment()
        {
            WriteFileAsJSON(original);
            File.Copy(original, current);
        }

        private bool WorkingEnvironmentExists()
        {
            return File.Exists(original) && File.Exists(current);
        }

        private void FileEditorForm_Activated(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(path);
        }
    }
}
