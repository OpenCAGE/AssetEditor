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
    public partial class Form1 : Form
    {
        BinaryReader reader = null;
        List<int> FileOffsets = new List<int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Filter = "Alien: Isolation PAK|*.PAK";
            if (filePicker.ShowDialog() == DialogResult.OK)
            {
                //Open PAK and validate it's PAK2
                OpenPAK(filePicker.FileName);
                if (!ValidatePAK())
                {
                    MessageBox.Show("The selected PAK is currently unsupported.", "Unsupported PAK.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Parse the PAK2
                ParsePAK2();
            }
        }

        private void OpenPAK(string filepath)
        {
            //Open new PAK
            if (reader != null) { reader.Close(); }
            reader = new BinaryReader(File.Open(filepath, FileMode.Open));
        }

        private bool ValidatePAK()
        {
            //Be nice and maintain the reader position :)
            long ReaderPos = reader.BaseStream.Position;
            reader.BaseStream.Position = 0;

            //Check header magic
            string FileMagic = "";
            for (int i = 0; i < 4; i++)
            {
                FileMagic += reader.ReadChar();
            }

            //If we aren't dealing with a PAK2 file, it's currently unsupported
            reader.BaseStream.Position = ReaderPos;
            return (FileMagic == "PAK2");
        }

        private void ParsePAK2()
        {
            if (reader == null) { return; }

            //Read the header info
            reader.BaseStream.Position += 4; //Skip magic
            int OffsetListBegin = reader.ReadInt32() + 16;
            int NumberOfEntries = reader.ReadInt32();
            int DataSize = reader.ReadInt32();
            
            //Read all file names
            for (int i = 0; i < NumberOfEntries; i++)
            {
                string ThisFileName = "";
                for (byte b; (b = reader.ReadByte()) != 0x00;)
                {
                    ThisFileName += (char)b;
                }
                FileList.Items.Add(ThisFileName);
            }

            //Read all file offsets
            reader.BaseStream.Position = OffsetListBegin;
            FileOffsets.Add(OffsetListBegin + (NumberOfEntries * DataSize));
            for (int i = 0; i < NumberOfEntries; i++)
            {
                FileOffsets.Add(reader.ReadInt32());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //For testing purposes
            //OpenPAK(@"E:\Program Files\Steam\steamapps\common\Alien Isolation\DATA\UI.PAK");
            //ParsePAK2();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (FileList.SelectedIndex == -1)
            {
                return;
            }

            //Let the user decide where to save
            SaveFileDialog filePicker = new SaveFileDialog();
            filePicker.Filter = "Exported File|*" + Path.GetExtension(FileList.SelectedItem.ToString());
            filePicker.FileName = Path.GetFileName(FileList.SelectedItem.ToString());
            if (filePicker.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            //Update reader position and work out file size
            reader.BaseStream.Position = FileOffsets.ElementAt(FileList.SelectedIndex);
            int FileLength = FileOffsets.ElementAt(FileList.SelectedIndex + 1) - FileOffsets.ElementAt(FileList.SelectedIndex);

            //Grab the file's contents (this can probably be optimised!)
            List<byte> FileExport = new List<byte>();
            for (int i = 0; i < FileLength; i++)
            {
                FileExport.Add(reader.ReadByte());
            }

            //Write the file's contents out
            File.WriteAllBytes(filePicker.FileName, FileExport.ToArray());
        }
    }
}
