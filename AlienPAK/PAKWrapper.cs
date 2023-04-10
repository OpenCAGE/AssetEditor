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

        /* Load a PAK file */
        public List<string> Load(string path)
        {
            Unload();
            
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

        /* Free-up from a loaded PAK */
        public void Unload()
        {
            if (_file == null) return;
            switch (_type)
            {
                case PAKType.TEXTURE:
                    ((Textures)_file).Entries.Clear();
                    break;
                case PAKType.MODEL:
                    ((Models)_file).Entries.Clear();
                    break;
                case PAKType.MATERIAL_MAPPINGS:
                    ((MaterialMappings)_file).Entries.Clear();
                    break;
                case PAKType.SCRIPT:
                    ((Commands)_file).Entries.Clear();
                    break;
                /*
                case PAKType.SHADER:
                    ((Shaders)_file).Entries.Clear();
                    break;
                */
                case PAKType.ANIMATION:
                case PAKType.UI:
                    ((PAK2)_file).Entries.Clear();
                    break;
            }
            _file = null;
        }

        /* Get a file from the loaded PAK as a byte array */
        public byte[] GetFileContent(string FileName)
        {
            //TODO: model handling
            switch (_type)
            {
                case PAKType.TEXTURE:
                    Textures.TEX4 texture = ((Textures)_file).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == FileName.Replace('\\', '/'));
                    if (texture != null && texture.tex_HighRes != null)
                        return texture.tex_HighRes.Content; //new DDSWriter(texture.tex_HighRes.Content, texture.tex_HighRes.Width, texture.tex_HighRes.Height).Save;
                    if (texture != null && texture.tex_LowRes != null)
                        return texture.tex_LowRes.Content;
                    return null;
                case PAKType.ANIMATION:
                case PAKType.UI:
                    return ((PAK2)_file).Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == FileName.Replace('\\', '/'))?.Content;
                default:
                    return null;
            }
        }

        /* Functionality provided by the currently loaded PAK */
        public PAKFunction Function
        {
            get
            {
                switch (_type)
                {
                    case PAKType.SCRIPT:
                    case PAKType.NONE_SPECIFIED:
                        return PAKFunction.NONE;
                    default:
                        return PAKFunction.CAN_EXPORT_FILES | PAKFunction.CAN_IMPORT_FILES | PAKFunction.CAN_REPLACE_FILES | PAKFunction.CAN_DELETE_FILES;
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

    [Flags]
    public enum PAKFunction
    {
        NONE = 0,
        CAN_EXPORT_FILES = 1,
        CAN_IMPORT_FILES = 2,
        CAN_REPLACE_FILES = 4,
        CAN_DELETE_FILES = 8,
    }
}
