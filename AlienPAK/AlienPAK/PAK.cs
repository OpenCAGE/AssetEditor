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
        private string ArchivePath = "";
        private BinaryReader ArchiveFile = null;
        private List<string> FileList = new List<string>();
        private List<int> FileOffsets = new List<int>();
        private List<int> FilePadding = new List<int>();
        int OffsetListBegin = -1;
        int NumberOfEntries = -1;
        int DataSize = -1;

        //External info
        public enum PAKType { PAK2, UNRECOGNISED };
        public PAKType Format { get; private set; }

        /* Open a PAK archive */
        public void Open(string FilePath)
        {
            //Open new PAK
            if (ArchiveFile != null) { ArchiveFile.Close(); }
            ArchiveFile = new BinaryReader(File.Open(FilePath, FileMode.Open));

            //Update our info
            ArchivePath = FilePath;
            Format = GetType();
        }
        
        /* Check the format of the opened PAK archive */
        new private PAKType GetType()
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

        /* Get the PAK index of the file by name */
        private int GetFileIndex(string FileName)
        {
            //Get index
            for (int i = 0; i < FileList.Count; i++)
            {
                if (FileList[i] == FileName)
                {
                    return i;
                }
            }

            //Couldn't find - sometimes CA use "\" instead of "/"... try that
            FileName = FileName.Replace('/', '\\');
            for (int i = 0; i < FileList.Count; i++)
            {
                if (FileList[i] == FileName)
                {
                    return i;
                }
            }

            //Failed to find
            return -1;
        }

        /* --- PAK2 --- */

        /* Parse a PAK2 archive */
        public List<string> ParsePAK2()
        {
            if (ArchiveFile == null) { return null; }

            //Read the header info
            ArchiveFile.BaseStream.Position += 4; //Skip magic
            OffsetListBegin = ArchiveFile.ReadInt32() + 16;
            NumberOfEntries = ArchiveFile.ReadInt32();
            DataSize = ArchiveFile.ReadInt32();

            //Reset global lists
            FileList.Clear();
            FileOffsets.Clear();
            FilePadding.Clear();

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
            List<string> debug = new List<string>();
            for (int i = 0; i < NumberOfEntries; i++)
            {
                FileOffsets.Add(ArchiveFile.ReadInt32());
                debug.Add(FileOffsets.ElementAt(i).ToString());
            }

            //Hacky way to store byte alignment values
            for (int i = 0; i < NumberOfEntries; i++)
            {
                ArchiveFile.BaseStream.Position = FileOffsets[i];
                FilePadding.Add(0);
                for (int x = 0; x < DataSize + 1; x++) 
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
        public bool ExportFilePAK2(string FileName, string ExportPath)
        {
            try
            {
                //Update reader position and work out file size
                int FileIndex = GetFileIndex(FileName);
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
            catch (Exception e)
            {
                string error = e.ToString();
                return false;
            }
        }

        /* Import a file to the PAK2 archive */
        public bool ImportFilePAK2(string FileName, string ImportPath)
        {
            try
            {
                //Open PAK for writing, and read contents of import file
                BinaryReader ImportFile = new BinaryReader(File.OpenRead(ImportPath));

                //Old/new file lengths
                int FileIndex = GetFileIndex(FileName);
                int OldLength = FileOffsets.ElementAt(FileIndex + 1) - FileOffsets.ElementAt(FileIndex);
                int NewLength = (int)ImportFile.BaseStream.Length + FilePadding.ElementAt(FileIndex);

                //Old/new "padding" (next file in sequence's byte alignment)
                int OldNextPadding = FilePadding.ElementAt(FileIndex + 1);
                int NewNextPadding = 0; //This will be set later

                //Grab the first section of the archive
                ArchiveFile.BaseStream.Position = 0;
                byte[] ArchivePt1 = new byte[FileOffsets.ElementAt(FileIndex)];
                for (int i = 0; i < ArchivePt1.Length; i++)
                {
                    ArchivePt1[i] = ArchiveFile.ReadByte();
                }

                //Update file offset information
                for (int i = 0; i < (NumberOfEntries - FileIndex); i++)
                {
                    //Read original offset
                    byte[] OffsetRaw = new byte[DataSize];
                    for (int x = 0; x < OffsetRaw.Length; x++)
                    {
                        OffsetRaw[x] = ArchivePt1[OffsetListBegin + ((FileIndex + i) * DataSize) + x]; //+1?
                    }

                    //Update original offset
                    int Offset = BitConverter.ToInt32(OffsetRaw, 0);
                    Offset = Offset - OldLength + NewLength;
                    if (i == 0)
                    {
                        //Correct the byte alignment for first trailing file
                        while (Offset % 4 != 0)
                        {
                            Offset += 1;
                            NewNextPadding += 1;
                        }
                        FilePadding[FileIndex + 1] = NewNextPadding;
                    }
                    OffsetRaw = BitConverter.GetBytes(Offset);

                    //Write back new offset
                    for (int x = 0; x < OffsetRaw.Length; x++)
                    {
                        ArchivePt1[OffsetListBegin + ((FileIndex + i) * DataSize) + x] = OffsetRaw[x]; //+1?
                    }
                }

                //Grab the second half of the archive after the file, and correct the byte offset
                ArchiveFile.BaseStream.Position = FileOffsets.ElementAt(FileIndex + 1);
                byte[] ArchivePt2 = new byte[FileOffsets.ElementAt(FileOffsets.Count - 1) - FileOffsets.ElementAt(FileIndex + 1) + (OldNextPadding - NewNextPadding)];
                ArchiveFile.BaseStream.Position += OldNextPadding;
                for (int i = 0; i < NewNextPadding; i++)
                {
                    ArchivePt2[i] = 0x00;
                }
                for (int i = NewNextPadding; i < ArchivePt2.Length; i++)
                {
                    ArchivePt2[i] = ArchiveFile.ReadByte();
                }

                //Compose new archive from the two old parts and the new file stuck in the middle
                byte[] NewArchive = new byte[FileOffsets.ElementAt(FileOffsets.Count - 1) - OldLength + NewLength];
                int ArchiveIndex = 0;
                for (int i = 0; i < ArchivePt1.Length; i++)
                {
                    NewArchive[ArchiveIndex] = ArchivePt1[i];
                    ArchiveIndex++;
                }
                for (int i = 0; i < FilePadding.ElementAt(FileIndex); i++)
                {
                    NewArchive[ArchiveIndex] = 0x00;
                    ArchiveIndex++;
                }
                for (int i = 0; i < (int)ImportFile.BaseStream.Length; i++)
                {
                    NewArchive[ArchiveIndex] = ImportFile.ReadByte();
                    ArchiveIndex++;
                }
                for (int i = 0; i < ArchivePt2.Length; i++)
                {
                    NewArchive[ArchiveIndex] = ArchivePt2[i];
                    ArchiveIndex++;
                }

                //Dispose of the old archive
                ArchiveFile.Close();
                File.Delete(ArchivePath);
                
                //Write out the new archive
                BinaryWriter ArchiveFileWrite = new BinaryWriter(File.OpenWrite(ArchivePath));
                ArchiveFileWrite.Write(NewArchive);
                ArchiveFileWrite.Close();

                //Reload the archive for us
                Open(ArchivePath);
                ParsePAK2();

                return true;
            }
            catch (Exception e)
            {
                string error = e.ToString();
                return false;
            }
        }
    }
}
