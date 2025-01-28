//#define REGEN_SOUNDBANK_INFO

using AlienPAK.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AlienPAK.Sounds
{
    public static class SoundbankInfo
    {
        public static List<WwiseSound> Sounds { get; private set; }

        static SoundbankInfo()
        {
            byte[] content = Resources.soundbankinfo;
#if DEBUG && REGEN_SOUNDBANK_INFO
            (List<DialogueEvent> DialogueEvents, List<SoundFile> StreamedFiles, List<SoundBank> SoundBanks) = ParseXML("M:\\Modding\\SoundbanksInfo.xml");
            content = Serialize(CalcSoundFilesCombined(DialogueEvents, StreamedFiles, SoundBanks).OrderBy(o => o.SoundFile.Path).ToList());
            File.WriteAllBytes("../AlienPAK/Resources/soundbankinfo.bin", content);
#endif
            Sounds = Read(content);
        }

        private static (List<DialogueEvent>, List<SoundFile>, List<SoundBank>) ParseXML(string xmlPath)
        {
            XDocument doc = XDocument.Parse(File.ReadAllText(xmlPath));

            List<DialogueEvent> DialogueEvents = doc.Descendants("DialogueEvents")
                .Descendants("DialogueEvent")
                .Select(ev => new DialogueEvent
                {
                    Id = (string)ev.Attribute("Id"),
                    Name = (string)ev.Attribute("Name"),
                    Arguments = ev.Descendants("Argument")
                    .Select(arg => new DialogueEvent.Argument
                    {
                        Id = (string)arg.Attribute("Id"),
                        Name = (string)arg.Attribute("Name")
                    }).ToList(),
                })
                .ToList();

            List<SoundFile> StreamedFiles = doc.Descendants("StreamedFiles")
                .Descendants("File")
                .Select(file => new SoundFile
                {
                    Id = (string)file.Attribute("Id"),
                    Language = (string)file.Attribute("Language"),
                    ShortName = (string)file.Element("ShortName"),
                    Path = (string)file.Element("Path")
                })
                .ToList();

            List<SoundBank> SoundBanks = doc.Descendants("SoundBanks")
                .Descendants("SoundBank")
                .Select(sb => new SoundBank
                {
                    Id = (string)sb.Attribute("Id"),
                    Language = (string)sb.Attribute("Language"),
                    ShortName = (string)sb.Element("ShortName"),
                    Path = (string)sb.Element("Path"),
                    IncludedEvents = sb.Descendants("IncludedEvents").Descendants("Event")
                    .Select(ev => new SoundEvent
                    {
                        Id = (string)ev.Attribute("Id"),
                        Name = (string)ev.Attribute("Name")
                        //MaxAttenuation
                    }).ToList(),
                    IncludedDialogueEvents = sb.Descendants("IncludedDialogueEvents").Descendants("DialogueEvent")
                    .Select(ev => new SoundEvent
                    {
                        Id = (string)ev.Attribute("Id"),
                        Name = (string)ev.Attribute("Name")
                    }).ToList(),
                    ReferencedStreamedFiles = sb.Descendants("ReferencedStreamedFiles").Descendants("File").Select(ev => (string)ev.Attribute("Id")).ToList(),
                    IncludedFullFiles = sb.Descendants("IncludedFullFiles").Descendants("File")
                    .Select(file => new SoundFile
                    {
                        Id = (string)file.Attribute("Id"),
                        Language = (string)file.Attribute("Language"),
                        ShortName = (string)file.Element("ShortName"),
                        Path = (string)file.Element("Path")
                    }).ToList(),
                    IncludedPrefetchFiles = sb.Descendants("IncludedPrefetchFiles").Descendants("File")
                    .Select(file => new SoundFile
                    {
                        Id = (string)file.Attribute("Id"),
                        Language = (string)file.Attribute("Language"),
                        ShortName = (string)file.Element("ShortName"),
                        Path = (string)file.Element("Path")
                        //PrefetchMilliseconds
                    }).ToList()
                    //ExternalSources
                })
                .ToList();

            return (DialogueEvents, StreamedFiles, SoundBanks);
        }

        private static List<SoundFileCombined> CalcSoundFilesCombined(List<DialogueEvent> DialogueEvents, List<SoundFile> StreamedFiles, List<SoundBank> SoundBanks)
        {
            Dictionary<SoundFile, SoundFileCombined> dict = new Dictionary<SoundFile, SoundFileCombined>();

            foreach (SoundFile file in StreamedFiles)
            {
                if (!dict.TryGetValue(file, out SoundFileCombined comb))
                {
                    comb = new SoundFileCombined() { SoundFile = file };
                    dict[file] = comb;
                }
            }

            foreach (SoundBank bnk in SoundBanks)
            {
                foreach (string fileID in bnk.ReferencedStreamedFiles)
                {
                    var comb = dict.Values.FirstOrDefault(o => o.SoundFile.Id == fileID);
                    if (comb == null)
                    {
                        throw new Exception("unexpected"); //we should've had these added in the StreamedFiles above
                    }
                    comb.SoundBank_Referenced.Add(bnk);
                }

                foreach (SoundFile file in bnk.IncludedFullFiles)
                {
                    if (!dict.TryGetValue(file, out SoundFileCombined comb))
                    {
                        comb = new SoundFileCombined() { SoundFile = file };
                        dict[file] = comb;
                    }
                    comb.SoundBank_Included.Add(bnk);
                }

                foreach (SoundFile file in bnk.IncludedPrefetchFiles)
                {
                    if (!dict.TryGetValue(file, out SoundFileCombined comb))
                    {
                        comb = new SoundFileCombined() { SoundFile = file };
                        dict[file] = comb;
                    }
                    comb.SoundBank_IncludedPrefetch.Add(bnk);
                }
            }

            return dict.Values.ToList();
        }

        private static byte[] Serialize(List<SoundFileCombined> SoundFilesCombined)
        {
            byte[] content;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.BaseStream.SetLength(0);
                    writer.Write(SoundFilesCombined.Count);
                    foreach (SoundFileCombined sfc in SoundFilesCombined)
                    {
                        writer.Write(Convert.ToUInt32(sfc.SoundFile.Id));
                        writer.Write(sfc.SoundFile.Path);

                        writer.Write(sfc.SoundBank_Referenced.Count);
                        foreach (SoundBank sb in sfc.SoundBank_Referenced)
                        {
                            writer.Write(Convert.ToUInt32(sb.Id));
                            writer.Write(sb.Path);
                        }
                        writer.Write(sfc.SoundBank_Included.Count);
                        foreach (SoundBank sb in sfc.SoundBank_Included)
                        {
                            writer.Write(Convert.ToUInt32(sb.Id));
                            writer.Write(sb.Path);
                        }
                        writer.Write(sfc.SoundBank_IncludedPrefetch.Count);
                        foreach (SoundBank sb in sfc.SoundBank_IncludedPrefetch)
                        {
                            writer.Write(Convert.ToUInt32(sb.Id));
                            writer.Write(sb.Path);
                        }
                    }
                }
                content = stream.ToArray();
            }
            return content;
        }

        private static List<WwiseSound> Read(byte[] content)
        {
            List<WwiseSound> sounds;
            using (MemoryStream stream = new MemoryStream(content))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int count = reader.ReadInt32();
                    sounds = new List<WwiseSound>(count);

                    for (int i = 0; i < count; i++)
                    {
                        WwiseSound sound = new WwiseSound();
                        sound.Id = reader.ReadUInt32();
                        sound.Path = reader.ReadString();

                        sound.SoundBank_Referenced = new List<Tuple<uint, string>>(reader.ReadInt32());
                        for (int x = 0; x < sound.SoundBank_Referenced.Capacity; x++)
                        {
                            sound.SoundBank_Referenced.Add(new Tuple<uint, string>(reader.ReadUInt32(), reader.ReadString()));
                        }
                        sound.SoundBank_Included = new List<Tuple<uint, string>>(reader.ReadInt32());
                        for (int x = 0; x < sound.SoundBank_Included.Capacity; x++)
                        {
                            sound.SoundBank_Included.Add(new Tuple<uint, string>(reader.ReadUInt32(), reader.ReadString()));
                        }
                        sound.SoundBank_IncludedPrefetch = new List<Tuple<uint, string>>(reader.ReadInt32());
                        for (int x = 0; x < sound.SoundBank_IncludedPrefetch.Capacity; x++)
                        {
                            sound.SoundBank_IncludedPrefetch.Add(new Tuple<uint, string>(reader.ReadUInt32(), reader.ReadString()));
                        }
                        sounds.Add(sound);
                    }
                }
            }
            return sounds;
        }

        private class DialogueEvent
        {
            public string Id;
            public string Name;
            public List<Argument> Arguments;

            public class Argument
            {
                public string Id;
                public string Name;
            }
        }

        private class SoundFile : IEquatable<SoundFile>
        {
            public string Id;
            public string Language;
            public string ShortName;
            public string Path;

            public override bool Equals(object obj)
            {
                return Equals(obj as SoundFile);
            }

            public bool Equals(SoundFile other)
            {
                if (other == null)
                    return false;

                return string.Equals(Id, other.Id, StringComparison.Ordinal) &&
                       string.Equals(Language, other.Language, StringComparison.Ordinal) &&
                       string.Equals(ShortName, other.ShortName, StringComparison.Ordinal) &&
                       string.Equals(Path, other.Path, StringComparison.Ordinal);
            }

            public override int GetHashCode()
            {
                int hashCode = -82735865;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Language);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
                return hashCode;
            }

            public static bool operator ==(SoundFile left, SoundFile right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                if (left is null || right is null)
                    return false;

                return left.Equals(right);
            }

            public static bool operator !=(SoundFile left, SoundFile right)
            {
                return !(left == right);
            }
        }

        private class SoundEvent
        {
            public string Id;
            public string Name;
        }

        private class SoundBank
        {
            public string Id;
            public string Language;
            public string ShortName;
            public string Path;
            public List<SoundEvent> IncludedEvents;
            public List<SoundEvent> IncludedDialogueEvents;
            public List<string> ReferencedStreamedFiles;
            public List<SoundFile> IncludedFullFiles;
            public List<SoundFile> IncludedPrefetchFiles;
            //There's also ExternalSources here which I think is always empty? validate
        }

        private class SoundFileCombined
        {
            public SoundFile SoundFile = null;
            public List<SoundBank> SoundBank_Referenced = new List<SoundBank>();
            public List<SoundBank> SoundBank_Included = new List<SoundBank>();
            public List<SoundBank> SoundBank_IncludedPrefetch = new List<SoundBank>();
        }
    }

    public class WwiseSound
    {
        //NOTE: These IDs should be usable via Utilities.SoundHashedString

        public uint Id;
        public string Path;

        public List<Tuple<uint, string>> SoundBank_Referenced = new List<Tuple<uint, string>>();
        public List<Tuple<uint, string>> SoundBank_Included = new List<Tuple<uint, string>>();
        public List<Tuple<uint, string>> SoundBank_IncludedPrefetch = new List<Tuple<uint, string>>();
    }
}
