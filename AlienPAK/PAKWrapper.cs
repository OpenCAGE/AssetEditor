using CATHODE;
using CathodeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    internal class PAKWrapper
    {
        CathodeFile _file = null;
        public CathodeFile File { get { return _file; } }

        PAKType _type = PAKType.NONE_SPECIFIED;
        public PAKType Type { get { return _type; } }

        public List<string> Load(string path)
        {
            List<string> files = new List<string>();
            switch (Path.GetFileName(path).ToUpper())
            {
                case "GLOBAL_TEXTURES.ALL.PAK":
                case "LEVEL_TEXTURES.ALL.PAK":
                    _file = new Textures(path);
                    _type = PAKType.TEXTURE;
                    for (int i = 0; i < ((Textures)_file).Entries.Count; i++)
                        files.Add(((Textures)_file).Entries[i].Name);
                    break;
                case "GLOBAL_MODELS.PAK":
                case "LEVEL_MODELS.PAK":
                    _file = new Models(path);
                    _type = PAKType.MODEL;
                    for (int i = 0; i < ((Models)_file).Entries.Count; i++)
                        files.Add(((Models)_file).Entries[i].Name);
                    break;
                case "MATERIAL_MAPPINGS.PAK":
                    _file = new MaterialMappings(path);
                    _type = PAKType.MATERIAL_MAPPINGS;
                    for (int i = 0; i < ((MaterialMappings)_file).Entries.Count; i++)
                        files.Add(((MaterialMappings)_file).Entries[i].MapFilename);
                    break;
                case "COMMANDS.PAK":
                    _file = new Commands(path);
                    _type = PAKType.SCRIPT;
                    for (int i = 0; i < ((Commands)_file).Entries.Count; i++)
                        files.Add(((Commands)_file).Entries[i].name);
                    break;
                /*
                case "LEVEL_SHADERS_DX11.PAK":
                case "BESPOKESHADERS_DX11.PAK":
                case "DEFERREDSHADERS_DX11.PAK":
                case "POSTPROCESSINGSHADERS_DX11.PAK":
                case "REQUIREDSHADERS_DX11.PAK":
                    _file = new Shaders(filename);
                    _type = AlienContentType.SHADER;
                    for (int i = 0; i < ((Shaders)AlienPAKs).Entries.Count; i++)
                        files.Add(((Shaders)AlienPAKs).Entries[i].name);
                    break;
                */
                case "ANIMATION.PAK":
                    _file = new PAK2(path);
                    _type = PAKType.ANIMATION;
                    for (int i = 0; i < ((PAK2)_file).Entries.Count; i++)
                        files.Add(((PAK2)_file).Entries[i].Filename);
                    break;
                case "UI.PAK":
                    _file = new PAK2(path);
                    _type = PAKType.UI;
                    for (int i = 0; i < ((PAK2)_file).Entries.Count; i++)
                        files.Add(((PAK2)_file).Entries[i].Filename);
                    break;
                default:
                    _file = null;
                    _type = PAKType.NONE_SPECIFIED;
                    MessageBox.Show("The selected PAK is currently unsupported.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
            return files;
        }

        public bool CanImport
        {
            get
            {
                switch (_type)
                {
                    case PAKType.SCRIPT:
                    case PAKType.NONE_SPECIFIED:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public bool CanExport
        {
            get
            {
                switch (_type)
                {
                    case PAKType.SCRIPT:
                    case PAKType.NONE_SPECIFIED:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public bool CanReplace
        {
            get
            {
                switch (_type)
                {
                    case PAKType.SCRIPT:
                    case PAKType.NONE_SPECIFIED:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public bool CanDelete
        {
            get
            {
                switch (_type)
                {
                    case PAKType.SCRIPT:
                    case PAKType.NONE_SPECIFIED:
                        return false;
                    default:
                        return true;
                }
            }
        }
    }

    public enum PAKType
    {
        TEXTURE,
        MODEL,
        UI,
        SCRIPT,
        ANIMATION,
        MATERIAL_MAPPINGS,
        SHADER,
        NONE_SPECIFIED
    };
}
