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
    /// Interaction logic for ExplorerControlsWPF.xaml
    /// </summary>
    public partial class ExplorerControlsWPF : UserControl
    {
        public Action<string> OnLevelSelected;

        public Action OnImportRequested;
        public Action OnExportRequested;
        public Action OnDeleteRequested;
        public Action OnReplaceRequested;
        public Action OnExportAllRequested;

        public bool FilePreviewVisible
        {
            get
            {
                return imagePreviewGroup.Visibility == Visibility.Visible;
            }
        }

        public bool ModelPreviewVisible
        {
            get
            {
                return modelPreviewGroup.Visibility == Visibility.Visible;
            }
        }

        private Bitmap _filePreviewBitmap = null;
        public Bitmap FilePreviewBitmap
        {
            get { return _filePreviewBitmap; }
        }

        public ExplorerControlsWPF()
        {
            InitializeComponent();
        }

        /* Set the information for the currently selected file in UI */
        public void SetFileInfo(string name, string size, bool modelMode = false)
        {
            fileInfoGroup.Visibility = Visibility.Visible;
            fileNameText.Text = name;

            fileSizeText.Text = size + (modelMode ? " verts" : " bytes");
            fileSizeLabel.Visibility = size == null || size == "" ? Visibility.Collapsed : Visibility.Visible;
            fileSizeText.Visibility = fileSizeLabel.Visibility;

            fileTypeText.Text = GetFileType(name);
            fileTypeLabel.Visibility = fileTypeText.Text == "" ? Visibility.Collapsed : Visibility.Visible;
            fileTypeText.Visibility = fileTypeLabel.Visibility;
        }

        /* Show the image preview for the selected file in UI if possible */
        public void SetImagePreview(byte[] content)
        {
            _filePreviewBitmap = content?.ToBitmap();
            imagePreviewGroup.Visibility = _filePreviewBitmap == null ? Visibility.Collapsed : Visibility.Visible;
            modelPreviewGroup.Visibility = Visibility.Collapsed;
            if (_filePreviewBitmap == null) return;
            filePreviewImage.Source = _filePreviewBitmap?.ToImageSource();
        }

        /* Show the model preview for the selected file in UI */
        public void SetModelPreview(Model3DGroup content)
        {
            modelPreviewGroup.Visibility = Visibility.Visible;
            imagePreviewGroup.Visibility = Visibility.Collapsed;
            filePreviewModel.Content = content;
            filePreviewModelContainer.ZoomExtents();
        }

        /* Show the level selection dropdown if requested */
        public void ShowLevelSelect(bool show, PAKType type)
        {
            levelSelectGroup.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            if (show)
            {
                levelSelectDropdown.Items.Clear();
                List<string> mapList = Directory.GetFiles(SharedData.pathToAI + "/DATA/ENV/PRODUCTION/", "COMMANDS.PAK", SearchOption.AllDirectories).ToList<string>();
                //if (type == PAKType.TEXTURES/* || type == PAKType.MODELS*/)
                //    levelSelectDropdown.Items.Add("GLOBAL");      //TODO: currently not doing this for TEXTURES as we need to support GLOBAL index updating in ALL maps
                for (int i = 0; i < mapList.Count; i++)
                {
                    string[] fileSplit = mapList[i].Split(new[] { "PRODUCTION" }, StringSplitOptions.None);
                    levelSelectDropdown.Items.Add(fileSplit[fileSplit.Length - 1].Substring(1, fileSplit[fileSplit.Length - 1].Length - 20));
                }
                if (type == PAKType.COMMANDS || type == PAKType.MATERIAL_MAPPINGS)
                {
                    levelSelectDropdown.Items.Remove("DLC\\BSPNOSTROMO_RIPLEY");
                    levelSelectDropdown.Items.Remove("DLC\\BSPNOSTROMO_TWOTEAMS");
                }
                else
                {
                    levelSelectDropdown.Items.Remove("DLC\\BSPNOSTROMO_RIPLEY_PATCH");
                    levelSelectDropdown.Items.Remove("DLC\\BSPNOSTROMO_TWOTEAMS_PATCH");
                }
                levelSelectDropdown.SelectedIndex = 0;
            }

            LevelSelected(null, null);
        }

        /* Toggle available buttons given the functionality of the current PAK */
        public void ShowFunctionButtons(PAKFunction function, bool isModelPAK, bool hasSelectedFile) 
        {
            imagePreviewGroup.Visibility = Visibility.Collapsed;
            modelPreviewGroup.Visibility = Visibility.Collapsed;
            fileInfoGroup.Visibility = Visibility.Collapsed;

            //TODO: this is a temp hack to show the model button before i implement a nicer method when textures have a window too
            replaceBtn.Content = isModelPAK ? "Modify Selected" : "Replace Selected";

            exportBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_FILES, hasSelectedFile);
            replaceBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_REPLACE_FILES, hasSelectedFile);
            deleteBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_DELETE_FILES, hasSelectedFile);
            fileUtiltiesGroup.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_FILES | PAKFunction.CAN_REPLACE_FILES | PAKFunction.CAN_DELETE_FILES, hasSelectedFile);

            importBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_IMPORT_FILES);
            exportAllBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_ALL);
            archiveUtilitiesGroup.Visibility = FlagToVisibility(function, PAKFunction.CAN_IMPORT_FILES | PAKFunction.CAN_EXPORT_FILES);
        }
        private Visibility FlagToVisibility(PAKFunction function, PAKFunction flag, bool? hasSelectedFile = null)
        {
            return function.HasFlag(flag) && ((hasSelectedFile != null && hasSelectedFile == true) || hasSelectedFile == null) ? Visibility.Visible : Visibility.Collapsed;
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
        private void LevelSelected(object sender, EventArgs e)
        {
            OnLevelSelected?.Invoke(levelSelectDropdown.Text);
        }

        /* Get a nicely formatted string for the type of a file */
        private string GetFileType(string path)
        {
            string extension = Path.GetExtension(path);
            if (extension == "") return "";
            switch (extension.Substring(1).ToUpper())
            {
                case "DDS":
                    return "DDS (Image)";
                case "TGA":
                    return "TGA (Image)";
                case "PNG":
                    return "PNG (Image)";
                case "JPG":
                    return "JPG (Image)";
                case "GFX":
                    return "GFX (Adobe Flash)";
                case "CS2":
                    return "CS2 (Model)";
                case "BIN":
                    return "BIN (Binary File)";
                case "BML":
                    return "BML (Binary XML)";
                case "XML":
                    return "XML (Markup)";
                case "TXT":
                    return "TXT (Text)";
                case "DXBC":
                    return "DXBC (Compiled HLSL)";
                default:
                    return extension.Substring(1).ToUpper();
            }
        }
    }
}
