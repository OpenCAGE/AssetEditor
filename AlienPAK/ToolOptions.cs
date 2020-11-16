using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class ToolOptions : Form
    {
        ToolOptionsHandler ToolSettings = new ToolOptionsHandler();

        public ToolOptions()
        {
            InitializeComponent();
        }

        /* Update settings values on launch */
        private void ToolOptions_Load(object sender, EventArgs e)
        {
            enableExperimentalTextureImport.Checked = ToolSettings.GetSetting(ToolOptionsHandler.Settings.EXPERIMENTAL_TEXTURE_IMPORT);
        }

        /* Enable/disable experimental texture import */
        private void enableExperimentalTextureImport_CheckedChanged(object sender, EventArgs e)
        {
            ToolSettings.UpdateSetting(enableExperimentalTextureImport.Checked, ToolOptionsHandler.Settings.EXPERIMENTAL_TEXTURE_IMPORT);
        }
    }
}
