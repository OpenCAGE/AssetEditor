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
            ((LandingWPF)elementHost1.Child).SetVersionInfo(ProductVersion);
        }
        
        private void FormClosingEvent(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
