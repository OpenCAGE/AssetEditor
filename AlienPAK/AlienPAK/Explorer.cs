using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class Explorer : Form
    {
        PAK AlienPAK = new PAK();

        public Explorer()
        {
            InitializeComponent();
        }

        /* Open a PAK and populate the GUI */
        private void OpenFileAndPopulateGUI(string filename)
        {
            //Open PAK
            AlienPAK.Open(filename);

            //Parse the PAK depending on its format
            List<string> ParsedFiles = new List<string>();
            switch (AlienPAK.Format)
            {
                case PAK.PAKType.PAK2:
                    ParsedFiles = AlienPAK.ParsePAK2();
                    break;
                default:
                    MessageBox.Show("The selected PAK is currently unsupported.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            //Populate the GUI with the files found within the archive
            FileTree.Nodes.Clear();
            foreach (string FileName in ParsedFiles)
            {
                /* THIS IS CURRENTLY BUGGED TO NOT ADD SOME FILES DEPENDING ON THE NUMBER OF RECURSIONS... */
                string[] FileNameParts = FileName.Split('/');
                if (FileNameParts.Length == 1) { FileNameParts = FileName.Split('\\'); }
                AddFileToTree(FileNameParts, 0, FileTree.Nodes);
            }
        }

        /* Add a file to the GUI tree structure */
        private void AddFileToTree(string[] FileNameParts, int index, TreeNodeCollection LoopedNodeCollection)
        {
            if (FileNameParts.Length <= index)
            {
                return;
            }
            
            bool should = true;
            foreach (TreeNode ThisFileNode in LoopedNodeCollection)
            {
                if (ThisFileNode.Text == FileNameParts[index])
                {
                    should = false;
                    AddFileToTree(FileNameParts, index + 1, ThisFileNode.Nodes);
                    break;
                }
            }
            if (should)
            {
                TreeNode FileNode = new TreeNode(FileNameParts[index]);
                if (FileNameParts.Length-1 == index)
                {
                    //Node is a file, tag it with the path
                    for (int i = 0; i < FileNameParts.Length; i++)
                    {
                        FileNode.Tag += FileNameParts[i] + "/";
                    }
                    FileNode.Tag = FileNode.Tag.ToString().Substring(0, FileNode.Tag.ToString().Length - 1);
                }
                else
                {
                    //Node is a directory
                    AddFileToTree(FileNameParts, index + 1, FileNode.Nodes);
                }
                LoopedNodeCollection.Add(FileNode);
            }
        }

        /* Form loads */
        private void Form1_Load(object sender, EventArgs e)
        {
            //For testing purposes
            //OpenFileAndPopulateGUI(@"E:\Program Files\Steam\steamapps\common\Alien Isolation\DATA\UI.PAK");
        }

        /* User requests to open a PAK */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Allow selection of a PAK from filepicker, then open
            OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Filter = "Alien: Isolation PAK|*.PAK";
            if (filePicker.ShowDialog() == DialogResult.OK)
            {
                OpenFileAndPopulateGUI(filePicker.FileName);
            }
        }

        /* User requests to export a file from the PAK */
        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (FileTree.SelectedNode == null)
            {
                MessageBox.Show("Please select a file from the list.", "No file selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Let the user decide where to save, then save
            SaveFileDialog filePicker = new SaveFileDialog();
            filePicker.Filter = "Exported File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
            filePicker.FileName = Path.GetFileName(FileTree.SelectedNode.Text);
            if (filePicker.ShowDialog() == DialogResult.OK)
            {
                bool ExportSuccess = false;
                switch (AlienPAK.Format)
                {
                    case PAK.PAKType.PAK2:
                        ExportSuccess = AlienPAK.ExportFilePAK2(FileTree.SelectedNode.Tag.ToString(), filePicker.FileName);
                        break;
                    default:
                        MessageBox.Show("This PAK does not support file exporting.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                if (ExportSuccess)
                {
                    MessageBox.Show("The selected file was exported successfully.", "Exported file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("An error occurred while exporting the selected file.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /* User requests to import a file to the PAK */
        private void ImportButton_Click(object sender, EventArgs e)
        {
            if (FileTree.SelectedNode == null)
            {
                MessageBox.Show("Please select a file from the list.", "No file selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Allow selection of a file (force extension), then drop it in
            OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Filter = "Import File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
            if (filePicker.ShowDialog() == DialogResult.OK)
            {
                bool ImportSuccess = false;
                switch (AlienPAK.Format)
                {
                    case PAK.PAKType.PAK2:
                        ImportSuccess = AlienPAK.ImportFilePAK2(FileTree.SelectedNode.Tag.ToString(), filePicker.FileName);
                        break;
                    default:
                        MessageBox.Show("This PAK does not support file importing.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                if (ImportSuccess)
                {
                    MessageBox.Show("The selected file was imported successfully.", "Imported file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("An error occurred while importing the selected file.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /* Expand/collapse all nodes in the tree */
        private void ExpandTree_Click(object sender, EventArgs e)
        {
            FileTree.ExpandAll();
        }
        private void ShrinkTree_Click(object sender, EventArgs e)
        {
            FileTree.CollapseAll();
        }
    }
}
