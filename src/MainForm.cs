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

        public MainForm()
        {
            InitializeComponent();
        }

        private void openPAR()
        {
            if (par != null)
                par.Dispose();

            OpenFileDialog dialogue = new OpenFileDialog();
            dialogue.Title = "Open PAR Archive";
            dialogue.Filter = "PARC archives (*.par)|*.par|All files (*.*)|*.*";

            if (dialogue.ShowDialog() == DialogResult.OK)
            {
                project = dialogue.FileName + ".boil\\";

                if (!Directory.Exists(project))
                    Directory.CreateDirectory(project);

                if (!File.Exists(project + dialogue.SafeFileName + ".orig"))
                    File.Copy(dialogue.FileName, project + dialogue.SafeFileName + ".orig");


                var parameters = new ParArchiveReaderParameters { Recursive = false, };

                par = NodeFactory.FromFile(dialogue.FileName);

                par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(parameters);

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
            }
        }

        private void tSMI_openPAR_Click(object sender, EventArgs e)
        {
            openPAR();
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

            switch(file.Format)
            {
                case MSGFormat:
                    OpenInFileEditor(file);
                    break;
            }            
        }

        private void TransformToFileTypeFormat (Node file)
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