using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    /*
     *
     * Our PAK handler.
     * Created by Matt Filer: http://www.mattfiler.co.uk
     * 
     * Intended to support PAK2/TexturePAK/ModelPAK.
     * Potentially will also add the ability to make your own PAK2 archives.
     * Currently a WORK IN PROGRESS.
     * 
    */
    class PAK
    {
        /* --- COMMON PAK --- */
        private string ArchivePath = "";
        private BinaryReader ArchiveFile = null;
        private BinaryReader ArchiveFileBin = null;
        private List<string> FileList = new List<string>();
        private int NumberOfEntries = -1;
        private enum PAKType { PAK2, PAK_TEXTURES, PAK_MODELS, UNRECOGNISED };
        private PAKType Format = PAKType.UNRECOGNISED;

        /* Open a PAK archive */
        public void Open(string FilePath)
        {
            //Open new PAK
            if (ArchiveFile != null) { ArchiveFile.Close(); }
            if (ArchiveFileBin != null) { ArchiveFileBin.Close(); }
            ArchiveFile = new BinaryReader(File.OpenRead(FilePath));

            //Update our info
            ArchivePath = FilePath;
            switch (Path.GetFileName(FilePath))
            {
                case "LEVEL_TEXTURES.ALL.PAK":
                    Format = PAKType.PAK_TEXTURES;
                    break;
                case "LEVEL_MODELS.PAK":
                    Format = PAKType.PAK_MODELS;
                    break;
                default:
                    try
                    {
                        string PAKMagic = "";
                        for (int i = 0; i < 4; i++)
                        {
                            PAKMagic += ArchiveFile.ReadChar();
                        }
                        ArchiveFile.BaseStream.Position = 0;
                        if (PAKMagic == "PAK2")
                        {
                            Format = PAKType.PAK2;
                            break;
                        }
                    }
                    catch { }
                    Format = PAKType.UNRECOGNISED;
                    break;
            }

            //Certain formats have associated BIN files
            switch (Format)
            {
                case PAKType.PAK_TEXTURES:
                    ArchiveFileBin = new BinaryReader(File.OpenRead(FilePath.Substring(0, FilePath.Length - ("LEVEL_TEXTURES.ALL.PAK").Length) + "LEVEL_TEXTURE_HEADERS.ALL.BIN"));
                    break;
                case PAKType.PAK_MODELS:
                    ArchiveFileBin = new BinaryReader(File.OpenRead(FilePath.Substring(0, FilePath.Length - ("LEVEL_MODELS.PAK").Length) + "MODELS_LEVEL.BIN"));
                    break;
            }
        }

        /* Parse a PAK archive */
        public List<string> Parse()
        {
            if (ArchiveFile == null) { return null; }
            
            FileList.Clear();
            FileOffsets.Clear();
            FilePadding.Clear();

            switch (Format)
            {
                case PAKType.PAK2:
                    return ParsePAK2();
                case PAKType.PAK_TEXTURES:
                    return ParseTexturePAK();
                case PAKType.PAK_MODELS:
                    //return ParseModelPAK(); <= Even bigger WIP than textures!
                default:
                    return null;
            }
        }

        /* Get the size of a file within the PAK archive */
        public int GetFileSize(string FileName)
        {
            if (ArchiveFile == null) { return -1; }

            switch (Format)
            {
                case PAKType.PAK2:
                    return FileSizePAK2(FileName);
                case PAKType.PAK_TEXTURES:
                    return FileSizeTexturePAK(FileName);
                case PAKType.PAK_MODELS:
                    return FileSizeModelPAK(FileName);
                default:
                    return -1;
            }
        }

        /* Export from a PAK archive */
        public bool ExportFile(string FileName, string ExportPath)
        {
            if (ArchiveFile == null) { return false; }

            switch (Format)
            {
                case PAKType.PAK2:
                    return ExportFilePAK2(FileName, ExportPath);
                case PAKType.PAK_TEXTURES:
                    return ExportFileTexturePAK(FileName, ExportPath);
                case PAKType.PAK_MODELS:
                    return ExportFileModelPAK(FileName, ExportPath);
                default:
                    return false;
            }
        }

        /* Import to a PAK archive */
        public bool ImportFile(string FileName, string ImportPath)
        {
            if (ArchiveFile == null) { return false; }

            switch (Format)
            {
                case PAKType.PAK2:
                    return ImportFilePAK2(FileName, ImportPath);
                case PAKType.PAK_TEXTURES:
                    return ImportFileTexturePAK(FileName, ImportPath);
                case PAKType.PAK_MODELS:
                    return ImportFileModelPAK(FileName, ImportPath);
                default:
                    return false;
            }
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
        private List<int> FileOffsets = new List<int>();
        private List<int> FilePadding = new List<int>();
        private int OffsetListBegin = -1;
        private int DataSize = -1;

        /* Parse a PAK2 archive */
        private List<string> ParsePAK2()
        {
            //Read the header info
            ArchiveFile.BaseStream.Position += 4; //Skip magic
            OffsetListBegin = ArchiveFile.ReadInt32() + 16;
            NumberOfEntries = ArchiveFile.ReadInt32();
            DataSize = ArchiveFile.ReadInt32();

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

        /* Get a file's size from the PAK2 archive */
        private int FileSizePAK2(string FileName)
        {
            int FileIndex = GetFileIndex(FileName);
            ArchiveFile.BaseStream.Position = FileOffsets[FileIndex] + FilePadding[FileIndex];
            return FileOffsets.ElementAt(FileIndex + 1) - (int)ArchiveFile.BaseStream.Position;
        }

        /* Export a file from the PAK2 archive */
        private bool ExportFilePAK2(string FileName, string ExportPath)
        {
            try
            {
                //Update reader position and work out file size
                int FileLength = FileSizePAK2(FileName);

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
        private bool ImportFilePAK2(string FileName, string ImportPath)
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
                int OldNextPadding = (FileIndex != FilePadding.Count-1) ? FilePadding.ElementAt(FileIndex + 1) : 0;
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
                    if (i == 0 && FileIndex != NumberOfEntries-1)
                    {
                        //Correct the byte alignment for first trailing file (if we have one)
                        while ((Offset + NewNextPadding) % 4 != 0)
                        {
                            NewNextPadding += 1;
                        }
                        FilePadding[FileIndex + 1] = NewNextPadding;
                    }
                    else
                    {
                        //Flow new padding over to each following file
                        Offset += (NewNextPadding - OldNextPadding);
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
                byte[] ArchivePt2 = new byte[FileOffsets.ElementAt(FileOffsets.Count - 1) - FileOffsets.ElementAt(FileIndex + 1) + (NewNextPadding - OldNextPadding)];
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
                byte[] NewArchive = new byte[FileOffsets.ElementAt(FileOffsets.Count - 1) - OldLength + NewLength + (NewNextPadding - OldNextPadding)];
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
                
                try
                {
                    //Write out the new archive
                    BinaryWriter ArchiveFileWrite = new BinaryWriter(File.OpenWrite(ArchivePath));
                    ArchiveFileWrite.Write(NewArchive);
                    ArchiveFileWrite.Close();
                }
                catch
                {
                    //File is probably in-use by the game, re-open for reading and exit as fail
                    Open(ArchivePath);
                    return false;
                }

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


        /* --- TEXTURE PAK --- */
        int HeaderListBegin = -1;
        int NumberOfEntriesPAK = -1;
        int NumberOfEntriesBIN = -1;
        List<TEX4> TextureEntries = new List<TEX4>();

        /* Parse the file listing for a texture PAK */
        private List<string> ParseTexturePAK()
        {
            //Read the header info from the BIN
            ArchiveFileBin.BaseStream.Position += 4; //Skip unused value (version?)
            NumberOfEntriesBIN = ArchiveFileBin.ReadInt32();
            HeaderListBegin = ArchiveFileBin.ReadInt32();

            //Read all file names from BIN
            for (int i = 0; i < NumberOfEntriesBIN; i++)
            {
                string ThisFileName = "";
                for (byte b; (b = ArchiveFileBin.ReadByte()) != 0x00;)
                {
                    ThisFileName += (char)b;
                }
                if (Path.GetExtension(ThisFileName).ToUpper() != ".DDS")
                {
                    ThisFileName += ".dds";
                }
                FileList.Add(ThisFileName);
            }
            
            //Read the texture headers from the BIN
            ArchiveFileBin.BaseStream.Position = HeaderListBegin + 12;
            for (int i = 0; i < NumberOfEntriesBIN; i++)
            {
                TEX4 TextureEntry = new TEX4();
                ArchiveFileBin.BaseStream.Position += 4; //Skip magic
                TextureEntry.TextureFormat = (TextureFormats)ArchiveFileBin.ReadInt32();
                ArchiveFileBin.BaseStream.Position += 8; //Skip unknowns
                TextureEntry.Texture_V1.Width = ArchiveFileBin.ReadInt16();
                TextureEntry.Texture_V1.Height = ArchiveFileBin.ReadInt16();
                ArchiveFileBin.BaseStream.Position += 2; //Skip unknown
                TextureEntry.Texture_V2.Width = ArchiveFileBin.ReadInt16();
                TextureEntry.Texture_V2.Height = ArchiveFileBin.ReadInt16();
                ArchiveFileBin.BaseStream.Position += 22; //Skip unknowns
                TextureEntry.FileName = FileList[i];
                TextureEntries.Add(TextureEntry);
            }

            //Read the header info from the PAK
            BigEndianUtils BigEndian = new BigEndianUtils();
            ArchiveFile.BaseStream.Position += 12; //Skip unknowns
            NumberOfEntriesPAK = BigEndian.ReadInt32(ArchiveFile);
            ArchiveFile.BaseStream.Position += 16; //Skip unknowns

            //Read the texture headers from the PAK
            int OffsetTracker = (NumberOfEntriesPAK * 48) + 32;
            for (int i = 0; i < NumberOfEntriesPAK; i++)
            {
                //Pull the size info
                int EntrySize = 0;
                ArchiveFile.BaseStream.Position += 8; //Skip unknowns
                EntrySize = BigEndian.ReadInt32(ArchiveFile);
                if (EntrySize != BigEndian.ReadInt32(ArchiveFile)) { continue; }
                ArchiveFile.BaseStream.Position += 18; //Skip unknowns

                //Pull the index info and use that to find the texture entry
                TEX4 TextureEntry = TextureEntries[BigEndian.ReadInt16(ArchiveFile)];

                //Assign size info to the entry with the calculated offset
                if (!TextureEntry.Texture_V1.Saved)
                {
                    TextureEntry.Texture_V1.StartPos = OffsetTracker;
                    TextureEntry.Texture_V1.Length = EntrySize;
                    TextureEntry.Texture_V1.Saved = true;
                }
                else
                {
                    TextureEntry.Texture_V2.StartPos = OffsetTracker;
                    TextureEntry.Texture_V2.Length = EntrySize;
                    TextureEntry.Texture_V2.Saved = true;
                }
                OffsetTracker += EntrySize;

                //Skip the rest of the header
                ArchiveFile.BaseStream.Position += 12; //Skip unknowns
            }

            return FileList;
        }

        /* Get a file's size from the texture PAK */
        private int FileSizeTexturePAK(string FileName)
        {
            int FileIndex = GetFileIndex(FileName);
            if (TextureEntries[FileIndex].Texture_V2.Saved)
            {
                return TextureEntries[FileIndex].Texture_V2.Length + 148;
            }
            //Fallback to V1 if this texture has no V2
            else if (TextureEntries[FileIndex].Texture_V1.Saved)
            {
                return TextureEntries[FileIndex].Texture_V1.Length + 148;
            }
            return -1; //Should never get here
        }

        /* Export a file from the texture PAK */
        private bool ExportFileTexturePAK(string FileName, string ExportPath)
        {
            try
            {
                //Get the texture index
                int FileIndex = GetFileIndex(FileName);

                //Get the biggest texture part stored
                TEX4_Part TexturePart;
                if (TextureEntries[FileIndex].Texture_V2.Saved)
                {
                    TexturePart = TextureEntries[FileIndex].Texture_V2;
                }
                else if (TextureEntries[FileIndex].Texture_V1.Saved)
                {
                    TexturePart = TextureEntries[FileIndex].Texture_V1;
                }
                else
                {
                    return false;
                }

                //Pull the texture part content from the archive
                ArchiveFile.BaseStream.Position = TexturePart.StartPos;
                byte[] TexturePartContent = ArchiveFile.ReadBytes(TexturePart.Length);

                //Generate a DDS header based on the tex4's information
                DDSWriter TextureOutput;
                bool FailsafeSave = false;
                switch (TextureEntries[FileIndex].TextureFormat)
                {
                    case TextureFormats.DXGI_FORMAT_BC5_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, DDSWriter.DDS_Format.ATI2N);
                        break;
                    case TextureFormats.DXGI_FORMAT_BC1_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, DDSWriter.DDS_Format.Dxt1);
                        break;
                    case TextureFormats.DXGI_FORMAT_BC3_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, DDSWriter.DDS_Format.Dxt5);
                        break;
                    case TextureFormats.DXGI_FORMAT_B8G8R8A8_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, DDSWriter.DDS_Format.UNCOMPRESSED_GENERAL);
                        break;
                    case TextureFormats.DXGI_FORMAT_BC7_UNORM:
                    default:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height);
                        FailsafeSave = true;
                        break;
                }
                ExportPath += ".dds";

                //Save out the part
                if (FailsafeSave)
                {
                    TextureOutput.SaveCrude(ExportPath);
                    return true;
                }
                TextureOutput.Save(ExportPath);
                return true;
            }
            catch (Exception e)
            {
                string error = e.ToString();
                return false;
            }
        }

        /* Import a file to the texture PAK */
        private bool ImportFileTexturePAK(string FileName, string ExportPath)
        {
            //WIP
            return false;
        }


        /* --- MODEL PAK --- */
        int TableCountPt1 = -1;
        int TableCountPt2 = -1;
        int FilenameListEnd = -1;

        /* Parse the file listing for a model PAK */
        private List<string> ParseModelPAK()
        {
            //Read the header info from BIN
            ArchiveFileBin.BaseStream.Position += 4; //Skip magic
            TableCountPt2 = ArchiveFileBin.ReadInt32();
            ArchiveFileBin.BaseStream.Position += 4; //Skip unknown
            TableCountPt1 = ArchiveFileBin.ReadInt32();

            //Skip past table 1
            for (int i = 0; i < TableCountPt1; i++)
            {
                byte ThisByte = 0x00;
                while (ThisByte != 0xFF)
                {
                    ThisByte = ArchiveFileBin.ReadByte();
                }
            }
            ArchiveFileBin.BaseStream.Position += 23;

            //Read file list info
            FilenameListEnd = ArchiveFileBin.ReadInt32();

            //Read all file names
            string ThisFileName = "";
            for (int i = 0; i < FilenameListEnd-4; i++)
            {
                byte ThisByte = ArchiveFileBin.ReadByte();
                if (ThisByte == 0x00)
                {
                    FileList.Add(ThisFileName);
                    ThisFileName = "";
                    continue;
                }
                ThisFileName += (char)ThisByte;
            }

            return FileList;
        }

        /* Get a file's size from the model PAK */
        private int FileSizeModelPAK(string FileName)
        {
            //WIP
            return -1;
        }

        /* Export a file from the model PAK */
        private bool ExportFileModelPAK(string FileName, string ExportPath)
        {
            //WIP
            return false;
        }

        /* Import a file to the model PAK */
        private bool ImportFileModelPAK(string FileName, string ExportPath)
        {
            //WIP
            return false;
        }
    }
}
