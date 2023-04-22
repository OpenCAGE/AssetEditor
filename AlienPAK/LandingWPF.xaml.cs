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
            LaunchEditor(PAKType.MODELS);
        }

        private void OpenTextures(object sender, RoutedEventArgs e)
        {
            LaunchEditor(PAKType.TEXTURES);
        }
        private void OpenModels(object sender, RoutedEventArgs e)
        {
            LaunchEditor(PAKType.MODELS);
        }
        private void OpenMaterials(object sender, RoutedEventArgs e)
        {

        }
        private void OpenSounds(object sender, RoutedEventArgs e)
        {
            
        }
        private void OpenUI(object sender, RoutedEventArgs e)
        {
            LaunchEditor(PAKType.UI);
        }
        private void OpenAnimations(object sender, RoutedEventArgs e)
        {
            LaunchEditor(PAKType.ANIMATIONS);
        }
        private void OpenMaterialMaps(object sender, RoutedEventArgs e)
        {
            LaunchEditor(PAKType.MATERIAL_MAPPINGS);
        }

        private void LaunchEditor(PAKType type)
        {
            Explorer interfaceTool = new Explorer(type);
            interfaceTool.Show();
        }
    }
}
