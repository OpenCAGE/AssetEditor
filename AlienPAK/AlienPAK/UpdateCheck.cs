using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class UpdateCheck : Form
    {
        public UpdateCheck()
        {
            InitializeComponent();
        }

        private void UpdateCheck_Load(object sender, EventArgs e)
        {
            try
            {
                //Get current Github version
                WebClient webClient = new WebClient();
                Random random = new Random();
                Stream webStream = webClient.OpenRead("https://raw.githubusercontent.com/MattFiler/AlienPAK/master/AlienPAK/AlienPAK/Properties/AssemblyInfo.cs?v=" + ProductVersion + "&r = " + random.Next(5000).ToString());
                StreamReader webRead = new StreamReader(webStream);

                //Check we're updated
                string[] LatestVersionArray = webRead.ReadToEnd().Split(new[] { "AssemblyFileVersion(\"" }, StringSplitOptions.None);
                string LatestVersionNumber = LatestVersionArray[1].Substring(0, LatestVersionArray[1].Length - 4);
                if (ProductVersion != LatestVersionNumber)
                {
                    DialogResult TakeToUpdate = MessageBox.Show("An update is available!\nWould you like to download?", "Alien: Isolation PAK Tool Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (TakeToUpdate == DialogResult.Yes)
                    {
                        Process.Start("https://raw.githubusercontent.com/MattFiler/AlienPAK/master/AlienPAK.exe");
                        Application.Exit();
                    }
                }
            }
            catch { }
            
            //Exit out of the update check always
            this.Close();
        }
    }
}
