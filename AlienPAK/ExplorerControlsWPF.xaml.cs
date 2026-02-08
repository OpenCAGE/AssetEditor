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

                    if (!isCubemapStrip)
                    {
                        double lon = Math.Atan2(z, x);
                        double lat = Math.Asin(Math.Max(-1, Math.Min(1, y)));
                        double u = lon / (2 * Math.PI) + 0.5;
                        double v = 0.5 - lat / Math.PI;
                        textureCoords.Add(new System.Windows.Point(u, v));
                    }
                }
            }

            if (isCubemapStrip)
            {
                int facePixels = texture.Width / 6;
                double eps = (facePixels > 1) ? 0.5 / facePixels : 0.0;

                var outPositions = new Point3DCollection();
                var outTextureCoords = new PointCollection();
                var outIndices = new Int32Collection();

                for (int j = 0; j < phiSegments; j++)
                {
                    for (int i = 0; i < thetaSegments; i++)
                    {
                        int i0 = j * (thetaSegments + 1) + i;
                        int i1 = i0 + 1;
                        int i2 = i0 + (thetaSegments + 1);
                        int i3 = i2 + 1;

                        Point3D p0 = positions[i0], p1 = positions[i1], p2 = positions[i2];
                        Point3D c1 = new Point3D(
                            (p0.X + p2.X + p1.X) / 3,
                            (p0.Y + p2.Y + p1.Y) / 3,
                            (p0.Z + p2.Z + p1.Z) / 3);
                        int baseIdx = outPositions.Count;
                        CubemapFaceUV(c1, p0, p1, p2, eps, outPositions, outTextureCoords);
                        outIndices.Add(baseIdx); outIndices.Add(baseIdx + 1); outIndices.Add(baseIdx + 2);

                        p0 = positions[i1]; p1 = positions[i3]; p2 = positions[i2];
                        Point3D c2 = new Point3D(
                            (p0.X + p1.X + p2.X) / 3,
                            (p0.Y + p1.Y + p2.Y) / 3,
                            (p0.Z + p1.Z + p2.Z) / 3);
                        baseIdx = outPositions.Count;
                        CubemapFaceUV(c2, p0, p1, p2, eps, outPositions, outTextureCoords);
                        outIndices.Add(baseIdx); outIndices.Add(baseIdx + 1); outIndices.Add(baseIdx + 2);
                    }
                }

                positions = outPositions;
                textureCoords = outTextureCoords;
                indices = outIndices;
            }
            else
            {
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
        private static void CubemapFaceUV(Point3D centroid, Point3D p0, Point3D p1, Point3D p2, double eps, Point3DCollection outPositions, PointCollection outTextureCoords)
        {
            double cx = centroid.X, cy = centroid.Y, cz = centroid.Z;
            double ax = Math.Abs(cx), ay = Math.Abs(cy), az = Math.Abs(cz);
            int face;
            if (ax >= ay && ax >= az)
                face = cx > 0 ? 0 : 1;
            else if (ay >= ax && ay >= az)
                face = cy > 0 ? 2 : 3;
            else
                face = cz > 0 ? 4 : 5;

            foreach (Point3D p in new[] { p0, p1, p2 })
            {
                double x = p.X, y = p.Y, z = p.Z;
                double uf, vf;
                double inv;
                switch (face)
                {
                    case 0: inv = 1.0 / (Math.Abs(x) + 1e-10); uf = (-z * inv + 1) * 0.5; vf = (-y * inv + 1) * 0.5; break;
                    case 1: inv = 1.0 / (Math.Abs(x) + 1e-10); uf = (z * inv + 1) * 0.5; vf = (-y * inv + 1) * 0.5; break;
                    case 2: inv = 1.0 / (Math.Abs(y) + 1e-10); uf = (x * inv + 1) * 0.5; vf = (z * inv + 1) * 0.5; break;
                    case 3: inv = 1.0 / (Math.Abs(y) + 1e-10); uf = (x * inv + 1) * 0.5; vf = (-z * inv + 1) * 0.5; break;
                    case 4: inv = 1.0 / (Math.Abs(z) + 1e-10); uf = (x * inv + 1) * 0.5; vf = (-y * inv + 1) * 0.5; break;
                    default: inv = 1.0 / (Math.Abs(z) + 1e-10); uf = (-x * inv + 1) * 0.5; vf = (-y * inv + 1) * 0.5; break;
                }
                uf = Math.Max(0, Math.Min(1, uf));
                vf = Math.Max(0, Math.Min(1, vf));
                double ufInset = uf * (1 - 2 * eps) + eps;
                double vfInset = vf * (1 - 2 * eps) + eps;
                double u = (face + ufInset) / 6.0;
                double v = vfInset;

                outPositions.Add(p);
                outTextureCoords.Add(new System.Windows.Point(u, v));
            }
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
