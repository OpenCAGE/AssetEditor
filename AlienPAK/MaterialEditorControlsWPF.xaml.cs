﻿using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Drawing.Imaging;
using CATHODE;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Numerics;

namespace AlienPAK
{
    /// <summary>
    /// Interaction logic for MaterialEditorControlsWPF.xaml
    /// </summary>
    public partial class MaterialEditorControlsWPF : UserControl
    {
        public Action<int> OnMaterialTextureIndexSelected;

        public Action<int, bool> OnTextureIndexChange;
        public Action<bool> OnGlobalOptionChange;

        public Action<MaterialProperty, float> FloatMaterialPropertyChanged;
        public Action<MaterialProperty, Vector4> Vec4MaterialPropertyChanged;

        public Action<string> OnNameUpdated;

        public MaterialEditorControlsWPF()
        {
            InitializeComponent();
        }

        public void ShowTexturePreview(bool show)
        {
            materialPreviewGroup.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void textureUseGlobal_Checked(object sender, RoutedEventArgs e)
        {
            OnGlobalOptionChange?.Invoke(textureUseGlobal.IsChecked == true);
        }

        public void PopulateTextureDropdown(List<string> textures)
        {
            textureFile.Items.Clear();
            for (int i = 0; i < textures.Count; i++)
                textureFile.Items.Add(textures[i]);
        }

        private void textureFile_DropDownClosed(object sender, EventArgs e)
        {
            OnTextureIndexChange?.Invoke(textureFile.SelectedIndex, textureUseGlobal.IsChecked == true);
        }

        private void MaterialTextureSelected(object sender, EventArgs e)
        {
            OnMaterialTextureIndexSelected?.Invoke(materialTextureSelection.SelectedIndex);
        }

        private void matDiffuseScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            matDiffuseScale.Text = matDiffuseScale.Text.ForceStringNumeric(true);
            FloatMaterialPropertyChanged?.Invoke(MaterialProperty.DIFFUSE_SCALE, Convert.ToSingle(matDiffuseScale.Text));
        }

        private void matDiffuseOffset_TextChanged(object sender, TextChangedEventArgs e)
        {
            matDiffuseOffset.Text = matDiffuseOffset.Text.ForceStringNumeric(true);
            FloatMaterialPropertyChanged?.Invoke(MaterialProperty.DIFFUSE_OFFSET, Convert.ToSingle(matDiffuseOffset.Text));
        }

        private void matNormalScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            matNormalScale.Text = matNormalScale.Text.ForceStringNumeric(true);
            FloatMaterialPropertyChanged?.Invoke(MaterialProperty.NORMAL_SCALE, Convert.ToSingle(matNormalScale.Text));
        }

        private void matColour_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colourPicker = new System.Windows.Forms.ColorDialog();
            System.Windows.Media.Color colour = ((SolidColorBrush)matColour.Background).Color;
            colourPicker.Color = System.Drawing.Color.FromArgb(colour.A, colour.R, colour.G, colour.B);

            if (colourPicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Vector4 colourVec = new Vector4((float)colourPicker.Color.R / 255.0f, (float)colourPicker.Color.G / 255.0f, (float)colourPicker.Color.B / 255.0f, (float)colourPicker.Color.A / 255.0f);
                ((SolidColorBrush)matColour.Background).Color = System.Windows.Media.Color.FromArgb(colourPicker.Color.A, colourPicker.Color.R, colourPicker.Color.G, colourPicker.Color.B);
                Vec4MaterialPropertyChanged?.Invoke(MaterialProperty.COLOUR, colourVec);
            }
        }

        private void fileNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnNameUpdated?.Invoke(fileNameText.Text);
        }
    }

    public enum MaterialProperty
    {
        COLOUR,
        DIFFUSE_SCALE,
        DIFFUSE_OFFSET,
        NORMAL_SCALE,
    }
}
