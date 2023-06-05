using ParBoil.RGGFormats;
using System.Collections.Immutable;
using System.Data;
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


            if (!node.Tags.ContainsKey("SelectedVersion"))
            {
                if (!node.Tags.ContainsKey("IncludedVersion"))
                {
                    node.Tags["LoadedVersions"] = new Dictionary<string, RGGFormat>();

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
                    node.Tags["LoadedVersions"][PM.Original].GenerateControls(Size, ForeColor, EditableColor, BackColor, EditorFont);
                    Controls.Add(node.Tags["LoadedVersions"][PM.Original].Handle);

                    PopulateVersionSelector();
                    LoadVersion(node.Tags["IncludedVersion"]);
                    node.Tags["SelectedVersion"] = node.Tags["IncludedVersion"];
                    tS_VersionSelector.SelectedItem = node.Tags["IncludedVersion"];
                }

                file.GenerateControls(Size, ForeColor, EditableColor, BackColor, EditorFont);
            }
            else
            {
                PopulateVersionSelector();

                foreach (RGGFormat format in node.Tags["LoadedVersions"].Values)
                    Controls.Add(format.Handle);

                tS_VersionSelector.SelectedItem = node.Tags["SelectedVersion"];
            }

            tS_VersionSelector.Enabled = tS_VersionSelector.Items.Count > 1;

            if (!Controls.Contains(file.Handle))
                Controls.Add(file.Handle);

            if (node.Tags["LoadedVersions"].ContainsKey(node.Tags["SelectedVersion"]))
                Controls.SetChildIndex(node.Tags["LoadedVersions"][node.Tags["SelectedVersion"]].Handle, 1);
            else
                Controls.SetChildIndex(file.Handle, 1);

            UpdateTitle();
        }

        private readonly Node node;
        private RGGFormat file;
        private readonly string WorkingFolder;

        private readonly Color EditableColor = Color.FromArgb(45, 45, 45);
        private readonly Font EditorFont = new Font("MS Mincho", 18, FontStyle.Regular);

        private bool respondToEdits = true;


        private DataStream WriteFileAsJSON(string jsoname)
        {
            var jsonStream = file.ToJSONStream();

            if (!File.Exists(jsoname))
                jsonStream.WriteTo(jsoname);

            return jsonStream;
        }

        private void SaveNewVersion(string name = "", bool selectNewVersion = true)
        {
            if (!node.Tags["LoadedVersions"].ContainsKey(node.Tags["SelectedVersion"]))
            {
                var previousVersion = file.CopyFormat(duplicateStream: true);
                previousVersion.GenerateControls(Size, ForeColor, EditableColor, BackColor, EditorFont);
                node.Tags["LoadedVersions"][node.Tags["SelectedVersion"]] = previousVersion;
                if (!Controls.Contains(previousVersion.Handle))
                    Controls.Add(previousVersion.Handle);
            }

            file.ProcessEdits();
            UpdateTitle();

            if (name == "")
            {
                int count = Directory.GetFiles(WorkingFolder, "*.json").Length;

                name = $"Version {count}";
            }

            tS_VersionSelector.Items.Insert(0, name);
            WriteFileAsJSON($"{name}.json");

            if (selectNewVersion)
            {
                node.Tags["SelectedVersion"] = name;

                tS_VersionSelector.SelectedIndex = 0;
            }
        }

        private void LoadVersion(string version)
        {
            string name = version + ".json";

            if (File.Exists(name))
            {
                using var json = DataStreamFactory.FromFile(name, FileOpenMode.Read);
                file.LoadFromJSON(json);
            }
        }

        private void PopulateVersionSelector()
        {
            foreach (var filename in Directory.EnumerateFiles(WorkingFolder, "*.json").OrderByDescending(f => File.GetCreationTime(f)))
                tS_VersionSelector.Items.Add(Path.GetFileNameWithoutExtension(filename));
        }


        private void CreateWorkingEnvironment()
        {
            tS_VersionSelector.Items.Add(PM.Original);

            WriteFileAsJSON(PM.Original + ".json");

            node.Tags["SelectedVersion"] = PM.Original;

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

            using var json = DataStreamFactory.FromFile(mostRecentFile, FileOpenMode.Read);

            file.LoadFromJSON(json);

            node.Tags["SelectedVersion"] = (string)tS_VersionSelector.Items[0];

            tS_VersionSelector.SelectedIndex = 0;
        }

        private bool WorkingEnvironmentExists() => File.Exists(PM.Original + ".json");


        public void UpdateTitle()
        {
            if (file.EditedControls != null && respondToEdits == true)
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
                    return;
                }
                else
                {
                    if (result == DialogResult.Yes)
                    {
                        SaveNewVersion(); // Automatic name for now
                        file.UpdateStream(overwrite: true);
                        PM.IncludeFile(node, file); // Automatic for now
                        node.Tags["IncludedVersion"] = tS_VersionSelector.SelectedItem;
                    }
                    else
                    {
                        // Controls are no longer regenerated on editor open.
                        // Currently, then, the edits remain, just unsaved.
                    }

                    Directory.SetCurrentDirectory(PM.Project);
                }
            }

            // We don't want the controls disposed of. Getting rid of all controls is fine,
            // because the new form will always have its own instances of its native controls.
            Controls.Clear();
        }

        private void FileEditorForm_Resize(object sender, EventArgs e) => file.Resize();

        private void tS_VersionSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            // (!node.Tags.ContainsKey("SelectedVersion") should be an error if it occurs here.
            if (tS_VersionSelector.SelectedIndex != tS_VersionSelector.Items.IndexOf(node.Tags["SelectedVersion"]))
            {
                if (file.EditedControls != null && file.EditedControls.Count > 0)
                {
                    // Do you want to save your changes as a new version automatically?
                    SaveNewVersion(selectNewVersion: false);
                }
                else if (!node.Tags["LoadedVersions"].ContainsKey(node.Tags["SelectedVersion"]))
                {
                    var previousVersion = file.CopyFormat(duplicateStream: true);
                    previousVersion.GenerateControls(Size, ForeColor, EditableColor, BackColor, EditorFont);
                    node.Tags["LoadedVersions"][node.Tags["SelectedVersion"]] = previousVersion;
                    if (!Controls.Contains(previousVersion.Handle))
                        Controls.Add(previousVersion.Handle);
                }

                node.Tags["SelectedVersion"] = tS_VersionSelector.SelectedItem;

                if (!node.Tags["LoadedVersions"].ContainsKey(node.Tags["SelectedVersion"]))
                {
                    LoadVersion(node.Tags["SelectedVersion"]);

                    respondToEdits = false;
                    file.UpdateControls();
                    respondToEdits = true;
                }
                else
                {
                    file = node.Tags["LoadedVersions"][node.Tags["SelectedVersion"]];
                    if (!Controls.Contains(file.Handle))
                        Controls.Add(file.Handle);
                }

                Controls.SetChildIndex(file.Handle, 1);

                tS_Include.Enabled = tS_VersionSelector.SelectedItem != PM.Original;

                UpdateTitle();
            }
        }
    }
}
