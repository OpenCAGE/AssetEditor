using CATHODE;
using CathodeLib;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

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
        public Action OnPortRequested;
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
            UpdatePreviewRowHeight();
            if (_filePreviewBitmap == null) return;
            filePreviewImage.Source = _filePreviewBitmap?.ToImageSource();
        }

        /* Show the model preview for the selected file in UI */
        public void SetModelPreview(Model3DGroup content, bool zoomExtents = true)
        {
            modelPreviewGroup.Visibility = Visibility.Visible;
            imagePreviewGroup.Visibility = Visibility.Collapsed;
            UpdatePreviewRowHeight();
            filePreviewModel.Content = content;

            filePreviewModelContainer.ModelUpDirection = new Vector3D(0, 1, 0);
            filePreviewModelContainer.Camera.UpDirection = new Vector3D(0, 1, 0);
            filePreviewModelContainer.Camera.LookDirection = new Vector3D(-0.5, -0.5, -1.0f);
            
            if (zoomExtents)
                filePreviewModelContainer.ZoomExtents();
        }

        /* Show cubemap texture on a sphere in the model viewport so the user can orbit and look around */
        public void SetCubemapPreview(byte[] ddsContent)
        {
            Bitmap bmp = ddsContent?.ToBitmap();
            if (bmp == null)
            {
                modelPreviewGroup.Visibility = Visibility.Collapsed;
                imagePreviewGroup.Visibility = Visibility.Collapsed;
                UpdatePreviewRowHeight();
                return;
            }

            filePreviewModelContainer.Camera = new PerspectiveCamera();
            filePreviewModelContainer.CameraMode = CameraMode.FixedPosition;
            filePreviewModelContainer.Camera.Position = new Point3D(0, 0, 0);

            filePreviewModelContainer.IsZoomEnabled = false;
            filePreviewModelContainer.IsMoveEnabled = false;
            filePreviewModelContainer.ShowViewCube = false;

            Model3DGroup cubemapModel = CreateSphereWithTexture(bmp);
            SetModelPreview(cubemapModel, false);
        }
        private static Model3DGroup CreateSphereWithTexture(Bitmap texture)
        {
            const int thetaSegments = 48;
            const int phiSegments = 24;
            bool isCubemapStrip = texture.Width >= 4 * texture.Height;

            var positions = new Point3DCollection();
            var textureCoords = new PointCollection();
            var indices = new Int32Collection();

            for (int j = 0; j <= phiSegments; j++)
            {
                double phi = Math.PI * j / phiSegments;
                double y = Math.Cos(phi);
                double sinPhi = Math.Sin(phi);
                for (int i = 0; i <= thetaSegments; i++)
                {
                    double theta = 2 * Math.PI * i / thetaSegments;
                    double x = sinPhi * Math.Cos(theta);
                    double z = sinPhi * Math.Sin(theta);
                    positions.Add(new Point3D(x, y, z));

                    double u, v;
                    if (isCubemapStrip)
                    {
                        double ax = Math.Abs(x), ay = Math.Abs(y), az = Math.Abs(z);
                        int face;
                        double uf, vf;
                        if (ax >= ay && ax >= az)
                        {
                            face = x > 0 ? 0 : 1;
                            if (x > 0) { uf = (-z + 1) * 0.5; vf = (-y + 1) * 0.5; }
                            else { uf = (z + 1) * 0.5; vf = (-y + 1) * 0.5; }
                        }
                        else if (ay >= ax && ay >= az)
                        {
                            face = y > 0 ? 2 : 3;
                            if (y > 0) { uf = (x + 1) * 0.5; vf = (z + 1) * 0.5; }
                            else { uf = (x + 1) * 0.5; vf = (-z + 1) * 0.5; }
                        }
                        else
                        {
                            face = z > 0 ? 4 : 5;
                            if (z > 0) { uf = (x + 1) * 0.5; vf = (-y + 1) * 0.5; }
                            else { uf = (-x + 1) * 0.5; vf = (-y + 1) * 0.5; }
                        }
                        u = (face + uf) / 6.0;
                        v = vf;
                    }
                    else
                    {
                        double lon = Math.Atan2(z, x);
                        double lat = Math.Asin(Math.Max(-1, Math.Min(1, y)));
                        u = lon / (2 * Math.PI) + 0.5;
                        v = 0.5 - lat / Math.PI;
                    }
                    textureCoords.Add(new System.Windows.Point(u, v));
                }
            }

            for (int j = 0; j < phiSegments; j++)
            {
                for (int i = 0; i < thetaSegments; i++)
                {
                    int i0 = j * (thetaSegments + 1) + i;
                    int i1 = i0 + 1;
                    int i2 = i0 + (thetaSegments + 1);
                    int i3 = i2 + 1;
                    indices.Add(i0); indices.Add(i2); indices.Add(i1);
                    indices.Add(i1); indices.Add(i2); indices.Add(i3);
                }
            }

            var geometry = new MeshGeometry3D
            {
                Positions = positions,
                TextureCoordinates = textureCoords,
                TriangleIndices = indices
            };

            ImageSource imageSource = texture.ToImageSource();
            var brush = new ImageBrush(imageSource);
            var material = new DiffuseMaterial(brush);
            var backMaterial = new DiffuseMaterial(brush);

            var model = new GeometryModel3D(geometry, material) { BackMaterial = backMaterial };
            var group = new Model3DGroup() { Transform = Transform3D.Identity };
            group.Children.Add(model);
            return group;
        }

        /* Show the level selection dropdown if requested */
        public void ShowLevelSelect(bool show, PAKType type)
        {
            levelSelectGroup.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            if (show)
            {
                levelSelectDropdown.Items.Clear();
                List<string> levels = Level.GetLevels(SharedData.pathToAI, (type == PAKType.COMMANDS || type == PAKType.MATERIAL_MAPPINGS));
                for (int i = 0; i < levels.Count; i++) levelSelectDropdown.Items.Add(levels[i]);
                //if (type == PAKType.TEXTURES || type == PAKType.MODELS) levelSelectDropdown.Items.Add("GLOBAL");
                levelSelectDropdown.SelectedIndex = 0;
            }

            LevelSelected(null, null);
        }

        /* Toggle available buttons given the functionality of the current PAK */
        public void ShowFunctionButtons(PAKFunction function, PAKType type, bool hasSelectedFile) 
        {
            imagePreviewGroup.Visibility = Visibility.Collapsed;
            modelPreviewGroup.Visibility = Visibility.Collapsed;
            UpdatePreviewRowHeight();
            fileInfoGroup.Visibility = Visibility.Collapsed;

            //TODO: this is a temp hack to show the model button before i implement a nicer method when textures have a window too
            replaceBtn.Content = type == PAKType.MODELS ? "Modify Selected" : "Replace Selected";

            exportBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_FILES, hasSelectedFile);
            replaceBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_REPLACE_FILES, hasSelectedFile);
            deleteBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_DELETE_FILES, hasSelectedFile);
            fileUtiltiesGroup.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_FILES | PAKFunction.CAN_REPLACE_FILES | PAKFunction.CAN_DELETE_FILES, hasSelectedFile);

            importBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_IMPORT_FILES);
            exportAllBtn.Visibility = FlagToVisibility(function, PAKFunction.CAN_EXPORT_ALL);
            archiveUtilitiesGroup.Visibility = FlagToVisibility(function, PAKFunction.CAN_IMPORT_FILES | PAKFunction.CAN_EXPORT_FILES);
        }
        private void UpdatePreviewRowHeight()
        {
            bool anyPreviewVisible = imagePreviewGroup.Visibility == Visibility.Visible || modelPreviewGroup.Visibility == Visibility.Visible;
            previewRow.Height = anyPreviewVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            if (anyPreviewVisible)
                previewRow.MinHeight = 120;
            else
                previewRow.MinHeight = 0;
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
        private void PortBtn(object sender, RoutedEventArgs e)
        {
            OnPortRequested?.Invoke();
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
