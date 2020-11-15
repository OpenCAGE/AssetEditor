# Alien: Isolation Asset PAK Tool

<img src="https://i.imgur.com/HoVJuSo.png" align="right" width="40%">

A tool to browse and modify Alien: Isolation's various asset PAK archives.

The latest stable version can be downloaded [by clicking here](https://github.com/OpenCAGE/AlienPAK/raw/master/AlienPAK.exe).

On launch, the toolkit will automatically alert you if a new version is available.

Check out [CathodeEditorGUI](https://github.com/OpenCAGE/CathodeEditorGUI) to open the COMMANDS.PAK files.


## Currently supported

- [PAK2 (UI.PAK, ANIMATIONS.PAK)](https://github.com/OpenCAGE/AlienPAK/wiki/PAK2)
  - Open archive
  - Create archive
  - Add files
  - Replace files
  - Export files
  - Remove files
  
- [Texture PAK (LEVEL_TEXTURES.ALL.PAK, GLOBAL_TEXTURES.ALL.PAK)](https://github.com/OpenCAGE/AlienPAK/wiki/PAK-BIN-Format)
  - Open archive
  - Import files [experimental!]
  - Export files

- [Models PAK (LEVEL_MODELS.PAK, GLOBAL_MODELS.PAK)](https://github.com/MattFiler/OpenCAGE/wiki/PAK-BIN-Format)
	- Open archive

- [Material Mappings PAK (MATERIAL_MAPPINGS.PAK)](https://github.com/MattFiler/OpenCAGE/wiki/Material-Mappings)
	- Open archive
    - Replace files
    - Export files

- [Shaders PAK (*_SHADERS_DX11.PAK)](https://github.com/MattFiler/OpenCAGE/wiki/Shaders)
	- Open archive
    - Export files (without names)


## Recommended tools

 * [JPEXS Flash Decompiler](https://github.com/jindrapetrik/jpexs-decompiler) is recommended for editing exported UI .GFX files.
 * [Pico Pixel](https://pixelandpolygon.com/) is recommended for viewing exported texture .DDS files.
 * [DirectXTex](https://github.com/microsoft/DirectXTex/releases) compiled binary is recommended for converting to/from .DDS formats.
 * [io_scene_aliens](https://forum.xentax.com/viewtopic.php?t=12079&start=90#p103131) Blender plugin is recommended for viewing exported models.


## Final mentions

 * The DDS header compiler used to export textures was created by [Cra0kalo](https://github.com/cra0kalo) and [Volfin](https://github.com/volfin). 
