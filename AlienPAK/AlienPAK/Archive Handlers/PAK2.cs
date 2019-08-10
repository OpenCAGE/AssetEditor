﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class PAK2
    {
        private List<EntryPAK2> Pak2Files = new List<EntryPAK2>();
        private int OffsetListBegin = -1;
        private int NumberOfEntries = -1;
        private string FilePath = "";

        /* Initialise the PAK2 class with the intended PAK2 location (existing or not) */
        public PAK2(string PathToPAK)
        {
            FilePath = PathToPAK;
        }

        /* Load the contents of an existing PAK2 */
        public PAKReturnType Load()
        {
            if (!File.Exists(FilePath))
            {
                return PAKReturnType.FAIL_TRIED_TO_LOAD_VIRTUAL_ARCHIVE;
            }

            try
            {
                //Open PAK
                BinaryReader ArchiveFile = new BinaryReader(File.OpenRead(FilePath));

                //Read the header info
                string MagicValidation = "";
                for (int i = 0; i < 4; i++) { MagicValidation += ArchiveFile.ReadChar(); }
                if (MagicValidation != "PAK2") { ArchiveFile.Close(); return PAKReturnType.FAIL_ARCHIVE_IS_NOT_EXCPETED_TYPE; }
                OffsetListBegin = ArchiveFile.ReadInt32() + 16;
                NumberOfEntries = ArchiveFile.ReadInt32();
                ArchiveFile.BaseStream.Position += 4; //Skip "4"

                //Read all file names and create entries
                for (int i = 0; i < NumberOfEntries; i++)
                {
                    string ThisFileName = "";
                    for (byte b; (b = ArchiveFile.ReadByte()) != 0x00;)
                    {
                        ThisFileName += (char)b;
                    }

                    EntryPAK2 NewPakFile = new EntryPAK2();
                    NewPakFile.Filename = ThisFileName;
                    Pak2Files.Add(NewPakFile);
                }

                //Read all file offsets
                ArchiveFile.BaseStream.Position = OffsetListBegin;
                List<int> FileOffsets = new List<int>();
                FileOffsets.Add(OffsetListBegin + (NumberOfEntries * 4));
                for (int i = 0; i < NumberOfEntries; i++)
                {
                    FileOffsets.Add(ArchiveFile.ReadInt32());
                    Pak2Files[i].Offset = FileOffsets[i];
                }

                //Read in the files to entries
                ExtraBinaryUtils BinaryUtils = new ExtraBinaryUtils();
                for (int i = 0; i < NumberOfEntries; i++)
                {
                    //Must pass to RemoveLeadingNulls as each file starts with 0-3 null bytes to align files to a 4-byte block reader
                    Pak2Files[i].Content = BinaryUtils.RemoveLeadingNulls(ArchiveFile.ReadBytes(FileOffsets[i + 1] - FileOffsets[i]));
                }

                //Close PAK
                ArchiveFile.Close();
                return PAKReturnType.SUCCESS;
            }
            catch (IOException e) { return PAKReturnType.FAIL_COULD_NOT_ACCESS_FILE; }
            catch (Exception e) { return PAKReturnType.FAIL_UNKNOWN; }
        }

        /* Return a list of filenames for files in the PAK2 archive */
        public List<string> GetFileNames()
        {
            List<string> FileNameList = new List<string>();
            foreach (EntryPAK2 ArchiveFile in Pak2Files)
            {
                FileNameList.Add(ArchiveFile.Filename);
            }
            return FileNameList;
        }

        /* Get the file size of an archive entry */
        public int GetFilesize(string FileName)
        {
            return Pak2Files[GetFileIndex(FileName)].Content.Length;
        }

        /* Find the a file entry object by name */
        private int GetFileIndex(string FileName)
        {
            for (int i = 0; i < Pak2Files.Count; i++)
            {
                if (Pak2Files[i].Filename == FileName || Pak2Files[i].Filename == FileName.Replace('/', '\\'))
                {
                    return i;
                }
            }
            throw new Exception("Could not find the requested file in PAK2! Fatal logic error.");
        }

        /* Add a file to the PAK2 */
        public PAKReturnType AddFile(string PathToNewFile, int TrimFromPath = 0) //TrimFromPath is available to leave some directory trace in the filename
        {
            try
            {
                EntryPAK2 NewFile = new EntryPAK2();
                if (TrimFromPath == 0) { NewFile.Filename = Path.GetFileName(PathToNewFile).ToUpper(); } //Virtual directory support here would be nice too
                else { NewFile.Filename = PathToNewFile.Substring(TrimFromPath).ToUpper(); } //Easy to fail here, so be careful on function usage!
                NewFile.Content = File.ReadAllBytes(PathToNewFile);
                Pak2Files.Add(NewFile);
                return PAKReturnType.SUCCESS;
            }
            catch (IOException e) { return PAKReturnType.FAIL_COULD_NOT_ACCESS_FILE; }
            catch (Exception e) { return PAKReturnType.FAIL_UNKNOWN; }
        }

        /* Delete a file from the PAK2 */
        public PAKReturnType DeleteFile(string FileName)
        {
            try
            {
                Pak2Files.RemoveAt(GetFileIndex(FileName));
                return PAKReturnType.SUCCESS;
            }
            catch
            {
                return PAKReturnType.FAIL_UNKNOWN;
            }
        }

        /* Replace an existing file in the PAK2 archive */
        public PAKReturnType ReplaceFile(string PathToNewFile, string FileName)
        {
            try
            {
                Pak2Files[GetFileIndex(FileName)].Content = File.ReadAllBytes(PathToNewFile);
                return PAKReturnType.SUCCESS;
            }
            catch (IOException e) { return PAKReturnType.FAIL_COULD_NOT_ACCESS_FILE; }
            catch (Exception e) { return PAKReturnType.FAIL_UNKNOWN; }
        }

        /* Export an existing file from the PAK2 archive */
        public PAKReturnType ExportFile(string PathToExport, string FileName)
        {
            try
            {
                File.WriteAllBytes(PathToExport, Pak2Files[GetFileIndex(FileName)].Content);
                return PAKReturnType.SUCCESS;
            }
            catch (IOException e) { return PAKReturnType.FAIL_COULD_NOT_ACCESS_FILE; }
            catch (Exception e) { return PAKReturnType.FAIL_UNKNOWN; }
        }

        /* Save out our PAK2 archive */
        public PAKReturnType Save()
        {
            try
            {
                //Open/create PAK2 for writing
                BinaryWriter ArchiveFileWrite;
                if (File.Exists(FilePath))
                {
                    ArchiveFileWrite = new BinaryWriter(File.OpenWrite(FilePath));
                    ArchiveFileWrite.BaseStream.SetLength(0);
                }
                else
                {
                    ArchiveFileWrite = new BinaryWriter(File.Create(FilePath));
                }
                ExtraBinaryUtils BinaryUtils = new ExtraBinaryUtils();

                //Write header
                BinaryUtils.WriteString("PAK2", ArchiveFileWrite);
                int OffsetListBegin_New = 0;
                for (int i = 0; i < Pak2Files.Count; i++)
                {
                    OffsetListBegin_New += Pak2Files[i].Filename.Length + 1;
                }
                ArchiveFileWrite.Write(OffsetListBegin_New);
                ArchiveFileWrite.Write(Pak2Files.Count);
                ArchiveFileWrite.Write(4);

                //Write filenames
                for (int i = 0; i < Pak2Files.Count; i++)
                {
                    BinaryUtils.WriteString(Pak2Files[i].Filename, ArchiveFileWrite);
                    ArchiveFileWrite.Write((byte)0x00);
                }

                //Write placeholder offsets for now, we'll correct them after writing the content
                OffsetListBegin = (int)ArchiveFileWrite.BaseStream.Position;
                for (int i = 0; i < Pak2Files.Count; i++)
                {
                    ArchiveFileWrite.Write(0);
                }

                //Write files
                for (int i = 0; i < Pak2Files.Count; i++)
                {
                    while (ArchiveFileWrite.BaseStream.Position % 4 != 0)
                    {
                        ArchiveFileWrite.Write((byte)0x00);
                    }
                    ArchiveFileWrite.Write(Pak2Files[i].Content);
                    Pak2Files[i].Offset = (int)ArchiveFileWrite.BaseStream.Position;
                }

                //Re-write offsets with correct values
                ArchiveFileWrite.BaseStream.Position = OffsetListBegin;
                for (int i = 0; i < Pak2Files.Count; i++)
                {
                    ArchiveFileWrite.Write(Pak2Files[i].Offset);
                }

                ArchiveFileWrite.Close();
                return PAKReturnType.SUCCESS;
            }
            catch (IOException e) { return PAKReturnType.FAIL_COULD_NOT_ACCESS_FILE; }
            catch (Exception e) { return PAKReturnType.FAIL_UNKNOWN; }
        }
    }
}
