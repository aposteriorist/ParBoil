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
    using PM = ProjectManager;

    public partial class FileEditorForm : Form
    {
        public FileEditorForm(Node node)
        {
            InitializeComponent();

            this.node = node;
            file = node.GetFormatAs<RGGFormat>();
            Text += node.Name;

            WorkingFolder = PM.Project + node.Path[1..];

            if (!Directory.Exists(WorkingFolder))
                Directory.CreateDirectory(WorkingFolder);

            Directory.SetCurrentDirectory(WorkingFolder);


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

        private readonly Node node;
        private readonly RGGFormat file;
        private readonly string WorkingFolder;

        private readonly string current = "current.json";

        private readonly Color EditableColor = Color.FromArgb(45, 45, 45);
        private readonly Font EditorFont = new Font("MS Mincho", 18, FontStyle.Regular);


        private void WriteFileAsJSON(string jsoname)
        {
            if (!File.Exists(jsoname))
                file.ToJSONStream().WriteTo(jsoname);
        }

        private void SaveNewVersion()
        {
            file.ProcessEdits();

            uint count = 0;
            foreach (string file in Directory.GetFiles(WorkingFolder))
                if (file[^12..].StartsWith("ver"))
                    count++;

            File.Move(current, String.Format("ver{0:D4}.json", count), false);
            WriteFileAsJSON(current);
        }

        private void LoadVersion(uint version)
        {
            // Currently ignores all consequences of loading.
            string name = String.Format("ver{0:D4}.json", version);

            if (File.Exists(name))
            {
                using var json = DataStreamFactory.FromFile(name, FileOpenMode.Read);

                file.LoadFromJSON(json);
            }
        }


        private void CreateWorkingEnvironment() => WriteFileAsJSON(current);

        private void LoadWorkingEnvironment()
        {
            using var json = DataStreamFactory.FromFile(current, FileOpenMode.Read);

            file.LoadFromJSON(json);
        }

        private bool WorkingEnvironmentExists() => File.Exists(current);


        public void UpdateTitle()
        {
            if (file.EditedControls != null)
            {
                if (file.EditedControls.Count > 0 && Text[^1] != '*')
                {
                    Text += '*';
                }
                else if (file.EditedControls.Count == 0 && Text[^1] == '*')
                    Text = Text[..^1];
            }
        }

        private void FileEditorForm_Activated(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(WorkingFolder);
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

                SaveNewVersion();
                file.AsBinStream(overwrite: true).WriteTo(name);

                // The format's stream is the node's stream, so having edited it in RGGFormat.WriteToBin means that job's done.

                // When that's done, generate a makeshift dropdown version selector. Just put it on a little generated form, for now.
                // The generated form should move when the editor moves, and close when it closes.
            }

            Directory.SetCurrentDirectory(PM.Project);
        }

        private void FileEditorForm_Resize(object sender, EventArgs e) => file.Resize();
    }
}
