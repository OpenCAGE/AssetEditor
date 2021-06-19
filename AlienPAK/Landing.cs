using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class Landing : Form
    {
        public Landing()
        {
            InitializeComponent();
        }

        private void Landing_ContentTools_Load(object sender, EventArgs e)
        {
            //Set fonts & parents
            HeaderText.Font = FontManager.GetFont(1, 80);
            HeaderText.Parent = HeaderImage;
            InterfaceTools.Font = FontManager.GetFont(0, 40);
            ModelTools.Font = FontManager.GetFont(0, 40);
            TextureTools.Font = FontManager.GetFont(0, 40);
        }

        bool closedManually = false;
        private void CloseButton_Click(object sender, EventArgs e)
        {
            closedManually = true;
            this.Close();
            Application.Exit();
            Environment.Exit(0);
        }

        //When closing, check to see if we were manually closed
        //If not, halt the whole process to avoid lingering in background
        private void FormClosingEvent(object sender, FormClosingEventArgs e)
        {
            if (!closedManually)
            {
                Application.Exit();
                Environment.Exit(0);
            }
        }

        //UI IMPORT/EXPORT
        private void InterfaceTools_Click(object sender, EventArgs e)
        {
            Explorer interfaceTool = new Explorer(new string[] { }, AlienContentType.UI);
            interfaceTool.Show();
        }

        //MODEL IMPORT/EXPORT
        private void ModelTools_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The model import/export tool is currently a work in progress!\nAll models are listed, but none can currently be imported/exported.\nStay tuned: this functionality is coming soon!", "Work in progress!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Explorer interfaceTool = new Explorer(new string[] { }, AlienContentType.MODEL);
            interfaceTool.Show();
        }

        //TEXTURE IMPORT/EXPORT
        private void TextureTools_Click(object sender, EventArgs e)
        {
            Explorer textureTool = new Explorer(new string[] { }, AlienContentType.TEXTURE);
            textureTool.Show();
        }

        //SOUND IMPORT/EXPORT
        private void SoundTool_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The sound import/export tool is currently a work in progress!\nStay tuned: this functionality is coming soon!", "Coming soon!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
    }
}
