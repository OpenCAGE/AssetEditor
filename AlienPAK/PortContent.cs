using CATHODE;
using CathodeLib;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AlienPAK
{
    public partial class PortContent : Form
    {
        PAKType _pakType;
        CathodeFile _file;

        Models.CS2 _model;
        Textures.TEX4 _texture;

        public PortContent()
        {
            InitializeComponent();
        }

        public void Setup(PAKType type, CathodeFile file, string entryName, string level)
        {
            levelList.BeginUpdate();
            levelList.Items.AddRange(Level.GetLevels(SharedData.pathToAI, true).ToArray());
            levelList.Items.Remove(level);
            levelList.EndUpdate();

            levelList.SelectedIndex = 0;

            _pakType = type;
            _file = file;

            switch (_pakType)
            {
                case PAKType.MODELS:
                    _model = ((Models)_file).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == entryName.Replace('\\', '/'));
                    this.Text = "Port \"" + _model.Name + "\"";
                    label1.Text = "Port model to level:";
                    break;
                case PAKType.TEXTURES:
                    _texture = ((Textures)_file).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == entryName.Replace('\\', '/'));
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

            Explorer explorer = new Explorer(levelList.SelectedItem.ToString(), _pakType.ToString());
            explorer.Hide();
            switch (_pakType)
            {
                case PAKType.MODELS:
                    //TODO: there are some indexes in the CS2 that will need patching
                    Models modelPAK = (Models)explorer.pak.File;
                    Models.CS2 existingModel = modelPAK.Entries.FirstOrDefault(o => o.Name == _model.Name);
                    if (existingModel != null && overwrite.Checked)
                    {
                        modelPAK.Entries[modelPAK.Entries.IndexOf(existingModel)] = _model;
                    }
                    else if (existingModel == null)
                    {
                        modelPAK.Entries.Add(_model);
                    }
                    explorer.SaveModelsAndUpdateREDS();
                    break;
                case PAKType.TEXTURES:
                    Textures texturePAK = (Textures)explorer.pak.File;
                    Textures.TEX4 existingTexture = texturePAK.Entries.FirstOrDefault(o => o.Name == _model.Name);
                    if (existingTexture != null && overwrite.Checked)
                    {
                        texturePAK.Entries[texturePAK.Entries.IndexOf(existingTexture)] = _texture;
                    }
                    else if (existingTexture == null)
                    {
                        texturePAK.Entries.Add(_texture);
                    }
                    Explorer.SaveTexturesAndUpdateMaterials(texturePAK, new Materials(SharedData.pathToAI + "/DATA/ENV/PRODUCTION/" + levelList.SelectedItem.ToString() + "/RENDERABLE/LEVEL_MODELS.MTL"));
                    break;
            }
            explorer.Close();
        }
    }
}
