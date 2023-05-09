using ParLibrary;
using ParLibrary.Converter;
using System.Diagnostics;
using System.Xml.Linq;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParBoil
{
    public partial class mainForm : Form
    {
        private Node par;

        public mainForm()
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

                    child.Tag = node;
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
            textBox_SelectedFileInfo.Text += $"\r\n\r\nSize in PAR: {file.Stream.Length} bytes";
        }
    }
}