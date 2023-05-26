using ParBoil.RGGFormats;
using ParLibrary;
using ParLibrary.Converter;
using ParLibrary.Sllz;
using System.Diagnostics;
using System.Xml.Linq;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParBoil;

using PM = ProjectManager;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        PM.Initialize();
    }

    private void OpenPAR()
    {
        OpenFileDialog dialogue = new OpenFileDialog();
        dialogue.Title = "Open PAR Archive";
        dialogue.Filter = "RGG PAR archives (*.par)|*.par|All files (*.*)|*.*";

        if (dialogue.ShowDialog() == DialogResult.OK)
        {
            PM.FromFile(dialogue.FileName);

            treeViewPar.BeginUpdate();
            treeViewPar.Nodes.Clear();

            labelFileName.Text = Path.GetFileName(dialogue.FileName);

            TreeNode parent = null;
            TreeNode child;

            foreach (Node node in Navigator.IterateNodes(PM.Par))
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

            tSMI_savePARAs.Enabled = true;
        }
    }

    // TO-DO: Test saving with compression of a single file, I suppose in comparison with PARC Shinada.
    // And eventually, loading the project will also need to mean, on PAR load, grab every file with edits.
    // Not sure how I'll go about doing that.
    private void SavePARAs()
    {
        SaveFileDialog dialogue = new SaveFileDialog();
        dialogue.Title = "Save PAR Archive As";
        dialogue.Filter = "RGG PAR archives (*.par)|*.par|All files (*.*)|*.*";

        if (dialogue.ShowDialog() == DialogResult.OK)
        {
            // SaveFileDialog has already asked the user if they're okay with overwriting,
            // so we're good to go.

            Enabled = false;

            PM.ToFile(dialogue.FileName);

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
        ProjectManager.Close();
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

            var editor = new FileEditorForm(ProjectManager.Project, file);
        editor.Show();
    }
}