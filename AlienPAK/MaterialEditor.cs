﻿using CATHODE;
using CATHODE.LEGACY;
using CathodeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using static CATHODE.Materials.Material;

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
        ShadersPAK.ShaderEntry _selectedMaterialShader = null;

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
            _controls.FloatMaterialPropertyChanged += MaterialPropertyChanged;
            _controls.Vec4MaterialPropertyChanged += MaterialPropertyChanged;
            _controls.OnGlobalOptionChange += OnGlobalOptionChange;
            _controls.OnTextureIndexChange += OnTextureIndexChange;
            _controls.OnNameUpdated += OnNameUpdated;

            PopulateUI(material);
        }

        private void OnNameUpdated(string name)
        {
            if (materialList.SelectedIndex == -1) return;
            _sortedMaterials[materialList.SelectedIndex].Name = name;
            materialList.Items[materialList.SelectedIndex] = name;
        }

        //todo: this whole flow needs a bit of a refactor as i've changed quite a bit
        private void OnGlobalOptionChange(bool global)
        {
            Console.WriteLine("OnGlobalOptionChange");
            UpdateTextureDropdown(global, !_doingSelection);
        }

        private void UpdateTextureDropdown(bool global, bool changeIndex = false)
        {
            Console.WriteLine("UpdateTextureDropdown");
            List<string> textures = new List<string>();
            Textures textureDB = global ? _texturesGlobal : _textures;
            for (int i = 0; i < textureDB.Entries.Count; i++) textures.Add(textureDB.Entries[i].Name);
            textures.Add("NONE"); //temp holder for no texture
            _controls.PopulateTextureDropdown(textures);

            if (!changeIndex) return;
            Console.WriteLine(" --> UPDATING INDEX");
            _controls.textureFile.SelectedIndex = 0;
            OnTextureIndexChange(0, global);
        }

        private void OnTextureIndexChange(int index, bool global)
        {
            Console.WriteLine("OnTextureIndexChange");
            _doingSelection = true;
            Textures texDB = (global ? _texturesGlobal : _textures);
            ShadersPAK.MaterialTextureContext textureInfo = _selectedMaterialMeta.textures[_controls.materialTextureSelection.SelectedIndex];
            if (textureInfo.TextureInfo == null)
            {
                Texture tex = new Texture();
                textureInfo.TextureInfo = tex;
                _sortedMaterials[materialList.SelectedIndex].TextureReferences[_controls.materialTextureSelection.SelectedIndex] = tex;
            }
            if (index >= texDB.Entries.Count)
            {
                textureInfo.TextureInfo = null;
                _sortedMaterials[materialList.SelectedIndex].TextureReferences[_controls.materialTextureSelection.SelectedIndex] = null;
            }
            else
            {
                textureInfo.TextureInfo.BinIndex = texDB.GetWriteIndex(texDB.Entries[index]);
                textureInfo.TextureInfo.Source = global ? Texture.TextureSource.GLOBAL : Texture.TextureSource.LEVEL;
            }
            ShowTextureForMaterial(_controls.materialTextureSelection.SelectedIndex);
            _doingSelection = false;
        }

        private void PopulateUI(Materials.Material material = null)
        {
            Console.WriteLine("PopulateUI");
            _sortedMaterials.Clear();
            _sortedMaterials.AddRange(_materials.Entries);
            _sortedMaterials = _sortedMaterials.OrderBy(o => o.Name).ToList();

            materialList.BeginUpdate();
            materialList.Items.Clear();
            for (int i = 0; i < _sortedMaterials.Count; i++)
            {
                materialList.Items.Add(_sortedMaterials[i].Name);
                if (_sortedMaterials[i] == material) materialList.SelectedIndex = i;
            }
            materialList.EndUpdate();
        }

        private void MaterialPropertyChanged<T>(MaterialProperty property, T val)
        {
            Console.WriteLine("MaterialPropertyChanged");
            if (_selectedMaterialMeta == null || materialList.SelectedIndex == -1 || _sortedMaterials[materialList.SelectedIndex] == null || _selectedMaterialShader == null) return;
            int offset = _sortedMaterials[materialList.SelectedIndex].ConstantBuffers[2].Offset;
            switch (property)
            {
                case MaterialProperty.COLOUR:
                    offset += _selectedMaterialShader.CSTLinks[2][_selectedMaterialMeta.cstIndexes.DiffuseIndex];
                    break;
                case MaterialProperty.DIFFUSE_SCALE:
                    offset += _selectedMaterialShader.CSTLinks[2][_selectedMaterialMeta.cstIndexes.DiffuseUVMultiplierIndex];
                    break;
                case MaterialProperty.DIFFUSE_OFFSET:
                    offset += _selectedMaterialShader.CSTLinks[2][_selectedMaterialMeta.cstIndexes.DiffuseUVAdderIndex];
                    break;
                case MaterialProperty.NORMAL_SCALE:
                    offset += _selectedMaterialShader.CSTLinks[2][_selectedMaterialMeta.cstIndexes.NormalUVMultiplierIndex];
                    break;
                default:
                    return;
            }
            BinaryWriter cstWriter = new BinaryWriter(new MemoryStream(_materials.CSTData[2]));
            WriteToCST<T>(ref cstWriter, offset * 4, val);
            cstWriter.Close();
        }

        private void OnMaterialTextureIndexSelected(int index)
        {
            Console.WriteLine("OnMaterialTextureIndexSelected");
            ShowTextureForMaterial(_controls.materialTextureSelection.SelectedIndex);
        }

        bool _doingSelection = false; //temp hack for crap global implementation fix
        private void materialList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("materialList_SelectedIndexChanged");
            _doingSelection = true;
            _selectedMaterialMeta = null;
            _selectedMaterialShader = null;
            if (materialList.SelectedIndex == -1)
            {
                _doingSelection = false;
                return;
            }
            _selectedMaterialMeta = _shaders.GetMaterialMetadataFromShader(_sortedMaterials[materialList.SelectedIndex], _shadersIDX);

            int shaderIndex = _shadersIDX.Datas[_sortedMaterials[materialList.SelectedIndex].UberShaderIndex].Index;
            _selectedMaterialShader = _shaders.Shaders[shaderIndex];

            _controls.fileNameText.Text = _sortedMaterials[materialList.SelectedIndex].Name;

            _controls.materialTextureSelection.Items.Clear();
            for (int i = 0; i < _selectedMaterialMeta.textures.Count; i++)
            {
                _controls.materialTextureSelection.Items.Add(_selectedMaterialMeta.textures[i].Type.ToString());
                if (_selectedMaterialMeta.textures[i].Type == ShadersPAK.ShaderSlot.DIFFUSE_MAP) _controls.materialTextureSelection.SelectedIndex = i;
            }
            _controls.ShowTexturePreview(_selectedMaterialMeta.textures.Count != 0);
            if (_controls.materialTextureSelection.SelectedIndex == -1 && _selectedMaterialMeta.textures.Count != 0)
                _controls.materialTextureSelection.SelectedIndex = 0;
            ShowTextureForMaterial(_controls.materialTextureSelection.SelectedIndex);

            _controls.shaderName.Text = _selectedMaterialShader.Index + " (" + _selectedMaterialMeta.shaderCategory.ToString() + ")";

            UpdateShaderCSTInfo(_selectedMaterialMeta.cstIndexes, _selectedMaterialShader, _sortedMaterials[materialList.SelectedIndex]);
            _doingSelection = false;
        }

        private void UpdateShaderCSTInfo(ShadersPAK.MaterialPropertyIndex cstIndexes, ShadersPAK.ShaderEntry shader, Materials.Material InMaterial)
        {
            BinaryReader cstReader = new BinaryReader(new MemoryStream(_materials.CSTData[2]));
            int baseOffset = (InMaterial.ConstantBuffers[2].Offset * 4);

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
            Console.WriteLine("ShowTextureForMaterial");
            _controls.materialTexturePreview.Source = null;
            if (index == -1) return;
            ShadersPAK.MaterialTextureContext mdlMetaDiff = _selectedMaterialMeta.textures[index];
            if (mdlMetaDiff == null || mdlMetaDiff.TextureInfo == null)
            {
                _controls.textureFile.SelectedItem = "NONE";
                _controls.materialTexturePreview.Source = null;
                return;
            }

            bool isGlobal = mdlMetaDiff.TextureInfo.Source == Texture.TextureSource.GLOBAL;
            if (isGlobal != _controls.textureUseGlobal.IsChecked || _controls.textureFile.Items.Count == 0)
            {
                _controls.textureUseGlobal.IsChecked = isGlobal;
                UpdateTextureDropdown(isGlobal);
            }
            Textures.TEX4 diff = (mdlMetaDiff.TextureInfo.Source == Texture.TextureSource.GLOBAL ? _texturesGlobal : _textures).GetAtWriteIndex(mdlMetaDiff.TextureInfo.BinIndex);
            _controls.textureFile.SelectedItem = diff == null ? "NONE" : diff.Name;
            _controls.materialTexturePreview.Source = diff?.ToDDS()?.ToBitmap()?.ToImageSource();
        }

        private T LoadFromCST<T>(ref BinaryReader cstReader, int offset)
        {
            cstReader.BaseStream.Position = offset;
            return Utilities.Consume<T>(cstReader);
        }
        private void WriteToCST<T>(ref BinaryWriter cstWriter, int offset, T content)
        {
            cstWriter.BaseStream.Position = offset;
            Utilities.Write<T>(cstWriter, content);
        }
        private bool CSTIndexValid(int i, ref ShadersPAK.ShaderEntry Shader)
        {
            return i >= 0 && i < Shader.Header.CSTCounts[2] && (int)Shader.CSTLinks[2][i] != -1 && Shader.CSTLinks[2][i] != 255;
        }

        private void selectMaterial_Click(object sender, EventArgs e)
        {
            OnMaterialSelected?.Invoke(_materials.GetWriteIndex(_sortedMaterials[materialList.SelectedIndex]));
            this.Close();
        }

        private void duplicateMaterial_Click(object sender, EventArgs e)
        {
            Materials.Material newMaterial = _sortedMaterials[materialList.SelectedIndex].Copy();
            newMaterial.Name += " Clone";
            for (int i = 0; i < newMaterial.ConstantBuffers.Length; i++)
            {
                if (newMaterial.ConstantBuffers == null) continue;
                byte[] cstData = null;
                using (MemoryStream stream = new MemoryStream(_materials.CSTData[i]))
                {
                    using (BinaryReader cstReader = new BinaryReader(stream))
                    {
                        cstReader.BaseStream.Position = newMaterial.ConstantBuffers[i].Offset * 4;
                        cstData = cstReader.ReadBytes(newMaterial.ConstantBuffers[i].Length * 4);
                    }
                }
                newMaterial.ConstantBuffers[i] = new Materials.Material.ConstantBuffer();
                if (cstData != null)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        using (BinaryWriter cstWriter = new BinaryWriter(stream))
                        {
                            cstWriter.Write(_materials.CSTData[i]);
                            newMaterial.ConstantBuffers[i].Offset = (int)cstWriter.BaseStream.Position / 4;
                            newMaterial.ConstantBuffers[i].Length = cstData.Length / 4;
                            cstWriter.Write(cstData);

                            _materials.CSTData[i] = stream.ToArray();
                        }
                    }
                }
            }
            _materials.Entries.Add(newMaterial);
            Explorer.SaveTexturesAndUpdateMaterials(_textures, _materials);
            PopulateUI(newMaterial);
        }
    }
}
