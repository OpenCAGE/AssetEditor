using Assimp;
using Assimp.Unmanaged;
using CATHODE;
using CathodeLib;
using DirectXTexNet;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AlienPAK
{
    public partial class Explorer : Form
    {
        Level LevelContent = null;
        private PAK2 Archive = null;

        TreeUtility treeHelper;
        ExplorerControlsWPF preview;

        PAKType LaunchMode;
        string baseTitle;

        /* Functionality provided by the currently loaded PAK */
        public PAKFunction Functionality
        {
            get
            {
                switch (LaunchMode)
                {
                    case PAKType.MODELS:
                        return PAKFunction.CAN_EXPORT_FILES | PAKFunction.CAN_IMPORT_FILES | PAKFunction.CAN_REPLACE_FILES | PAKFunction.CAN_DELETE_FILES;
                    case PAKType.ANIMATIONS:
                    case PAKType.UI:
                    case PAKType.CHR_INFO:
                    case PAKType.TEXTURES:
                        return PAKFunction.CAN_EXPORT_FILES | PAKFunction.CAN_IMPORT_FILES | PAKFunction.CAN_REPLACE_FILES | PAKFunction.CAN_DELETE_FILES | PAKFunction.CAN_EXPORT_ALL;
                    default:
                        return PAKFunction.NONE;
                }
            }
        }

        public Explorer(PAKType LaunchAs)
        {
            LaunchMode = LaunchAs;
            InitializeComponent();

            FileTree.ImageList = imageList1;

            baseTitle = "OpenCAGE Asset Editor";

            this.Text = baseTitle;

            preview = (ExplorerControlsWPF)elementHost1.Child;
            preview.OnLevelSelected += LoadModePAK;
            preview.OnImportRequested += ImportNewFile;
            preview.OnExportRequested += ExportSelectedFile;
            preview.OnReplaceRequested += ReplaceSelectedFile;
            preview.OnDeleteRequested += DeleteSelectedFile;
            preview.OnExportAllRequested += ExportAllFiles;
            preview.ShowFunctionButtons(PAKFunction.NONE, LaunchMode, false);
            preview.ShowLevelSelect(LaunchMode != PAKType.ANIMATIONS && LaunchMode != PAKType.UI, LaunchMode);
        }

        /* Load the appropriate PAK for the given launch mode */
        private void LoadModePAK(string level)
        {
            Archive = null;
            LevelContent = null;

            string path = SharedData.pathToAI + "/DATA/";
            Cursor.Current = Cursors.WaitCursor;
            switch (LaunchMode)
            {
                case PAKType.ANIMATIONS:
                    Archive = new PAK2(path + "GLOBAL/ANIMATION.PAK");
                    break;
                case PAKType.UI:
                    Archive = new PAK2(path + "UI.PAK");
                    break;
                default:
                    LevelContent = Utilities.LoadLevel(SharedData.pathToAI, level);
                    break;
            }
            UpdateUI();
            Cursor.Current = Cursors.Default;
        }

        private void UpdateUI()
        {
            this.Text = baseTitle + ((LevelContent?.Name == null || LevelContent?.Name == "") ? "" : " - " + LevelContent.Name) + " - " + LaunchMode;
            switch (LaunchMode)
            {
                case PAKType.ANIMATIONS:
                case PAKType.UI:
                    treeHelper = new TreeUtility(FileTree);
                    {
                        List<string> fileNames = new List<string>();
                        for (int i = 0; i < Archive.Entries.Count; i++)
                            fileNames.Add(Archive.Entries[i].Filename);
                        treeHelper.UpdateFileTree(fileNames, null);
                    }
                    break;
                case PAKType.MODELS:
                    treeHelper = new TreeUtility(FileTree, true);
                    {
                        List<string> allModelFileNames = new List<string>();
                        List<string> allModelTagsNames = new List<string>();
                        List<Models.CS2.Component> allModelTagsModels = new List<Models.CS2.Component>();
                        foreach (Models.CS2 mesh in LevelContent.Models.Entries)
                        {
                            foreach (Models.CS2.Component component in mesh.Components)
                            {
                                if (component.LODs.Count == 0)
                                    continue;

                                Models.CS2.Component.LOD lod0 = component.LODs[0];

                                if (lod0.Submeshes.Count == 0)
                                    continue;

                                Models.CS2.Component.LOD.Submesh submesh0 = lod0.Submeshes[0];
                                allModelFileNames.Add(CreateTagForMesh(mesh, submesh0, lod0, component));
                                allModelTagsNames.Add(LevelContent.Models.GetWriteIndex(submesh0).ToString());
                                allModelTagsModels.Add(component);
                            }
                        }
                        treeHelper.UpdateFileTree(allModelFileNames, null, allModelTagsNames, allModelTagsModels);
                    }
                    break;
                case PAKType.TEXTURES:
                    treeHelper = new TreeUtility(FileTree);
                    {
                        List<string> textureNames = new List<string>();
                        for (int i = 0; i < LevelContent.Textures.Entries.Count; i++)
                        {
                            string texPath = LevelContent.Textures.Entries[i].Name;
                            LevelContent.Textures.Entries[i].Name = texPath;
                            textureNames.Add(texPath);
                        }
                        treeHelper.UpdateFileTree(textureNames, null);
                    }
                    break;
            }
            UpdateSelectedFilePreview();
        }

        private string CreateTagForMesh(Models.CS2 cs2, Models.CS2.Component.LOD.Submesh submesh, Models.CS2.Component.LOD lod, Models.CS2.Component component)
        {
            string tag = cs2.Name.Replace('\\', '/') + "/[" + cs2.Components.IndexOf(component).ToString("00") + "] " + lod.Name.Replace('\\', '/');
            if (tag.Length > 0 && tag[0] == '/')
                tag = tag.Substring(1);
            return tag;
        }

        /* Import a new file to the PAK */
        private void ImportNewFile()
        {
            OpenFileDialog FilePicker = new OpenFileDialog();
            switch (LaunchMode)
            {
                case PAKType.ANIMATIONS:
                case PAKType.UI:
                case PAKType.CHR_INFO:
                    FilePicker.Filter = "All Files|*.*";
                    break;
                case PAKType.TEXTURES:
                    FilePicker.Filter = "DDS Files|*.DDS"; //"PNG Image|*.png|JPG Image|*.jpg|DDS Image|*.dds";
                    break;
                case PAKType.MODELS:
                    FilePicker.Filter = "FBX Model|*.fbx|GLTF Model|*.gltf|OBJ Model|*.obj"; //TODO: we can support loads here with assimp (importer.GetSupportedExportFormats())
                    break;
                default:
                    MessageBox.Show("This PAK type does not support file importing!", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }
            if (FilePicker.ShowDialog() != DialogResult.OK) return;
            string newFileName = Path.GetFileNameWithoutExtension(FilePicker.FileName);

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                switch (LaunchMode)
                {
                    case PAKType.ANIMATIONS:
                    case PAKType.UI:
                        newFileName = Path.GetFileName(FilePicker.FileName);
                        Archive.Entries.Add(new PAK2.File() { Filename = newFileName, Content = File.ReadAllBytes(FilePicker.FileName) });
                        break;
                    case PAKType.TEXTURES:
                        newFileName += ".dds";
                        Textures.TEX4 texture = new Textures.TEX4() { Name = Path.GetFileName(FilePicker.FileName) };
                        Textures.TEX4.Texture part = File.ReadAllBytes(FilePicker.FileName)?.ToTEX4Part(out texture.Format, out texture.StateFlags, out texture.UsageFlags);
                        if (part == null)
                        {
                            MessageBox.Show("Please select a DX10 DDS image!\nIf you have converted this DDS yourself, you've converted it wrong - try using a tool like Nvidia Texture Tools Exporter.", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                        texture.TextureStreamed = part.Copy(); 
                        texture.TexturePersistent = part.Copy(); //todo: i think we can just set persistent or streamed?
                        LevelContent.Textures.Entries.Add(texture);
                        break;
                    case PAKType.MODELS:
                        newFileName += ".cs2";
                        Models.CS2 cs2 = new Models.CS2();
                        cs2.Name = newFileName;
                        cs2.Components.Add(new Models.CS2.Component());
                        cs2.Components[0].LODs.Add(new Models.CS2.Component.LOD(newFileName));
                        using (AssimpContext importer = new AssimpContext())
                        {
                            //TODO: utilise aiProcess_SplitLargeMeshes to avoid passing our vert limit
                            Scene model = importer.ImportFile(FilePicker.FileName,
                                PostProcessSteps.Triangulate | PostProcessSteps.FindDegenerates | PostProcessSteps.LimitBoneWeights | 
                                PostProcessSteps.GenerateBoundingBoxes | PostProcessSteps.FlipUVs | PostProcessSteps.FlipWindingOrder | PostProcessSteps.MakeLeftHanded);
                            ushort biggestSF = 0;
                            for (int i = 0; i < model.Meshes.Count; i++)
                            {
                                ushort newSF = model.Meshes[i].CalculateScaleFactor();
                                if (newSF > biggestSF) biggestSF = newSF;
                            }
                            for (int i = 0; i < model.Meshes.Count; i++)
                            {
                                Models.CS2.Component.LOD.Submesh submesh = model.Meshes[i].ToSubmesh(biggestSF);
                                if (submesh == null)
                                {
                                    MessageBox.Show("Failed to generate CS2 submesh from imported model submesh " + i + ".\nPlease check your submesh polycount - each may not exceed " + Int16.MaxValue + " verts.", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                cs2.Components[0].LODs[0].Submeshes.Add(submesh);
                            }
                        }
                        if (cs2.Components[0].LODs[0].Submeshes.Count == 0)
                        {
                            MessageBox.Show("Failed to generate CS2 from selected model: could not find any mesh data! Please ensure all meshes are children of the scene's root node.", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        LevelContent.Models.Entries.Add(cs2);
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            UpdateUI();
            Cursor.Current = Cursors.Default;
        }

        /* Delete the selected file in the PAK */
        private void DeleteSelectedFile()
        {
            if (FileTree.SelectedNode == null) return;
            TreeItemType nodeType = ((TreeItem)FileTree.SelectedNode.Tag).Item_Type;
            string nodeVal = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            switch (nodeType)
            {
                case TreeItemType.EXPORTABLE_FILE:
                    DialogResult ConfirmRemoval = MessageBox.Show("Are you sure you would like to remove this file?", "About to remove selected file...", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (ConfirmRemoval != DialogResult.Yes) return;

                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        switch (LaunchMode)
                        {
                            case PAKType.ANIMATIONS:
                            case PAKType.UI:
                                Archive.Entries.RemoveAll(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                break;
                            case PAKType.TEXTURES:
                                LevelContent.Textures.Entries.RemoveAll(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                break;
                            case PAKType.MODELS:
                                LevelContent.Models.Entries.Remove(LevelContent.Models.FindModelForComponent(((TreeItem)FileTree.SelectedNode.Tag).Model_Value));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    UpdateUI();
                    Cursor.Current = Cursors.Default;
                    break;
            }

            return;
        }

        /* Export all files in the PAK */
        private void ExportAllFiles()
        {
            exportAllToolStripMenuItem_Click(null, null);
        }

        /* Import a file to replace the selected PAK entry */
        private void ReplaceSelectedFile()
        {
            if (FileTree.SelectedNode == null) return;
            TreeItemType nodeType = ((TreeItem)FileTree.SelectedNode.Tag).Item_Type;
            string nodeVal = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            switch (nodeType)
            {
                case TreeItemType.DIRECTORY:
                case TreeItemType.EXPORTABLE_FILE:
                    if (LaunchMode == PAKType.MODELS)
                    {
                        ModelEditor modelEditor = new ModelEditor(LevelContent.Models.FindModelForComponent(((TreeItem)FileTree.SelectedNode.Tag).Model_Value), LevelContent.Textures, LevelContent.Global.Textures, LevelContent.Materials, LevelContent.Shaders);
                        modelEditor.FormClosed += ModelEditor_FormClosed;
                        modelEditor.Show();
                        break;
                    }

                    if (nodeType == TreeItemType.DIRECTORY)
                        break;

                    string filter = "File|*" + Path.GetExtension(FileTree.SelectedNode.Text);

                    OpenFileDialog FilePicker = new OpenFileDialog();
                    FilePicker.Filter = filter;
                    if (FilePicker.ShowDialog() != DialogResult.OK) break;

                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        switch (LaunchMode)
                        {
                            case PAKType.ANIMATIONS:
                            case PAKType.UI:
                                Archive.Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/')).Content = File.ReadAllBytes(FilePicker.FileName);
                                break;
                            case PAKType.TEXTURES:
                                Textures.TEX4 texture = LevelContent.Textures.Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                byte[] content = File.ReadAllBytes(FilePicker.FileName);
                                Textures.TEX4.Texture part = content?.ToTEX4Part(out texture.Format, out texture.StateFlags, out texture.UsageFlags);
                                if (part == null)
                                {
                                    MessageBox.Show("Please select a DX10 DDS image!\nIf you have converted this DDS yourself, you've converted it wrong - try using a tool like Nvidia Texture Tools Exporter.", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                }
                                texture.TextureStreamed = part.Copy();
                                texture.TexturePersistent = part.Copy();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Cursor.Current = Cursors.Default;
                    UpdateSelectedFilePreview();
                    break;
            }
        }
        private void ModelEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateUI();
            UpdateSelectedFilePreview();
            this.BringToFront();
            this.Focus();
        }
        
        private void ExportSelectedFile()
        {
            ExportNode(FileTree.SelectedNode);
        }

        /* Export the selected PAK entry as a standalone file */
        private void ExportNode(TreeNode node, string outputFolder = "") //If you set outputFolder, the user won't get a filepicker
        {
            if (node == null) return;
            TreeItemType nodeType = ((TreeItem)node.Tag).Item_Type;
            string nodeVal = ((TreeItem)node.Tag).String_Value;

            string filter = "File|*" + Path.GetExtension(node.Text);
            if (preview.FilePreviewVisible && preview.FilePreviewBitmap != null) filter = "DDS Image|*.dds|PNG Image|*.png|JPG Image|*.jpg";
            if (preview.ModelPreviewVisible) filter = "FBX Model|*.fbx|GLTF Model|*.gltf|OBJ Model|*.obj"; //TODO: we can support loads here with assimp (importer.GetSupportedExportFormats())

            string fileName = Path.GetFileName(node.Text);
            while (Path.GetExtension(fileName).Length != 0) fileName = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length); //Remove extensions from output filename

            string pickedFileName = "";
            if (outputFolder == "")
            {
                SaveFileDialog picker = new SaveFileDialog();
                picker.Filter = filter;
                picker.FileName = fileName;
                if (picker.ShowDialog() != DialogResult.OK) return;
                pickedFileName = picker.FileName;
            }
            else
            {
                // This is hit when exporting all (the user has no filepicker) -> the exportBaseType and exportConvertedType are sometimes set by our conversion popup
                string filename = node.FullPath;
                if (Path.GetExtension(node.FullPath).ToUpper() == "." + _exportBaseType.ToUpper())
                {
                    filename = node.FullPath.Substring(0, node.FullPath.Length - Path.GetExtension(node.FullPath).Length) + _exportConvertedType;
                }
                pickedFileName = outputFolder + "/" + filename; 

                string dir = pickedFileName.Substring(0, pickedFileName.Length - Path.GetFileName(pickedFileName).Length);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                switch (LaunchMode)
                {
                    case PAKType.ANIMATIONS:
                    case PAKType.UI:
                    case PAKType.CHR_INFO:
                        File.WriteAllBytes(pickedFileName, Archive.Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/'))?.Content);
                        break;
                    case PAKType.TEXTURES:
                        Textures.TEX4 texture = LevelContent.Textures.Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                        byte[] content = texture?.ToDDS();
                        if (Path.GetExtension(pickedFileName).ToUpper() == ".DDS")
                        {
                            File.WriteAllBytes(pickedFileName, content);
                            break;
                        }
                        content?.ToBitmap()?.Save(pickedFileName);
                        break;
                    case PAKType.MODELS:
                        LevelContent.Models.FindModelForComponent(((TreeItem)FileTree.SelectedNode.Tag).Model_Value).ExportMesh(pickedFileName);
                        break;
                    default:
                        if (outputFolder == "")
                            MessageBox.Show("This PAK type does not support file exporting!", "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }
                if (outputFolder == "")
                    MessageBox.Show("Successfully exported file!", "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (outputFolder == "")
                    MessageBox.Show(ex.ToString(), "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        /* Try free-up when closing */
        private void Explorer_FormClosed(object sender, FormClosedEventArgs e)
        {
            LevelContent = null;
            treeHelper.UpdateFileTree(new List<string>());
            treeHelper = null;
            preview = null;
        }

        /* Update file preview */
        private void UpdateSelectedFilePreview()
        {
            preview.ShowFunctionButtons(Functionality, LaunchMode, false);
            if (FileTree.SelectedNode == null) return;
            TreeItemType nodeType = ((TreeItem)FileTree.SelectedNode.Tag).Item_Type;
            string nodeVal = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            switch (LaunchMode)
            {
                case PAKType.ANIMATIONS:
                case PAKType.UI:
                    switch (nodeType)
                    {
                        case TreeItemType.EXPORTABLE_FILE:
                            PAK2.File file = Archive.Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                            preview.ShowFunctionButtons(Functionality, LaunchMode, true);
                            preview.SetFileInfo(Path.GetFileName(nodeVal), file?.Content.Length.ToString());
                            preview.SetImagePreview(file.Content);
                            break;
                    }
                    break;
                case PAKType.TEXTURES:
                    switch (nodeType)
                    {
                        case TreeItemType.EXPORTABLE_FILE:
                            Textures.TEX4 texture = LevelContent.Textures.Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                            byte[] content = texture?.ToDDS();
                            preview.ShowFunctionButtons(Functionality, LaunchMode, true);
                            preview.SetFileInfo(Path.GetFileName(nodeVal), content?.Length.ToString());
                            if (texture != null && texture.StateFlags.HasFlag(Textures.TextureStateFlag.CUBE))
                                preview.SetCubemapPreview(content);
                            else
                                preview.SetImagePreview(content);
                            break;
                    }
                    break;
                case PAKType.MODELS:
                    Model3DGroup model = new Model3DGroup();
                    string name = "";
                    int verts = 0;
                    switch (nodeType)
                    {
                        case TreeItemType.DIRECTORY:
                            if (!(FileTree.SelectedNode.Nodes.Count > 0 && FileTree.SelectedNode.Nodes[0].Nodes.Count == 0))
                                return;
                            {
                                Models.CS2 cs2 = LevelContent.Models.FindModelForComponent(((TreeItem)FileTree.SelectedNode.Tag).Model_Value);
                                foreach (Models.CS2.Component component in cs2.Components)
                                {
                                    foreach (Models.CS2.Component.LOD lod in component.LODs)
                                    {
                                        foreach (Models.CS2.Component.LOD.Submesh submesh in lod.Submeshes)
                                        {
                                            GeometryModel3D submeshGeo = submesh.ToGeometryModel3D();
                                            verts += submesh.VertexCount;
                                            model.Children.Add(submeshGeo); //TODO: are there some offsets/scaling we should be accounting for here?
                                        }
                                    }
                                }
                                name = Path.GetFileNameWithoutExtension(cs2.Name);
                                preview.ShowFunctionButtons(Functionality, LaunchMode, true);
                            }
                            break;
                        case TreeItemType.EXPORTABLE_FILE:
                            {
                                //TODO: show a selection between LODs
                                Models.CS2.Component component = ((TreeItem)FileTree.SelectedNode.Tag).Model_Value;
                                foreach (Models.CS2.Component.LOD lod in component.LODs)
                                {
                                    foreach (Models.CS2.Component.LOD.Submesh submesh in lod.Submeshes)
                                    {
                                        GeometryModel3D submeshGeo = submesh.ToGeometryModel3D();
                                        verts += submesh.VertexCount;
                                        model.Children.Add(submeshGeo); //TODO: are there some offsets/scaling we should be accounting for here?
                                    }
                                }
                                name = Path.GetFileNameWithoutExtension(LevelContent.Models.FindModelForComponent(component).Name); //todo - show component name or index
                            }
                            break;
                    }
                    preview.SetFileInfo(name, verts.ToString(), true);
                    preview.SetModelPreview(model);
                    break;
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
            ReplaceSelectedFile();
        }
        private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelectedFile();
        }

        /* Import/export selected file (context menu) */
        private void importFileContext_Click(object sender, EventArgs e)
        {
            ReplaceSelectedFile();
        }
        private void exportFileContext_Click(object sender, EventArgs e)
        {
            ExportSelectedFile();
        }

        /* Item selected (show preview info) */
        private void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateSelectedFilePreview();
        }

        //This is a hacked way of exporting all files - needs tidying up in future, bit of a proof of concept for now
        BulkExportTypeSelection _exportTypeSelect = null;
        string _exportPath = "";
        string _exportBaseType = "";
        string _exportConvertedType = "";
        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select output folder";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _exportPath = dialog.SelectedPath;

                    List<string> types = new List<string>();
                    _exportBaseType = "";
                    switch (LaunchMode)
                    {
                        case PAKType.MODELS:
                            _exportBaseType = "CS2";
                            types.Add(".fbx");
                            types.Add(".obj");
                            types.Add(".gltf");
                            break;
                        case PAKType.TEXTURES:
                            _exportBaseType = "DDS";
                            types.Add(".dds");
                            types.Add(".png");
                            types.Add(".jpg");
                            break;
                    }

                    if (_exportTypeSelect != null)
                    {
                        _exportTypeSelect.OnTypeSelected -= OnExportTypeSelected;
                        _exportTypeSelect.Close();
                        _exportTypeSelect = null;
                    }
                    if (types.Count != 0)
                    {
                        _exportTypeSelect = new BulkExportTypeSelection();
                        _exportTypeSelect.SetTypes(types, _exportBaseType);
                        _exportTypeSelect.Show();
                        _exportTypeSelect.OnTypeSelected += OnExportTypeSelected;
                    }
                    else
                    {
                        OnExportTypeSelected("");
                    }
                }
            }
        }
        private void OnExportTypeSelected(string type)
        {
            _exportConvertedType = type;

            foreach (TreeNode node in FileTree.Nodes)
            {
                RecursiveExport(node, _exportPath);
            }

            Process.Start(_exportPath);
            MessageBox.Show("Export complete", "Exported!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void RecursiveExport(TreeNode node, string path)
        {
            ExportNode(node, path);

            foreach (TreeNode n in node.Nodes)
            {
                RecursiveExport(n, path);
            }
        }

        private void Explorer_Load(object sender, EventArgs e)
        {
            this.FormClosing += Explorer_Closing;
        }
        private void Explorer_Closing(object sender, EventArgs e)
        {
            treeHelper?.ForceClearTree();
            treeHelper = null;
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            //Close alien down if it's open, it conflicts with our write locks!
            List<Process> allProcesses = new List<Process>(Process.GetProcessesByName("AI"));
            for (int x = 0; x < allProcesses.Count; x++)
            {
                try
                {
                    allProcesses[x].Kill();
                    allProcesses[x].WaitForExit();
                }
                catch { }
            }

            Archive?.Save();
            LevelContent?.Save();

            Cursor.Current = Cursors.Default;

            MessageBox.Show("All changes have been saved successfully.", "Successfully saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
