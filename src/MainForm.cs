using ParBoil.RGGFormats;
using ParLibrary;
using ParLibrary.Converter;
using ParLibrary.Sllz;
using System.Diagnostics;
using System.Xml.Linq;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParBoil
{
    public partial class MainForm : Form
    {
        private Node par;
        private string project;

        private ParArchiveReaderParameters readerParams;
        private ParArchiveWriterParameters writerParams;
        private IDictionary<string, dynamic> parTags;

        public MainForm()
        {
            InitializeComponent();

            writerParams = new ParArchiveWriterParameters
            {
                CompressorVersion = 3,
                IncludeDots = false,
            };
        }

        private void OpenPAR()
        {
            OpenFileDialog dialogue = new OpenFileDialog();
            dialogue.Title = "Open PAR Archive";
            dialogue.Filter = "PARC archives (*.par)|*.par|All files (*.*)|*.*";

            if (dialogue.ShowDialog() == DialogResult.OK)
            {
                if (par != null)
                    par.Dispose();

                project = dialogue.FileName + ".boil\\";

                if (!Directory.Exists(project))
                    Directory.CreateDirectory(project);

                if (!File.Exists(project + dialogue.SafeFileName + ".orig"))
                    File.Copy(dialogue.FileName, project + dialogue.SafeFileName + ".orig");


                par = NodeFactory.FromFile(dialogue.FileName);

                parTags = new Dictionary<string, dynamic>();

                readerParams = new ParArchiveReaderParameters
                {
                    Recursive = false,
                    Tags = parTags,
                };

                par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParams);
                
                foreach (var tag in parTags)
                    par.Tags.Add(tag);

                treeViewPar.BeginUpdate();
                treeViewPar.Nodes.Clear();

                labelFileName.Text = Path.GetFileName(dialogue.FileName);

                TreeNode parent = null;
                TreeNode child;

                foreach (Node node in Navigator.IterateNodes(par))
                {
                    child = new TreeNode(node.Name);
                    child.Name = node.Name;
                    child.Tag = node;

                    if (parent == null || node.Parent.Name == labelFileName.Text)
                    {
                        parent = child;
                        treeViewPar.Nodes.Add(parent);
                    }

                    else
                    {
                        if (parent.Name != node.Parent.Name)
                        {
                            parent = treeViewPar.Nodes.Find(node.Parent.Name, true)[0];
                        }
                        parent.Nodes.Add(child);
                    }
                }

                foreach (TreeNode node in treeViewPar.Nodes)
                    if (node.Parent == null) node.Expand();

                treeViewPar.Nodes[0].EnsureVisible();
                treeViewPar.EndUpdate();

                Directory.SetCurrentDirectory(project);

                tSMI_savePARAs.Enabled = true;
            }
        }

        private void SavePARAs()
        {
            SaveFileDialog dialogue = new SaveFileDialog();
            dialogue.Title = "Save PAR Archive As";
            dialogue.Filter = "PARC archives (*.par)|*.par|All files (*.*)|*.*";

            if (dialogue.ShowDialog() == DialogResult.OK)
            {
                // SaveFileDialog has already asked the user if they're okay with overwriting,
                // so we're good to go.

                writerParams.OutputPath = dialogue.FileName;

                Enabled = false;
                DateTime startTime = DateTime.Now;
                Debug.WriteLine("Creating PAR...");
                // TO-DO: We need some clones.
                // The nuget release of YARHL doesn't have any cloning, but it's there on Github.
                // So I'll need to update the version being used myself.
                par.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(writerParams);
                Debug.WriteLine("Done.");
                DateTime endTime = DateTime.Now;
                Debug.WriteLine($"Time elapsed: {endTime - startTime:g}");
                Enabled = true;
            }
        }

        private void tSMI_openPAR_Click(object sender, EventArgs e)
        {
            OpenPAR();
        }

        private void tSMI_savePARAs_Click(object sender, EventArgs e)
        {
            SavePARAs();
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (par != null)
                par.Dispose();
        }

        private string getFileType(Node node)
        {
            return node.Name[^4..^1];
        }

        private void treeViewPar_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Node file = (Node)treeViewPar.SelectedNode.Tag;
            textBox_SelectedFileInfo.Text = treeViewPar.SelectedNode.Name;

            if (file.IsContainer)
            {
                textBox_SelectedFileInfo.Text += $"\r\n\r\nNumber of files: {file.Children.Count}";
            }
            else
            {
                textBox_SelectedFileInfo.Text += $"\r\n\r\nSize in PAR: {file.Stream.Length} bytes";
            }
        }

        private void treeViewPar_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenFileFormat();
        }

        private void OpenFileFormat()
        {
            if (treeViewPar.SelectedNode == null) return;


            Node file = (Node)treeViewPar.SelectedNode.Tag;

            if (file == null || file.IsContainer) return;

            if (file.Format is ParFile)
            {
                if (file.GetFormatAs<ParFile>().IsCompressed)
                    file.TransformWith<Decompressor>();
                TransformToFileTypeFormat(file);
            }

            switch (file.Format)
            {
                case MSGFormat:
                    OpenInFileEditor(file);
                    break;
            }
        }

        private void TransformToFileTypeFormat(Node file)
        {
            switch (file.Name[^4..])
            {
                case ".msg":
                    if (file.Format is not MSGFormat)
                        file.TransformWith<MSGFormat>();
                    break;
            }
        }

        private void OpenInFileEditor(Node file)
        {
            if (file.Format is not MSGFormat)
                return;

            var editor = new FileEditorForm(project, file);
            editor.Show();
        }
    }
}