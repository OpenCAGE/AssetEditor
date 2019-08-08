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

        //WIP: Slowly moving all archive handling to specialised classes... only PAK2 atm.
        PAK2 HandlerPAK2;
        TexturePAK HandlerTexturePAK;
        //Todo: depreciate PAKReturnType in favour of just true/false - or roll out PAKReturnType to each PAK handler class?

        /* --- COMMON PAK --- */
        private string ArchivePath = "";
        private string ArchivePathBin = "";
        private BinaryReader ArchiveFile = null;
        private BinaryReader ArchiveFileBin = null;
        private List<string> FileList = new List<string>();
        public enum PAKType { PAK2, PAK_TEXTURES, PAK_MODELS, PAK_SCRIPTS, PAK_MATERIALMAPS, UNRECOGNISED };
        public PAKType Format = PAKType.UNRECOGNISED;
        public enum PAKReturnType { FAILED_UNKNOWN, FAILED_UNSUPPORTED, SUCCESS, FAILED_LOGIC_ERROR, FAILED_FILE_IN_USE }
        public string LatestError = "";

        /* Open a PAK archive */
        public void Open(string FilePath)
        {
            //Close old PAK if open
            if (ArchiveFile != null) { ArchiveFile.Close(); }
            if (ArchiveFileBin != null) { ArchiveFileBin.Close(); }

            //Update our info
            ArchivePath = FilePath;
            ArchivePathBin = "";
            string FileName = Path.GetFileName(ArchivePath);
            switch (FileName)
            {
                case "GLOBAL_TEXTURES.ALL.PAK":
                case "LEVEL_TEXTURES.ALL.PAK":
                    HandlerTexturePAK = new TexturePAK(ArchivePath);
                    if (HandlerTexturePAK.Load())
                    {
                        Format = PAKType.PAK_TEXTURES;
                        return;
                    }
                    Format = PAKType.UNRECOGNISED;
                    return;
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
                    HandlerPAK2 = new PAK2(ArchivePath);
                    if (HandlerPAK2.Load())
                    {
                        Format = PAKType.PAK2;
                        return;
                    }
                    Format = PAKType.UNRECOGNISED;
                    return;
            }

            //Open new PAK
            ArchiveFile = new BinaryReader(File.OpenRead(FilePath));

            //Certain formats have associated BIN files
            switch (Format)
            {
                case PAKType.PAK_TEXTURES:
                    if (FileName.Substring(0, 5).ToUpper() == "LEVEL")
                    {
                        ArchivePathBin = ArchivePath.Substring(0, ArchivePath.Length - FileName.Length) + "LEVEL_TEXTURE_HEADERS.ALL.BIN";
                        ArchiveFileBin = new BinaryReader(File.OpenRead(ArchivePathBin));
                    }
                    else
                    {
                        ArchivePathBin = ArchivePath.Substring(0, ArchivePath.Length - FileName.Length) + "GLOBAL_TEXTURES_HEADERS.ALL.BIN";
                        ArchiveFileBin = new BinaryReader(File.OpenRead(ArchivePathBin));
                    }
                    break;
                case PAKType.PAK_MODELS:
                    ArchivePathBin = ArchivePath.Substring(0, ArchivePath.Length - FileName.Length) + "MODELS_" + FileName.Substring(0, FileName.Length - 11) + ".BIN";
                    ArchiveFileBin = new BinaryReader(File.OpenRead(ArchivePathBin));
                    break;
            }
        }

        /* Parse a PAK archive */
        public List<string> Parse()
        {
            if (ArchiveFile == null && (Format != PAKType.PAK2 && Format != PAKType.PAK_TEXTURES)) { return null; }

            FileList.Clear();
            MaterialMappingEntries.Clear();
            CommandsEntries.Clear();
            ModelEntries.Clear();

            switch (Format)
            {
                case PAKType.PAK2:
                    return HandlerPAK2.GetFileNames();
                case PAKType.PAK_TEXTURES:
                    return HandlerTexturePAK.GetFileNames();
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
            if (ArchiveFile == null && (Format != PAKType.PAK2 && Format != PAKType.PAK_TEXTURES)) { return -1; }

            switch (Format)
            {
                case PAKType.PAK2:
                    return HandlerPAK2.GetFilesize(FileName);
                case PAKType.PAK_TEXTURES:
                    return HandlerTexturePAK.GetFilesize(FileName);
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
            if (ArchiveFile == null && (Format != PAKType.PAK2 && Format != PAKType.PAK_TEXTURES)) { return PAKReturnType.FAILED_LOGIC_ERROR; }

            switch (Format)
            {
                case PAKType.PAK2:
                    if (HandlerPAK2.ExportFile(ExportPath, FileName))
                    {
                        return PAKReturnType.SUCCESS;
                    }
                    return PAKReturnType.FAILED_UNKNOWN;
                case PAKType.PAK_TEXTURES:
                    if (HandlerTexturePAK.ExportFile(ExportPath, FileName))
                    {
                        return PAKReturnType.SUCCESS;
                    }
                    return PAKReturnType.FAILED_UNKNOWN;
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
            if (ArchiveFile == null && (Format != PAKType.PAK2 && Format != PAKType.PAK_TEXTURES)) { return PAKReturnType.FAILED_LOGIC_ERROR; }

            switch (Format)
            {
                case PAKType.PAK2:
                    HandlerPAK2.ReplaceFile(ImportPath, FileName);
                    if (HandlerPAK2.Save())
                    {
                        return PAKReturnType.SUCCESS;
                    }
                    return PAKReturnType.FAILED_FILE_IN_USE;
                case PAKType.PAK_TEXTURES:
                    if (HandlerTexturePAK.ReplaceFile(ImportPath, FileName))
                    { //We have no Save() method here as TexturePAK is handled slightly differently.
                        return PAKReturnType.SUCCESS;
                    }
                    return PAKReturnType.FAILED_FILE_IN_USE;
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

        /* Remove from a PAK archive */
        public PAKReturnType RemoveFile(string FileName)
        {
            if (Format != PAKType.PAK2) { return PAKReturnType.FAILED_UNSUPPORTED; } //Currently only supported in PAK2
            HandlerPAK2.DeleteFile(FileName);
            if (HandlerPAK2.Save())
            {
                return PAKReturnType.SUCCESS;
            }
            return PAKReturnType.FAILED_FILE_IN_USE;
        }

        /* Add to a PAK archive */
        public PAKReturnType AddNewFile(string NewFile)
        {
            if (Format != PAKType.PAK2) { return PAKReturnType.FAILED_UNSUPPORTED; } //Currently only supported in PAK2
            HandlerPAK2.AddFile(NewFile);
            if (HandlerPAK2.Save())
            {
                return PAKReturnType.SUCCESS;
            }
            return PAKReturnType.FAILED_FILE_IN_USE;
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

            //Failed to find - fatal issue
            throw new Exception("Could not find PAK entry - fatal!");
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
            //First, parse the MTL file to find material info
            string PathToMTL = ArchivePath.Substring(0, ArchivePath.Length - 3) + "MTL";
            BinaryReader ArchiveFileMtl = new BinaryReader(File.OpenRead(PathToMTL));
            
            //Header
            ArchiveFileMtl.BaseStream.Position += 40; //There are some knowns here, just not required for us yet
            int MaterialEntryCount = ArchiveFileMtl.ReadInt16();
            ArchiveFileMtl.BaseStream.Position += 2; //Skip unknown

            //Strings - more work will be done on materials eventually, 
            //but taking their names for now is good enough for model export
            List<string> MaterialEntries = new List<string>();
            string ThisMaterialString = "";
            for (int i = 0; i < MaterialEntryCount; i++)
            {
                while (true)
                {
                    byte ThisByte = ArchiveFileMtl.ReadByte();
                    if (ThisByte == 0x00)
                    {
                        MaterialEntries.Add(ThisMaterialString);
                        ThisMaterialString = "";
                        break;
                    }
                    ThisMaterialString += (char)ThisByte;
                }
            }
            ArchiveFileMtl.Close();

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
                new_entry.MaterialName = MaterialEntries[new_entry.MaterialLibaryIndex];
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
                ArchiveFile.BaseStream.Position += 8; //Skip unknowns
                int ThisPakSize = BigEndian.ReadInt32(ArchiveFile);
                if (ThisPakSize != BigEndian.ReadInt32(ArchiveFile))
                {
                    //Dud entry... handle this somehow?
                }
                int ThisPakOffset = BigEndian.ReadInt32(ArchiveFile);
                ArchiveFile.BaseStream.Position += 14;
                int ThisIndex = BigEndian.ReadInt16(ArchiveFile);
                ArchiveFile.BaseStream.Position += 12;

                if (ThisIndex == -1)
                {
                    continue; //Again, dud entry. Need to look into this!
                }

                //Push it into the correct entry
                ModelEntries[ThisIndex].PakSize = ThisPakSize;
                ModelEntries[ThisIndex].PakOffset = ThisPakOffset;
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
            return PAKReturnType.FAILED_UNSUPPORTED; //Disabling export for main branch

            try
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

                //Extract each submesh into a CS2 folder by material and submesh name
                Directory.CreateDirectory(ExportPath);
                foreach (CS2 Submesh in ModelSubmeshes)
                {
                    ArchiveFile.BaseStream.Position = HeaderListEnd + Submesh.PakOffset;

                    string ThisExportPath = ExportPath;
                    if (Submesh.ModelPartName != "")
                    {
                        ThisExportPath = ExportPath + "/" + Submesh.ModelPartName;
                        Directory.CreateDirectory(ThisExportPath);
                    }
                    File.WriteAllBytes(ThisExportPath + "/" + Submesh.MaterialName, ArchiveFile.ReadBytes(Submesh.PakSize));
                }

                //Done!
                return PAKReturnType.SUCCESS;
            }
            catch
            {
                //Failed
                return PAKReturnType.FAILED_UNKNOWN;
            }
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
            return CommandsEntries[GetFileIndex(FileName)].ScriptContent.Count;
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
            int size = 0;
            foreach (string MatMap in MaterialMappingEntries[GetFileIndex(FileName)].MapMatEntries)
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
