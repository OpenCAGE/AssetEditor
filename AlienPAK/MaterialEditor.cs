using CATHODE;
using CATHODE.Enums;
using CATHODE.ShaderTypes;
using CathodeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using static CATHODE.Materials.Material;

namespace AlienPAK
{
    public partial class MaterialEditor : Form
    {
        Materials _materials = null;
        Shaders _shaders = null;
        Textures _textures = null;
        Textures _texturesGlobal = null;

        List<Materials.Material> _sortedMaterials = new List<Materials.Material>();
        Shaders.Shader _selectedMaterialShader = null;

        //Sampler information: (sampler name, sampler index, texture reference index)
        List<Tuple<string, int, int>> _samplerInfo = new List<Tuple<string, int, int>>();

        MaterialEditorControlsWPF _controls = null;

        public Action<Materials.Material> OnMaterialSelected;

        public MaterialEditor(Materials.Material material = null, Materials materials = null, Shaders shaders = null, Textures textures = null, Textures texturesGlobal = null)
        {
            _materials = materials;
            _shaders = shaders;
            _textures = textures;
            _texturesGlobal = texturesGlobal;

            InitializeComponent();
            if (_materials == null) return;

            _controls = (MaterialEditorControlsWPF)elementHost1.Child;
            _controls.OnSamplerSelected += OnSamplerSelected;
            _controls.OnParameterSelected += OnParameterSelected;
            _controls.OnPickTexture += OnPickTexture;

            PopulateUI(material);
        }

        private void MaterialEditor_Load(object sender, EventArgs e)
        {
            if (materialList.SelectedItems.Count > 0)
            {
                materialList.SelectedItems[0].EnsureVisible();
                materialList.EnsureVisible(materialList.SelectedItems[0].Index);
            }
        }

        private void OnSamplerSelected(int samplerTabIndex)
        {
            // Sampler tab changed - no additional behaviour needed for now.
        }

        private void OnPickTexture()
        {
            if (materialList.SelectedItems.Count == 0 || _controls.SamplerTabControl.SelectedIndex < 0) return;

            Materials.Material material = materialList.SelectedItems[0].Tag as Materials.Material;
            if (material == null) return;
            int samplerTabIndex = _controls.SamplerTabControl.SelectedIndex;
            if (samplerTabIndex >= _samplerInfo.Count) return;
            
            var samplerInfo = _samplerInfo[samplerTabIndex];
            int samplerIndex = samplerInfo.Item2;
            int textureRefIndex = samplerInfo.Item3;

            if (_textures == null && _texturesGlobal == null)
            {
                MessageBox.Show("No texture data is loaded for this material.", "Cannot pick texture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Textures.TEX4 currentTexture = null;
            if (textureRefIndex != 255 && textureRefIndex < material.TextureReferences.Count)
            {
                var textureRef = material.TextureReferences[textureRefIndex];
                currentTexture = textureRef?.Texture;
            }

            using (var picker = new TexturePicker(_textures, _texturesGlobal, currentTexture))
            {
                if (picker.ShowDialog(this) != DialogResult.OK)
                    return;

                var chosenTexture = picker.SelectedTexture;
                if (chosenTexture == null)
                    return;

                if (textureRefIndex != 255 && textureRefIndex < material.TextureReferences.Count)
                {
                    var textureRef = material.TextureReferences[textureRefIndex];
                    textureRef.Texture = chosenTexture;
                }
                else
                {
                    if (material.Shader.SamplerRemaps.Count <= samplerIndex)
                    {
                        while (material.Shader.SamplerRemaps.Count <= samplerIndex)
                            material.Shader.SamplerRemaps.Add(255);
                    }

                    textureRefIndex = material.TextureReferences.Count;
                    TexturePtr texturePtr = new TexturePtr
                    {
                        Texture = chosenTexture
                    };
                    material.TextureReferences.Add(texturePtr);
                    material.Shader.SamplerRemaps[samplerIndex] = textureRefIndex;

                    _samplerInfo[samplerTabIndex] = new Tuple<string, int, int>(samplerInfo.Item1, samplerIndex, textureRefIndex);
                }
            }

            materialList_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void ClearSamplerTexture()
        {
            if (materialList.SelectedItems.Count == 0 || _controls.SamplerTabControl.SelectedIndex < 0) return;

            Materials.Material material = materialList.SelectedItems[0].Tag as Materials.Material;
            if (material == null) return;
            int samplerTabIndex = _controls.SamplerTabControl.SelectedIndex;
            if (samplerTabIndex >= _samplerInfo.Count) return;

            var samplerInfo = _samplerInfo[samplerTabIndex];
            int samplerIndex = samplerInfo.Item2;
            int textureRefIndex = samplerInfo.Item3;

            if (textureRefIndex != 255 && textureRefIndex < material.TextureReferences.Count)
            {
                var textureRef = material.TextureReferences[textureRefIndex];
                textureRef.Texture = null;
            }

            if (samplerIndex < material.Shader.SamplerRemaps.Count)
                material.Shader.SamplerRemaps[samplerIndex] = 255;

            _samplerInfo[samplerTabIndex] = new Tuple<string, int, int>(samplerInfo.Item1, samplerIndex, 255);

            materialList_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void OnFeatureCheckboxChanged(Materials.Material material, int featureIndex, bool isChecked)
        {
            if (isChecked)
                material.Shader.UbershaderFeatureFlags |= (1L << featureIndex);
            else
                material.Shader.UbershaderFeatureFlags &= ~(1L << featureIndex);
        }

        private void OnParameterSelected(string parameterName)
        {
            _controls.ParameterDetailsPanel.Children.Clear();

            if (materialList.SelectedItems.Count == 0) return;

            Materials.Material material = materialList.SelectedItems[0].Tag as Materials.Material;
            if (material == null) return;

            int parameterIndex = ShaderUtility.GetShaderFunctionalityIndex(material.Shader.Ubershader, ShaderIndexType.PARAMETERS, parameterName).Value;
            UberShaderParameterType parameterType = ShaderUtility.GetParameterType(material.Shader.Ubershader, parameterName).Value;
            int remappedIndex = material.Shader.PixelShaderParameterRemaps[parameterIndex];
            int floatCount = GetFloatCountForParameterType(parameterType);

            bool isInt = parameterType == UberShaderParameterType.Int;
            bool isColorLike =
                (parameterType == UberShaderParameterType.Float3 || parameterType == UberShaderParameterType.Float4 ||
                 parameterType == UberShaderParameterType.Half3 || parameterType == UberShaderParameterType.Half4) &&
                (parameterName.IndexOf("color", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 parameterName.IndexOf("colour", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 parameterName.IndexOf("tint", StringComparison.OrdinalIgnoreCase) >= 0);

            TextBlock header = new TextBlock
            {
                Margin = new System.Windows.Thickness(0, 0, 0, 5),
                Text = $"{parameterName} ({parameterType})"
            };
            _controls.ParameterDetailsPanel.Children.Add(header);

            System.Windows.Controls.StackPanel row = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Margin = new System.Windows.Thickness(0, 0, 0, 5)
            };

            Func<float, string> toString = v => v.ToString("F6", CultureInfo.InvariantCulture);

            Action<System.Windows.Controls.TextBox, int> attachFloatHandler = (box, componentOffset) =>
            {
                box.LostKeyboardFocus += (s, eArgs) =>
                {
                    if (remappedIndex + componentOffset >= material.PixelShaderConstants.Count)
                        return;

                    float current = material.PixelShaderConstants[remappedIndex + componentOffset];

                    if (isInt)
                    {
                        if (int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
                        {
                            material.PixelShaderConstants[remappedIndex + componentOffset] = intVal;
                            box.Text = intVal.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            box.Text = ((int)current).ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        if (float.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                        {
                            material.PixelShaderConstants[remappedIndex + componentOffset] = val;
                            box.Text = toString(val);
                        }
                        else
                        {
                            box.Text = toString(current);
                        }
                    }
                };
            };

            if (floatCount == 1)
            {
                float v = remappedIndex < material.PixelShaderConstants.Count ? material.PixelShaderConstants[remappedIndex] : 0f;

                TextBlock label = new TextBlock
                {
                    Text = isInt ? "Value (int):" : "Value:",
                    Margin = new System.Windows.Thickness(0, 0, 5, 0),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                row.Children.Add(label);

                System.Windows.Controls.TextBox box = new System.Windows.Controls.TextBox
                {
                    Width = 100,
                    Text = isInt ? ((int)v).ToString(CultureInfo.InvariantCulture) : toString(v)
                };
                attachFloatHandler(box, 0);
                row.Children.Add(box);
            }
            else
            {
                string[] labels = floatCount == 2
                    ? new[] { "X:", "Y:" }
                    : floatCount == 3 ? new[] { "X:", "Y:", "Z:" } : new[] { "X:", "Y:", "Z:", "W:" };

                for (int i = 0; i < floatCount; i++)
                {
                    float v = (remappedIndex + i) < material.PixelShaderConstants.Count
                        ? material.PixelShaderConstants[remappedIndex + i]
                        : 0f;

                    TextBlock label = new TextBlock
                    {
                        Text = labels[i],
                        Margin = new System.Windows.Thickness(i == 0 ? 0 : 10, 0, 5, 0),
                        VerticalAlignment = System.Windows.VerticalAlignment.Center
                    };
                    row.Children.Add(label);

                    System.Windows.Controls.TextBox box = new System.Windows.Controls.TextBox
                    {
                        Width = 80,
                        Text = isInt ? ((int)v).ToString(CultureInfo.InvariantCulture) : toString(v)
                    };
                    attachFloatHandler(box, i);
                    row.Children.Add(box);
                }
            }

            _controls.ParameterDetailsPanel.Children.Add(row);

            if (isColorLike)
            {
                System.Windows.Controls.Button colorButton = new System.Windows.Controls.Button
                {
                    Content = "Pick Color...",
                    Margin = new System.Windows.Thickness(0, 5, 0, 0),
                    Width = 100
                };

                colorButton.Click += (s, eArgs) =>
                {
                    float r = remappedIndex + 0 < material.PixelShaderConstants.Count ? material.PixelShaderConstants[remappedIndex + 0] : 0f;
                    float g = remappedIndex + 1 < material.PixelShaderConstants.Count ? material.PixelShaderConstants[remappedIndex + 1] : 0f;
                    float b = remappedIndex + 2 < material.PixelShaderConstants.Count ? material.PixelShaderConstants[remappedIndex + 2] : 0f;
                    float a = remappedIndex + 3 < material.PixelShaderConstants.Count ? material.PixelShaderConstants[remappedIndex + 3] : 1f;

                    using (var dialog = new ColorDialog())
                    {
                        int clampByte(float f) => Math.Max(0, Math.Min(255, (int)Math.Round(f * 255f)));

                        dialog.Color = System.Drawing.Color.FromArgb(
                            clampByte(a),
                            clampByte(r),
                            clampByte(g),
                            clampByte(b));

                        if (dialog.ShowDialog() != DialogResult.OK)
                            return;

                        float fromByte(byte c) => c / 255f;

                        if (remappedIndex + 0 < material.PixelShaderConstants.Count)
                            material.PixelShaderConstants[remappedIndex + 0] = fromByte(dialog.Color.R);
                        if (remappedIndex + 1 < material.PixelShaderConstants.Count)
                            material.PixelShaderConstants[remappedIndex + 1] = fromByte(dialog.Color.G);
                        if (remappedIndex + 2 < material.PixelShaderConstants.Count)
                            material.PixelShaderConstants[remappedIndex + 2] = fromByte(dialog.Color.B);
                        if (floatCount == 4 && remappedIndex + 3 < material.PixelShaderConstants.Count)
                            material.PixelShaderConstants[remappedIndex + 3] = fromByte(dialog.Color.A);

                        // Refresh UI by re-invoking this handler
                        OnParameterSelected(parameterName);
                    }
                };

                _controls.ParameterDetailsPanel.Children.Add(colorButton);
            }
        }

        private int GetFloatCountForParameterType(UberShaderParameterType parameterType)
        {
            switch (parameterType)
            {
                case UberShaderParameterType.Float:
                case UberShaderParameterType.Half:
                case UberShaderParameterType.Int:
                    return 1;
                case UberShaderParameterType.Float2:
                case UberShaderParameterType.Half2:
                    return 2;
                case UberShaderParameterType.Float3:
                case UberShaderParameterType.Half3:
                    return 3;
                case UberShaderParameterType.Float4:
                case UberShaderParameterType.Half4:
                    return 4;
                default:
                    return 1;
            }
        }

        private void PopulateUI(Materials.Material material = null, string filter = null)
        {
            _sortedMaterials.Clear();
            IEnumerable<Materials.Material> source = _materials.Entries;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                string trimmedFilter = filter.Trim();
                source = source.Where(m => !string.IsNullOrEmpty(m.Name) &&
                                           m.Name.IndexOf(trimmedFilter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _sortedMaterials.AddRange(source);
            _sortedMaterials = _sortedMaterials.OrderBy(o => o.Name).ToList();

            materialList.BeginUpdate();
            materialList.Items.Clear();
            materialList.Groups.Clear();
            materialList.Columns.Clear();
            
            materialList.Columns.Add("Material Name", 360);
            
            var groupedMaterials = _sortedMaterials.Where(m => m.Shader != null).GroupBy(m => m.Shader.Ubershader).OrderBy(g => g.Key.ToString());

            foreach (var group in groupedMaterials)
            {
                string groupName = group.Key.ToString();
                System.Windows.Forms.ListViewGroup listGroup = new System.Windows.Forms.ListViewGroup(groupName, groupName);
                materialList.Groups.Add(listGroup);
                
                foreach (var mat in group.OrderBy(m => m.Name))
                {
                    System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(mat.Name);
                    item.Group = listGroup;
                    item.Tag = mat;
                    materialList.Items.Add(item);

                    if (mat == material)
                        item.Selected = true;
                }
            }
            materialList.EndUpdate();
        }

        private void ApplyMaterialSearch()
        {
            string filter = materialSearchTextBox.Text;

            Materials.Material selectedMaterial = null;
            if (materialList.SelectedItems.Count > 0)
                selectedMaterial = materialList.SelectedItems[0].Tag as Materials.Material;

            PopulateUI(selectedMaterial, filter);
        }

        private void materialSearchButton_Click(object sender, EventArgs e)
        {
            ApplyMaterialSearch();
        }

        private void materialSearchClearButton_Click(object sender, EventArgs e)
        {
            Materials.Material selectedMaterial = null;
            if (materialList.SelectedItems.Count > 0)
                selectedMaterial = materialList.SelectedItems[0].Tag as Materials.Material;

            materialSearchTextBox.Text = string.Empty;
            PopulateUI(selectedMaterial);
        }

        private void materialSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                materialSearchButton.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void materialList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _controls.SamplerTabControl.Items.Clear();
            _samplerInfo.Clear();
            _controls.ParameterSelection.Items.Clear();
            _controls.FeatureDetailsPanel.Children.Clear();
            _controls.ParameterDetailsPanel.Children.Clear();
            _controls.ShaderType.Text = "";
            _controls.MaterialName.Text = "";

            if (materialList.SelectedItems.Count == 0) return;

            Materials.Material material = materialList.SelectedItems[0].Tag as Materials.Material;
            if (material == null) return;

            _controls.MaterialName.Text = material.Name;
            _controls.ShaderType.Text = material.Shader.Ubershader.ToString();

            List<string> samplers = ShaderUtility.GetSamplers(material.Shader.Ubershader);
            int firstSamplerWithTextureIndex = -1;
            for (int i = 0; i < samplers.Count; i++)
            {
                string sampler = samplers[i];
                int? samplerIndexNullable = ShaderUtility.GetShaderFunctionalityIndex(material.Shader.Ubershader, ShaderIndexType.SAMPLERS, sampler);
                if (!samplerIndexNullable.HasValue) continue;

                int samplerIndex = samplerIndexNullable.Value;
                int textureRefIndex = 255;
                if (samplerIndex < material.Shader.SamplerRemaps.Count)
                    textureRefIndex = material.Shader.SamplerRemaps[samplerIndex];

                bool hasTexture = false;
                Textures.TEX4 texture = null;
                if (textureRefIndex != 255 && textureRefIndex < material.TextureReferences.Count)
                {
                    var textureRef = material.TextureReferences[textureRefIndex];
                    if (textureRef?.Texture != null)
                    {
                        hasTexture = true;
                        texture = textureRef.Texture;
                    }
                }

                if (hasTexture && firstSamplerWithTextureIndex == -1)
                    firstSamplerWithTextureIndex = _controls.SamplerTabControl.Items.Count;

                TextBlock tabHeader = new System.Windows.Controls.TextBlock
                {
                    Text = sampler,
                    FontWeight = hasTexture ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal
                };

                StackPanel tabContent = new StackPanel { Margin = new System.Windows.Thickness(10) };
                
                TextBlock textureFileText = new TextBlock 
                { 
                    Text = $"Sampler: {sampler}",
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                };
                
                ScrollViewer imageScrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                
                System.Windows.Controls.Image texturePreview = new System.Windows.Controls.Image
                {
                    Source = texture?.ToDDS()?.ToBitmap()?.ToImageSource(),
                    Stretch = Stretch.Uniform,
                    MaxHeight = 400
                };
                imageScrollViewer.Content = texturePreview;

                System.Windows.Controls.StackPanel samplerButtonsPanel = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Margin = new System.Windows.Thickness(0, 10, 0, 0)
                };

                System.Windows.Controls.Button pickTextureButton = new System.Windows.Controls.Button
                {
                    Content = hasTexture ? "Edit Texture..." : "Pick Texture...",
                    Margin = new System.Windows.Thickness(0, 0, 10, 0)
                };
                pickTextureButton.Click += (s, args) => OnPickTexture();

                System.Windows.Controls.Button clearTextureButton = new System.Windows.Controls.Button
                {
                    Content = "Clear Texture",
                    IsEnabled = textureRefIndex != 255,
                    Margin = new System.Windows.Thickness(0, 0, 0, 0)
                };
                clearTextureButton.Click += (s, args) => ClearSamplerTexture();

                samplerButtonsPanel.Children.Add(pickTextureButton);
                samplerButtonsPanel.Children.Add(clearTextureButton);

                TextBlock detailsText = new TextBlock
                {
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Margin = new System.Windows.Thickness(0, 10, 0, 0)
                };
                
                StringBuilder details = new StringBuilder();
                details.AppendLine($"Sampler Name: {sampler}");
                details.AppendLine($"Sampler Index: {samplerIndex}");
                details.AppendLine($"Texture Reference Index: {(textureRefIndex == 255 ? "Not assigned" : textureRefIndex.ToString())}");
                
                if (hasTexture && texture != null)
                {
                    textureFileText.Text += $"\nTexture: {texture.Name}";
                    details.AppendLine($"Texture Name: {texture.Name}");
                    details.AppendLine($"Texture Format: {texture.Format}");
                }
                else if (textureRefIndex != 255 && textureRefIndex < material.TextureReferences.Count)
                {
                    textureFileText.Text += "\nTexture: (Empty slot)";
                    details.AppendLine("Texture: Empty slot (no texture assigned)");
                }
                else
                {
                    textureFileText.Text += "\nTexture: (Not assigned)";
                    details.AppendLine("Texture: Not assigned to this sampler");
                }
                
                detailsText.Text = details.ToString();
                
                tabContent.Children.Add(textureFileText);
                tabContent.Children.Add(imageScrollViewer);
                tabContent.Children.Add(samplerButtonsPanel);
                tabContent.Children.Add(detailsText);
                
                TabItem tabItem = new TabItem
                {
                    Header = tabHeader,
                    Content = tabContent
                };
                
                _controls.SamplerTabControl.Items.Add(tabItem);
                _samplerInfo.Add(new Tuple<string, int, int>(sampler, samplerIndex, textureRefIndex));
            }
            if (_controls.SamplerTabControl.Items.Count != 0)
            {
                int selectedIndex = firstSamplerWithTextureIndex >= 0 ? firstSamplerWithTextureIndex : 0;
                _controls.SamplerTabControl.SelectedIndex = selectedIndex;
            }

            List<string> features = ShaderUtility.GetFeatures(material.Shader.Ubershader);
            foreach (string feature in features)
            {
                int? featureIndexNullable = ShaderUtility.GetShaderFunctionalityIndex(material.Shader.Ubershader, ShaderIndexType.FEATURES, feature);
                if (!featureIndexNullable.HasValue) continue;

                int featureIndex = featureIndexNullable.Value;
                bool isEnabled = (material.Shader.UbershaderFeatureFlags & (1L << featureIndex)) != 0;

                System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox
                {
                    Content = feature,
                    IsChecked = isEnabled,
                    Margin = new System.Windows.Thickness(0, 0, 0, 5)
                };

                checkBox.Checked += (s, args) => OnFeatureCheckboxChanged(material, featureIndex, true);
                checkBox.Unchecked += (s, args) => OnFeatureCheckboxChanged(material, featureIndex, false);

                _controls.FeatureDetailsPanel.Children.Add(checkBox);
            }

            List<string> parameters = ShaderUtility.GetParameters(material.Shader.Ubershader);
            foreach (string parameter in parameters)
            {
                int parameterIndex = ShaderUtility.GetShaderFunctionalityIndex(material.Shader.Ubershader, ShaderIndexType.PARAMETERS, parameter).Value;
                if (parameterIndex >= material.Shader.PixelShaderParameterRemaps.Count) continue;

                int remappedIndex = material.Shader.PixelShaderParameterRemaps[parameterIndex];
                if (remappedIndex != 255 && remappedIndex < material.PixelShaderConstants.Count)
                {
                    _controls.ParameterSelection.Items.Add(parameter);
                }
            }
            if (_controls.ParameterSelection.Items.Count != 0)
                _controls.ParameterSelection.SelectedIndex = 0;
        }


        private void selectMaterial_Click(object sender, EventArgs e)
        {
            if (materialList.SelectedItems.Count == 0) return;
            
            Materials.Material material = materialList.SelectedItems[0].Tag as Materials.Material;
            if (material == null) return;
            
            OnMaterialSelected?.Invoke(material);
            this.Close();
        }

        private void duplicateMaterial_Click(object sender, EventArgs e)
        {
            if (materialList.SelectedItems.Count == 0) return;

            Materials.Material material = materialList.SelectedItems[0].Tag as Materials.Material;
            Materials.Material newMaterial = new Materials.Material();

            newMaterial.Name = material.Name + " Clone";
            for (int i = 0; i < material.TextureReferences.Count; i++)
            {
                TexturePtr texturePtr = new TexturePtr();
                texturePtr.Texture = material.TextureReferences[i].Texture;
                texturePtr.Location = material.TextureReferences[i].Location;
                newMaterial.TextureReferences.Add(texturePtr);
            }
            newMaterial.EngineConstants = new List<float>(material.EngineConstants);
            newMaterial.VertexShaderConstants = new List<float>(material.VertexShaderConstants);
            newMaterial.PixelShaderConstants = new List<float>(material.PixelShaderConstants);
            newMaterial.HullShaderConstants = new List<float>(material.HullShaderConstants);
            newMaterial.DomainShaderConstants = new List<float>(material.DomainShaderConstants);
            newMaterial.OfflineLightFeatures = material.OfflineLightFeatures.Copy();
            newMaterial.Shader = material.Shader;
            newMaterial.PhysicalMaterialIndex = material.PhysicalMaterialIndex;
            newMaterial.EnvironmentMapIndex = material.EnvironmentMapIndex;
            newMaterial.Priority = material.Priority;

            _materials.Entries.Add(newMaterial);
            PopulateUI(newMaterial);
            //ensure we select the new one
        }
    }
}
