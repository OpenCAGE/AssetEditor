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
        ErrorMessages AlienErrors = new ErrorMessages();
        ToolOptionsHandler ToolSettings = new ToolOptionsHandler();

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
            Cursor.Current = Cursors.WaitCursor;
            AlienPAK.Open(filename);

            //Parse the PAK's file list
            List<string> ParsedFiles = AlienPAK.Parse();
            if (ParsedFiles == null || ParsedFiles.Count == 0)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("The selected PAK is currently unsupported, or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Populate the GUI with the files found within the archive
            UpdateFileTree(ParsedFiles);

            //Update title
            this.Text = "Alien: Isolation PAK Tool - " + Path.GetFileName(filename);
            Cursor.Current = Cursors.Default;

            //Show/hide extended archive support if appropriate
            if (AlienPAK.Format == PAKType.PAK2)
            {
                groupBox4.Show();
                return;
            }
            groupBox4.Hide();
        }

        /* Update the file tree GUI */
        private void UpdateFileTree(List<string> FilesToList)
        {
            FileTree.Nodes.Clear();
            foreach (string FileName in FilesToList)
            {
                string[] FileNameParts = FileName.Split('/');
                if (FileNameParts.Length == 1) { FileNameParts = FileName.Split('\\'); }
                AddFileToTree(FileNameParts, 0, FileTree.Nodes);
            }
            UpdateSelectedFilePreview();
            FileTree.Sort();
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
            if (FileExtension == "")
            {
                if (AlienPAK.Format == PAKType.PAK_SCRIPTS)
                {
                    return "Cathode Script";
                }
                return "Unknown Type";
            }
            switch (FileExtension.Substring(1).ToUpper())
            {
                case "DDS":
                    return "DDS (Image)";
                case "TGA":
                    return "TGA (Image)";
                case "PNG":
                    return "PNG (Image)";
                case "JPG":
                    return "JPG (Image)";
                case "GFX":
                    return "GFX (Adobe Flash)";
                case "CS2":
                    return "CS2 (Model)";
                case "BIN":
                    return "BIN (Binary File)";
                case "BML":
                    return "BML (Binary XML)";
                case "XML":
                    return "XML (Markup)";
                case "TXT":
                    return "TXT (Text)";
                case "DXBC":
                    return "DXBC (Compiled HLSL)";
                default:
                    return FileExtension.Substring(1).ToUpper();
            }
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
            removeFile.Enabled = false;
            addFile.Enabled = true; //Eventually move this to only be enabled on directory selection

            //Exit early if nothing selected
            if (FileTree.SelectedNode == null) {
                return;
            }
            
            //Handle file selection
            if (((TreeItem)FileTree.SelectedNode.Tag).Item_Type == TreeItemType.EXPORTABLE_FILE)
            {
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
                removeFile.Enabled = true;
            }
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
            OpenFileDialog FilePicker = new OpenFileDialog();
            FilePicker.Filter = "Import File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
            if (FilePicker.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                PAKReturnType ResponseCode = AlienPAK.ImportFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value, FilePicker.FileName);
                MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
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
            SaveFileDialog FilePicker = new SaveFileDialog();
            FilePicker.Filter = "Exported File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
            FilePicker.FileName = Path.GetFileName(FileTree.SelectedNode.Text);
            if (FilePicker.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                PAKReturnType ResponseCode = AlienPAK.ExportFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value, FilePicker.FileName);
                MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
        }

        /* Add file to the loaded archive */
        private void AddFileToArchive_Click(object sender, EventArgs e)
        {
            //Let the user decide what file to add, then add it
            OpenFileDialog FilePicker = new OpenFileDialog();
            FilePicker.Filter = "Any File|*.*";
            if (FilePicker.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                PAKReturnType ResponseCode = AlienPAK.AddNewFile(FilePicker.FileName);
                MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
            //This is an expensive call for any PAK except PAK2, as it uses the new system.
            //We only can call with PAK2 here so it's fine, but worth noting.
            UpdateFileTree(AlienPAK.Parse());
        }

        /* Remove selected file from the archive */
        private void RemoveFileFromArchive_Click(object sender, EventArgs e)
        {
            if (FileTree.SelectedNode == null || ((TreeItem)FileTree.SelectedNode.Tag).Item_Type != TreeItemType.EXPORTABLE_FILE)
            {
                MessageBox.Show("Please select a file from the list.", "No file selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult ConfirmRemoval = MessageBox.Show("Are you sure you would like to remove this file?", "About to remove selected file...", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ConfirmRemoval == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                PAKReturnType ResponseCode = AlienPAK.RemoveFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value);
                MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
            //This is an expensive call for any PAK except PAK2, as it uses the new system.
            //We only can call with PAK2 here so it's fine, but worth noting.
            UpdateFileTree(AlienPAK.Parse()); 
        }

        /* Form loads */
        private void Form1_Load(object sender, EventArgs e)
        {
            //For testing purposes
            //OpenFileAndPopulateGUI(@"E:\Program Files\Steam\steamapps\common\Alien Isolation\DATA\SHADERS\UBERSHADER\DEFERREDSHADERS_DX11.PAK");
        }

        /* User requests to open a PAK */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Allow selection of a PAK from filepicker, then open
            OpenFileDialog ArchivePicker = new OpenFileDialog();
            ArchivePicker.Filter = "Alien: Isolation PAK|*.PAK";
            if (ArchivePicker.ShowDialog() == DialogResult.OK)
            {
                OpenFileAndPopulateGUI(ArchivePicker.FileName);
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

        /* Open options pane */
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolOptions OptionsForm = new ToolOptions();
            OptionsForm.Show();
        }

        /* Create a PAK2 archive from a specified directory */
        private void createPAK2FromDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please select a folder to convert to PAK2.", "Select folder...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FolderBrowserDialog FolderToParse = new FolderBrowserDialog();
            if (FolderToParse.ShowDialog() == DialogResult.OK)
            {
                List<string> FilesToAdd = new List<string>();
                ListAllFiles(FolderToParse.SelectedPath, FilesToAdd);

                MessageBox.Show("Please select a location to save the new PAK2 archive.", "Select output location...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SaveFileDialog PathToPAK2 = new SaveFileDialog();
                PathToPAK2.Filter = "PAK2 Archive|*.PAK";
                if (PathToPAK2.ShowDialog() == DialogResult.OK)
                {
                    PAK2 NewArchive = new PAK2(PathToPAK2.FileName);
                    foreach (string FileName in FilesToAdd)
                    {
                        NewArchive.AddFile(FileName, FolderToParse.SelectedPath.Length+1);
                    }
                    NewArchive.Save();
                    MessageBox.Show("Archive successfully created!", "Finished...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            MessageBox.Show("The PAK2 was not created.", "Process cancelled.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void ListAllFiles(string ThisDirectory, List<string> FilesInDir)
        {
            try
            {
                foreach (string ThisFile in Directory.GetFiles(ThisDirectory))
                {
                    FilesInDir.Add(ThisFile);
                }
                foreach (string NextDirectory in Directory.GetDirectories(ThisDirectory))
                {
                    ListAllFiles(NextDirectory, FilesInDir);
                }
            } catch { }
        }

        /* Export all files from the current archive */
        private void exportAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Load all file names currently in the UI
            if (AlienPAK.Format == PAKType.UNRECOGNISED)
            {
                MessageBox.Show("No files to export!\nPlease load a PAK archive.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            List<string> AllFiles = AlienPAK.Parse();
            Cursor.Current = Cursors.WaitCursor;

            //Select the folder to dump to
            FolderBrowserDialog FolderToExportTo = new FolderBrowserDialog();
            if (FolderToExportTo.ShowDialog() != DialogResult.OK) return;

            //Go through all filenames and request an export
            int SuccessCount = 0;
            for (int i = 0; i < AllFiles.Count; i++)
            {
                string ExportPath = FolderToExportTo.SelectedPath + "\\" + AllFiles[i];
                Directory.CreateDirectory(ExportPath.Substring(0, ExportPath.Length - Path.GetFileName(ExportPath).Length));
                PAKReturnType ErrorCode = AlienPAK.ExportFile(AllFiles[i], ExportPath);
                if (ErrorCode == PAKReturnType.SUCCESS || ErrorCode == PAKReturnType.SUCCESS_WITH_WARNINGS) SuccessCount++;
            }

            //Complete!
            Cursor.Current = Cursors.Default;
            if (SuccessCount == AllFiles.Count)
            {
                MessageBox.Show("Successfully exported all files from this PAK!", "Export complete.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Export process complete, but " + (AllFiles.Count - SuccessCount) + " files encountered errors.\nPerhaps try a directory with a shorter filepath, or check write access.", "Export complete, with warnings.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
