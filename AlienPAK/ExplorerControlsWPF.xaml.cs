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

        public ExplorerControlsWPF()
        {
            InitializeComponent();
        }

        /* Set the information for the currently selected file in UI */
        public void SetFileInfo(string name, string size)
        {
            fileInfoGroup.Visibility = Visibility.Visible;
            fileNameText.Text = name;

            fileSizeText.Text = size + " bytes";
            fileSizeLabel.Visibility = size == null || size == "" ? Visibility.Collapsed : Visibility.Visible;
            fileSizeText.Visibility = fileSizeLabel.Visibility;

            fileTypeText.Text = GetFileType(name);
            fileTypeLabel.Visibility = fileTypeText.Text == "" ? Visibility.Collapsed : Visibility.Visible;
            fileTypeText.Visibility = fileTypeLabel.Visibility;
        }

        /* Show the image preview for the selected file in UI if possible */
        public void SetImagePreview(byte[] content)
        {
            Bitmap bitmap = GetAsBitmap(content);
            filePreviewGroup.Visibility = bitmap == null ? Visibility.Collapsed : Visibility.Visible;
            if (bitmap == null) return;
            filePreviewImage.Source = ImageSourceFromBitmap(bitmap);
        }

        /* Show the level selection dropdown if requested */
        public void ShowLevelSelect(bool show, PAKType type)
        {
            levelSelectGroup.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            if (show)
            {
                levelSelectDropdown.Items.Clear();
                List<string> mapList = Directory.GetFiles(SharedData.pathToAI + "/DATA/ENV/PRODUCTION/", "COMMANDS.PAK", SearchOption.AllDirectories).ToList<string>();
                if (type == PAKType.TEXTURES/* || type == PAKType.MODELS*/)
                    levelSelectDropdown.Items.Add("GLOBAL");
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

            // We auto-forward through to a button click when visibility is set here, to trigger a load.
            // NOTE: This is desired behaviour in OpenCAGE mode, but not in regular standalone mode.
            LevelSelected(null, null);
        }

        /* Toggle available buttons given the functionality of the current PAK */
        public void ShowFunctionButtons(PAKFunction function)
        {
            filePreviewGroup.Visibility = Visibility.Collapsed;
            fileInfoGroup.Visibility = Visibility.Collapsed;

            exportBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_FILES);
            replaceBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_REPLACE_FILES);
            deleteBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_DELETE_FILES);
            fileUtiltiesGroup.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_FILES | PAKFunction.CAN_REPLACE_FILES | PAKFunction.CAN_DELETE_FILES);

            importBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_IMPORT_FILES);
            exportAllBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_FILES);
            archiveUtilitiesGroup.Visibility = FlagToVisibility(function, PAKFunction.CAN_IMPORT_FILES | PAKFunction.CAN_EXPORT_FILES);
        }
        private Visibility FlagToVisibility(PAKFunction function, PAKFunction flag)
        {
            return function.HasFlag(flag) ? Visibility.Visible : Visibility.Collapsed;
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

        /* Convert a DDS file to bitmap */
        private Bitmap GetAsBitmap(byte[] content)
        {
            Bitmap toReturn = null;
            if (content == null) return null;
            try
            {
                MemoryStream imageStream = new MemoryStream(content);
                using (var image = Pfim.Pfim.FromStream(imageStream))
                {
                    PixelFormat format = PixelFormat.DontCare;
                    switch (image.Format)
                    {
                        case Pfim.ImageFormat.Rgba32:
                            format = PixelFormat.Format32bppArgb;
                            break;
                        case Pfim.ImageFormat.Rgb24:
                            format = PixelFormat.Format24bppRgb;
                            break;
                        default:
                            Console.WriteLine("Unsupported DDS: " + image.Format);
                            break;
                    }
                    if (format != PixelFormat.DontCare)
                    {
                        var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                        try
                        {
                            var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                            toReturn = new Bitmap(image.Width, image.Height, image.Stride, format, data);
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return toReturn;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /* Convert a Bitmap to ImageSource */
        public System.Windows.Media.ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}
