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
using System.Windows.Media;
using System.Numerics;

namespace AlienPAK
{
    /// <summary>
    /// Interaction logic for MaterialEditorControlsWPF.xaml
    /// </summary>
    public partial class MaterialEditorControlsWPF : UserControl
    {
        public Action<int> OnSamplerSelected;
        public Action<string> OnParameterSelected;
        public Action OnPickTexture;

        public TabControl SamplerTabControl => samplerTabControl;
        public StackPanel FeatureDetailsPanel => featureDetailsPanel;
        public ComboBox ParameterSelection => parameterSelection;
        public StackPanel ParameterDetailsPanel => parameterDetailsPanel;
        public TextBlock ShaderType => shaderType;

        public MaterialEditorControlsWPF()
        {
            InitializeComponent();
        }

        public void ShowTexturePreview(bool show)
        {
            materialPreviewGroup.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SamplerTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (samplerTabControl.SelectedIndex >= 0)
                OnSamplerSelected?.Invoke(samplerTabControl.SelectedIndex);
        }

        private void ParameterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (parameterSelection.SelectedItem != null)
                OnParameterSelected?.Invoke(parameterSelection.SelectedItem.ToString());
        }
    }
}
