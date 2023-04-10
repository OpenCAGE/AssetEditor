using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AlienPAK
{
    /// <summary>
    /// Interaction logic for LandingWPF.xaml
    /// </summary>
    public partial class LandingWPF : UserControl
    {
        public LandingWPF()
        {
            InitializeComponent();
        }
        public void SetVersionInfo(string version)
        {
            VersionText.Content = "Version " + version;
        }

        private void OpenTextures(object sender, RoutedEventArgs e)
        {
            LaunchEditor(AlienContentType.TEXTURE);
        }
        private void OpenModels(object sender, RoutedEventArgs e)
        {
            LaunchEditor(AlienContentType.MODEL);
        }
        private void OpenMaterials(object sender, RoutedEventArgs e)
        {

        }
        private void OpenSounds(object sender, RoutedEventArgs e)
        {
            
        }
        private void OpenUI(object sender, RoutedEventArgs e)
        {
            LaunchEditor(AlienContentType.UI);
        }
        private void OpenAnimations(object sender, RoutedEventArgs e)
        {
            LaunchEditor(AlienContentType.ANIMATION);
        }
        private void OpenMaterialMaps(object sender, RoutedEventArgs e)
        {
            LaunchEditor(AlienContentType.MATERIAL_MAPPINGS);
        }

        private void LaunchEditor(AlienContentType type)
        {
            Explorer interfaceTool = new Explorer(type);
            interfaceTool.Show();
        }
    }
}
