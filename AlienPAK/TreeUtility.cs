using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AlienPAK
{
    public enum TreeItemType
    {
        EXPORTABLE_FILE, //An exportable file
        LOADED_STRING, //A loaded string (WIP for COMMANDS.PAK)
        PREVIEW_ONLY_FILE, //A read-only file (export not supported yet)
        DIRECTORY //A parent directory listing
    };
    public enum TreeItemIcon
    {
        FOLDER,
        FILE,
        FOLDER_OPEN
    };

    public struct TreeItem
    {
        public string String_Value;
        public TreeItemType Item_Type;
    }

    class TreeUtility
    {
        private TreeView _fileTree;
        private bool _isModelTree;

        public TreeUtility(TreeView tree, bool isModelTree = false)
        {
            _fileTree = tree;
            _isModelTree = isModelTree;

            _fileTree.AfterExpand += FileTree_AfterExpand;
            _fileTree.AfterCollapse += FileTree_AfterCollapse;
        }

        ~TreeUtility()
        {
            ForceClearTree();
        }

        public void ForceClearTree()
        {
            if (_fileTree != null)
            {
                _fileTree.Nodes.Clear();
                _fileTree.Dispose();
            }
        }

        /* Update the file tree GUI */
        public void UpdateFileTree(List<string> FilesToList, ContextMenuStrip contextMenu = null, List<string> tags = null)
        {
            _fileTree.SuspendLayout();
            _fileTree.BeginUpdate();
            _fileTree.Nodes.Clear();
            for (int i = 0; i < FilesToList.Count; i++)
            {
                string[] FileNameParts = FilesToList[i].Split('/');
                if (FileNameParts.Length == 1) { FileNameParts = FilesToList[i].Split('\\'); }
                AddFileToTree(FileNameParts, 0, _fileTree.Nodes, contextMenu, (tags == null) ? "" : tags[i]);
            }
            _fileTree.Sort();
            _fileTree.EndUpdate();
            _fileTree.ResumeLayout();
        }

        /* Add a file to the GUI tree structure */
        private void AddFileToTree(string[] FileNameParts, int index, TreeNodeCollection LoopedNodeCollection, ContextMenuStrip contextMenu = null, string tag = "")
        {
            if (FileNameParts.Length <= index)
            {
                return;
            }

            bool should = true;
            foreach (TreeNode ThisFileNode in LoopedNodeCollection)
            {
                if (ThisFileNode.Text == FileNameParts[index])
                {
                    should = false;
                    AddFileToTree(FileNameParts, index + 1, ThisFileNode.Nodes, contextMenu, tag);
                    break;
                }
            }
            if (should && FileNameParts[index] != "")
            {
                TreeNode FileNode = new TreeNode(FileNameParts[index]);
                TreeItem ThisTag = new TreeItem();
                if (FileNameParts.Length - 1 == index)
                {
                    //Node is a file
                    for (int i = 0; i < FileNameParts.Length; i++) ThisTag.String_Value += FileNameParts[i] + "/";
                    ThisTag.String_Value = tag != "" ? tag : ThisTag.String_Value.ToString().Substring(0, ThisTag.String_Value.ToString().Length - 1);

                    FileNode.ImageIndex = (int)TreeItemIcon.FILE;
                    FileNode.SelectedImageIndex = FileNode.ImageIndex;

                    ThisTag.Item_Type = TreeItemType.EXPORTABLE_FILE;
                    if (contextMenu != null) FileNode.ContextMenuStrip = contextMenu;
                }
                else
                {
                    //Node is a directory
                    for (int i = 0; i < index + 1; i++) ThisTag.String_Value += FileNameParts[i] + "/";
                    ThisTag.String_Value = tag != "" ? tag : ThisTag.String_Value.ToString().Substring(0, ThisTag.String_Value.ToString().Length - 1);

                    ThisTag.Item_Type = TreeItemType.DIRECTORY;
                    FileNode.ImageIndex = (int)TreeItemIcon.FOLDER;
                    FileNode.SelectedImageIndex = (int)TreeItemIcon.FOLDER;
                    AddFileToTree(FileNameParts, index + 1, FileNode.Nodes, contextMenu, tag);
                }

                FileNode.Tag = ThisTag;
                LoopedNodeCollection.Add(FileNode);
            }
        }

        /* Select a node in the tree based on the path */
        public void SelectNode(string path)
        {
            string[] FileNameParts = path.Replace('\\', '/').Split('/');

            if (FileNameParts[FileNameParts.Length - 1] == "")
                Array.Resize(ref FileNameParts, FileNameParts.Length - 1);

            _fileTree.SelectedNode = null;

            TreeNodeCollection nodeCollection = _fileTree.Nodes;
            for (int x = 0; x < FileNameParts.Length; x++)
            {
                for (int i = 0; i < nodeCollection.Count; i++)
                {
                    if (nodeCollection[i].Text == FileNameParts[x])
                    {
                        if (x == FileNameParts.Length - 1)
                        {
                            _fileTree.SelectedNode = nodeCollection[i];
                        }
                        else
                        {
                            nodeCollection = nodeCollection[i].Nodes;
                        }
                        break;
                    }
                }
            }
            //_fileTree.Focus();
            //_fileTree.Select();
        }

        private void FileTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (((TreeItem)e.Node.Tag).Item_Type != TreeItemType.DIRECTORY) return;
            e.Node.ImageIndex = (int)TreeItemIcon.FOLDER;
            e.Node.SelectedImageIndex = (int)TreeItemIcon.FOLDER;
        }
        private void FileTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (((TreeItem)e.Node.Tag).Item_Type != TreeItemType.DIRECTORY) return;
            e.Node.ImageIndex = (int)TreeItemIcon.FOLDER_OPEN;
            e.Node.SelectedImageIndex = (int)TreeItemIcon.FOLDER_OPEN;
        }
    }
}
