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

        public Explorer(string[] args)
        {
            InitializeComponent();

            //Support "open with" from Windows on PAK files
            if (args.Length > 0 && File.Exists(args[0]))
            {
                OpenFileAndPopulateGUI(args[0]);
            }

            //Link image list to GUI elements for icons
            FileTree.ImageList = imageList1;

            //Run an update check
            UpdateCheck VersionControl = new UpdateCheck();
            VersionControl.Show();
        }

        /* Open a PAK and populate the GUI */
        private void OpenFileAndPopulateGUI(string filename)
        {
            //Open PAK
            AlienPAK.Open(filename);

            //Parse the PAK's file list
            List<string> ParsedFiles = new List<string>();
            ParsedFiles = AlienPAK.Parse();
            if (ParsedFiles == null)
            {
                MessageBox.Show("The selected PAK is currently unsupported.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Populate the GUI with the files found within the archive
            FileTree.Nodes.Clear();
            foreach (string FileName in ParsedFiles)
            {
                string[] FileNameParts = FileName.Split('/');
                if (FileNameParts.Length == 1) { FileNameParts = FileName.Split('\\'); }
                AddFileToTree(FileNameParts, 0, FileTree.Nodes);
            }
            UpdateSelectedFilePreview();
            
            //Update title
            this.Text = "Alien: Isolation PAK Tool - " + Path.GetFileName(filename);
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
                TreeItem ThisTag = new TreeItem();
                if (FileNameParts.Length-1 == index)
                {
                    //Node is a file
                    for (int i = 0; i < FileNameParts.Length; i++)
                    {
                        ThisTag.String_Value += FileNameParts[i] + "/";
                    }
                    ThisTag.String_Value = ThisTag.String_Value.ToString().Substring(0, ThisTag.String_Value.ToString().Length - 1);

                    ThisTag.Item_Type = TreeItemType.EXPORTABLE_FILE;
                    FileNode.ImageIndex = (int)TreeItemIcon.FILE;
                    FileNode.SelectedImageIndex = (int)TreeItemIcon.FILE;
                    FileNode.ContextMenuStrip = fileContextMenu;
                }
                else
                {
                    //Node is a directory
                    ThisTag.Item_Type = TreeItemType.DIRECTORY;
                    FileNode.ImageIndex = (int)TreeItemIcon.FOLDER;
                    FileNode.SelectedImageIndex = (int)TreeItemIcon.FOLDER;
                    AddFileToTree(FileNameParts, index + 1, FileNode.Nodes);
                }

                FileNode.Tag = ThisTag;
                LoopedNodeCollection.Add(FileNode);
            }
        }

        /* Get type description based on extension */
        private string GetFileTypeDescription(string FileExtension)
        {
            switch (FileExtension.Substring(1).ToUpper())
            {
                case "DDS":
                    return "DDS (Image)";
                case "TGA":
                    return "TGA (Image)";
                case "GFX":
                    return "GFX (Adobe Flash)";
                case "CS2":
                    return "CS2 (Model)";
                case "BIN":
                    return "BIN (Binary File)";
                case "BML":
                    return "BML (Binary XML)";
            }
            return "";
        }

        /* Update file preview */
        private void UpdateSelectedFilePreview()
        {
            //First, reset the GUI
            filePreviewImage.Image = null;
            fileNameInfo.Text = "";
            fileSizeInfo.Text = "";
            fileTypeInfo.Text = "";
            exportFile.Enabled = false;
            importFile.Enabled = false;

            //Exit early if not a file
            if (FileTree.SelectedNode == null || ((TreeItem)FileTree.SelectedNode.Tag).Item_Type != TreeItemType.EXPORTABLE_FILE)
            {
                return;
            }
            string FileName = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            //Populate filename/type info
            fileNameInfo.Text = Path.GetFileName(FileName);
            fileTypeInfo.Text = GetFileTypeDescription(Path.GetExtension(FileName));

            //Populate file size info
            int FileSize = AlienPAK.GetFileSize(FileName);
            if (FileSize == -1) { return; }
            fileSizeInfo.Text = FileSize.ToString() + " bytes";

            //Enable buttons
            exportFile.Enabled = true;
            importFile.Enabled = true;
        }

        /* Import a file to replace the selected PAK entry */
        private void ImportSelectedFile()
        {
            if (FileTree.SelectedNode == null || ((TreeItem)FileTree.SelectedNode.Tag).Item_Type != TreeItemType.EXPORTABLE_FILE)
            {
                MessageBox.Show("Please select a file from the list.", "No file selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Allow selection of a file (force extension), then drop it in
            OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Filter = "Import File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
            if (filePicker.ShowDialog() == DialogResult.OK)
            {
                switch (AlienPAK.ImportFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value, filePicker.FileName))
                {
                    case PAK.PAKReturnType.IMPORT_SUCCESS:
                        MessageBox.Show("The selected file was imported successfully.", "Imported file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case PAK.PAKReturnType.IMPORT_FAILED_UNSUPPORTED:
                        MessageBox.Show("An error occurred while importing the selected file.\nThe texture you are trying to replace is currently unsupported.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case PAK.PAKReturnType.IMPORT_FAILED_UNKNOWN:
                        MessageBox.Show("An error occurred while importing the selected file.\nIf Alien: Isolation is open, it must be closed.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case PAK.PAKReturnType.IMPORT_FAILED_LOGIC_ERROR:
                        MessageBox.Show("An error occurred while importing the selected file.\nPlease reload the PAK file.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            UpdateSelectedFilePreview();
        }

        /* Export the selected PAK entry as a standalone file */
        private void ExportSelectedFile()
        {
            if (FileTree.SelectedNode == null || ((TreeItem)FileTree.SelectedNode.Tag).Item_Type != TreeItemType.EXPORTABLE_FILE)
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
                switch (AlienPAK.ExportFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value, filePicker.FileName))
                {
                    case PAK.PAKReturnType.EXPORT_SUCCESS:
                        MessageBox.Show("The selected file was exported successfully.", "Exported file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case PAK.PAKReturnType.EXPORT_FAILED_UNSUPPORTED:
                        MessageBox.Show("An error occurred while exporting the selected file.\nThis file is currently unsupported.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case PAK.PAKReturnType.EXPORT_FAILED_UNKNOWN:
                        MessageBox.Show("An error occurred while exporting the selected file.\nMake sure you have write access to the export folder.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case PAK.PAKReturnType.EXPORT_FAILED_LOGIC_ERROR:
                        MessageBox.Show("An error occurred while exporting the selected file.\nPlease reload the PAK file.", "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
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

        /* Expand/collapse all nodes in the tree */
        private void expandAllDirectoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileTree.ExpandAll();
        }
        private void shrinkAllDirectoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileTree.CollapseAll();
        }

        /* Import/export selected file (main menu) */
        private void importFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportSelectedFile();
        }
        private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelectedFile();
        }

        /* Import/export selected file (context menu) */
        private void importFileContext_Click(object sender, EventArgs e)
        {
            ImportSelectedFile();
        }
        private void exportFileContext_Click(object sender, EventArgs e)
        {
            ExportSelectedFile();
        }

        /* Import/export selected file (gui buttons) */
        private void importFile_Click(object sender, EventArgs e)
        {
            ImportSelectedFile();
        }
        private void exportFile_Click(object sender, EventArgs e)
        {
            ExportSelectedFile();
        }

        /* Item selected (show preview info) */
        private void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateSelectedFilePreview();
        }
    }
}
