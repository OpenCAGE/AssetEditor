using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Assimp;
using Assimp.Unmanaged;
using CATHODE;
using CATHODE.LEGACY;
using CathodeLib;
using DirectXTexNet;
using static CATHODE.Materials.Material;

namespace AlienPAK
{
    public partial class Explorer : Form
    {
        public PAKWrapper pak = new PAKWrapper();
        string extraPath = "";

        //TODO: having implemented all this to get textured models, we might as well just use the CathodeLib Level func instead of the above PAK stuff
        private Textures textures = null;
        private Textures texturesGlobal = null;
        private Materials materials = null;
        private ShadersPAK shaders = null;
        private IDXRemap shadersIDX = null;

        TreeUtility treeHelper;
        ExplorerControlsWPF preview;

        PAKType LaunchMode;
        string baseTitle;

        public Explorer(string level = null, string mode = null)
        {
            if (level == null || mode == null)
            {
                Launch();
                return;
            }

            Enum.TryParse<PAKType>(mode, out PAKType modeEnum);
            Launch(modeEnum);
            LoadModePAK(level);
            explorerControlsWPF1.levelSelectDropdown.Text = level;
        }

        public Explorer(PAKType LaunchAs = PAKType.NONE)
        {
            Launch(LaunchAs);
        }

        private void Launch(PAKType LaunchAs = PAKType.NONE)
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
                    case PAKType.CHR_INFO:
                        baseTitle += " - Character Info";
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
                    case PAKType.MATERIAL_MAPPINGS:
                        baseTitle += " - Material Mappings";
                        break;
                }
            }
            this.Text = baseTitle;

            preview = (ExplorerControlsWPF)elementHost1.Child;
            preview.OnLevelSelected += LoadModePAK;
            preview.OnImportRequested += ImportNewFile;
            preview.OnExportRequested += ExportSelectedFile;
            preview.OnReplaceRequested += ReplaceSelectedFile;
            preview.OnDeleteRequested += DeleteSelectedFile;
            preview.OnExportAllRequested += ExportAllFiles;
            preview.OnPortRequested += PortSelectedFile;
            preview.ShowFunctionButtons(PAKFunction.NONE, LaunchMode, false);
            preview.ShowLevelSelect(LaunchMode != PAKType.NONE && LaunchMode != PAKType.ANIMATIONS && LaunchMode != PAKType.UI, LaunchMode);
        }

        /* Load the appropriate PAK for the given launch mode */
        private void LoadModePAK(string level)
        {
            string path = SharedData.pathToAI + "/DATA/";
            extraPath = "";
            textures = null;
            texturesGlobal = null;
            materials = null;
            shaders = null;
            shadersIDX = null;
            switch (LaunchMode)
            {
                case PAKType.ANIMATIONS:
                    path += "GLOBAL/ANIMATION.PAK";
                    break;
                case PAKType.UI:
                    path += "GLOBAL/UI.PAK";
                    break;
                case PAKType.CHR_INFO:
                    path += "CHR_INFO.PAK";
                    break;
                case PAKType.TEXTURES:
                    if (level == "GLOBAL")
                    {
                        //TODO: here we'll need to update ALL LEVELS that point to GLOBAL :/
                        //We probs shouldn't support GLOBAL until we do this...
                        path += "ENV/GLOBAL/WORLD/GLOBAL_TEXTURES.ALL.PAK";
                    }
                    else
                    {
                        extraPath = path + "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_MODELS.MTL";
                        path += "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_TEXTURES.ALL.PAK";
                    }
                    break;
                case PAKType.MODELS:
                    if (level == "GLOBAL")
                        throw new Exception("Not supporting this yet.");
                        //path += "ENV/GLOBAL/WORLD/GLOBAL_MODELS.PAK";
                    else
                    {
                        extraPath = path + "ENV/PRODUCTION/" + level + "/WORLD/REDS.BIN";

                        //TEMP!!
                        textures = new Textures(path + "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_TEXTURES.ALL.PAK");
                        texturesGlobal = new Textures(path + "ENV/GLOBAL/WORLD/GLOBAL_TEXTURES.ALL.PAK");
                        materials = new Materials(path + "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_MODELS.MTL");
                        shaders = new ShadersPAK(path + "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_SHADERS_DX11.PAK"); //legacy!
                        shadersIDX = new IDXRemap(path + "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_SHADERS_DX11_IDX_REMAP.PAK"); //legacy!

                        path += "ENV/PRODUCTION/" + level + "/RENDERABLE/LEVEL_MODELS.PAK";
                    }
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

            if (portPopup != null) portPopup.Close();

            Cursor.Current = Cursors.WaitCursor;
            List<string> files = pak.Load(filename);
            treeHelper.UpdateFileTree(files);
            UpdateSelectedFilePreview();
            Cursor.Current = Cursors.Default;
        }

        /* Import a new file to the PAK */
        private void ImportNewFile()
        {
            OpenFileDialog FilePicker = new OpenFileDialog();
            switch (pak.Type)
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
                switch (pak.Type)
                {
                    case PAKType.ANIMATIONS:
                    case PAKType.UI:
                    case PAKType.CHR_INFO:
                        PAK2 pak2PAK = (PAK2)pak.File;
                        newFileName = Path.GetFileName(FilePicker.FileName);
                        pak2PAK.Entries.Add(new PAK2.File() { Filename = newFileName, Content = File.ReadAllBytes(FilePicker.FileName) });
                        break;
                    case PAKType.TEXTURES:
                        Textures texturePAK = (Textures)pak.File;
                        newFileName += ".dds";
                        Textures.TEX4 texture = new Textures.TEX4() { Name = Path.GetFileName(FilePicker.FileName) };
                        if (Path.GetExtension(FilePicker.FileName).ToUpper() == ".DDS")
                        {
                            byte[] content = File.ReadAllBytes(FilePicker.FileName);
                            //TODO: perhaps we need a custom UI for this to allow swapping high/low res assets individually
                            Textures.TEX4.Part part = content?.ToTEX4Part(out texture.Format);
                            part.unk3 = 4294967295;
                            texture.Type = Textures.AlienTextureType.DIFFUSE; //todo: ui to allow selection of this
                            texture.tex_HighRes = part.Copy();
                            texture.tex_LowRes = part.Copy();
                            texture.tex_LowRes.unk2 = 32768;
                            texturePAK.Entries.Add(texture);
                            SaveTexturesAndUpdateMaterials((Textures)pak.File, new Materials(extraPath));
                            break;
                        }
                        //TODO: implement DDS conversion
                        break;
                    case PAKType.MODELS:
                        Models modelsPAK = (Models)pak.File;
                        Models.CS2 cs2 = new Models.CS2();
                        newFileName += ".cs2";
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
                                if (i == 0) submesh.Unknown2_ = 134282240;
                                else submesh.Unknown2_ = 134239232;
                                cs2.Components[0].LODs[0].Submeshes.Add(submesh);
                            }
                        }
                        if (cs2.Components[0].LODs[0].Submeshes.Count == 0)
                        {
                            MessageBox.Show("Failed to generate CS2 from selected model: could not find any mesh data! Please ensure all meshes are children of the scene's root node.", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        modelsPAK.Entries.Add(cs2);
                        SaveModelsAndUpdateREDS();
                        break;
                    default:
                        return;
                }
                MessageBox.Show("Successfully imported file!", "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Cursor.Current = Cursors.Default;
            pak.Contents.Add(newFileName);
            treeHelper.UpdateFileTree(pak.Contents);
            UpdateSelectedFilePreview();
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
                        switch (pak.Type)
                        {
                            case PAKType.ANIMATIONS:
                            case PAKType.UI:
                            case PAKType.CHR_INFO:
                                ((PAK2)pak.File).Entries.RemoveAll(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                pak.File.Save();
                                break;
                            case PAKType.TEXTURES:
                                ((Textures)pak.File).Entries.RemoveAll(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                SaveTexturesAndUpdateMaterials((Textures)pak.File, new Materials(extraPath));
                                break;
                            case PAKType.MATERIAL_MAPPINGS:
                                ((MaterialMappings)pak.File).Entries.RemoveAll(o => o.MapFilename.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                pak.File.Save();
                                break;
                            case PAKType.MODELS:
                                ((Models)pak.File).Entries.RemoveAll(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                SaveModelsAndUpdateREDS();
                                break;
                            default:
                                MessageBox.Show("This PAK type does not support file deleting!", "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                        }
                        MessageBox.Show("Successfully deleted file!", "Delete complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Cursor.Current = Cursors.Default;
                    pak.Contents.RemoveAll(o => o.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                    treeHelper.UpdateFileTree(pak.Contents);
                    UpdateSelectedFilePreview();
                    break;
            }

            return;
        }

        /* Export all files in the PAK */
        private void ExportAllFiles()
        {
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

        /* Show window to port the selected file to another level */
        PortContent portPopup = null;
        private void PortSelectedFile()
        {
            if (FileTree.SelectedNode == null) return;
            TreeItemType nodeType = ((TreeItem)FileTree.SelectedNode.Tag).Item_Type;
            if (nodeType != TreeItemType.EXPORTABLE_FILE) return;
            string nodeVal = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            if (portPopup == null)
            {
                portPopup = new PortContent();
                portPopup.FormClosed += Popup_FormClosed;
            }
            portPopup.Setup(pak.Type, pak.File, nodeVal.Replace('\\', '/'), explorerControlsWPF1.levelSelectDropdown.Text);
            portPopup.Show();
        }
        private void Popup_FormClosed(object sender, FormClosedEventArgs e)
        {
            portPopup = null;
            this.BringToFront();
            this.Focus();
        }

        /* Import a file to replace the selected PAK entry */
        private void ReplaceSelectedFile()
        {
            if (FileTree.SelectedNode == null) return;
            TreeItemType nodeType = ((TreeItem)FileTree.SelectedNode.Tag).Item_Type;
            string nodeVal = ((TreeItem)FileTree.SelectedNode.Tag).String_Value;

            switch (nodeType)
            {
                case TreeItemType.EXPORTABLE_FILE:
                    //TODO: refactor
                    if (pak.Type == PAKType.MODELS)
                    {
                        Models.CS2 cs2 = ((Models)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                        ModelEditor modelEditor = new ModelEditor(cs2, textures, texturesGlobal, materials, shaders, shadersIDX);
                        modelEditor.FormClosed += ModelEditor_FormClosed;
                        modelEditor.Show();
                        break;
                    }

                    string filter = "File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
                    //if (preview.FilePreviewVisible && preview.FilePreviewBitmap != null) filter = "PNG Image|*.png|JPG Image|*.jpg|DDS Image|*.dds";

                    OpenFileDialog FilePicker = new OpenFileDialog();
                    FilePicker.Filter = filter;
                    if (FilePicker.ShowDialog() != DialogResult.OK) break;

                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        switch (pak.Type)
                        {
                            case PAKType.ANIMATIONS:
                            case PAKType.UI:
                            case PAKType.CHR_INFO:
                                ((PAK2)pak.File).Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/')).Content = File.ReadAllBytes(FilePicker.FileName);
                                pak.File.Save();
                                break;
                            case PAKType.TEXTURES:
                                Textures.TEX4 texture = ((Textures)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                if (Path.GetExtension(FilePicker.FileName).ToUpper() == ".DDS")
                                {
                                    byte[] content = File.ReadAllBytes(FilePicker.FileName);
                                    Textures.TEX4.Part part = texture?.tex_HighRes?.Content != null ? texture.tex_HighRes : texture?.tex_LowRes?.Content != null ? texture.tex_LowRes : null;
                                    part = content?.ToTEX4Part(out texture.Format, part);
                                    if (texture?.tex_HighRes?.Content != null) texture.tex_HighRes = part;
                                    else texture.tex_LowRes = part;
                                    SaveTexturesAndUpdateMaterials((Textures)pak.File, new Materials(extraPath));
                                    break;
                                }
                                //TODO: implement this!!!! (into import new above too)
                                MessageBox.Show("PNG/JPG image import conversion is not currently supported!", "WIP", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                break;
                                ScratchImage img = TexHelper.Instance.LoadFromWICFile(FilePicker.FileName, WIC_FLAGS.FORCE_RGB).GenerateMipMaps(TEX_FILTER_FLAGS.DEFAULT, 10); /* Was using 11, but gives remainders - going for 10 */
                                ScratchImage imgDecom = img.Compress(DXGI_FORMAT.BC7_UNORM, TEX_COMPRESS_FLAGS.BC7_QUICK, 0.5f); //TODO use baseFormat
                                imgDecom.SaveToDDSFile(DDS_FLAGS.FORCE_DX10_EXT, FilePicker.FileName + ".DDS");
                                break;
                            default:
                                MessageBox.Show("This PAK type does not support file importing!", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                        }
                        MessageBox.Show("Successfully imported file!", "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            SaveModelsAndUpdateREDS();
            //Thread.Sleep(1500); //todo: temp hack 
            SaveTexturesAndUpdateMaterials(textures, materials);
            UpdateSelectedFilePreview();
            this.BringToFront();
            this.Focus();
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
                    string filter = "File|*" + Path.GetExtension(FileTree.SelectedNode.Text);
                    if (preview.FilePreviewVisible && preview.FilePreviewBitmap != null) filter = "PNG Image|*.png|JPG Image|*.jpg|DDS Image|*.dds";
                    if (preview.ModelPreviewVisible) filter = "FBX Model|*.fbx|GLTF Model|*.gltf|OBJ Model|*.obj"; //TODO: we can support loads here with assimp (importer.GetSupportedExportFormats())

                    string fileName = Path.GetFileName(FileTree.SelectedNode.Text);
                    while (Path.GetExtension(fileName).Length != 0) fileName = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length); //Remove extensions from output filename

                    SaveFileDialog picker = new SaveFileDialog();
                    picker.Filter = filter;
                    picker.FileName = fileName;
                    if (picker.ShowDialog() != DialogResult.OK) break;

                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        switch (pak.Type)
                        {
                            case PAKType.ANIMATIONS:
                            case PAKType.UI:
                            case PAKType.CHR_INFO:
                                File.WriteAllBytes(picker.FileName, ((PAK2)pak.File).Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == nodeVal.Replace('\\', '/'))?.Content);
                                break;
                            case PAKType.TEXTURES:
                                Textures.TEX4 texture = ((Textures)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                                byte[] content = texture?.ToDDS();
                                if (Path.GetExtension(picker.FileName).ToUpper() == ".DDS")
                                {
                                    File.WriteAllBytes(picker.FileName, content);
                                    break;
                                }
                                preview.FilePreviewBitmap.Save(picker.FileName); //TODO: this is a temp hacky workflow that should be avoided - need to move the bitmap conversion to the extension methods
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
                                AssimpContext exp = new AssimpContext();
                                exp.ExportFile(scene, picker.FileName, Path.GetExtension(picker.FileName).Replace(".", ""));
                                exp.Dispose();
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
            preview.ShowFunctionButtons(pak.Functionality, pak.Type, FileTree.SelectedNode != null && ((TreeItem)FileTree.SelectedNode.Tag).Item_Type == TreeItemType.EXPORTABLE_FILE);
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
                        case PAKType.CHR_INFO:
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
                            Models.CS2 cs2 = ((Models)pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == nodeVal.Replace('\\', '/'));
                            Model3DGroup model = new Model3DGroup();
                            int verts = 0;
                            foreach (Models.CS2.Component component in cs2.Components)
                            {
                                foreach (Models.CS2.Component.LOD lod in component.LODs)
                                {
                                    foreach (Models.CS2.Component.LOD.Submesh submesh in lod.Submeshes)
                                    {
                                        GeometryModel3D mdl = submesh.ToGeometryModel3D();
                                        verts += submesh.VertexCount;
                                        try
                                        {
                                            ShadersPAK.ShaderMaterialMetadata mdlMeta = shaders.GetMaterialMetadataFromShader(materials.GetAtWriteIndex(submesh.MaterialLibraryIndex), shadersIDX);
                                            ShadersPAK.MaterialTextureContext mdlMetaDiff = mdlMeta.textures.FirstOrDefault(o => o.Type == ShadersPAK.ShaderSlot.DIFFUSE_MAP);
                                            if (mdlMetaDiff != null)
                                            {
                                                Textures tex = mdlMetaDiff.TextureInfo.Source == Texture.TextureSource.GLOBAL ? texturesGlobal : textures;
                                                Textures.TEX4 diff = tex.GetAtWriteIndex(mdlMetaDiff.TextureInfo.BinIndex);
                                                byte[] diffDDS = diff?.ToDDS();
                                                mdl.Material = new DiffuseMaterial(new ImageBrush(diffDDS?.ToBitmap()?.ToImageSource()));
                                                //TODO: normals?
                                            }
                                        }
                                        catch (Exception ex2)
                                        {
                                            Console.WriteLine(ex2.ToString());
                                        }
                                        model.Children.Add(mdl); //TODO: are there some offsets/scaling we should be accounting for here?
                                    }
                                }
                            }
                            preview.SetFileInfo(Path.GetFileName(nodeVal), verts.ToString(), true);
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
            if (portPopup != null) portPopup.Close();
            UpdateSelectedFilePreview();
        }

        public void SaveModelsAndUpdateREDS()
        {
            Models modelsPAK = ((Models)pak.File);
            if (extraPath == "")
            {
                modelsPAK.Save();
                return;
            }
            RenderableElements reds = new RenderableElements(extraPath);
            List<Models.CS2.Component.LOD.Submesh> redsModels = new List<Models.CS2.Component.LOD.Submesh>();
            for (int i = 0; i < reds.Entries.Count; i++)
                redsModels.Add(modelsPAK.GetAtWriteIndex(reds.Entries[i].ModelIndex));
            modelsPAK.Save();
            for (int i = 0; i < reds.Entries.Count; i++)
                reds.Entries[i].ModelIndex = modelsPAK.GetWriteIndex(redsModels[i]);
            reds.Save();
        }

        public static void SaveTexturesAndUpdateMaterials(Textures texturesPAK, Materials materials)
        {
            List<Textures.TEX4> materialTextures = new List<Textures.TEX4>();
            for (int i = 0; i < materials.Entries.Count; i++)
            {
                for (int x = 0; x < materials.Entries[i].TextureReferences.Length; x++)
                {
                    if (materials.Entries[i].TextureReferences[x] == null) continue;
                    switch (materials.Entries[i].TextureReferences[x].Source)
                    {
                        case Materials.Material.Texture.TextureSource.LEVEL:
                            materialTextures.Add(texturesPAK.GetAtWriteIndex(materials.Entries[i].TextureReferences[x].BinIndex));
                            break;
                        case Materials.Material.Texture.TextureSource.GLOBAL:
                            materialTextures.Add(null/*GlobalTextures.GetAtWriteIndex(materials.Entries[i].TextureReferences[x].BinIndex)*/);
                            break;
                    }
                }
            }
            materials.Save();
            texturesPAK.Save();
            int y = 0;
            for (int i = 0; i < materials.Entries.Count; i++)
            {
                for (int x = 0; x < materials.Entries[i].TextureReferences.Length; x++)
                {
                    if (materials.Entries[i].TextureReferences[x] == null) continue;
                    switch (materials.Entries[i].TextureReferences[x].Source)
                    {
                        case Materials.Material.Texture.TextureSource.LEVEL:
                            materials.Entries[i].TextureReferences[x].BinIndex = texturesPAK.GetWriteIndex(materialTextures[y]);
                            break;
                        case Materials.Material.Texture.TextureSource.GLOBAL:
                            //materials.Entries[i].TextureReferences[x].BinIndex = GlobalTextures.GetWriteIndex(materialTextures[y]);
                            break;
                    }
                    y++;
                }
            }
            materials.Save();
        }
    }
}
