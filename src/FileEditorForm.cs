using ParBoil.RGGFormats;
using ParLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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


            if (!node.Tags.ContainsKey("LoadedVersions"))
            {
                node.Tags["LoadedVersions"] = new Dictionary<string, DataStream>();

                if (WorkingEnvironmentExists())
                {
                    LoadWorkingEnvironment();
                }
                else
                {
                    file.LoadFromBin();
                    CreateWorkingEnvironment();
                }
            }
            else
            {
                PopulateVersionSelector();
            }

            file.GenerateControls(Size, ForeColor, EditableColor, BackColor, EditorFont);
            Controls.Add(file.Handle);

            Refresh();
        }

        private readonly Node node;
        private readonly RGGFormat file;
        private readonly string WorkingFolder;

        private readonly string original = "Original.json";

        private readonly Color EditableColor = Color.FromArgb(45, 45, 45);
        private readonly Font EditorFont = new Font("MS Mincho", 18, FontStyle.Regular);


        private DataStream WriteFileAsJSON(string jsoname)
        {
            var jsonStream = file.ToJSONStream();

            if (!File.Exists(jsoname))
                jsonStream.WriteTo(jsoname);

            return jsonStream;
        }

        private void SaveNewVersion(string name = "", bool selectNewVersion = true)
        {
            file.ProcessEdits();
            UpdateTitle();

            if (name == "")
            {
                int count = Directory.GetFiles(WorkingFolder, "*.json").Length;

                name = $"Version {count}";
            }

            tS_VersionSelector.Items.Insert(0, name);
            node.Tags["LoadedVersions"][name] = WriteFileAsJSON($"{name}.json");

            if (selectNewVersion)
            {
                node.Tags["SelectedVersion"] = name;

                tS_VersionSelector.SelectedIndex = 0;
            }
        }

        private void LoadVersion(uint version)
        {
            // Currently ignores all consequences of loading.
            string name = String.Format("ver{0:D4}.json", version);

            if (File.Exists(name))
            {
                using var json = DataStreamFactory.FromFile(name, FileOpenMode.Read);

                file.LoadFromJSON(json);
                file.UpdateControls();
            }
        }

        private void PopulateVersionSelector()
        {
            foreach (var filename in Directory.EnumerateFiles(WorkingFolder, "*.json").OrderByDescending(f => File.GetCreationTime(f)))
                tS_VersionSelector.Items.Add(Path.GetFileNameWithoutExtension(filename));

        private void CreateWorkingEnvironment() => WriteFileAsJSON(current);
            tS_VersionSelector.SelectedItem = node.Tags["SelectedVersion"];
        }


        private void CreateWorkingEnvironment()
        {
            tS_VersionSelector.Items.Add(original[..^5]);

            node.Tags["LoadedVersions"][original[..^5]] = WriteFileAsJSON(original);
            node.Tags["SelectedVersion"] = original[..^5];

            tS_VersionSelector.SelectedIndex = 0;
        }

        private void LoadWorkingEnvironment()
        {
            // Instead of loading current.json, I want to load the most-recently created file.
            string mostRecentFile = null;
            foreach (var filename in Directory.EnumerateFiles(WorkingFolder, "*.json").OrderByDescending(f => File.GetCreationTime(f)))
            {
                mostRecentFile ??= filename;
                tS_VersionSelector.Items.Add(Path.GetFileNameWithoutExtension(filename));
            }

            var json = Program.CopyStreamFromFile(mostRecentFile, FileOpenMode.Read);

            file.LoadFromJSON(json);

            node.Tags["SelectedVersion"] = (string)tS_VersionSelector.Items[0];
            node.Tags["LoadedVersions"][node.Tags["SelectedVersion"]] = json;

            tS_VersionSelector.SelectedIndex = 0;
        }

        private bool WorkingEnvironmentExists() => File.Exists(original);


        public void UpdateTitle()
        {
            if (file.EditedControls != null)
            {
                if (file.EditedControls.Count > 0 && Text[^1] != '*')
                {
                    Text += '*';
                }
                else if (file.EditedControls.Count == 0 && Text[^1] == '*')
                {
                    Text = Text[..^1];
                }
            }
        }


        private void FileEditorForm_Activated(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(WorkingFolder);
        }

        private void FileEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (file.EditedControls.Count > 0)
            {
                var result = MessageBox.Show("There are unsaved changes." +
                    "\n\nDo you want to save your changes as a new version automatically?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    if (result == DialogResult.Yes)
                    {
                        SaveNewVersion();
                        PM.IncludeFile(node); // Automatic for now
                    }
                    else
                    {
                        // Nothing needs to be done. Controls are generated anew on form's open.
                        //file.EditedControls.Clear();
                    }

                    Directory.SetCurrentDirectory(PM.Project);
                }
            }
        }

        private void FileEditorForm_Resize(object sender, EventArgs e) => file.Resize();

        private void tS_VersionSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!node.Tags.ContainsKey("SelectedVersion") || tS_VersionSelector.SelectedIndex != tS_VersionSelector.Items.IndexOf(node.Tags["SelectedVersion"]))
            {
                if (file.EditedControls != null && file.EditedControls.Count > 0)
                {
                    // Do you want to save?
                    SaveNewVersion(selectNewVersion: false);
                }

                node.Tags["SelectedVersion"] = tS_VersionSelector.SelectedItem;

                if (!node.Tags["LoadedVersions"].ContainsKey(node.Tags["SelectedVersion"]))
                {
                    var json = Program.CopyStreamFromFile(node.Tags["SelectedVersion"] + ".json", FileOpenMode.Read);

                    node.Tags["LoadedVersions"][node.Tags["SelectedVersion"]] = json;
                }

                file.LoadFromJSON(node.Tags["LoadedVersions"][node.Tags["SelectedVersion"]]);
                file.UpdateControls();
            }
        }
    }
}
