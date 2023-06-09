using ParBoil.RGGFormats;
using System.Collections.Immutable;
using System.Data;
using System.Media;
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

            UpdateFileEditStatus();
        }

        private readonly Node node;
        private RGGFormat file;
        private readonly string WorkingFolder;

        private readonly Color EditableColor = Color.FromArgb(45, 45, 45);
        private readonly Font EditorFont = new Font("MS Mincho", 18, FontStyle.Regular);

        private bool respondToEdits = true;


        private DataStream WriteFileAsJSON(string jsoname, bool overwrite = false)
        {
            var jsonStream = file.ToJSONStream();

            if (overwrite || !File.Exists(jsoname))
            {
                using var jsonFile = DataStreamFactory.FromFile(jsoname, FileOpenMode.Write);
                jsonStream.WriteTo(jsonFile);
            }

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

            file.ApplyEdits();
            UpdateFileEditStatus();

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

        private void SaveVersion()
        {
            file.ApplyEdits();
            UpdateFileEditStatus();

            WriteFileAsJSON($"{node.Tags["SelectedVersion"]}.json", overwrite: true);
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
            // Load the most recently created file.
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


        public void UpdateFileEditStatus()
        {
            if (file.EditedControls != null && respondToEdits == true)
            {
                if (file.EditedControls.Count > 0 && Text[^1] != '*')
                {
                    Text += '*';
                    tS_Revert.Enabled = true;
                }
                else if (file.EditedControls.Count == 0 && Text[^1] == '*')
                {
                    Text = Text[..^1];
                    tS_Revert.Enabled = false;
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

                tS_Include.Enabled = (string)tS_VersionSelector.SelectedItem != PM.Original;

                UpdateFileEditStatus();
            }
        }

        private void tS_Revert_Click(object sender, EventArgs e)
        {
            file.RevertEdits();
            UpdateFileEditStatus();
        }

        private void tS_SaveVersion_Overwrite_Click(object sender, EventArgs e) => SaveVersion();
        private void tS_SaveNewVersion_DefaultName_Click(object sender, EventArgs e) => SaveNewVersion();

        private void tS_SaveNewVersion_WithName_Click(object sender, EventArgs e)
        {
            // Prompt for the new name.
            string name = "";
            var result = ShowNamePrompt(ref name);

            if (result != DialogResult.Cancel)
                SaveNewVersion(name);
        }

        private DialogResult ShowNamePrompt(ref string name)
        {
            var namePrompt = new Form()
            {
                Text = "Name the new version",
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(272, 110),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
            };

            var desc = new Label()
            {
                Text = "No more than 16 characters.\nNone of the following characters: \\/:*?\"<>|",
                Location = new Point(12, 12),
                AutoSize = true,
            };
            var box = new TextBox()
            {
                Location = new Point(16, 48),
                Multiline = false,
                MaxLength = 16,
                Width = 180,
            };
            var buttonOK = new Button()
            {
                Text = "Confirm",
                Location = new Point(12, 80),
                DialogResult = DialogResult.OK,
                Enabled = false,
            };
            var buttonDefault = new Button()
            {
                Text = "Use default",
                Location = new Point(buttonOK.Location.X + buttonOK.Width + 12, 80),
                DialogResult = DialogResult.Continue,
            };
            var buttonCancel = new Button()
            {
                Text = "Cancel",
                Location = new Point(buttonDefault.Location.X + buttonDefault.Width + 12, 80),
                DialogResult = DialogResult.Cancel,
            };


            var expr = "\\/:*?\"<>|".ToCharArray();
            var tip = new ToolTip()
            {
                InitialDelay = 0,
            };

            box.TextChanged += delegate
            {
                bool badBlood = box.Text.IndexOfAny(expr) != -1;
                if (badBlood)
                {
                    box.Undo();
                    SystemSounds.Beep.Play();
                    tip.Show("Disallowed characters.", box, box.Width + 8, 0, 3000);
                }
                buttonOK.Enabled = box.TextLength > 0;
                box.ClearUndo();
            };

            buttonDefault.Click += delegate { box.Text = ""; };


            namePrompt.Controls.AddRange(new Control[] { desc, box, buttonOK, buttonDefault, buttonCancel });
            namePrompt.AcceptButton = buttonOK;
            namePrompt.CancelButton = buttonCancel;

            var result = namePrompt.ShowDialog();
            name = box.Text;

            return result;
        }
    }
}
