using System.Collections.Generic;
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

namespace AlienPAK
{
    /// <summary>
    /// Interaction logic for MaterialEditorControlsWPF.xaml
    /// </summary>
    public partial class MaterialEditorControlsWPF : UserControl
    {
        public Action<int> OnMaterialTextureIndexSelected;

        public Action<float> DiffuseScaleChanged;

        public MaterialEditorControlsWPF()
        {
            InitializeComponent();
        }

        private void MaterialTextureSelected(object sender, EventArgs e)
        {
            OnMaterialTextureIndexSelected?.Invoke(materialTextureSelection.SelectedIndex);
        }

        private void matDiffuseScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            DiffuseScaleChanged?.Invoke(Convert.ToSingle(matDiffuseScale.Text));
        }
    }
}
