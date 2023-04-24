using OpenCAGE;
using System;
using System.Configuration;
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
        public Action DoHide;
        public Action DoFocus;

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
            //TODO: it'd be great to *not* need this warning in future, and handle file locks & editor reloading gracefully. Maybe even force close A:I.
            if (!SettingsManager.GetBool("CONFIG_HideAssetWarning"))
                MessageBox.Show("Ensure Alien: Isolation is closed while making edits to assets.\nYou'll also need to reload the script editor after any changes.\n\nYou can disable this warning in the OpenCAGE settings.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            Explorer interfaceTool = new Explorer(type);
            interfaceTool.FormClosed += InterfaceTool_FormClosed;
            interfaceTool.Show();
            DoHide?.Invoke();
        }
        private void InterfaceTool_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            DoFocus?.Invoke();
        }
    }
}
