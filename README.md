# Alien: Isolation PAK Tool

<img src="https://i.imgur.com/CNAPK4r.png" align="right" width="40%">

A tool to preview, export, and re-import files within Alien: Isolation's PAK archives.

The latest stable version can be downloaded [by clicking here](https://github.com/MattFiler/AlienPAK/raw/master/AlienPAK.exe).

On launch, the toolkit will automatically alert you if a new version is available.

***

This PAK Tool is available within the [Alien: Isolation Mod Tools](https://github.com/MattFiler/LegendPlugin) as of V0.6.3.0! Future updates will be ported, as development continues on this repo for the extended standalone version.


## Currently supported

- [PAK2 (UI.PAK, ANIMATIONS.PAK)](https://github.com/MattFiler/AlienPAK/wiki/PAK2)
  - Open archive
  - Create archive
  - Add files
  - Replace files
  - Export files
  - Remove files
  
- [Texture PAK (LEVEL_TEXTURES.ALL.PAK, GLOBAL_TEXTURES.ALL.PAK)](https://github.com/MattFiler/AlienPAK/wiki/PAK-BIN-Format)
  - Open archive
  - Import files [experimental!]
  - Export files

- [Models PAK (LEVEL_MODELS.PAK, GLOBAL_MODELS.PAK)](https://github.com/MattFiler/AlienPAK/wiki/PAK-BIN-Format)
	- Open archive

- [Scripts PAK (COMMANDS.PAK)](https://github.com/MattFiler/AlienPAK/wiki/Cathode-Scripts)
	- Open archive

- [Material Mappings PAK (MATERIAL_MAPPINGS.PAK)](https://github.com/MattFiler/AlienPAK/wiki/Material-Mappings)
	- Open archive
    - Replace files
    - Export files

- [Shaders PAK (*_SHADERS_DX11.PAK)](https://github.com/MattFiler/AlienPAK/wiki/Shaders)
	- Open archive
    - Export files (without names)


## Coming soon

Intended functionality for upcoming versions includes: 
- Expanded shader support for importing and understanding of Cathode headers.
- Functionality to batch export files from any archive type.
- Ability to view all materials in a level for editing material mappings.
- A tool to let you view material data (textures/colours/etc).
- Export options for models (potential import support too, will see).
- Ability to export/import scripts (lots of work still to do).
- Preview images in archives when selecting them.


## Recommended tools

 * [JPEXS Flash Decompiler](https://github.com/jindrapetrik/jpexs-decompiler) is recommended for editing exported UI .GFX files.
 * [Pico Pixel](https://pixelandpolygon.com/) is recommended for viewing exported texture .DDS files.
 * [DirectXTex](https://github.com/microsoft/DirectXTex/releases) compiled binary is recommended for converting to/from .DDS formats.
 * [io_scene_aliens](https://forum.xentax.com/viewtopic.php?t=12079&start=90#p103131) Blender plugin is recommended for viewing exported models.


## Final mentions

 * The DDS header compiler used to export textures was created by [Cra0kalo](https://github.com/cra0kalo) and [Volfin](https://github.com/volfin). 
