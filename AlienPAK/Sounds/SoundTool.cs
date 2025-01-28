using AlienPAK.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace AlienPAK.Sounds
{
    public partial class SoundTool : Form
    {
        WwiseSound SelectedSoundFile
        {
            get
            {
                if (treeView.SelectedNode?.Tag == null) return null;
                return (WwiseSound)treeView.SelectedNode.Tag;
            }
        }

        public SoundTool()
        {
            InitializeComponent();

            treeView.BeginUpdate();
            foreach (WwiseSound sound in SoundbankInfo.Sounds)
                AddFilePathToTree(sound);
            treeView.EndUpdate();
        }

        public void AddFilePathToTree(WwiseSound file)
        {
            var parts = file.Path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            TreeNodeCollection currentNodeCollection = treeView.Nodes;

            for (int i = 0; i < parts.Length; i++)
            {
                TreeNode existingNode = null;
                foreach (TreeNode node in currentNodeCollection)
                {
                    if (node.Text.Equals(parts[i], StringComparison.OrdinalIgnoreCase))
                    {
                        existingNode = node;
                        break;
                    }
                }

                if (existingNode == null)
                {
                    var newNode = new TreeNode(parts[i]);

                    if (i == parts.Length - 1)
                    {
                        newNode.Tag = file;
                        newNode.ImageIndex = 1;
                        newNode.SelectedImageIndex = 1;
                    }

                    currentNodeCollection.Add(newNode);
                    currentNodeCollection = newNode.Nodes;
                }
                else
                {
                    currentNodeCollection = existingNode.Nodes;
                }
            }
        }

        private void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            WwiseSound cmb = SelectedSoundFile;
            if (cmb == null) return;

            soundbanksReferenced.Items.Clear();
            foreach (Tuple<UInt32, string> bnk in cmb.SoundBank_Referenced)
                soundbanksReferenced.Items.Add(bnk.Item2);

            soundbanksIncluded.Items.Clear();
            foreach (Tuple<UInt32, string> bnk in cmb.SoundBank_Included)
                soundbanksIncluded.Items.Add(bnk.Item2);

            soundbanksIncludedPrefetch.Items.Clear();
            foreach (Tuple<UInt32, string> bnk in cmb.SoundBank_IncludedPrefetch)
                soundbanksIncludedPrefetch.Items.Add(bnk.Item2);
        }

        private void exportWEM_Click(object sender, EventArgs e)
        {

        }

        private void importWEM_Click(object sender, EventArgs e)
        {

        }
    }
}
