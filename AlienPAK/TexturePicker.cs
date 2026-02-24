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

        public Textures.TEX4 SelectedTexture { get; private set; }

        public TexturePicker(Textures textures, Textures texturesGlobal, Textures.TEX4 initialTexture = null)
        {
            _textures = textures;
            _texturesGlobal = texturesGlobal;

            InitializeComponent();

            BuildTree();
            PreselectInitialTexture(initialTexture);
        }

        private void BuildTree()
        {
            var paths = new List<string>();

            if (_textures != null)
            {
                foreach (var tex in _textures.Entries)
                {
                    if (!string.IsNullOrEmpty(tex.Name))
                        paths.Add(tex.Name.Replace('\\', '/'));
                }
            }

            if (_texturesGlobal != null)
            {
                foreach (var tex in _texturesGlobal.Entries)
                {
                    if (!string.IsNullOrEmpty(tex.Name))
                        paths.Add(tex.Name.Replace('\\', '/'));
                }
            }

            _treeHelper = new TreeUtility(fileTree);
            _treeHelper.UpdateFileTree(paths, null);
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

        private void fileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateSelectionFromNode(e.Node);
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
            if (_textures != null)
                texture = _textures.Entries.FirstOrDefault(t => t.Name.Replace('\\', '/') == path);

            if (texture == null && _texturesGlobal != null)
                texture = _texturesGlobal.Entries.FirstOrDefault(t => t.Name.Replace('\\', '/') == path);

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

        private void TexturePicker_FormClosed(object sender, FormClosedEventArgs e)
        {
            _treeHelper?.ForceClearTree();
            _treeHelper = null;
        }
    }
}

