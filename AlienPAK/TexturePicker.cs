using CATHODE;
using CathodeLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class TexturePicker : Form
    {
        private readonly Textures _textures;
        private readonly Textures _texturesGlobal;
        private TreeUtility _treeHelper;

        private enum TextureScope
        {
            Level,
            Global
        }

        private TextureScope _currentScope;
        private readonly List<string> _levelTexturePaths = new List<string>();
        private readonly List<string> _globalTexturePaths = new List<string>();

        public Textures.TEX4 SelectedTexture { get; private set; }

        public TexturePicker(Textures textures, Textures texturesGlobal, Textures.TEX4 initialTexture = null)
        {
            _textures = textures;
            _texturesGlobal = texturesGlobal;

            InitializeComponent();

            InitializeScopeFromInitialTexture(initialTexture);
            BuildTree();
            PreselectInitialTexture(initialTexture);
        }

        private void BuildTree()
        {
            _levelTexturePaths.Clear();
            _globalTexturePaths.Clear();

            if (_textures != null)
            {
                foreach (var tex in _textures.Entries)
                {
                    if (!string.IsNullOrEmpty(tex.Name))
                        _levelTexturePaths.Add(tex.Name.Replace('\\', '/'));
                }
            }

            if (_texturesGlobal != null)
            {
                foreach (var tex in _texturesGlobal.Entries)
                {
                    if (!string.IsNullOrEmpty(tex.Name))
                        _globalTexturePaths.Add(tex.Name.Replace('\\', '/'));
                }
            }

            _treeHelper = new TreeUtility(fileTree);
            ApplyTextureSearch();
        }

        private void PreselectInitialTexture(Textures.TEX4 initialTexture)
        {
            if (initialTexture == null || string.IsNullOrEmpty(initialTexture.Name) || _treeHelper == null)
                return;

            string path = initialTexture.Name.Replace('\\', '/');
            _treeHelper.SelectNode(path);

            if (fileTree.SelectedNode != null)
                UpdateSelectionFromNode(fileTree.SelectedNode);
        }

        private void ApplyTextureSearch()
        {
            if (_treeHelper == null)
                return;

            IEnumerable<string> source = GetCurrentScopePaths();
            string filter = textureSearchTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string trimmed = filter.Trim();
                source = source.Where(p => p.IndexOf(trimmed, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _treeHelper.UpdateFileTree(source.ToList(), null);

            SelectedTexture = null;
            previewImage.Image = null;
            selectedNameLabel.Text = source.Any() ? "No texture selected" : "No textures match search";
        }

        private void textureSearchButton_Click(object sender, EventArgs e)
        {
            ApplyTextureSearch();
        }

        private void textureSearchClearButton_Click(object sender, EventArgs e)
        {
            textureSearchTextBox.Text = string.Empty;
            ApplyTextureSearch();
        }

        private void textureSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyTextureSearch();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void fileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateSelectionFromNode(e.Node);
        }

        private IEnumerable<string> GetCurrentScopePaths()
        {
            return _currentScope == TextureScope.Global
                ? _globalTexturePaths
                : _levelTexturePaths;
        }

        private void InitializeScopeFromInitialTexture(Textures.TEX4 initialTexture)
        {
            _currentScope = TextureScope.Level;

            if (initialTexture != null)
            {
                if (IsTextureInCollection(initialTexture, _texturesGlobal))
                {
                    _currentScope = TextureScope.Global;
                }
                else if (IsTextureInCollection(initialTexture, _textures))
                {
                    _currentScope = TextureScope.Level;
                }
            }

            if (textureScopeTabs != null)
            {
                textureScopeTabs.SelectedIndex = _currentScope == TextureScope.Global ? 1 : 0;
            }
        }

        private static bool IsTextureInCollection(Textures.TEX4 texture, Textures collection)
        {
            if (texture == null || collection == null)
                return false;

            if (collection.Entries.Contains(texture))
                return true;

            if (string.IsNullOrEmpty(texture.Name))
                return false;

            return collection.Entries.Any(t => string.Equals(t.Name, texture.Name, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdateSelectionFromNode(TreeNode node)
        {
            SelectedTexture = null;
            previewImage.Image = null;
            selectedNameLabel.Text = "";

            if (node == null || node.Tag == null)
                return;

            TreeItem item = (TreeItem)node.Tag;
            if (item.Item_Type != TreeItemType.EXPORTABLE_FILE)
                return;

            string path = item.String_Value.Replace('\\', '/');

            Textures.TEX4 texture = null;
            Textures activeCollection = _currentScope == TextureScope.Global ? _texturesGlobal : _textures;
            if (activeCollection != null)
                texture = activeCollection.Entries.FirstOrDefault(t => t.Name.Replace('\\', '/') == path);

            SelectedTexture = texture;
            if (texture == null)
                return;

            byte[] dds = texture.ToDDS();
            Bitmap bmp = dds?.ToBitmap();
            previewImage.Image = bmp;
            selectedNameLabel.Text = texture.Name;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (SelectedTexture == null)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            SelectedTexture = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void fileTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (SelectedTexture != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void textureScopeTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentScope = textureScopeTabs.SelectedIndex == 1 ? TextureScope.Global : TextureScope.Level;
            ApplyTextureSearch();
        }

        private void TexturePicker_FormClosed(object sender, FormClosedEventArgs e)
        {
            _treeHelper?.ForceClearTree();
            _treeHelper = null;
        }
    }
}

