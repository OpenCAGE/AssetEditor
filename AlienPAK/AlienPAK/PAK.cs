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
     * Intended to support various PAK formats for Alien: Isolation (CATHODE).
     * Potentially will also add the ability to make your own PAK2 archives.
     * Currently a WORK IN PROGRESS.
     * 
    */
    class PAK
    {
        ToolOptionsHandler ToolSettings = new ToolOptionsHandler();

        /* --- COMMON PAK --- */
        private string ArchivePath = "";
        private string ArchivePathBin = "";
        private BinaryReader ArchiveFile = null;
        private BinaryReader ArchiveFileBin = null;
        private List<string> FileList = new List<string>();
        private int NumberOfEntries = -1;
        private enum PAKType { PAK2, PAK_TEXTURES, PAK_MODELS, PAK_SCRIPTS, PAK_MATERIALMAPS, UNRECOGNISED };
        private PAKType Format = PAKType.UNRECOGNISED;
        public enum PAKReturnType { FAILED_UNKNOWN, FAILED_UNSUPPORTED, SUCCESS, FAILED_LOGIC_ERROR, FAILED_FILE_IN_USE }
        public string LatestError = "";

        /* Open a PAK archive */
        public void Open(string FilePath)
        {
            //Open new PAK
            if (ArchiveFile != null) { ArchiveFile.Close(); }
            if (ArchiveFileBin != null) { ArchiveFileBin.Close(); }
            ArchiveFile = new BinaryReader(File.OpenRead(FilePath));

            //Update our info
            ArchivePath = FilePath;
            ArchivePathBin = "";
            string FileName = Path.GetFileName(FilePath);
            switch (FileName)
            {
                case "GLOBAL_TEXTURES.ALL.PAK":
                case "LEVEL_TEXTURES.ALL.PAK":
                    Format = PAKType.PAK_TEXTURES;
                    break;
                case "GLOBAL_MODELS.PAK":
                case "LEVEL_MODELS.PAK":
                    Format = PAKType.PAK_MODELS;
                    break;
                case "MATERIAL_MAPPINGS.PAK":
                    Format = PAKType.PAK_MATERIALMAPS;
                    break;
                case "COMMANDS.PAK":
                    Format = PAKType.PAK_SCRIPTS;
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
                    if (FileName.Substring(0, 5).ToUpper() == "LEVEL")
                    {
                        ArchivePathBin = FilePath.Substring(0, FilePath.Length - FileName.Length) + "LEVEL_TEXTURE_HEADERS.ALL.BIN";
                        ArchiveFileBin = new BinaryReader(File.OpenRead(ArchivePathBin));
                    }
                    else
                    {
                        ArchivePathBin = FilePath.Substring(0, FilePath.Length - FileName.Length) + "GLOBAL_TEXTURES_HEADERS.ALL.BIN";
                        ArchiveFileBin = new BinaryReader(File.OpenRead(ArchivePathBin));
                    }
                    break;
                case PAKType.PAK_MODELS:
                    ArchivePathBin = FilePath.Substring(0, FilePath.Length - FileName.Length) + "MODELS_" + FileName.Substring(0, FileName.Length - 11) + ".BIN";
                    ArchiveFileBin = new BinaryReader(File.OpenRead(ArchivePathBin));
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
            TextureEntries.Clear();
            MaterialMappingEntries.Clear();
            CommandsEntries.Clear();
            ModelEntries.Clear();

            switch (Format)
            {
                case PAKType.PAK2:
                    return ParsePAK2();
                case PAKType.PAK_TEXTURES:
                    return ParseTexturePAK();
                case PAKType.PAK_MODELS:
                    return ParseModelPAK();
                case PAKType.PAK_SCRIPTS:
                    return ParseCommandsPAK();
                case PAKType.PAK_MATERIALMAPS:
                    return ParseMaterialMappingsPAK();
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
                case PAKType.PAK_SCRIPTS:
                    return FileSizeCommandsPAK(FileName);
                case PAKType.PAK_MATERIALMAPS:
                    return FileSizeMaterialMappingsPAK(FileName);
                default:
                    return -1;
            }
        }

        /* Export from a PAK archive */
        public PAKReturnType ExportFile(string FileName, string ExportPath)
        {
            if (ArchiveFile == null) { return PAKReturnType.FAILED_LOGIC_ERROR; }

            switch (Format)
            {
                case PAKType.PAK2:
                    return ExportFilePAK2(FileName, ExportPath);
                case PAKType.PAK_TEXTURES:
                    return ExportFileTexturePAK(FileName, ExportPath);
                case PAKType.PAK_MODELS:
                    return ExportFileModelPAK(FileName, ExportPath);
                case PAKType.PAK_SCRIPTS:
                    return ExportFileCommandsPAK(FileName, ExportPath);
                case PAKType.PAK_MATERIALMAPS:
                    return ExportFileMaterialMappingsPAK(FileName, ExportPath);
                default:
                    return PAKReturnType.FAILED_UNSUPPORTED;
            }
        }

        /* Import to a PAK archive */
        public PAKReturnType ImportFile(string FileName, string ImportPath)
        {
            if (ArchiveFile == null) { return PAKReturnType.FAILED_LOGIC_ERROR; }

            switch (Format)
            {
                case PAKType.PAK2:
                    return ImportFilePAK2(FileName, ImportPath);
                case PAKType.PAK_TEXTURES:
                    return ImportFileTexturePAK(FileName, ImportPath);
                case PAKType.PAK_MODELS:
                    return ImportFileModelPAK(FileName, ImportPath);
                case PAKType.PAK_SCRIPTS:
                    return ImportFileCommandsPAK(FileName, ImportPath);
                case PAKType.PAK_MATERIALMAPS:
                    return ImportFileMaterialMappingsPAK(FileName, ImportPath);
                default:
                    return PAKReturnType.FAILED_UNSUPPORTED;
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
            if (FileIndex == -1) { return -1; }

            ArchiveFile.BaseStream.Position = FileOffsets[FileIndex] + FilePadding[FileIndex];
            return FileOffsets.ElementAt(FileIndex + 1) - (int)ArchiveFile.BaseStream.Position;
        }

        /* Export a file from the PAK2 archive */
        private PAKReturnType ExportFilePAK2(string FileName, string ExportPath)
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
                return PAKReturnType.SUCCESS;
            }
            catch (Exception e)
            {
                LatestError = e.ToString();
                return PAKReturnType.FAILED_UNKNOWN;
            }
        }

        /* Import a file to the PAK2 archive */
        private PAKReturnType ImportFilePAK2(string FileName, string ImportPath)
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
                int OldNextPadding = (FileIndex != FilePadding.Count - 1) ? FilePadding.ElementAt(FileIndex + 1) : 0;
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
                    if (i == 0 && FileIndex != NumberOfEntries - 1)
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

                ArchiveFile.Close();
                try
                {
                    //Write out the new archive
                    BinaryWriter ArchiveFileWrite = new BinaryWriter(File.OpenWrite(ArchivePath));
                    ArchiveFileWrite.BaseStream.SetLength(0);
                    ArchiveFileWrite.Write(NewArchive);
                    ArchiveFileWrite.Close();
                }
                catch
                {
                    //File is probably in-use by the game, re-open for reading and exit as fail
                    Open(ArchivePath);
                    return PAKReturnType.FAILED_FILE_IN_USE;
                }

                //Reload the archive for us
                Open(ArchivePath);
                ParsePAK2();

                return PAKReturnType.SUCCESS;
            }
            catch (Exception e)
            {
                LatestError = e.ToString();
                return PAKReturnType.FAILED_UNKNOWN;
            }
        }


        /* --- TEXTURE PAK --- */
        int HeaderListBeginBIN = -1;
        int HeaderListEndPAK = -1;
        int NumberOfEntriesPAK = -1;
        int NumberOfEntriesBIN = -1;
        List<TEX4> TextureEntries = new List<TEX4>();

        /* Parse the file listing for a texture PAK */
        private List<string> ParseTexturePAK()
        {
            //Read the header info from the BIN
            ArchiveFileBin.BaseStream.Position += 4; //Skip unused value (version?)
            NumberOfEntriesBIN = ArchiveFileBin.ReadInt32();
            HeaderListBeginBIN = ArchiveFileBin.ReadInt32();

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
            ArchiveFileBin.BaseStream.Position = HeaderListBeginBIN + 12;
            for (int i = 0; i < NumberOfEntriesBIN; i++)
            {
                int HeaderPosition = (int)ArchiveFileBin.BaseStream.Position;
                TEX4 TextureEntry = new TEX4();

                ArchiveFileBin.BaseStream.Position += 4; //Skip magic
                TextureEntry.Format = (TextureFormat)ArchiveFileBin.ReadInt32();
                ArchiveFileBin.BaseStream.Position += 4; //Skip V2 length
                ArchiveFileBin.BaseStream.Position += 4; //Skip V1 length
                TextureEntry.Texture_V1.Width = ArchiveFileBin.ReadInt16();
                TextureEntry.Texture_V1.Height = ArchiveFileBin.ReadInt16();
                ArchiveFileBin.BaseStream.Position += 2; //Skip unknown
                TextureEntry.Texture_V2.Width = ArchiveFileBin.ReadInt16();
                TextureEntry.Texture_V2.Height = ArchiveFileBin.ReadInt16();
                ArchiveFileBin.BaseStream.Position += 22; //Skip unknowns
                TextureEntry.FileName = FileList[i];
                TextureEntry.HeaderPos = HeaderPosition;

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
                //Header indexes are out of order, so optimise replacements by saving position
                int HeaderPosition = (int)ArchiveFile.BaseStream.Position;

                //Pull the size info
                ArchiveFile.BaseStream.Position += 8; //Skip unknowns
                int EntrySize = BigEndian.ReadInt32(ArchiveFile);
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
                    TextureEntry.Texture_V1.HeaderPos = HeaderPosition;
                }
                else
                {
                    TextureEntry.Texture_V2.StartPos = OffsetTracker;
                    TextureEntry.Texture_V2.Length = EntrySize;
                    TextureEntry.Texture_V2.Saved = true;
                    TextureEntry.Texture_V2.HeaderPos = HeaderPosition;
                }
                OffsetTracker += EntrySize;

                //Skip the rest of the header
                ArchiveFile.BaseStream.Position += 12; //Skip unknowns
            }
            HeaderListEndPAK = (int)ArchiveFile.BaseStream.Position;
            
            //TESTING CODE FOR MAT LINK PROJECT - REMOVE BEFORE PUSHING
            /*
            int index = 0;
            foreach (TEX4 texture_test in TextureEntries)
            {
                if (Path.GetFileNameWithoutExtension(texture_test.FileName) == "graffitti_13" || Path.GetFileNameWithoutExtension(texture_test.FileName) == "graffitti_13.tga")
                {
                    string stop_here = "";
                }
                index++;
            }
            */
            //END OF TEST CODE

            return FileList;
        }

        /* Get a file's size from the texture PAK */
        private int FileSizeTexturePAK(string FileName)
        {
            int FileIndex = GetFileIndex(FileName);
            if (FileIndex == -1) { return -1; }

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
        private PAKReturnType ExportFileTexturePAK(string FileName, string ExportPath)
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
                    return PAKReturnType.FAILED_UNSUPPORTED;
                }

                //Pull the texture part content from the archive
                ArchiveFile.BaseStream.Position = TexturePart.StartPos;
                byte[] TexturePartContent = ArchiveFile.ReadBytes(TexturePart.Length);

                //Generate a DDS header based on the tex4's information
                DDSWriter TextureOutput;
                bool FailsafeSave = false;
                switch (TextureEntries[FileIndex].Format)
                {
                    case TextureFormat.DXGI_FORMAT_BC5_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, TextureType.ATI2N);
                        break;
                    case TextureFormat.DXGI_FORMAT_BC1_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, TextureType.Dxt1);
                        break;
                    case TextureFormat.DXGI_FORMAT_BC3_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, TextureType.Dxt5);
                        break;
                    case TextureFormat.DXGI_FORMAT_B8G8R8A8_UNORM:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height, 32, 0, TextureType.UNCOMPRESSED_GENERAL);
                        break;
                    case TextureFormat.DXGI_FORMAT_BC7_UNORM:
                    default:
                        TextureOutput = new DDSWriter(TexturePartContent, TexturePart.Width, TexturePart.Height);
                        FailsafeSave = true;
                        break;
                }

                //Try and save out the part
                try
                {
                    if (FailsafeSave)
                    {
                        TextureOutput.SaveCrude(ExportPath);
                        return PAKReturnType.SUCCESS;
                    }
                    TextureOutput.Save(ExportPath);
                    return PAKReturnType.SUCCESS;
                }
                catch
                {
                    return PAKReturnType.FAILED_FILE_IN_USE;
                }
            }
            catch (Exception e)
            {
                LatestError = e.ToString();
                return PAKReturnType.FAILED_UNKNOWN;
            }
        }

        /* Import a file to the texture PAK */
        private PAKReturnType ImportFileTexturePAK(string FileName, string ImportPath)
        {
            try
            {
                //Get the texture entry & parse new DDS
                TEX4 TextureEntry = TextureEntries[GetFileIndex(FileName)];
                DDSReader NewTexture = new DDSReader(ImportPath);

                //Currently we only support textures that have V1 and V2
                if (TextureEntry.Texture_V2.HeaderPos == -1)
                {
                    return PAKReturnType.FAILED_UNSUPPORTED;
                }

                //Load the BIN to byte array
                ArchiveFileBin.BaseStream.Position = 0;
                byte[] BinFile = new byte[ArchiveFileBin.BaseStream.Length];
                for (int i = 0; i < BinFile.Length; i++)
                {
                    BinFile[i] = ArchiveFileBin.ReadByte();
                }

                //Update format in BIN
                int BinOffset = TextureEntry.HeaderPos + 4;
                byte[] NewFormat = BitConverter.GetBytes((int)NewTexture.Format);
                for (int i = 0; i < 4; i++)
                {
                    BinFile[BinOffset] = NewFormat[i];
                    BinOffset++;
                }

                //Change the new filesize dependant on options (SEE LINE 171)
                int FileSize = TextureEntry.Texture_V2.Length;
                if (ToolSettings.GetSetting(ToolOptionsHandler.Settings.EXPERIMENTAL_TEXTURE_IMPORT))
                {
                    FileSize = (int)NewTexture.DataBlock.Length;
                }

                //Update filesize in BIN
                byte[] NewEntrySize = BitConverter.GetBytes(FileSize);
                for (int i = 0; i < 4; i++)
                {
                    BinFile[BinOffset] = NewEntrySize[i];
                    BinOffset++;
                }
                BinOffset += 4; //Skip V1

                //Update dimensions in BIN (imported textures apply to V2 only)
                BinOffset += 6;
                byte[] NewWidth = BitConverter.GetBytes((Int16)NewTexture.Width);
                byte[] NewHeight = BitConverter.GetBytes((Int16)NewTexture.Height);
                for (int i = 0; i < 2; i++)
                {
                    BinFile[BinOffset] = NewWidth[i];
                    BinOffset++;
                }
                for (int i = 0; i < 2; i++)
                {
                    BinFile[BinOffset] = NewHeight[i];
                    BinOffset++;
                }

                //Take all headers up to the V2 header in PAK
                ArchiveFile.BaseStream.Position = 0;
                byte[] ArchivePt1 = new byte[TextureEntry.Texture_V2.HeaderPos];
                for (int i = 0; i < ArchivePt1.Length; i++)
                {
                    ArchivePt1[i] = ArchiveFile.ReadByte();
                }
                
                //Update V2 header for new image filesize in PAK
                byte[] ArchivePt2 = new byte[48];
                for (int i = 0; i < ArchivePt2.Length; i++)
                {
                    ArchivePt2[i] = ArchiveFile.ReadByte();
                }
                Array.Reverse(NewEntrySize); //This file is big endian
                for (int i = 0; i < 4; i++)
                {
                    ArchivePt2[8 + i] = NewEntrySize[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    ArchivePt2[12 + i] = NewEntrySize[i];
                }

                //Read to end of headers in PAK
                byte[] ArchivePt3 = new byte[HeaderListEndPAK - ArchivePt1.Length - 48];
                for (int i = 0; i < ArchivePt3.Length; i++)
                {
                    ArchivePt3[i] = ArchiveFile.ReadByte();
                }

                //Take all files up to V2 in PAK
                byte[] ArchivePt4 = new byte[TextureEntry.Texture_V2.StartPos - HeaderListEndPAK];
                for (int i = 0; i < ArchivePt4.Length; i++)
                {
                    ArchivePt4[i] = ArchiveFile.ReadByte();
                }
                ArchiveFile.BaseStream.Position += FileSize;

                //Take all files past V2 in PAK
                byte[] ArchivePt5 = new byte[ArchiveFile.BaseStream.Length - ArchiveFile.BaseStream.Position];
                for (int i = 0; i < ArchivePt5.Length; i++)
                {
                    ArchivePt5[i] = ArchiveFile.ReadByte();
                }

                //CATHODE seems to ignore texture header information regarding size, so as default, resize any imported textures to the original size.
                //An option is provided in the toolkit to write size information to the header (done above) however, so don't resize if that's the case.
                //More work needs to be done to figure out why CATHODE doesn't honour the header's size value.
                if (!ToolSettings.GetSetting(ToolOptionsHandler.Settings.EXPERIMENTAL_TEXTURE_IMPORT))
                {
                    Array.Resize(ref NewTexture.DataBlock, TextureEntry.Texture_V2.Length);
                }

                //It's time to try and save!
                try
                {
                    //Write out new BIN
                    ArchiveFileBin.Close();
                    BinaryWriter ArchiveFileWriteBin = new BinaryWriter(File.OpenWrite(ArchivePathBin));
                    ArchiveFileWriteBin.BaseStream.SetLength(0);
                    ArchiveFileWriteBin.Write(BinFile);
                    ArchiveFileWriteBin.Close();

                    //Write out new PAK
                    ArchiveFile.Close();
                    BinaryWriter ArchiveFileWrite = new BinaryWriter(File.OpenWrite(ArchivePath));
                    ArchiveFileWrite.BaseStream.SetLength(0);
                    ArchiveFileWrite.Write(ArchivePt1);
                    ArchiveFileWrite.Write(ArchivePt2);
                    ArchiveFileWrite.Write(ArchivePt3);
                    ArchiveFileWrite.Write(ArchivePt4);
                    ArchiveFileWrite.Write(NewTexture.DataBlock);
                    ArchiveFileWrite.Write(ArchivePt5);
                    ArchiveFileWrite.Close();
                }
                catch
                {
                    //File is probably in-use by the game, re-open for reading and exit as fail
                    Open(ArchivePath);
                    return PAKReturnType.FAILED_FILE_IN_USE;
                }

                //Reload the archive for us
                Open(ArchivePath);
                ParseTexturePAK();

                return PAKReturnType.SUCCESS;
            }
            catch (Exception e)
            {
                LatestError = e.ToString();
                return PAKReturnType.FAILED_UNKNOWN;
            }
        }


        /* --- MODEL PAK --- */
        int TableCountPt1 = -1;
        int TableCountPt2 = -1;
        int FilenameListEnd = -1;
        int HeaderListEnd = -1;
        List<CS2> ModelEntries = new List<CS2>();

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
            int FilenameListStart = (int)ArchiveFileBin.BaseStream.Position;

            //Read all file names (bytes)
            byte[] filename_bytes = ArchiveFileBin.ReadBytes(FilenameListEnd);

            //Read table 2 (skipping all unknowns for now)
            ExtraBinaryUtils BinaryUtils = new ExtraBinaryUtils();
            for (int i = 0; i < TableCountPt2; i++)
            {
                CS2 new_entry = new CS2();
                new_entry.FilenameOffset = ArchiveFileBin.ReadInt32();
                new_entry.Filename = BinaryUtils.GetStringFromByteArray(filename_bytes, new_entry.FilenameOffset);
                ArchiveFileBin.BaseStream.Position += 4;
                new_entry.ModelPartNameOffset = ArchiveFileBin.ReadInt32();
                new_entry.ModelPartName = BinaryUtils.GetStringFromByteArray(filename_bytes, new_entry.ModelPartNameOffset);
                ArchiveFileBin.BaseStream.Position += 44;
                new_entry.MaterialLibaryIndex = ArchiveFileBin.ReadInt32();
                ArchiveFileBin.BaseStream.Position += 8;
                new_entry.BlockSize = ArchiveFileBin.ReadInt32();
                ArchiveFileBin.BaseStream.Position += 14;
                new_entry.ScaleFactor = ArchiveFileBin.ReadInt16(); //Maybe?
                ArchiveFileBin.BaseStream.Position += 2;
                new_entry.VertCount = ArchiveFileBin.ReadInt16();
                new_entry.FaceCount = ArchiveFileBin.ReadInt16();
                new_entry.BoneCount = ArchiveFileBin.ReadInt16();
                ModelEntries.Add(new_entry);
            }

            //Get extra info from each header in the PAK
            BigEndianUtils BigEndian = new BigEndianUtils();
            ArchiveFile.BaseStream.Position += 32; //Skip header
            for (int i = 0; i < TableCountPt2; i++)
            {
                //I'm just assuming these will be in the right order!
                ArchiveFile.BaseStream.Position += 8; //Skip unknowns
                ModelEntries[i].PakSize = BigEndian.ReadInt32(ArchiveFile);
                if (ModelEntries[i].PakSize != BigEndian.ReadInt32(ArchiveFile)) {
                    throw new FormatException("Model entry header size mismatch."); //Shouldn't hit this hopefully!
                } 
                ModelEntries[i].PakOffset = BigEndian.ReadInt32(ArchiveFile);
                ArchiveFile.BaseStream.Position += 28;
            }
            HeaderListEnd = (int)ArchiveFile.BaseStream.Position;

            //Add all filenames to list (do we eventually want to list submeshes on their own?)
            foreach (CS2 ModelEntry in ModelEntries)
            {
                if (!FileList.Contains(ModelEntry.Filename))
                {
                    FileList.Add(ModelEntry.Filename);
                }
            }

            return FileList;
        }

        /* Get a file's size from the model PAK */
        private int FileSizeModelPAK(string FileName)
        {
            //Get the selected model's submeshes and add up their sizes
            int TotalSize = 0;
            foreach (CS2 ThisModel in ModelEntries)
            {
                if (ThisModel.Filename == FileName.Replace("/", "\\"))
                {
                    TotalSize += ThisModel.PakSize;
                }
            }

            return TotalSize;
        }

        /* Export a file from the model PAK */
        private PAKReturnType ExportFileModelPAK(string FileName, string ExportPath)
        {
            //Get the selected model's submeshes
            List<CS2> ModelSubmeshes = new List<CS2>();
            foreach (CS2 ThisModel in ModelEntries)
            {
                if (ThisModel.Filename == FileName.Replace("/", "\\"))
                {
                    ModelSubmeshes.Add(ThisModel);
                }
            }

            //Extract each submesh into a CS2 folder
            Directory.CreateDirectory(ExportPath);
            foreach (CS2 Submesh in ModelSubmeshes)
            {
                ArchiveFile.BaseStream.Position = HeaderListEnd + Submesh.PakOffset;
                File.WriteAllBytes(ExportPath + "/" + Submesh.ModelPartName, ArchiveFile.ReadBytes(Submesh.PakSize));
            }

            //Done!
            return PAKReturnType.SUCCESS;
        }

        /* Import a file to the model PAK */
        private PAKReturnType ImportFileModelPAK(string FileName, string ImportPath)
        {
            //WIP
            return PAKReturnType.FAILED_UNSUPPORTED;
        }


        /* --- COMMANDS PAK --- */
        List<EntryCommandsPAK> CommandsEntries = new List<EntryCommandsPAK>();

        /* Parse the entries in a scripts PAK (BIG WIP!) */
        private List<string> ParseCommandsPAK()
        {
            List<byte[]> CommandsHeaderMagics = new List<byte[]>();
            ExtraBinaryUtils BinaryUtils = new ExtraBinaryUtils();

            /* **************************** */
            /* *********  Header  ********* */

            ArchiveFile.BaseStream.Position = 28; //Skip header

            /* **************************** */
            /* ********* "Blocks" ********* */
            CommandsHeaderMagics.Add(new byte[] { 0xDA, 0x6B, 0xD7, 0x02 });
            CommandsHeaderMagics.Add(new byte[] { 0x84, 0x11, 0xCD, 0x38 });
            CommandsHeaderMagics.Add(new byte[] { 0x5E, 0x8E, 0x8E, 0x5A });
            CommandsHeaderMagics.Add(new byte[] { 0xBF, 0xA7, 0x62, 0x8C });
            CommandsHeaderMagics.Add(new byte[] { 0xF6, 0xAF, 0x08, 0x93 });
            CommandsHeaderMagics.Add(new byte[] { 0xF0, 0x0B, 0x76, 0x96 });
            CommandsHeaderMagics.Add(new byte[] { 0x38, 0x43, 0xFF, 0xBF });
            CommandsHeaderMagics.Add(new byte[] { 0x87, 0xC1, 0x25, 0xE7 });
            CommandsHeaderMagics.Add(new byte[] { 0xDC, 0x72, 0x74, 0xFD });

            //Block one
            ParseGenericCommandsPakBlock(CommandsHeaderMagics[0], CommandsHeaderMagics[1]);

            //String block
            List<string> ScriptStringDump = new List<string>();
            string ThisStringEntry = "";
            bool DidJustSubmit = false;
            for (int i = 0; i < 99999999; i++)
            {
                //Read a block of four
                byte[] ThisSegment = ArchiveFile.ReadBytes(4);

                //Check for header magics, and act accordingly
                if (ThisSegment.SequenceEqual(CommandsHeaderMagics[1]) || ThisSegment.SequenceEqual(CommandsHeaderMagics[2]))
                {
                    //Only submit if the string has content
                    ScriptStringDump.Add(ThisStringEntry);
                    ThisStringEntry = "";

                    //If we're still in the current list of strings, continue - else break
                    if (ThisSegment.SequenceEqual(CommandsHeaderMagics[2]))
                    {
                        ArchiveFile.BaseStream.Position -= 4;
                        break;
                    }
                    DidJustSubmit = true;
                    continue;
                }

                //We get eight bytes of unknown for each string - skip them for now 
                if (DidJustSubmit)
                {
                    ArchiveFile.BaseStream.Position += 4;
                    DidJustSubmit = false;
                    continue;
                }

                //We're still inside the current string, add the chars on if they're not null
                for (int x = 0; x < 4; x++)
                {
                    if (ThisSegment[x] != 0x00)
                    {
                        ThisStringEntry += (char)ThisSegment[x];
                    }
                }
            }

            //Blocks two through seven
            for (int i = 2; i < 8; i++)
            {
                ParseGenericCommandsPakBlock(CommandsHeaderMagics[i], CommandsHeaderMagics[i + 1]);
            }

            //Block eight
            List<List<byte>> AllBlockEntries = new List<List<byte>>(); //All entries
            List<byte> ThisBlockEntry = new List<byte>(); //A single entry
            for (int i = 0; i < 99999999; i++)
            {
                //Read a segment of four bytes
                byte[] ThisSegment = ArchiveFile.ReadBytes(4);

                //Each block here is 8 bytes known - so if the header matches, read the following four into our list
                if (ThisSegment.SequenceEqual(CommandsHeaderMagics[8]))
                {
                    byte[] ThisSegmentCont = ArchiveFile.ReadBytes(4);
                    for (int x = 0; x < 4; x++)
                    {
                        ThisBlockEntry.Add(ThisSegmentCont[x]);
                    }
                    AllBlockEntries.Add(ThisBlockEntry);
                    ThisBlockEntry.Clear();
                    continue;
                }

                //We didn't match the header, so we must be at the scripts
                ArchiveFile.BaseStream.Position -= 4;
                break;
            }


            /* ***************************** */
            /* ********* "Scripts" ********* */
            CommandsHeaderMagics.Add(new byte[] { 0x07, 0x00, 0x00, 0x00 }); //Not really a header, but used as such here.
            CommandsHeaderMagics.Add(new byte[] { 0x0E, 0x00, 0x00, 0x00 }); //Not really a header, but used as such here.

            //Parse each script entry
            //Parse each script entry
            EntryCommandsPAK NewScriptEntry;
            for (int i = 0; i < 99999999; i++)
            {
                byte[] ThisSegment = ArchiveFile.ReadBytes(4);

                //We reached the garbage block
                if (ThisSegment.SequenceEqual(CommandsHeaderMagics[9]))
                {
                    ArchiveFile.BaseStream.Position -= 4;
                    break;
                }

                //We didn't reach the garbage block - make a new entry and assign the bytes (ID)
                NewScriptEntry = new EntryCommandsPAK();
                NewScriptEntry.ScriptID = ThisSegment;

                //Read-in script string
                bool should_stop = false;
                for (int x = 0; x < 99999999; x++)
                {
                    byte[] ThisSegmentCont = ArchiveFile.ReadBytes(4);
                    for (int y = 0; y < 4; y++)
                    {
                        if (ThisSegmentCont[y] == 0x00)
                        {
                            should_stop = true;
                            break;
                        }
                        NewScriptEntry.ScriptName += (char)ThisSegmentCont[y];
                    }
                    if (should_stop) { break; }
                }

                //Get the script's magic (used to denote the start/end of the script)
                NewScriptEntry.ScriptMarker = ArchiveFile.ReadBytes(4);
                for (int x = 0; x < 4; x++) { NewScriptEntry.ScriptContent.Add(NewScriptEntry.ScriptMarker[x]); }

                //Capture the script until we hit the end magic
                bool TriggeredReset = false;
                ScriptLoopStart:
                for (int x = 0; x < 99999999; x++)
                {
                    byte[] ThisSegmentCont = ArchiveFile.ReadBytes(4);

                    //We've reached the end
                    if (!TriggeredReset && ThisSegmentCont.SequenceEqual(NewScriptEntry.ScriptMarker))
                    {
                        break;
                    }
                    TriggeredReset = false;

                    //We haven't reached the end, keep reading the script
                    for (int y = 0; y < 4; y++)
                    {
                        NewScriptEntry.ScriptContent.Add(ThisSegmentCont[y]);
                    }
                }

                //Parse the numbers at the bottom (?!?)
                for (int x = 0; x < 24; x++)
                {
                    NewScriptEntry.ScriptTrailingInts[x] = ArchiveFile.ReadInt32();
                }

                //Verify we should've exited when we did (this is a bug from the odd formatting of the PAK's script entry/exit points)
                int SanityDiff = NewScriptEntry.ScriptTrailingInts[2] - NewScriptEntry.ScriptTrailingInts[0];
                if (!(SanityDiff < (NewScriptEntry.ScriptTrailingInts[2].ToString().Length * 10000) && SanityDiff >= 0)) // This is a magic number that seems to work: a better solution is required really.
                {
                    ArchiveFile.BaseStream.Position -= 100;
                    TriggeredReset = true;
                    goto ScriptLoopStart;
                }

                //Append the magic to the end
                for (int x = 0; x < 4; x++) { NewScriptEntry.ScriptContent.Add(NewScriptEntry.ScriptMarker[x]); }
                
                //Add to list
                CommandsEntries.Add(NewScriptEntry);
            }


            /* ***************************** */
            /* ********** Garbage ********** */

            //Count up the "garbage" at the end - these numbers might actually be IDs for something
            try
            {
                for (int i = 0; i < 999999999; i++)
                {
                    int GarbageNumber = ArchiveFile.ReadInt32(); //do something with this
                }
            }
            catch { }


            //Compile all filenames for return
            foreach (EntryCommandsPAK ScriptEntry in CommandsEntries)
            {
                FileList.Add(ScriptEntry.ScriptName);
            }
            return FileList;
        }
        private List<List<byte>> ParseGenericCommandsPakBlock(byte[] ThisMagic, byte[] NextMagic)
        {
            List<List<byte>> AllBlockEntries = new List<List<byte>>(); //All entries
            List<byte> ThisBlockEntry = new List<byte>(); //A single entry

            for (int i = 0; i < 99999999; i++)
            {
                //Read a segment of four bytes
                byte[] ThisSegment = ArchiveFile.ReadBytes(4);

                //We're at the start of a new entry - submit the previous one
                if (ThisSegment.SequenceEqual(ThisMagic) || ThisSegment.SequenceEqual(NextMagic))
                {
                    //Only submit if the entry has content
                    if (ThisBlockEntry.Count > 0)
                    {
                        AllBlockEntries.Add(ThisBlockEntry);
                    }
                    ThisBlockEntry.Clear();

                    //If we're still in the current block, continue - else break
                    if (ThisSegment.SequenceEqual(NextMagic))
                    {
                        ArchiveFile.BaseStream.Position -= 4;
                        break;
                    }
                    continue;
                }

                //We're still inside the current entry, add it on!
                for (int x = 0; x < 4; x++)
                {
                    ThisBlockEntry.Add(ThisSegment[x]);
                }
            }

            return AllBlockEntries;
        }

        /* Get a file's size from the scripts PAK (compiled size, not actual) */
        private int FileSizeCommandsPAK(string FileName)
        {
            int FileIndex = GetFileIndex(FileName);
            if (FileIndex == -1) { return -1; }

            return CommandsEntries[FileIndex].ScriptContent.Count;
        }

        /* Export a file from the scripts PAK */
        private PAKReturnType ExportFileCommandsPAK(string FileName, string ExportPath)
        {
            //There's no point exporting/importing until the format is understood better.
            return PAKReturnType.FAILED_UNSUPPORTED;
        }

        /* Import a file to the scripts PAK */
        private PAKReturnType ImportFileCommandsPAK(string FileName, string ImportPath)
        {
            return PAKReturnType.FAILED_UNSUPPORTED;
        }


        /* --- MATERIAL MAPPING PAK --- */
        List<EntryMaterialMappingsPAK> MaterialMappingEntries = new List<EntryMaterialMappingsPAK>();

        /* Parse the entries in the material map PAK */
        private List<string> ParseMaterialMappingsPAK()
        {
            //Parse header
            ArchiveFile.BaseStream.Position += 8;
            int NumberOfFiles = ArchiveFile.ReadInt32();

            //Parse entries (XML is broken in the build files - doesn't get shipped)
            for (int x = 0; x < NumberOfFiles; x++)
            {
                //This entry
                EntryMaterialMappingsPAK NewMatEntry = new EntryMaterialMappingsPAK();
                NewMatEntry.MapHeader = ArchiveFile.ReadBytes(4);
                NewMatEntry.MapEntryCoupleCount = ArchiveFile.ReadInt32();
                ArchiveFile.BaseStream.Position += 4; //skip nulls (always nulls?)
                for (int p = 0; p < (NewMatEntry.MapEntryCoupleCount * 2) + 1; p++)
                {
                    //String
                    int NewMatStringLength = ArchiveFile.ReadInt32();
                    string NewMatString = "";
                    for (int i = 0; i < NewMatStringLength; i++)
                    {
                        NewMatString += ArchiveFile.ReadChar();
                    }

                    //First string is filename, others are materials
                    if (p == 0)
                    {
                        NewMatEntry.MapFilename = NewMatString;
                    }
                    else
                    {
                        NewMatEntry.MapMatEntries.Add(NewMatString);
                    }
                }
                MaterialMappingEntries.Add(NewMatEntry);
            }

            //Compile all filenames for return
            foreach (EntryMaterialMappingsPAK MatEntry in MaterialMappingEntries)
            {
                FileList.Add(MatEntry.MapFilename);
            }
            return FileList;
        }

        /* Get a file's size from the material map PAK (kinda faked for now) */
        private int FileSizeMaterialMappingsPAK(string FileName)
        {
            int FileIndex = GetFileIndex(FileName);
            if (FileIndex == -1) { return -1; }

            int size = 0;
            foreach (string MatMap in MaterialMappingEntries[FileIndex].MapMatEntries)
            {
                size += MatMap.Length;
            }
            return size;
        }

        /* Export a file from the material map PAK */
        private PAKReturnType ExportFileMaterialMappingsPAK(string FileName, string ExportPath)
        {
            //Files don't get shipped - how should we export the data?
            return PAKReturnType.FAILED_UNSUPPORTED;
        }

        /* Import a file to the material map PAK */
        private PAKReturnType ImportFileMaterialMappingsPAK(string FileName, string ImportPath)
        {
            return PAKReturnType.FAILED_UNSUPPORTED;
        }
    }
}
