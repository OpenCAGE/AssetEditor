using CATHODE;
using CATHODE.LEGACY;
using CathodeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

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

        public Action<int> OnMaterialSelected;

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

            int shaderIndex = _shadersIDX.Datas[_sortedMaterials[materialList.SelectedIndex].UberShaderIndex].Index;
            ShadersPAK.ShaderEntry shader = _shaders.Shaders[shaderIndex];
            _controls.shaderName.Text = shader.Index + " (" + _selectedMaterialMeta.shaderCategory.ToString() + ")";

            UpdateShaderCSTInfo(_selectedMaterialMeta.cstIndexes, shader, _sortedMaterials[materialList.SelectedIndex]);
        }

        private void UpdateShaderCSTInfo(ShadersPAK.MaterialPropertyIndex cstIndexes, ShadersPAK.ShaderEntry shader, Materials.Material InMaterial)
        {
            BinaryReader cstReader = new BinaryReader(new MemoryStream(_materials.CSTData[2]));
            int baseOffset = (InMaterial.ConstantBuffers[2].CstIndex * 4);

            _controls.matColourLabel.Visibility = CSTIndexValid(cstIndexes.DiffuseIndex, ref shader) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            _controls.matColour.Visibility = _controls.matColourLabel.Visibility;
            if (_controls.matColour.Visibility == System.Windows.Visibility.Visible)
            {
                Vector4 colour = LoadFromCST<Vector4>(ref cstReader, baseOffset + (shader.CSTLinks[2][cstIndexes.DiffuseIndex] * 4));
                System.Windows.Media.Color colour_c = System.Windows.Media.Color.FromArgb((byte)(int)(colour.W * 255.0f), (byte)(int)(colour.X * 255.0f), (byte)(int)(colour.Y * 255.0f), (byte)(int)(colour.Z * 255.0f));
                _controls.matColour.Background = new SolidColorBrush(colour_c);
            }

            _controls.matDiffuseScaleLabel.Visibility = CSTIndexValid(cstIndexes.DiffuseUVMultiplierIndex, ref shader) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            _controls.matDiffuseScale.Visibility = _controls.matDiffuseScaleLabel.Visibility;
            if (_controls.matDiffuseScale.Visibility == System.Windows.Visibility.Visible)
            {
                float offset = LoadFromCST<float>(ref cstReader, baseOffset + (shader.CSTLinks[2][cstIndexes.DiffuseUVMultiplierIndex] * 4));
                _controls.matDiffuseScale.Text = offset.ToString();
            }

            _controls.matDiffuseOffsetLabel.Visibility = CSTIndexValid(cstIndexes.DiffuseUVAdderIndex, ref shader) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            _controls.matDiffuseOffset.Visibility = _controls.matDiffuseOffsetLabel.Visibility;
            if (_controls.matDiffuseOffset.Visibility == System.Windows.Visibility.Visible)
            {
                float offset = LoadFromCST<float>(ref cstReader, baseOffset + (shader.CSTLinks[2][cstIndexes.DiffuseUVAdderIndex] * 4));
                _controls.matDiffuseOffset.Text = offset.ToString();
            }

            _controls.matNormalScaleLabel.Visibility = CSTIndexValid(cstIndexes.NormalUVMultiplierIndex, ref shader) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            _controls.matNormalScale.Visibility = _controls.matNormalScaleLabel.Visibility;
            if (_controls.matNormalScale.Visibility == System.Windows.Visibility.Visible)
            {
                float offset = LoadFromCST<float>(ref cstReader, baseOffset + (shader.CSTLinks[2][cstIndexes.NormalUVMultiplierIndex] * 4));
                _controls.matNormalScale.Text = offset.ToString();
            }

            cstReader.Close();
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
            _controls.materialTextureName.Text = diff == null ? "" : diff.Name;
            _controls.materialTexturePreview.Source = diff?.ToDDS()?.ToBitmap()?.ToImageSource();
        }

        //todo: implement WriteToCST type thing
        private T LoadFromCST<T>(ref BinaryReader cstReader, int offset)
        {
            cstReader.BaseStream.Position = offset;
            return Utilities.Consume<T>(cstReader);
        }
        private bool CSTIndexValid(int i, ref ShadersPAK.ShaderEntry Shader)
        {
            return i >= 0 && i < Shader.Header.CSTCounts[2] && (int)Shader.CSTLinks[2][i] != -1;
        }

        private void selectMaterial_Click(object sender, EventArgs e)
        {
            OnMaterialSelected?.Invoke(_materials.GetWriteIndex(_sortedMaterials[materialList.SelectedIndex]));
            this.Close();
        }
    }
}
