using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class PAK
    {
        //Internal info
        private BinaryReader ArchiveFile = null;
        private List<int> FileOffsets = new List<int>();
        private List<int> FilePadding = new List<int>();

        //External info
        public enum PAKType { PAK2, UNRECOGNISED };
        public PAKType Format { get; private set; }

        /* Open a PAK archive */
        public void Open(string filepath)
        {
            //Open new PAK
            if (ArchiveFile != null) { ArchiveFile.Close(); }
            ArchiveFile = new BinaryReader(File.Open(filepath, FileMode.Open));

            //Update our format info
            Format = Type();
        }
        
        /* Check the format of the opened PAK archive */
        private PAKType Type()
        {
            //Be nice and maintain the reader position :)
            long ReaderPos = ArchiveFile.BaseStream.Position;
            ArchiveFile.BaseStream.Position = 0;

            //Check header magic
            string FileMagic = "";
            for (int i = 0; i < 4; i++)
            {
                FileMagic += ArchiveFile.ReadChar();
            }

            //If we aren't dealing with a PAK2 file, it's currently unsupported
            ArchiveFile.BaseStream.Position = ReaderPos;
            return (FileMagic == "PAK2") ? PAKType.PAK2 : PAKType.UNRECOGNISED;
        }

        /* --- PAK2 --- */

        /* Parse a PAK2 archive */
        public List<string> ParsePAK2()
        {
            if (ArchiveFile == null) { return null; }
            List<string> FileList = new List<string>();

            //Read the header info
            ArchiveFile.BaseStream.Position += 4; //Skip magic
            int OffsetListBegin = ArchiveFile.ReadInt32() + 16;
            int NumberOfEntries = ArchiveFile.ReadInt32();
            int DataSize = ArchiveFile.ReadInt32();

            //Read all file names
            for (int i = 0; i < NumberOfEntries; i++)
            {
                string ThisFileName = "";
                for (byte b; (b = ArchiveFile.ReadByte()) != 0x00;)
                {
                    ThisFileName += (char)b;
                }
                FileList.Add(ThisFileName);
            }

            //Read all file offsets
            ArchiveFile.BaseStream.Position = OffsetListBegin;
            FileOffsets.Add(OffsetListBegin + (NumberOfEntries * DataSize));
            for (int i = 0; i < NumberOfEntries; i++)
            {
                FileOffsets.Add(ArchiveFile.ReadInt32());
            }

            //Check for padding at each offset (odd PAK2 bug(?))
            for (int i = 0; i < NumberOfEntries; i++)
            {
                ArchiveFile.BaseStream.Position = FileOffsets[i];
                FilePadding.Add(0);
                for (int x = 0; x < 50; x++) //Should never pass 50 (probably nowhere near actually)
                {
                    if (ArchiveFile.ReadByte() == 0x00)
                    {
                        FilePadding[i] += 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return FileList;
        }

        /* Export a file from the PAK2 archive */
        public bool ExportFilePAK2(int FileIndex, string ExportPath)
        {
            try
            {
                //Update reader position and work out file size
                ArchiveFile.BaseStream.Position = FileOffsets[FileIndex] + FilePadding[FileIndex];
                int FileLength = FileOffsets.ElementAt(FileIndex + 1) - (int)ArchiveFile.BaseStream.Position;

                //Grab the file's contents (this can probably be optimised!)
                List<byte> FileExport = new List<byte>();
                for (int i = 0; i < FileLength; i++)
                {
                    FileExport.Add(ArchiveFile.ReadByte());
                }

                //Write the file's contents out
                File.WriteAllBytes(ExportPath, FileExport.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
