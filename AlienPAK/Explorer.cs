using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using Assimp;
using CATHODE;
using CathodeLib;
using DirectXTexNet;
using Newtonsoft.Json;

namespace AlienPAK
{
    public partial class Explorer : Form
    {
        PAKWrapper pak = new PAKWrapper();

        TreeUtility treeHelper;
        ExplorerControlsWPF preview;

        PAKType LaunchMode;
        string baseTitle;

        public Explorer(PAKType LaunchAs = PAKType.NONE)
        {
            LaunchMode = LaunchAs;
            InitializeComponent();

            FileTree.ImageList = imageList1;
            treeHelper = new TreeUtility(FileTree);

            baseTitle = "OpenCAGE Asset Editor";
            if (LaunchMode != PAKType.NONE)
            {
                openToolStripMenuItem.Enabled = false;

                switch (LaunchMode)
                {
                    case PAKType.ANIMATIONS:
                        baseTitle += " - Animations";
                        break;
                    case PAKType.UI:
                        baseTitle += " - UI";
                        break;
                    case PAKType.MODELS:
                        baseTitle += " - Models";
                        break;
                    case PAKType.TEXTURES:
                        baseTitle += " - Textures";
                        break;
                    case PAKType.COMMANDS:
                        baseTitle += " - Scripts";
                        break;
                }
            }
            this.Text = baseTitle;

            preview = (ExplorerControlsWPF)elementHost1.Child;
            //preview.OnLevelSelected += LoadModePAK;
            preview.OnImportRequested += ImportFile;
            preview.OnExportRequested += ExportSelectedFile;
            preview.OnReplaceRequested += ImportSelectedFile;
            preview.OnDeleteRequested += DeleteSelectedFile;
            preview.OnExportAllRequested += ExportAll;
            preview.ShowFunctionButtons(PAKFunction.NONE);
            preview.ShowLevelSelect(LaunchMode != PAKType.NONE && LaunchMode != PAKType.ANIMATIONS && LaunchMode != PAKType.UI, LaunchMode);

            string lvlPath = "G:\\SteamLibrary\\steamapps\\common\\Alien Isolation\\DATA\\ENV\\PRODUCTION\\SOLACE";
            File.Copy(lvlPath + "\\RENDERABLE\\orig\\LEVEL_MODELS.PAK", lvlPath + "\\RENDERABLE\\LEVEL_MODELS.PAK", true);
            File.Copy(lvlPath + "\\RENDERABLE\\orig\\MODELS_LEVEL.BIN", lvlPath + "\\RENDERABLE\\MODELS_LEVEL.BIN", true);
            File.Copy(lvlPath + "\\RENDERABLE\\orig\\REDS.BIN", lvlPath + "\\RENDERABLE\\REDS.BIN", true);
            File.Copy(lvlPath + "\\RENDERABLE\\orig\\REDS.BIN", lvlPath + "\\WORLD\\REDS.BIN", true);
            Level lvl = new Level(lvlPath);

            AssimpContext importer = new AssimpContext();
            //"C:\\Users\\mattf\\Documents\\CUBE.fbx"
            //"C:\\Users\\mattf\\Downloads\\40-low-poly-cars-free_blender\\Low Poly Cars (Free)_blender\\LowPolyCars.obj"
            //"C:\\Users\\mattf\\Downloads\\de_dust2-cs-map\\source\\de_dust2\\de_dust2.obj"
            //"G:\\SteamLibrary\\steamapps\\common\\Alien Isolation\\DATA\\ENV\\PRODUCTION\\BSP_TORRENS\\WORLD\\ACID_DECAL_DISPLAY.fbx"
            //"C:\\Users\\mattf\\Downloads\\low-poly-dog\\source\\0eb7870dd86a4cfca6bbbce1d8afd42a.fbx.fbx"
            Scene model = importer.ImportFile("C:\\Users\\mattf\\Downloads\\realistic-young-girl-head-lowpoly-basemesh\\source\\monkey.fbx", PostProcessSteps.Triangulate | PostProcessSteps.FindDegenerates);
            importer.Dispose();
            Models.CS2.Component.LOD.Submesh car = model.Meshes[0].ToSubmesh();

            //"AYZ\\SCIENCE\\FEATURE_MED\\AMBULANCEDOCK_AIRLOCK\\AMBULANCEDOCK_AIRLOCK_DISPLAY.cs2"
            //"AYZ\\_PROPS_\\PHYSICS\\CARDBOARD_BOX_TEMPLATE\\CARDBOARD_BOX_TEMPLATE_DISPLAY.cs2"
            Models.CS2 cs2 = lvl.Models.Entries.FirstOrDefault(o => o.Name == "..\\CHARACTERS\\MARLOW_GP\\model0.cs2");
            cs2.Components[0].LODs[0].Submeshes[0].content = car.content;
            cs2.Components[0].LODs[0].Submeshes[0].IndexCount = car.IndexCount;
            cs2.Components[0].LODs[0].Submeshes[0].VertexCount = car.VertexCount;
            cs2.Components[0].LODs[0].Submeshes[0].VertexFormat = car.VertexFormat;
            cs2.Components[0].LODs[0].Submeshes[0].VertexFormatLowDetail = car.VertexFormatLowDetail;
            cs2.Components[0].LODs[0].Submeshes[0].ScaleFactor = 1;
            cs2.Components[0].LODs[0].Submeshes[0].MaterialLibraryIndex = 244/*lvl.Materials.Entries.IndexOf(lvl.Materials.Entries.FirstOrDefault(o => o.Name == "DEBUG_REPLACE_ME"))*/;
            for (int i = 1; i < cs2.Components.Count; i++)
            {
                cs2.Components[i].LODs[0].Submeshes[0].IndexCount = 0;
                cs2.Components[i].LODs[0].Submeshes[0].VertexCount = 0 ;
            }
            cs2.Name = "new";

            lvl.Save();

            //ProcessStartInfo alienProcess = new ProcessStartInfo();
            //alienProcess.WorkingDirectory = "G:\\SteamLibrary\\steamapps\\common\\Alien Isolation";
            //alienProcess.FileName = "G:\\SteamLibrary\\steamapps\\common\\Alien Isolation/AI.exe";
            //Process.Start(alienProcess);

            LoadModePAK("SOLACE");
        }

        /* Load the appropriate PAK for the given launch mode */
        private void LoadModePAK(string level)
        {
            string path = SharedData.pathToAI + "/DATA/";
            switch (LaunchMode)
            {
                case PAKType.ANIMATIONS:
                    path += "GLOBAL/ANIMATION.PAK";
                    break;
                case PAKType.UI:
                    path += "GLOBAL/UI.PAK";
                    break;
                case PAKType.TEXTURES:
                    if (level == "GLOBAL")
                        path += "ENV/GLOBAL/WORLD/GLOBAL_TEXTURES.ALL.PAK";
                    else
                        path += "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_TEXTURES.ALL.PAK";
                    break;
                case PAKType.MODELS:
                    if (level == "GLOBAL")
                        path += "ENV/GLOBAL/WORLD/GLOBAL_MODELS.PAK";
                    else
                        path += "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_MODELS.PAK";
                    break;
                case PAKType.COMMANDS:
                    path += "ENV/PRODUCTION/" + level + "/WORLD/COMMANDS.PAK";
                    break;
                case PAKType.MATERIAL_MAPPINGS:
                    path += "ENV/PRODUCTION/" + level + "/WORLD/MATERIAL_MAPPINGS.PAK";
                    break;
                default:
                    return;
            }
            this.Text = baseTitle + ((level == "") ? "" : " - " + level);
            LoadPAK(path);
        }

        /* Open a PAK and populate the GUI */
        private void LoadPAK(string filename, bool allowReload = false)
        {
            if (!allowReload && pak.File != null && pak.File.Filepath == filename)
                return;

            Cursor.Current = Cursors.WaitCursor;
            List<string> files = pak.Load(filename);
            treeHelper.UpdateFileTree(files);
            UpdateSelectedFilePreview();
            Cursor.Current = Cursors.Default;
        }

        /* Import a new file to the PAK */
        private void ImportFile()
        {
            return;

            /* This can only happen for UI files, so for OpenCAGE I'm forcing AlienPAKs[0] - might need changing for other PAKs that gain support */

            //Let the user decide what file to add, then add it
            OpenFileDialog FilePicker = new OpenFileDialog();
            FilePicker.Filter = "Any File|*.*";
            if (FilePicker.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                //PAKReturnType ResponseCode = AlienPAKs[0].AddNewFile(FilePicker.FileName);
                //MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
            //This is an expensive call for any PAK except PAK2, as it uses the new system.
            //We only can call with PAK2 here so it's fine, but worth noting.
            //treeHelper.UpdateFileTree(AlienPAKs[0].Parse());
            UpdateSelectedFilePreview();
        }

        /* Delete the selected file in the PAK */
        private void DeleteSelectedFile()
        {
            return;

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
                //PAKReturnType ResponseCode = AlienPAKs[0].RemoveFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value);
                //MessageBox.Show(AlienErrors.ErrorMessageBody(ResponseCode), AlienErrors.ErrorMessageTitle(ResponseCode), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
            }
            //This is an expensive call for any PAK except PAK2, as it uses the new system.
            //We only can call with PAK2 here so it's fine, but worth noting.
            //treeHelper.UpdateFileTree(AlienPAKs[0].Parse());
            UpdateSelectedFilePreview();
        }

        /* Export all files in the PAK */
        private void ExportAll()
        {
            return;

            //TODO: perhaps give a "convert to usable formats" checkbox on this export which converts to OBJ and PNG or something?
            Cursor.Current = Cursors.WaitCursor;
            FolderBrowserDialog FolderToExportTo = new FolderBrowserDialog();
            if (FolderToExportTo.ShowDialog() == DialogResult.OK)
            {
                int exportCount = 0;
                for (int i = 0; i < pak.Contents.Count; i++)
                {
                    byte[] content = pak.GetFileContent(pak.Contents[i]);
                    if (content == null) continue;
                    Directory.CreateDirectory(FolderToExportTo.SelectedPath + "/" + pak.Contents[i].Substring(0, pak.Contents[i].Length - Path.GetFileName(pak.Contents[i]).Length));
                    File.WriteAllBytes(FolderToExportTo.SelectedPath + "/" + pak.Contents[i], content);
                    exportCount++;
                }
                Process.Start(FolderToExportTo.SelectedPath);
            }
            Cursor.Current = Cursors.Default;
        }

        /* Import a file to replace the selected PAK entry */
        private void ImportSelectedFile()
        {
            return;

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
                byte[] ImageFile = pak.GetFileContent(((TreeItem)FileTree.SelectedNode.Tag).String_Value);
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
                        //MessageBox.Show("Failed to import as PNG!\nPlease try again as DDS.", AlienErrors.ErrorMessageTitle(PAKReturnType.FAIL_UNKNOWN), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                //Regular import
                if (ImportOK)
                {
                    //foreach (PAK thisPAK in AlienPAKs) thisPAK.ImportFile(((TreeItem)FileTree.SelectedNode.Tag).String_Value, FilePicker.FileName);
                    //if (ImportingConverted) File.Delete(FilePicker.FileName); //We temp dump out a converted file, which this cleans up
                    //MessageBox.Show(AlienErrors.ErrorMessageBody(PAKReturnType.SUCCESS), AlienErrors.ErrorMessageTitle(PAKReturnType.SUCCESS), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                Cursor.Current = Cursors.Default;
            }
            UpdateSelectedFilePreview();
        }

        /* Export the selected PAK entry as a standalone file */
        private void ExportSelectedFile()
        {
            if (FileTree.SelectedNode == null) return;
            TreeItemType nodeType = ((TreeItem)FileTree.SelectedNode.Tag).Item_Type;
            string nodeVal = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            switch (nodeType)
            {
                case TreeItemType.EXPORTABLE_FILE:
                    string filter = "Exported File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
                    if (preview.FilePreviewVisible && preview.FilePreviewBitmap != null) filter = "PNG Image|*.png|JPG Image|*.jpg|DDS Image|*.dds";
                    if (preview.ModelPreviewVisible) filter = "FBX Model|*.fbx|GLTF Model|*.gltf|OBJ Model|*.obj"; //TODO: we can support loads here with assimp (importer.GetSupportedExportFormats())

                    string fileName = Path.GetFileName(FileTree.SelectedNode.Text);
                    while (Path.GetExtension(fileName).Length != 0) fileName = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length); //Remove extensions from output filename

                    SaveFileDialog FilePicker = new SaveFileDialog();
                    FilePicker.Filter = filter;
                    FilePicker.FileName = fileName;
                    if (FilePicker.ShowDialog() == DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        try
                        {
                            switch (pak.Type)
                            {
                                case PAKType.ANIMATIONS:
                                case PAKType.UI:
                                    File.WriteAllBytes(FilePicker.FileName, pak.GetFileContent(nodeVal));
                                    break;
                                case PAKType.TEXTURES:
                                    Textures.TEX4 texture = ((Textures)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                    byte[] content = texture?.ToDDS();
                                    if (Path.GetExtension(FilePicker.FileName).ToUpper() == ".DDS")
                                    {
                                        File.WriteAllBytes(FilePicker.FileName, content);
                                        break;
                                    }
                                    preview.FilePreviewBitmap.Save(FilePicker.FileName); //TODO: this is a temp hacky workflow that should be avoided - need to move the bitmap conversion to the extension methods
                                    break;
                                case PAKType.MODELS:
                                    Scene scene = new Scene();
                                    scene.Materials.Add(new Assimp.Material());
                                    Models.CS2 cs2 = ((Models)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                    scene.RootNode = new Node(cs2.Name);
                                    for (int i = 0; i < cs2.Components.Count; i++)
                                    {
                                        Node componentNode = new Node(i.ToString());
                                        scene.RootNode.Children.Add(componentNode);
                                        for (int x = 0; x < cs2.Components[i].LODs.Count; x++)
                                        {
                                            Node lodNode = new Node(cs2.Components[i].LODs[x].Name);
                                            componentNode.Children.Add(lodNode);
                                            for (int y = 0; y < cs2.Components[i].LODs[x].Submeshes.Count; y++)
                                            {
                                                Node submeshNode = new Node(y.ToString());
                                                lodNode.Children.Add(submeshNode);

                                                Mesh mesh = cs2.Components[i].LODs[x].Submeshes[y].ToMesh();
                                                mesh.Name = cs2.Name + " [" + x + "] -> " + lodNode.Name + " [" + i + "]";
                                                scene.Meshes.Add(mesh);
                                                submeshNode.MeshIndices.Add(scene.Meshes.Count - 1);
                                            }
                                        }
                                    }
                                    AssimpContext importer = new AssimpContext();
                                    importer.ExportFile(scene, FilePicker.FileName, Path.GetExtension(FilePicker.FileName).Replace(".", ""));
                                    importer.Dispose();
                                    break;
                                default:
                                    MessageBox.Show("This PAK type does not support file exporting!", "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                            }
                            MessageBox.Show("Successfully exported file!", "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        Cursor.Current = Cursors.Default;
                    }
                    break;
            }
        }

        /* Try free-up when closing */
        private void Explorer_FormClosed(object sender, FormClosedEventArgs e)
        {
            pak?.Unload();
            treeHelper.UpdateFileTree(new List<string>());
            treeHelper = null;
            pak = null;
            preview = null;
        }

        /* Update file preview */
        private void UpdateSelectedFilePreview()
        {
            preview.ShowFunctionButtons(pak.Functionality);
            if (FileTree.SelectedNode == null) return;
            TreeItemType nodeType = ((TreeItem)FileTree.SelectedNode.Tag).Item_Type;
            string nodeVal = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            switch (nodeType)
            {
                case TreeItemType.EXPORTABLE_FILE:
                    switch (pak.Type)
                    {
                        //case PAKType.ANIMATIONS:
                        case PAKType.UI:
                            PAK2.File file = ((PAK2)pak.File).Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                            preview.SetFileInfo(Path.GetFileName(nodeVal), file?.Content.Length.ToString());
                            preview.SetImagePreview(file.Content);
                            break;
                        case PAKType.TEXTURES:
                            Textures.TEX4 texture = ((Textures)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                            byte[] content = texture?.ToDDS();
                            preview.SetFileInfo(Path.GetFileName(nodeVal), content?.Length.ToString());
                            preview.SetImagePreview(content);
                            break;
                        case PAKType.MODELS:
                            /*
                            Model3DGroup model1 = new Model3DGroup();
                            Models.CS2.Component.LOD.Submesh submesh1 = CathodeLibExtensions.ToSubmesh(null);
                            model1.Children.Add(submesh1.ToGeometryModel3D());
                            preview.SetModelPreview(model1);
                            break;*/
                            Models.CS2 cs2 = ((Models)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                            Model3DGroup model = new Model3DGroup();
                            foreach (Models.CS2.Component component in cs2.Components)
                                foreach (Models.CS2.Component.LOD lod in component.LODs)
                                    foreach (Models.CS2.Component.LOD.Submesh submesh in lod.Submeshes)
                                        model.Children.Add(submesh.ToGeometryModel3D()); //TODO: are there some offsets/scaling we should be accounting for here?
                            preview.SetModelPreview(model); //TODO: perhaps we should just pass the CS2 object to the model previewer and let that pick what to render
                            break;
                    }
                    break;
            }
        }

        /* User requests to open a PAK */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Allow selection of a PAK from filepicker, then open
            OpenFileDialog ArchivePicker = new OpenFileDialog();
            ArchivePicker.Filter = "Alien: Isolation PAK|*.PAK";
            if (ArchivePicker.ShowDialog() == DialogResult.OK)
            {
                LoadPAK(ArchivePicker.FileName);
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

        /* Item selected (show preview info) */
        private void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateSelectedFilePreview();
        }
    }
}
