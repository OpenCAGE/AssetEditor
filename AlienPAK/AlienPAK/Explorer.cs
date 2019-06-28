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
    public partial class Explorer : Form
    {
        PAK AlienPAK = new PAK();

        public Explorer()
        {
            InitializeComponent();
        }

        /* Open a PAK and populate the GUI */
        private void OpenFileAndPopulateGUI(string filename)
        {
            //Open PAK
            AlienPAK.Open(filename);

            //Parse the PAK depending on its format
            List<string> ParsedFiles = new List<string>();
            switch (AlienPAK.Format)
            {
                case PAK.PAKType.PAK2:
                    ParsedFiles = AlienPAK.ParsePAK2();
                    break;
                case PAK.PAKType.UNRECOGNISED:
                    MessageBox.Show("The selected PAK is currently unsupported.", "Unsupported PAK.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            //Populate the GUI with the files found within the archive
            FileList.Items.Clear();
            foreach (string FileName in ParsedFiles)
            {
                FileList.Items.Add(FileName);
            }
        }

        /* Form loads */
        private void Form1_Load(object sender, EventArgs e)
        {
            //For testing purposes
            OpenFileAndPopulateGUI(@"E:\Program Files\Steam\steamapps\common\Alien Isolation\DATA\UI.PAK");
        }

        /* User requests to open a PAK */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Allow selection of a PAK from filepicker, then open
            OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Filter = "Alien: Isolation PAK|*.PAK";
            if (filePicker.ShowDialog() == DialogResult.OK)
            {
                OpenFileAndPopulateGUI(filePicker.FileName);
            }
        }

        /* User requests to export a file from the PAK */
        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (FileList.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a file from the list.", "No file selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Let the user decide where to save, then save
            SaveFileDialog filePicker = new SaveFileDialog();
            filePicker.Filter = "Exported File|*" + Path.GetExtension(FileList.SelectedItem.ToString());
            filePicker.FileName = Path.GetFileName(FileList.SelectedItem.ToString());
            if (filePicker.ShowDialog() == DialogResult.OK)
            {
                AlienPAK.ExportFilePAK2(FileList.SelectedIndex, filePicker.FileName);
            }
        }
    }
}
