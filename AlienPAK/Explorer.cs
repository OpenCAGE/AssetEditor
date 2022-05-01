using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CATHODE;
using CathodeLib;
using DirectXTexNet;

namespace AlienPAK
{
    /*
     * 
     * This is a modified version of AlienPAK, which can be found on its own repo as a standalone tool:
     * https://github.com/MattFiler/AlienPAK
     * 
     * While all AlienPAK classes are largely a direct port, some minor tweaks have been made, marked with an OPENCAGE comment.
     * This form is heavily modified from the base AlienPAK implementation.
     * Major changes:
     *  - Cannot load a PAK from the form
     *  - Loading as texture edit mode will load all PAKs
     *  - texconv is used instead of DirectXTexNet
     * 
    */

    public partial class Explorer : Form
    {
        List<PAK> AlienPAKs = new List<PAK>();
        ErrorMessages AlienErrors = new ErrorMessages();
        TreeUtility treeHelper;

        public Explorer(string[] args, AlienContentType LaunchAs)
        {
            LaunchMode = LaunchAs;
            InitializeComponent();

            //Support "open with" from Windows on PAK files
            if (args.Length > 0 && File.Exists(args[0]))
            {
                OpenFileAndPopulateGUI(args[0]);
            }

            //Link image list to GUI elements for icons
            FileTree.ImageList = imageList1;

            treeHelper = new TreeUtility(FileTree);
            AlienModToolsAdditions();
        }

        /* Open a PAK and populate the GUI */
        private void OpenFileAndPopulateGUI(string filename)
        {
            //Open PAK
            Cursor.Current = Cursors.WaitCursor;
            AlienPAKs.Clear();
            AlienPAKs.Add(new PAK());
            AlienPAKs[0].Open(filename);

            //Parse the PAK's file list
            List<string> ParsedFiles = new List<string>();
            ParsedFiles = AlienPAKs[0].Parse();
            if (ParsedFiles == null)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("The selected PAK is currently unsupported.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Populate the GUI with the files found within the archive
            treeHelper.UpdateFileTree(ParsedFiles);
            UpdateSelectedFilePreview();

            //Update title
            //this.Text = "Alien: Isolation PAK Tool - " + Path.GetFileName(filename);
            Cursor.Current = Cursors.Default;

            //Show/hide extended archive support if appropriate
            if (AlienPAKs[0].Format == PAKType.PAK2)
            {
                groupBox3.Show();
                return;
            }
            groupBox3.Hide();
        }

        /* Get type description based on extension */
        private string GetFileTypeDescription(string FileExtension)
        {
            if (FileExtension == "")
            {
                /*
                if (AlienPAK.Format == PAKType.PAK_SCRIPTS)
                {
                    return "Cathode Script";
                }
                */
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

        /* Temp function to get a file as a byte array */
        private byte[] GetFileAsBytes(string FileName)
        {
            PAKReturnType ResponseCode = PAKReturnType.FAIL_UNKNOWN;
            foreach (PAK thisPAK in AlienPAKs)
            {
                ResponseCode = thisPAK.ExportFile(FileName, "temp"); //Should really be able to pull from PAK as bytes
                if (ResponseCode == PAKReturnType.SUCCESS || ResponseCode == PAKReturnType.SUCCESS_WITH_WARNINGS) break;
            }
            if (!File.Exists("temp")) return new byte[] { };
            byte[] ExportedFile = File.ReadAllBytes("temp");
            File.Delete("temp");
            return ExportedFile;
        }

        /* Update file preview */
        private void UpdateSelectedFilePreview()
        {
            //First, reset the GUI
            groupBox1.Visible = false;
            filePreviewImage.BackgroundImage = null;
            fileNameInfo.Text = "";
            fileSizeInfo.Text = "";
            fileTypeInfo.Text = "";
            exportFile.Enabled = false;
            importFile.Enabled = false;
            removeFile.Enabled = false;
            addFile.Enabled = true; //Eventually move this to only be enabled on directory selection

            //Exit early if nothing selected
            if (FileTree.SelectedNode == null)
            {
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
                int FileSize = -1;
                foreach (PAK thisPAK in AlienPAKs) if (FileSize == -1) FileSize = thisPAK.GetFileSize(FileName);
                if (FileSize == -1) { return; }
                fileSizeInfo.Text = FileSize.ToString() + " bytes";

                //Show file preview if selected an image
                if (Path.GetExtension(FileName).ToUpper() == ".DDS")
                {
                    MemoryStream imageStream = new MemoryStream(GetFileAsBytes(FileName));
                    using (var image = Pfim.Pfim.FromStream(imageStream))
                    {
                        PixelFormat format = PixelFormat.DontCare;
                        switch (image.Format)
                        {
                            case Pfim.ImageFormat.Rgba32:
                                format = PixelFormat.Format32bppArgb;
                                break;
                            case Pfim.ImageFormat.Rgb24:
                                format = PixelFormat.Format24bppRgb;
                                break;
                            default:
                                Console.WriteLine("Unsupported DDS: " + image.Format);
                                break;
                        }
                        if (format != PixelFormat.DontCare)
                        {
                            var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                            try
                            {
                                var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                                var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, data);
                                ResizeImagePreview((Bitmap)bitmap);
                            }
                            finally
                            {
                                handle.Free();
                            }
                        }
                    }
                }
                groupBox1.Visible = (filePreviewImage.BackgroundImage != null);

                //Enable buttons
                exportFile.Enabled = true;
                importFile.Enabled = true;
                removeFile.Enabled = true;
            }
        }

        /* Set the image in the preview window and scale appropriately */
        private void ResizeImagePreview(Bitmap image)
        {
            filePreviewImage.BackgroundImage = image;
            if (image.Width >= filePreviewImage.Width || image.Height >= filePreviewImage.Height) filePreviewImage.BackgroundImageLayout = ImageLayout.Zoom;
            else filePreviewImage.BackgroundImageLayout = ImageLayout.None;
        }

        /* Import a file to replace the selected PAK entry */
        private void ImportSelectedFile()
        {
            if (FileTree.SelectedNode == null || ((TreeItem)FileTree.SelectedNode.Tag).Item_Type != TreeItemType.EXPORTABLE_FILE)
            {
                MessageBox.Show("Please select a file from the list.", "No file selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //If import file is DDS, check first to see if it can be imported as WIC format
            string filter = "Import File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
            DXGI_FORMAT baseFormat = DXGI_FORMAT.UNKNOWN;
            if (Path.GetExtension(FileTree.SelectedNode.Text).ToUpper() == ".DDS")
            {
                byte[] ImageFile = GetFileAsBytes(((TreeItem)FileTree.SelectedNode.Tag).String_Value);
                try
                {
                    ScratchImage img = TexHelper.Instance.LoadFromDDSMemory(Marshal.UnsafeAddrOfPinnedArrayElement(ImageFile, 0), ImageFile.Length, DDS_FLAGS.NONE);
                    baseFormat = img.GetMetadata().Format;
                    if (baseFormat != DXGI_FORMAT.UNKNOWN) filter = "PNG Image|*.png|DDS Image|*.dds"; //Can import as WIC
                }
                catch { }
            }

            //Allow selection of a file (force extension), then drop it in
            OpenFileDialog FilePicker = new OpenFileDialog();
            FilePicker.Filter = filter;
            if (FilePicker.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;

                //Special import for DDS conversion
                bool ImportOK = true;
                bool ImportingConverted = false;
                if (baseFormat != DXGI_FORMAT.UNKNOWN && Path.GetExtension(FilePicker.FileName).ToUpper() == ".PNG")
                {
                    try
                    {
                        ScratchImage img = TexHelper.Instance.LoadFromWICFile(FilePicker.FileName, WIC_FLAGS.FORCE_RGB).GenerateMipMaps(TEX_FILTER_FLAGS.DEFAULT, 10); /* Was using 11, but gives remainders - going for 10 */
                        ScratchImage imgDecom = img.Compress(DXGI_FORMAT.BC7_UNORM, TEX_COMPRESS_FLAGS.BC7_QUICK, 0.5f); //TODO use baseFormat
                        imgDecom.SaveToDDSFile(DDS_FLAGS.FORCE_DX10_EXT, FilePicker.FileName + ".DDS");
                        FilePicker.FileName += ".DDS";
                        ImportingConverted = true;
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.ToString());
                        ImportOK = false;
                        MessageBox.Show("Failed to import as PNG!\nPlease try again as DDS.", AlienErrors.ErrorMessageTitle(PAKReturnType.FAIL_UNKNOWN), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                //Regular import
                if (ImportOK)
                {
                    foreach (PAK thisPAK in AlienPAKs) thisPAK.ImportFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value, FilePicker.FileName);
                    if (ImportingConverted) File.Delete(FilePicker.FileName); //We temp dump out a converted file, which this cleans up
                    MessageBox.Show(AlienErrors.ErrorMessageBody(PAKReturnType.SUCCESS), AlienErrors.ErrorMessageTitle(PAKReturnType.SUCCESS), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

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

            //If export file is DDS, check first to see if we can export as WIC format
            string filter = "Exported File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
            byte[] ImageFile = new byte[] { };
            if (Path.GetExtension(FileTree.SelectedNode.Text).ToUpper() == ".DDS")
            {
                ImageFile = GetFileAsBytes(((TreeItem)FileTree.SelectedNode.Tag).String_Value);
                try
                {
                    TexHelper.Instance.LoadFromDDSMemory(Marshal.UnsafeAddrOfPinnedArrayElement(ImageFile, 0), ImageFile.Length, DDS_FLAGS.NONE);
                    filter = "PNG Image|*.png|DDS Image|*.dds"; //Can export as WIC
                }
                catch
                {
                    ImageFile = new byte[] { };
                }
            }

            //Remove extension from output filename
            string filename = Path.GetFileName(FileTree.SelectedNode.Text);
            while (Path.GetExtension(filename).Length != 0) filename = filename.Substring(0, filename.Length - Path.GetExtension(filename).Length);

            //Let the user decide where to save, then save
            SaveFileDialog FilePicker = new SaveFileDialog();
            FilePicker.Filter = filter;
            FilePicker.FileName = filename;
            if (FilePicker.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                //Special export for DDS conversion
                if (ImageFile.Length > 0 && FilePicker.FilterIndex == 1) //Index 1 == PNG, if ImageFile hasn't been cleared (we can export as WIC)
                {
                    try
                    {
                        ScratchImage img = TexHelper.Instance.LoadFromDDSMemory(Marshal.UnsafeAddrOfPinnedArrayElement(ImageFile, 0), ImageFile.Length, DDS_FLAGS.NONE);
                        ScratchImage imgDecom = img.Decompress(DXGI_FORMAT.UNKNOWN);
                        imgDecom.SaveToWICFile(0, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG), FilePicker.FileName);
                        MessageBox.Show(AlienErrors.ErrorMessageBody(PAKReturnType.SUCCESS), AlienErrors.ErrorMessageTitle(PAKReturnType.SUCCESS), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to export as PNG!\nPlease try again as DDS.", AlienErrors.ErrorMessageTitle(PAKReturnType.FAIL_UNKNOWN), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                //Regular export
                else
                {
                    PAKReturnType ResponseCode = PAKReturnType.FAIL_UNKNOWN;
                    foreach (PAK thisPAK in AlienPAKs)
                    {
                        ResponseCode = thisPAK.ExportFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value, FilePicker.FileName);
                        if (ResponseCode == PAKReturnType.SUCCESS || ResponseCode == PAKReturnType.SUCCESS_WITH_WARNINGS) break;
                    }
                    MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                Cursor.Current = Cursors.Default;
            }
        }

        /* Add file to the loaded archive */
        private void AddFileToArchive_Click(object sender, EventArgs e)
        {
            /* This can only happen for UI files, so for OpenCAGE I'm forcing AlienPAKs[0] - might need changing for other PAKs that gain support */

            //Let the user decide what file to add, then add it
            OpenFileDialog FilePicker = new OpenFileDialog();
            FilePicker.Filter = "Any File|*.*";
            if (FilePicker.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                PAKReturnType ResponseCode = AlienPAKs[0].AddNewFile(FilePicker.FileName);
                MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
            //This is an expensive call for any PAK except PAK2, as it uses the new system.
            //We only can call with PAK2 here so it's fine, but worth noting.
            treeHelper.UpdateFileTree(AlienPAKs[0].Parse());
            UpdateSelectedFilePreview();
        }

        /* Remove selected file from the archive */
        private void RemoveFileFromArchive_Click(object sender, EventArgs e)
        {
            /* This can only happen for UI files, so for OpenCAGE I'm forcing AlienPAKs[0] - might need changing for other PAKs that gain support */

            if (FileTree.SelectedNode == null || ((TreeItem)FileTree.SelectedNode.Tag).Item_Type != TreeItemType.EXPORTABLE_FILE)
            {
                MessageBox.Show("Please select a file from the list.", "No file selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult ConfirmRemoval = MessageBox.Show("Are you sure you would like to remove this file?", "About to remove selected file...", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ConfirmRemoval == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                PAKReturnType ResponseCode = AlienPAKs[0].RemoveFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value);
                MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
            //This is an expensive call for any PAK except PAK2, as it uses the new system.
            //We only can call with PAK2 here so it's fine, but worth noting.
            treeHelper.UpdateFileTree(AlienPAKs[0].Parse());
            UpdateSelectedFilePreview();
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

        /* Create a PAK2 archive from a specified directory */
        private void createPAK2FromDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                    Cursor.Current = Cursors.WaitCursor;

                    PAK2 NewArchive = new PAK2(PathToPAK2.FileName);
                    foreach (string FileName in FilesToAdd)
                    {
                        NewArchive.AddFile(FileName, FolderToParse.SelectedPath.Length + 1);
                    }
                    PAKReturnType ErrorCode = NewArchive.Save();

                    Cursor.Current = Cursors.Default;
                    if (ErrorCode == PAKReturnType.SUCCESS || ErrorCode == PAKReturnType.SUCCESS_WITH_WARNINGS)
                        MessageBox.Show("Archive successfully created!", "Finished...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show(AlienErrors.ErrorMessageBody(ErrorCode), AlienErrors.ErrorMessageTitle(ErrorCode), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
            }
            catch { }
        }

        /* Export all files from the current archive */
        private void exportAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Load all file names currently in the UI
            if (AlienPAKs[0].Format == PAKType.UNRECOGNISED)
            {
                MessageBox.Show("No files to export!\nPlease load a PAK archive.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            List<string> AllFiles = AlienPAKs[0].Parse();
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
                PAKReturnType ErrorCode = AlienPAKs[0].ExportFile(AllFiles[i], ExportPath);
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

        /* ADDITIONS FOR OPENCAGE BELOW */
        AlienContentType LaunchMode;

        //Run on init
        private void AlienModToolsAdditions()
        {
            this.Text = "OpenCAGE Content Editor";
            if (LaunchMode == AlienContentType.NONE_SPECIFIED) return;
            openToolStripMenuItem.Enabled = false;

            //Populate the form with the UI.PAK if launched as so, and exit early
            if (LaunchMode == AlienContentType.UI)
            {
                this.Text += " - UI";
                OpenFileAndPopulateGUI(SharedData.pathToAI + "/DATA/UI.PAK");
                return;
            }

            //Populate the form with the ANIMATION.PAK if launched as so, and exit early
            if (LaunchMode == AlienContentType.ANIMATION)
            {
                this.Text += " - Animations";
                OpenFileAndPopulateGUI(SharedData.pathToAI + "/DATA/GLOBAL/ANIMATION.PAK");
                return;
            }

            //Work out what file to use from our launch type
            string levelFileToUse = "";
            string globalFileToUse = "";
            switch (LaunchMode)
            {
                case AlienContentType.MODEL:
                    levelFileToUse = "LEVEL_MODELS.PAK";
                    globalFileToUse = "GLOBAL_MODELS.PAK";
                    this.Text += " - Models";
                    break;
                case AlienContentType.TEXTURE:
                    levelFileToUse = "LEVEL_TEXTURES.ALL.PAK";
                    globalFileToUse = "GLOBAL_TEXTURES.ALL.PAK";
                    this.Text += " - Textures";
                    break;
                case AlienContentType.SCRIPT:
                    levelFileToUse = "COMMANDS.PAK";
                    this.Text += " - Scripts";
                    break;
            }

            //Load the files for all levels
            Cursor.Current = Cursors.WaitCursor;
            List<string> allLevelPAKs = Directory.GetFiles(SharedData.pathToAI + "/DATA/ENV/PRODUCTION/", levelFileToUse, SearchOption.AllDirectories).ToList<string>();
            if (globalFileToUse != "") allLevelPAKs.Add(SharedData.pathToAI + "/DATA/ENV/GLOBAL/WORLD/" + globalFileToUse);
            List<string> parsedFiles = new List<string>();
            foreach (string levelPAK in allLevelPAKs)
            {
                PAK thisPAK = new PAK();
                thisPAK.Open(levelPAK);
                List<string> theseFiles = thisPAK.Parse();
                foreach (string thisPAKEntry in theseFiles)
                {
                    if (!parsedFiles.Contains(thisPAKEntry)) parsedFiles.Add(thisPAKEntry);
                }
                AlienPAKs.Add(thisPAK);
            }
            treeHelper.UpdateFileTree(parsedFiles);
            UpdateSelectedFilePreview();
            Cursor.Current = Cursors.Default;
            groupBox3.Hide();

            //If we got here, we are using multiple PAKs, so disable some bits for one only
            exportAllFilesToolStripMenuItem.Enabled = false;
        }
    }
}
