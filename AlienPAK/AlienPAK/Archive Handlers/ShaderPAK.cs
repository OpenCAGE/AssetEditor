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
     * Shader PAK handler.
     * Currently doesn't support import/export. WIP!
     * Also doesn't parse much info out, although the basic file structure is there for the initial PAK.
     * Work needs to be done on parsing the _BIN and how that links to the initial PAK.
     * 
    */
    class ShaderPAK : AnyPAK
    {
        List<CathodeShaderString> StringDump = new List<CathodeShaderString>();
        List<CathodeShaderHeader> HeaderDump = new List<CathodeShaderHeader>();

        /* Initialise the ShaderPAK class with the intended location (existing or not) */
        public ShaderPAK(string PathToPAK)
        {
            FilePathPAK = PathToPAK;
            FilePathBIN = Path.GetFileNameWithoutExtension(FilePathPAK) + "_BIN.PAK";
        }

        /* Load the contents of an existing ShaderPAK set (massive WIP) */
        public override PAKReturnType Load()
        {
            if (!File.Exists(FilePathPAK))
            {
                return PAKReturnType.FAIL_COULD_NOT_ACCESS_FILE;
            }
            
            try
            {
                //Open PAK
                BinaryReader ArchiveFile = new BinaryReader(File.OpenRead(FilePathPAK));
                ExtraBinaryUtils BinaryUtils = new ExtraBinaryUtils();

                ArchiveFile.BaseStream.Position = 8; //Skip magic (there seems to be two types, D2 and D3 - are their formats different?)

                int VersionNum = ArchiveFile.ReadInt32(); //Assumed
                int NumOfStringPairs = ArchiveFile.ReadInt32();
                int NumOfStringPairs_Alt = ArchiveFile.ReadInt32(); //these dont always match, but this number seemingly means nothing
                ArchiveFile.BaseStream.Position += 12; //Skip unknown header info (seems to always be the same)

                //Read string block
                for (int i = 0; i < NumOfStringPairs; i++)
                {
                    CathodeShaderString newStringEntry = new CathodeShaderString();
                    ArchiveFile.BaseStream.Position += 8; //skip blanks

                    //read header magic (todo: use this somehow)
                    newStringEntry.HeaderMagic1 = ArchiveFile.ReadBytes(4);
                    newStringEntry.HeaderMagic2 = ArchiveFile.ReadBytes(4);
                    
                    //parse useful info
                    newStringEntry.Number1 = ArchiveFile.ReadInt32(); //some sort of index, potentially actually int16 not 32
                    ArchiveFile.BaseStream.Position += 8; //skip blanks and indicator that we're approaching string 1 (odd)
                    newStringEntry.StringPart1 = ArchiveFile.ReadBytes(4);
                    newStringEntry.Number2 = ArchiveFile.ReadInt32(); //some sort of index, potentially actually int8 or int16 not 32
                    ArchiveFile.BaseStream.Position += 8; //skip blanks
                    newStringEntry.StringPart2 = ArchiveFile.ReadBytes(4);
                
                    StringDump.Add(newStringEntry);
                }

                //Read shader headers
                for (int i = 0; i < NumOfStringPairs; i++)
                {
                    CathodeShaderHeader newHeaderEntry = new CathodeShaderHeader();

                    //Get the name (or type) of this header
                    ArchiveFile.BaseStream.Position += 32;
                    byte[] entryNameOrType = ArchiveFile.ReadBytes(40);
                    for (int x = 0; x < entryNameOrType.Length; x++)
                    {
                        if (entryNameOrType[x] != 0x00)
                        {
                            newHeaderEntry.ShaderType += (char)entryNameOrType[x];
                            continue;
                        }
                        break;
                    }

                    //Skip over the rest for now just so I can scrape all the names (this needs parsing obvs)
                    for (int x = 0; x < 999999; x++)
                    {
                        if (ArchiveFile.ReadBytes(4).SequenceEqual(new byte[] { 0xA4, 0xBB, 0x25, 0x77 }))
                        {
                            ArchiveFile.BaseStream.Position -= 4;
                            break;
                        }
                    }

                    HeaderDump.Add(newHeaderEntry);
                }
                
                //Done!
                ArchiveFile.Close();
                return PAKReturnType.SUCCESS;
            }
            catch (IOException) { return PAKReturnType.FAIL_COULD_NOT_ACCESS_FILE; }
            catch (Exception) { return PAKReturnType.FAIL_UNKNOWN; }
        }

        /* Return a list of filenames for shaders in the ShaderPAK archive (massive WIP) */
        public override List<string> GetFileNames()
        {
            List<string> FileNameList = new List<string>();
            foreach (CathodeShaderHeader header in HeaderDump)
            {
                if (!FileNameList.Contains(header.ShaderType))
                {
                    FileNameList.Add(header.ShaderType);
                }
            }
            return FileNameList;
        }

        /* Get the size of the requested shader (not yet implemented) */
        public override int GetFilesize(string FileName)
        {
            return 0;
        }

        /* Find the shader entry object by name (not yet implemented) */
        protected override int GetFileIndex(string FileName)
        {
            return 0;
        }
    }
}
