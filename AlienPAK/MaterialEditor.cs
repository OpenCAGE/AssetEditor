using CATHODE;
using CATHODE.LEGACY;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class MaterialEditor : Form
    {
        Materials _materials = null;
        ShadersPAK _shaders = null;
        Textures _textures = null;
        Textures _texturesGlobal = null;
        IDXRemap _shadersIDX = null;

        List<Materials.Material> _sortedMaterials = new List<Materials.Material>();
        ShadersPAK.ShaderMaterialMetadata _selectedMaterialMeta = null;

        MaterialEditorControlsWPF _controls = null;

        public MaterialEditor(Materials.Material material = null, Materials materials = null, ShadersPAK shaders = null, Textures textures = null, Textures texturesGlobal = null, IDXRemap shadersIDX = null)
        {
            _materials = materials;
            _shaders = shaders;
            _textures = textures;
            _texturesGlobal = texturesGlobal;
            _shadersIDX = shadersIDX;

            InitializeComponent();
            if (_materials == null) return;

            _controls = (MaterialEditorControlsWPF)elementHost1.Child;
            _controls.OnMaterialTextureIndexSelected += OnMaterialTextureIndexSelected;

            _sortedMaterials.AddRange(_materials.Entries);
            _sortedMaterials = _sortedMaterials.OrderBy(o => o.Name).ToList();

            for (int i = 0; i < _sortedMaterials.Count; i++)
            {
                materialList.Items.Add(_sortedMaterials[i].Name);
                if (_sortedMaterials[i] == material) materialList.SelectedIndex = i;
            }
        }

        private void OnMaterialTextureIndexSelected(int index)
        {
            ShowTextureForMaterial(_controls.materialTextureSelection.SelectedIndex);
        }

        private void materialList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedMaterialMeta = null;
            if (materialList.SelectedIndex == -1) return;
            _selectedMaterialMeta = _shaders.GetMaterialMetadataFromShader(_sortedMaterials[materialList.SelectedIndex], _shadersIDX);

            _controls.fileNameText.Text = _sortedMaterials[materialList.SelectedIndex].Name;

            _controls.materialTextureSelection.Items.Clear();
            for (int i = 0; i < _selectedMaterialMeta.textures.Count; i++)
            {
                _controls.materialTextureSelection.Items.Add(_selectedMaterialMeta.textures[i].Type.ToString());
                if (_selectedMaterialMeta.textures[i].Type == ShadersPAK.ShaderSlot.DIFFUSE_MAP) _controls.materialTextureSelection.SelectedIndex = i;
            }
            if (_controls.materialTextureSelection.SelectedIndex == -1 && _selectedMaterialMeta.textures.Count != 0)
                _controls.materialTextureSelection.SelectedIndex = 0;
            ShowTextureForMaterial(_controls.materialTextureSelection.SelectedIndex);

            int RemappedIndex = _shadersIDX.Datas[_sortedMaterials[materialList.SelectedIndex].UberShaderIndex].Index;
            ShadersPAK.ShaderEntry shader = _shaders.Shaders[RemappedIndex];
            _controls.shaderName.Text = shader.Index + " (" + _selectedMaterialMeta.shaderCategory.ToString() + ")";
        }

        private void ShowTextureForMaterial(int index)
        {
            _controls.materialTexturePreview.Source = null;
            _controls.materialTextureName.Text = "";
            if (index == -1) return;
            ShadersPAK.MaterialTextureContext mdlMetaDiff = _selectedMaterialMeta.textures[index];
            if (mdlMetaDiff == null || mdlMetaDiff.TextureInfo == null) return;

            Textures tex = mdlMetaDiff.TextureInfo.Source == Materials.Material.Texture.TextureSource.GLOBAL ? _texturesGlobal : _textures;
            Textures.TEX4 diff = tex.GetAtWriteIndex(mdlMetaDiff.TextureInfo.BinIndex);
            _controls.materialTextureName.Text = diff.Name;
            _controls.materialTexturePreview.Source = diff?.ToDDS()?.ToBitmap()?.ToImageSource();
        }
    }
}
