using CATHODE.LEGACY;
using CATHODE;
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
using static CATHODE.Materials.Material;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using static CATHODE.Models;
using static CATHODE.Models.CS2.Component;
using System.Windows.Media.Animation;
using static CATHODE.Models.CS2.Component.LOD;
using Assimp;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static CATHODE.Models.CS2;

namespace AlienPAK
{
    public partial class ModelEditor : Form
    {
        private TreeUtility _treeHelper;
        private CS2 _model = null;
        private List<StringMeshLookup> _treeLookup = new List<StringMeshLookup>();

        private Textures _textures = null;
        private Textures _texturesGlobal = null;
        private Materials _materials = null;
        private ShadersPAK _shaders = null;
        private IDXRemap _shadersIDX = null;

        private const string _fileFilter = "FBX Model|*.fbx|GLTF Model|*.gltf|OBJ Model|*.obj"; //TODO: we can support loads here with assimp (importer.GetSupportedExportFormats())

        ModelEditorControlsWPF _controls;

        public ModelEditor(CS2 model = null, Textures textures = null, Textures texturesGlobal = null, Materials materials = null, ShadersPAK shaders = null, IDXRemap shadersIDX = null)
        {
            _model = model;
            _textures = textures;
            _texturesGlobal = texturesGlobal;
            _materials = materials;
            _shaders = shaders;
            _shadersIDX = shadersIDX;

            InitializeComponent();
            _treeHelper = new TreeUtility(FileTree);
            if (_model == null) return;
            this.Text = _model.Name;

            RefreshTree();

            _controls = (ModelEditorControlsWPF)elementHost1.Child;
            _controls.renderMaterials.IsChecked = true; //todo: remember this option
            _controls.OnEditMaterialRequested += OnEditMaterialRequested;
            _controls.OnMaterialRenderCheckChanged += OnMaterialRenderCheckChanged;
            _controls.OnAddRequested += OnAddRequested;
            _controls.OnReplaceRequested += OnReplaceRequested;
            _controls.OnDeleteRequested += OnDeleteRequested;
            _controls.OnExportRequested += OnExportRequested;
        }

        /* Export model by selected part */
        private void OnExportRequested()
        {
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode?.FullPath);

            string fileName = Path.GetFileName(FileTree.SelectedNode.Text);
            while (Path.GetExtension(fileName).Length != 0) fileName = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length); //Remove extensions from output filename

            SaveFileDialog picker = new SaveFileDialog();
            picker.Filter = _fileFilter;
            picker.FileName = fileName;
            if (picker.ShowDialog() != DialogResult.OK) return;

            try
            {
                Scene scene = new Scene();
                scene.Materials.Add(new Assimp.Material());
                scene.RootNode = new Node(_model.Name);
                for (int i = 0; i < _model.Components.Count; i++)
                {
                    if (lookup != null && lookup.component != null && _model.Components[i] != lookup.component) continue;
                    Node componentNode = new Node(i.ToString());
                    scene.RootNode.Children.Add(componentNode);
                    for (int x = 0; x < _model.Components[i].LODs.Count; x++)
                    {
                        if (lookup != null && lookup.lod != null && _model.Components[i].LODs[x] != lookup.lod) continue;
                        Node lodNode = new Node(_model.Components[i].LODs[x].Name);
                        componentNode.Children.Add(lodNode);
                        for (int y = 0; y < _model.Components[i].LODs[x].Submeshes.Count; y++)
                        {
                            if (lookup != null && lookup.submesh != null && _model.Components[i].LODs[x].Submeshes[y] != lookup.submesh) continue;
                            Node submeshNode = new Node(y.ToString());
                            lodNode.Children.Add(submeshNode);

                            Mesh mesh = _model.Components[i].LODs[x].Submeshes[y].ToMesh();
                            mesh.Name = _model.Name + " [" + x + "] -> " + lodNode.Name + " [" + i + "]";
                            scene.Meshes.Add(mesh);
                            submeshNode.MeshIndices.Add(scene.Meshes.Count - 1);
                        }
                    }
                }
                AssimpContext exp = new AssimpContext();
                exp.ExportFile(scene, picker.FileName, Path.GetExtension(picker.FileName).Replace(".", ""));
                exp.Dispose();
                MessageBox.Show("Successfully exported file!", "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /* Delete selected model part */
        private void OnDeleteRequested()
        {
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode?.FullPath);

            if (lookup?.cs2 != null)
                lookup.cs2.Components.Clear();
            if (lookup?.component != null)
                lookup.component.LODs.Clear();
            if (lookup?.lod != null)
                lookup.lod.Submeshes.Clear();
            if (lookup?.submesh != null)
                for (int i = 0; i < _model.Components.Count; i++)
                    for (int x = 0; x < _model.Components[i].LODs.Count; x++)
                        _model.Components[i].LODs[x].Submeshes.Remove(lookup.submesh);

            RefreshTree();
            RefreshSelectedModelPreview();
        }

        /* Replace selected model part */
        private void OnReplaceRequested()
        {
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode?.FullPath);
            if (lookup == null || lookup.submesh == null) return;

            OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Filter = _fileFilter;
            if (filePicker.ShowDialog() != DialogResult.OK) return;

            Models.CS2.Component.LOD.Submesh submesh = null;
            try
            {
                using (AssimpContext importer = new AssimpContext())
                {
                    Scene model = importer.ImportFile(filePicker.FileName, PostProcessSteps.Triangulate | PostProcessSteps.FindDegenerates | PostProcessSteps.LimitBoneWeights | PostProcessSteps.GenerateBoundingBoxes); //PostProcessSteps.PreTransformVertices
                    submesh = model.Meshes[0].ToSubmesh();
                }
            }
            catch { }
            if (submesh == null)
            {
                MessageBox.Show("An error occurred while generating the CS2 submesh!\nPlease try again, or use a different model.\nYour model must contain a single mesh in the root node.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            lookup.submesh.content = submesh.content;
            lookup.submesh.IndexCount = submesh.IndexCount;
            lookup.submesh.VertexCount = submesh.VertexCount;
            lookup.submesh.VertexFormat = submesh.VertexFormat;
            lookup.submesh.VertexFormatLowDetail = submesh.VertexFormatLowDetail;
            lookup.submesh.ScaleFactor = submesh.ScaleFactor;
            lookup.submesh.AABBMax = submesh.AABBMax;
            lookup.submesh.AABBMin = submesh.AABBMin;
            //lookup.submesh.LODMaxDistance_ = 99999999;
            //lookup.submesh.LODMinDistance_ = -99999999;
            RefreshTree();
            RefreshSelectedModelPreview();
        }

        /* Refresh the model tree in UI */
        private void RefreshTree()
        {
            List<string> contents = new List<string>();
            string componentString = "";
            string lodString = "";
            string submeshString = "";
            for (int i = 0; i < _model.Components.Count; i++)
            {
                componentString = Path.GetFileName(_model.Name) + "\\Component " + i;
                _treeLookup.Add(new StringMeshLookup() { String = componentString, component = _model.Components[i] });
                for (int x = 0; x < _model.Components[i].LODs.Count; x++)
                {
                    lodString = componentString + "\\Part " + x + ": " + _model.Components[i].LODs[x].Name;
                    _treeLookup.Add(new StringMeshLookup() { String = lodString, lod = _model.Components[i].LODs[x] });
                    for (int y = 0; y < _model.Components[i].LODs[x].Submeshes.Count; y++)
                    {
                        submeshString = lodString + "\\Mesh " + y;
                        _treeLookup.Add(new StringMeshLookup() { String = submeshString, submesh = _model.Components[i].LODs[x].Submeshes[y] });
                        contents.Add(submeshString);
                    }
                }
            }
            _treeHelper.UpdateFileTree(contents);
            FileTree.ExpandAll();
        }

        /* Add a new submesh to the model at selected location */
        private void OnAddRequested(SelectedModelType type)
        {
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode?.FullPath);
            if (lookup == null) return;

            OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Filter = _fileFilter; 
            if (filePicker.ShowDialog() != DialogResult.OK) return;

            Models.CS2.Component.LOD.Submesh submesh = null;
            using (AssimpContext importer = new AssimpContext())
            {
                Scene model = importer.ImportFile(filePicker.FileName, PostProcessSteps.Triangulate | PostProcessSteps.FindDegenerates | PostProcessSteps.LimitBoneWeights | PostProcessSteps.GenerateBoundingBoxes); //PostProcessSteps.PreTransformVertices
                submesh = model.Meshes[0].ToSubmesh();
            }
            if (submesh == null)
            {
                MessageBox.Show("An error occurred while generating the CS2 submesh!\nPlease try again, or use a different model.\nYour model must contain a single mesh in the root node.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            submesh.Unknown2_ = 134282240;

            switch (type)
            {
                case SelectedModelType.COMPONENT:
                    if (lookup.cs2 != null)
                    {
                        CS2.Component newComponent = new CS2.Component();
                        LOD newLOD = new LOD(Path.GetFileNameWithoutExtension(filePicker.FileName));
                        newLOD.Submeshes.Add(submesh); 
                        newComponent.LODs.Add(newLOD);
                        lookup.cs2.Components.Add(newComponent);
                    }
                    break;
                case SelectedModelType.LOD:
                    if (lookup.component != null)
                    {
                        LOD newLOD = new LOD(Path.GetFileNameWithoutExtension(filePicker.FileName));
                        newLOD.Submeshes.Add(submesh);
                        lookup.component.LODs.Add(newLOD); 
                    }
                    break;
                case SelectedModelType.SUBMESH:
                    if (lookup.lod != null)
                    {
                        submesh.Unknown2_ = lookup.lod.Submeshes.Count == 0 ? (uint)134282240 : (uint)134239232; 
                        lookup.lod.Submeshes.Add(submesh);
                    }
                    break;
            }
            RefreshTree();
        }

        /* Edit the material of the selected submesh */
        private void OnEditMaterialRequested()
        {
            if (FileTree.SelectedNode == null) return;
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode.FullPath);
            if (lookup == null || lookup.submesh == null) return;
            Materials.Material material = _materials.GetAtWriteIndex(lookup.submesh.MaterialLibraryIndex);
            if (material == null) return;

            MaterialEditor materialEditor = new MaterialEditor(material, _materials, _shaders, _textures, _texturesGlobal, _shadersIDX);
            materialEditor.OnMaterialSelected += OnMaterialSelected;
            materialEditor.Show();
        }
        private void OnMaterialSelected(int index)
        {
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode.FullPath);
            Submesh submesh = lookup?.submesh;
            if (submesh == null) return;
            submesh.MaterialLibraryIndex = index;
            RefreshSelectedModelPreview(false);
        }

        /* Enable/disable materials in render */
        private void OnMaterialRenderCheckChanged(bool check)
        {
            RefreshSelectedModelPreview(false);
        }

        /* Select model tree object */
        private void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedModelPreview();
        }
        
        /* Update model preview based on selected tree object */
        private void RefreshSelectedModelPreview(bool doZoom = true)
        {
            _controls.ShowContextualButtons(SelectedModelType.NONE);
            _controls.SetModelPreview(null, "", 0, "");

            if (FileTree.SelectedNode == null) return;
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode.FullPath);

            _controls.ShowContextualButtons(lookup?.component != null ? SelectedModelType.COMPONENT : lookup?.lod != null ? SelectedModelType.LOD : lookup?.submesh != null ? SelectedModelType.SUBMESH : SelectedModelType.CS2);

            Model3DGroup model = new Model3DGroup();
            int vertCount = 0;
            string materialInfo = "";
            foreach (Models.CS2.Component component in _model.Components)
            {
                if (lookup != null && lookup.component != null && component != lookup.component) continue;
                foreach (Models.CS2.Component.LOD lod in component.LODs)
                {
                    if (lookup != null && lookup.lod != null && lod != lookup.lod) continue;
                    foreach (Models.CS2.Component.LOD.Submesh submesh in lod.Submeshes)
                    {
                        if (lookup != null && lookup.submesh != null && submesh != lookup.submesh) continue;
                        GeometryModel3D mdl = submesh.ToGeometryModel3D();
                        try
                        {
                            Materials.Material material = _materials.GetAtWriteIndex(submesh.MaterialLibraryIndex);
                            if (lookup?.submesh != null) materialInfo = material.Name;
                            if (_controls.renderMaterials.IsChecked == true)
                            {
                                ShadersPAK.ShaderMaterialMetadata mdlMeta = _shaders.GetMaterialMetadataFromShader(material, _shadersIDX);
                                ShadersPAK.MaterialTextureContext mdlMetaDiff = mdlMeta.textures.FirstOrDefault(o => o.Type == ShadersPAK.ShaderSlot.DIFFUSE_MAP);
                                if (mdlMetaDiff != null)
                                {
                                    Textures tex = mdlMetaDiff.TextureInfo.Source == Texture.TextureSource.GLOBAL ? _texturesGlobal : _textures;
                                    Textures.TEX4 diff = tex.GetAtWriteIndex(mdlMetaDiff.TextureInfo.BinIndex);
                                    byte[] diffDDS = diff?.ToDDS();
                                    mdl.Material = new DiffuseMaterial(new ImageBrush(diffDDS?.ToBitmap()?.ToImageSource()));
                                    mdl.BackMaterial = null;
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine(ex2.ToString());
                        }
                        MeshGeometry3D geo = (MeshGeometry3D)mdl.Geometry;
                        if (geo != null) vertCount += geo.Positions.Count;
                        model.Children.Add(mdl); //TODO: are there some offsets/scaling we should be accounting for here?
                    }
                }
            }

            string[] nameContents = (FileTree.SelectedNode.FullPath + "\\").Split('\\');
            _controls.SetModelPreview(model, nameContents[nameContents.Length - 2], vertCount, materialInfo, doZoom); 
        }

        private class StringMeshLookup
        {
            public string String;

            public CATHODE.Models.CS2.Component.LOD.Submesh submesh = null;
            public CATHODE.Models.CS2.Component.LOD lod = null;
            public CATHODE.Models.CS2.Component component = null;
            public CATHODE.Models.CS2 cs2 = null;
        }
    }
}
