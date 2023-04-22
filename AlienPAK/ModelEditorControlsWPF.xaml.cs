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
    /// Interaction logic for ModelEditorControlsWPF.xaml
    /// </summary>
    public partial class ModelEditorControlsWPF : UserControl
    {
        public Action OnExportRequested;
        public Action OnDeleteRequested;
        public Action OnReplaceRequested;
        public Action<SelectedModelType> OnAddRequested;
        public Action OnEditMaterialRequested;

        public Action<bool> OnMaterialRenderCheckChanged;

        public ModelEditorControlsWPF()
        {
            InitializeComponent();
        }

        /* Show the model preview for the selected file in UI */
        public void SetModelPreview(Model3DGroup content, string filename, int vertCount, string material)
        {
            filePreviewModel.Content = content;
            filePreviewModelContainer.ZoomExtents();

            fileNameText.Text = filename;
            vertexCount.Text = vertCount.ToString();
            materialInfo.Text = material;
            materialLabel.Visibility = material != "" ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowContextualButtons(SelectedModelType type)
        {
            exportBtn.Visibility = type != SelectedModelType.NONE ? Visibility.Visible : Visibility.Collapsed;
            replaceBtn.Visibility = type == SelectedModelType.SUBMESH ? Visibility.Visible : Visibility.Collapsed;
            editMaterialBtn.Visibility = type == SelectedModelType.SUBMESH ? Visibility.Visible : Visibility.Collapsed;
            deleteBtn.Visibility = type != SelectedModelType.CS2 && type != SelectedModelType.NONE ? Visibility.Visible : Visibility.Collapsed;
            addComponentBtn.Visibility = type == SelectedModelType.CS2 ? Visibility.Visible : Visibility.Collapsed;
            addLODBtn.Visibility = type == SelectedModelType.COMPONENT ? Visibility.Visible : Visibility.Collapsed;
            addSubmeshBtn.Visibility = type == SelectedModelType.LOD ? Visibility.Visible : Visibility.Collapsed;
        }

        /* Button event triggers */
        private void ExportBtn(object sender, RoutedEventArgs e)
        {
            OnExportRequested?.Invoke();
        }
        private void DeleteBtn(object sender, RoutedEventArgs e)
        {
            OnDeleteRequested?.Invoke();
        }
        private void ReplaceBtn(object sender, RoutedEventArgs e)
        {
            OnReplaceRequested?.Invoke();
        }
        private void AddComponentBtn(object sender, RoutedEventArgs e)
        {
            OnAddRequested?.Invoke(SelectedModelType.COMPONENT);
        }
        private void AddLODBtn(object sender, RoutedEventArgs e)
        {
            OnAddRequested?.Invoke(SelectedModelType.LOD);
        }
        private void AddSubmeshBtn(object sender, RoutedEventArgs e)
        {
            OnAddRequested?.Invoke(SelectedModelType.SUBMESH);
        }
        private void EditMaterialBtn(object sender, RoutedEventArgs e)
        {
            OnEditMaterialRequested?.Invoke();
        }

        private void OnRenderMaterialsChecked(object sender, RoutedEventArgs e)
        {
            OnMaterialRenderCheckChanged?.Invoke(renderMaterials.IsChecked == true);
        }
    }

    public enum SelectedModelType
    {
        NONE,
        CS2,
        COMPONENT,
        LOD,
        SUBMESH
    }
}
