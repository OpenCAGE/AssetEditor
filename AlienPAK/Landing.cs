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
            LandingWPF wpf = (LandingWPF)elementHost1.Child;
            wpf.SetVersionInfo(ProductVersion);
            wpf.DoFocus += DoFocus;
            wpf.DoHide += DoHide;
        }

        private void DoHide()
        {
            this.Hide();
        }

        private void DoFocus()
        {
            this.Show();
            this.BringToFront();
            this.Focus();
        }
        
        private void FormClosingEvent(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
