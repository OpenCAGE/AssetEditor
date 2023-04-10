using CATHODE;
using CathodeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    internal class PAKWrapper
    {
        //PAK file
        CathodeFile _file = null;
        public CathodeFile File { get { return _file; } }

        //Files within the PAK
        List<string> _contents = new List<string>();
        public List<string> Contents { get { return _contents; } }

        //Type of PAK
        PAKType _type = PAKType.NONE;
        public PAKType Type { get { return _type; } }

        /* Load a PAK file */
        public List<string> Load(string path)
        {
            Unload();

            _contents = new List<string>();
            switch (Path.GetFileName(path).ToUpper())
            {
                case "GLOBAL_TEXTURES.ALL.PAK":
                case "LEVEL_TEXTURES.ALL.PAK":
                    Textures texFile = new Textures(path);
                    _type = PAKType.TEXTURES;
                    for (int i = 0; i < texFile.Entries.Count; i++)
                    {
                        //NOTE: we ensure files end in .DDS here, since we do our own DDS processing to them later
                        string texPath = texFile.Entries[i].Name;
                        if (Path.GetExtension(texPath).ToUpper() != ".DDS") texPath += ".dds";
                        texFile.Entries[i].Name = texPath;
                        _contents.Add(texPath);
                    }
                    _file = texFile;
                    break;
                case "GLOBAL_MODELS.PAK":
                case "LEVEL_MODELS.PAK":
                    _file = new Models(path);
                    _type = PAKType.MODELS;
                    for (int i = 0; i < ((Models)_file).Entries.Count; i++)
                        _contents.Add(((Models)_file).Entries[i].Name);
                    break;
                case "MATERIAL_MAPPINGS.PAK":
                    _file = new MaterialMappings(path);
                    _type = PAKType.MATERIAL_MAPPINGS;
                    for (int i = 0; i < ((MaterialMappings)_file).Entries.Count; i++)
                        _contents.Add(((MaterialMappings)_file).Entries[i].MapFilename);
                    break;
                case "COMMANDS.PAK":
                    _file = new Commands(path);
                    _type = PAKType.COMMANDS;
                    for (int i = 0; i < ((Commands)_file).Entries.Count; i++)
                        _contents.Add(((Commands)_file).Entries[i].name);
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
                        _files.Add(((Shaders)AlienPAKs).Entries[i].name);
                    break;
                */
                case "ANIMATION.PAK":
                    _file = new PAK2(path);
                    _type = PAKType.ANIMATIONS;
                    for (int i = 0; i < ((PAK2)_file).Entries.Count; i++)
                        _contents.Add(((PAK2)_file).Entries[i].Filename);
                    break;
                case "UI.PAK":
                    _file = new PAK2(path);
                    _type = PAKType.UI;
                    for (int i = 0; i < ((PAK2)_file).Entries.Count; i++)
                        _contents.Add(((PAK2)_file).Entries[i].Filename);
                    break;
                default:
                    _file = null;
                    _type = PAKType.NONE;
                    MessageBox.Show("The selected PAK is currently unsupported.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
            return _contents;
        }

        /* Free-up from a loaded PAK */
        public void Unload()
        {
            if (_file == null) return;
            switch (_type)
            {
                case PAKType.TEXTURES:
                    ((Textures)_file).Entries.Clear();
                    break;
                case PAKType.MODELS:
                    ((Models)_file).Entries.Clear();
                    break;
                case PAKType.MATERIAL_MAPPINGS:
                    ((MaterialMappings)_file).Entries.Clear();
                    break;
                case PAKType.COMMANDS:
                    ((Commands)_file).Entries.Clear();
                    break;
                /*
                case PAKType.SHADER:
                    ((Shaders)_file).Entries.Clear();
                    break;
                */
                case PAKType.ANIMATIONS:
                case PAKType.UI:
                    ((PAK2)_file).Entries.Clear();
                    break;
            }
            _file = null;
        }

        /* Get a file from the loaded PAK as a byte array */
        public byte[] GetFileContent(string FileName)
        {
            switch (_type)
            {
                case PAKType.TEXTURES:
                    Textures.TEX4 texture = ((Textures)_file).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == FileName.Replace('\\', '/'));
                    Textures.TEX4_Part part = texture?.tex_HighRes?.Content != null ? texture.tex_HighRes : texture?.tex_LowRes?.Content != null ? texture.tex_LowRes : null;
                    if (part == null) return null;
                    DDSWriter _writer;
                    bool _crude = false;
                    switch (texture.Format)
                    {
                        case Textures.TextureFormat.DXGI_FORMAT_BC5_UNORM:
                            _writer = new DDSWriter(part.Content, part.Width, part.Height, 32, 0, TextureType.ATI2N);
                            break;
                        case Textures.TextureFormat.DXGI_FORMAT_BC1_UNORM:
                            _writer = new DDSWriter(part.Content, part.Width, part.Height, 32, 0, TextureType.Dxt1);
                            break;
                        case Textures.TextureFormat.DXGI_FORMAT_BC3_UNORM:
                            _writer = new DDSWriter(part.Content, part.Width, part.Height, 32, 0, TextureType.Dxt5);
                            break;
                        case Textures.TextureFormat.DXGI_FORMAT_B8G8R8A8_UNORM:
                            _writer = new DDSWriter(part.Content, part.Width, part.Height, 32, 0, TextureType.UNCOMPRESSED_GENERAL);
                            break;
                        case Textures.TextureFormat.DXGI_FORMAT_BC7_UNORM:
                        default:
                            _writer = new DDSWriter(part.Content, part.Width, part.Height);
                            _crude = true;
                            break;
                    }
                    return _crude ? _writer.ToBytesCrude() : _writer.ToBytes();
                case PAKType.MATERIAL_MAPPINGS:
                    //TODO!
                    MaterialMappings.Mapping map = ((MaterialMappings)_file).Entries.FirstOrDefault(o => o.MapFilename.Replace('\\', '/') == FileName.Replace('\\', '/'));
                    return null;
                    break;
                case PAKType.MODELS:
                    //TODO!!
                    return null;
                    break;
                case PAKType.ANIMATIONS:
                case PAKType.UI:
                    return ((PAK2)_file).Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == FileName.Replace('\\', '/'))?.Content;
                default:
                    return null;
            }
        }

        /* Functionality provided by the currently loaded PAK */
        public PAKFunction Functionality
        {
            get
            {
                switch (_type)
                {
                    case PAKType.SHADERS:
                    case PAKType.COMMANDS:
                    case PAKType.NONE:
                        return PAKFunction.NONE;
                    default:
                        return PAKFunction.CAN_EXPORT_FILES | PAKFunction.CAN_IMPORT_FILES | PAKFunction.CAN_REPLACE_FILES | PAKFunction.CAN_DELETE_FILES;
                }
            }
        }
    }

    public enum PAKType
    {
        TEXTURES,
        MODELS,
        UI,
        COMMANDS,
        ANIMATIONS,
        MATERIAL_MAPPINGS,
        SHADERS,

        NONE
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
