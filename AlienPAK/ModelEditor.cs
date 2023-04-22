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

            List<string> contents = new List<string>();
            string componentString = "";
            string lodString = "";
            string submeshString = "";
            for (int i = 0; i < model.Components.Count; i++)
            {
                componentString = Path.GetFileName(model.Name) + "\\Component " + i;
                _treeLookup.Add(new StringMeshLookup() { String = componentString, component = model.Components[i] });
                for (int x = 0; x < model.Components[i].LODs.Count; x++)
                {
                    lodString = componentString + "\\Part " + x + ": " + model.Components[i].LODs[x].Name;
                    _treeLookup.Add(new StringMeshLookup() { String = lodString, lod = model.Components[i].LODs[x] });
                    for (int y = 0; y < model.Components[i].LODs[x].Submeshes.Count; y++)
                    {
                        submeshString = lodString + "\\Mesh " + y;
                        _treeLookup.Add(new StringMeshLookup() { String = submeshString, submesh = model.Components[i].LODs[x].Submeshes[y] });
                        contents.Add(submeshString);
                    }
                }
            }
            _treeHelper.UpdateFileTree(contents);
            FileTree.ExpandAll();

            _controls = (ModelEditorControlsWPF)elementHost1.Child;
            _controls.renderMaterials.IsChecked = true; //todo: remember this option
            _controls.OnEditMaterialRequested += OnEditMaterialRequested;
            _controls.OnMaterialRenderCheckChanged += OnMaterialRenderCheckChanged;
        }

        private void OnEditMaterialRequested()
        {
            if (FileTree.SelectedNode == null) return;
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode.FullPath);
            if (lookup == null || lookup.submesh == null) return;
            Materials.Material material = _materials.GetAtWriteIndex(lookup.submesh.MaterialLibraryIndex);
            if (material == null) return;

            MaterialEditor materialEditor = new MaterialEditor(material);
            materialEditor.Show();
        }

        private void OnMaterialRenderCheckChanged(bool check)
        {
            FileTree_AfterSelect(null, null);
        }

        private void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (FileTree.SelectedNode == null) return;
            StringMeshLookup lookup = _treeLookup.FirstOrDefault(o => o.String == FileTree.SelectedNode.FullPath);

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
                                    //TODO: normals?
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine(ex2.ToString());
                        }
                        vertCount += ((MeshGeometry3D)mdl.Geometry).Positions.Count;
                        model.Children.Add(mdl); //TODO: are there some offsets/scaling we should be accounting for here?
                    }
                }
            }
            _controls.SetModelPreview(model); 
            string[] nameContents = (FileTree.SelectedNode.FullPath + "\\").Split('\\');
            _controls.fileNameText.Text = nameContents[nameContents.Length - 2];
            _controls.vertexCount.Text = vertCount.ToString();
            _controls.materialInfo.Text = materialInfo;
            _controls.materialLabel.Visibility = materialInfo != "" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
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
