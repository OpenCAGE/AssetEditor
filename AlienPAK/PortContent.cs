using CATHODE;
using CathodeLib;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class PortContent : Form
    {
        Explorer _explorer;

        Models.CS2 _model;
        Textures.TEX4 _texture;

        public PortContent()
        {
            InitializeComponent();
        }

        public void Setup(Explorer explorer, string entryName, string level)
        {
            levelList.BeginUpdate();
            levelList.Items.AddRange(Level.GetLevels(SharedData.pathToAI, true).ToArray());
            levelList.Items.Remove(level);
            levelList.EndUpdate();

            levelList.SelectedIndex = 0;

            _explorer = explorer;

            switch (_explorer.pak.Type)
            {
                case PAKType.MODELS:
                    _model = ((Models)_explorer.pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == entryName.Replace('\\', '/')).Copy();
                    this.Text = "Port \"" + _model.Name + "\"";
                    label1.Text = "Port model to level:";
                    break;
                case PAKType.TEXTURES:
                    _texture = ((Textures)_explorer.pak.File).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == entryName.Replace('\\', '/')).Copy();
                    this.Text = "Port \"" + _texture.Name + "\"";
                    label1.Text = "Port texture to level:";
                    copyAll.Visible = false;
                    break;
            }
        }

        private void export_Click(object sender, EventArgs e)
        {
            if (levelList.SelectedIndex == -1) return;

            //TODO: This highlights that the logic inside Explorer should be split into a separate non-GUI class...

            Level destinationLevel = new Level(SharedData.pathToAI + "/DATA/ENV/PRODUCTION/" + levelList.SelectedItem.ToString());
            destinationLevel.Save();

            //return;
            switch (_explorer.pak.Type)
            {
                case PAKType.MODELS:
                    {
                        if (copyAll.Checked)
                        {
                            //Copy resources
                            foreach (Models.CS2.Component component in _model.Components)
                            {
                                foreach (Models.CS2.Component.LOD lod in component.LODs)
                                {
                                    foreach (Models.CS2.Component.LOD.Submesh submesh in lod.Submeshes)
                                    {
                                        //Copy material
                                        Materials.Material originalMat = _explorer.materials.GetAtWriteIndex(submesh.MaterialLibraryIndex);
                                        Materials.Material existingMat = destinationLevel.Materials.Entries.FirstOrDefault(o => o.Name == originalMat.Name);
                                        if (existingMat != null && overwrite.Checked)
                                        {
                                            destinationLevel.Materials.Entries[destinationLevel.Materials.Entries.IndexOf(existingMat)] = originalMat.Copy();
                                        }
                                        else if (existingMat == null)
                                        {
                                            destinationLevel.Materials.Entries.Add(originalMat.Copy());
                                        }

                                        //Copy shader
                                        // TODO

                                        //Copy textures
                                        foreach (Materials.Material.Texture textureRef in originalMat.TextureReferences)
                                        {
                                            if (textureRef == null) continue;
                                            if (textureRef.Source == Materials.Material.Texture.TextureSource.GLOBAL) continue;

                                            Textures.TEX4 originalTex = _explorer.textures.GetAtWriteIndex(textureRef.BinIndex);
                                            Textures.TEX4 existingTex = destinationLevel.Textures.Entries.FirstOrDefault(o => o.Name == originalTex.Name);
                                            if (existingTex != null && overwrite.Checked)
                                            {
                                                destinationLevel.Textures.Entries[destinationLevel.Textures.Entries.IndexOf(existingTex)] = originalTex.Copy();
                                            }
                                            else if (existingTex == null)
                                            {
                                                destinationLevel.Textures.Entries.Add(originalTex.Copy());
                                            }
                                        }
                                    }
                                }
                            }

                            //Save now to recalculate the write indexes
                            destinationLevel.Save();

                            //Update the indexes 
                            foreach (Models.CS2.Component component in _model.Components)
                            {
                                foreach (Models.CS2.Component.LOD lod in component.LODs)
                                {
                                    foreach (Models.CS2.Component.LOD.Submesh submesh in lod.Submeshes)
                                    {
                                        //Update index of material 
                                        Materials.Material originalMaterial = _explorer.materials.GetAtWriteIndex(submesh.MaterialLibraryIndex);
                                        Materials.Material copiedMaterial = destinationLevel.Materials.Entries.FirstOrDefault(o => o.Name == originalMaterial.Name);
                                        submesh.MaterialLibraryIndex = destinationLevel.Materials.GetWriteIndex(copiedMaterial);

                                        //Update index of shader
                                        // TODO

                                        //Update indexes of textures
                                        for (int i = 0; i < originalMaterial.TextureReferences.Length; i++)
                                        {
                                            if (originalMaterial.TextureReferences[i] == null) continue;
                                            if (originalMaterial.TextureReferences[i].Source == Materials.Material.Texture.TextureSource.GLOBAL) continue;

                                            //TODO: update cst
                                            
                                            Textures.TEX4 originalTex = _explorer.textures.GetAtWriteIndex(originalMaterial.TextureReferences[i].BinIndex);
                                            Textures.TEX4 copiedTex = destinationLevel.Textures.Entries.FirstOrDefault(o => o.Name == originalTex.Name);
                                            copiedMaterial.TextureReferences[i].BinIndex = destinationLevel.Textures.GetWriteIndex(copiedTex);
                                        }
                                    }
                                }
                            }
                        }

                        //TODO: there are some hierarchy indexes in the CS2 that will probs need patching
                        Models.CS2 existingModel = destinationLevel.Models.Entries.FirstOrDefault(o => o.Name == _model.Name);
                        if (existingModel != null && overwrite.Checked)
                        {
                            destinationLevel.Models.Entries[destinationLevel.Models.Entries.IndexOf(existingModel)] = _model;
                        }
                        else if (existingModel == null)
                        {
                            destinationLevel.Models.Entries.Add(_model);
                        }
                        break;
                    }
                    
                case PAKType.TEXTURES:
                    {
                        Textures.TEX4 existingTex = destinationLevel.Textures.Entries.FirstOrDefault(o => o.Name == _model.Name);
                        if (existingTex != null && overwrite.Checked)
                        {
                            destinationLevel.Textures.Entries[destinationLevel.Textures.Entries.IndexOf(existingTex)] = _texture;
                        }
                        else if (existingTex == null)
                        {
                            destinationLevel.Textures.Entries.Add(_texture);
                        }
                        break;
                    }
            }
            destinationLevel.Save();

            this.Close();
        }
    }
}
