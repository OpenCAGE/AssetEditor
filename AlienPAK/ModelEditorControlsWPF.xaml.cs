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
        public Action OnImportRequested;
        public Action OnExportRequested;
        public Action OnDeleteRequested;
        public Action OnReplaceRequested;
        public Action OnExportAllRequested;
        public Action OnEditMaterialRequested;

        public Action<bool> OnMaterialRenderCheckChanged;

        public bool ModelPreviewVisible
        {
            get
            {
                return modelPreviewGroup.Visibility == Visibility.Visible;
            }
        }

        public ModelEditorControlsWPF()
        {
            InitializeComponent();
        }

        /* Show the model preview for the selected file in UI */
        public void SetModelPreview(Model3DGroup content)
        {
            modelPreviewGroup.Visibility = Visibility.Visible;
            filePreviewModel.Content = content;
            filePreviewModelContainer.ZoomExtents();
        }

        /* Button event triggers */
        private void ImportBtn(object sender, RoutedEventArgs e)
        {
            OnImportRequested?.Invoke();
        }
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
        private void ExportAll(object sender, RoutedEventArgs e)
        {
            OnExportAllRequested?.Invoke();
        }
        private void AddComponentBtn(object sender, RoutedEventArgs e)
        {

        }
        private void AddLODBtn(object sender, RoutedEventArgs e)
        {

        }
        private void AddSubmeshBtn(object sender, RoutedEventArgs e)
        {

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
}
