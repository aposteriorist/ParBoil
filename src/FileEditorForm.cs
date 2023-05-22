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

            file.GenerateControls(Size, ForeColor, EditableColor, BackColor, EditorFont);
            Controls.Clear();
            Controls.Add(file.Handle);

            Refresh();
        }

        private string project;
        private string path;
        private string name;
        private RGGFormat file;

        private string current = "current.json";

        private Color EditableColor = Color.FromArgb(45, 45, 45);
        private Font EditorFont = new Font("MS Mincho", 18, FontStyle.Regular);


        private void WriteFileAsJSON(string jsoname)
        {
            if (!File.Exists(jsoname))
                file.ToJSONStream().WriteTo(jsoname);
        }


        private void CreateWorkingEnvironment() => WriteFileAsJSON(current);

        private void LoadWorkingEnvironment()
        {
            using var json = DataStreamFactory.FromFile(current, FileOpenMode.Read);

            file.LoadFromJSON(json);
        }

        private bool WorkingEnvironmentExists() => File.Exists(current);


        private void FileEditorForm_Activated(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(path);
        }

        private void FileEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Focus(); // Take focus away from the currently-focused control to have its LostFocus event occur here.

            if (file.EditedControls.Count > 0)
            {
                if (MessageBox.Show("There are unsaved changes." +
                    "\n\n(For now, pressing OK will save changes to JSON, and output the file directly.)", "Warning", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                file.FormClosing();

                uint count = 0;
                // Write to the JSON on close. For now, make it version files automatically.
                foreach (string file in Directory.GetFiles(path))
                    if (file[^12..].StartsWith("ver"))
                        count++;

                File.Move(current, String.Format("ver{0:D4}.json", count), false);
                WriteFileAsJSON(current);

                // The format's stream is the node's stream, so having edited it in RGGFormat.WriteToBin means that job's done.

                // When that's done, generate a makeshift dropdown version selector. Just put it on a little generated form, for now.
                // The generated form should move when the editor moves, and close when it closes.
            }

            Directory.SetCurrentDirectory(project);
        }

        private void FileEditorForm_Resize(object sender, EventArgs e) => file.Resize();
    }
}
