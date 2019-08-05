# Alien: Isolation PAK Tool

<img src="https://i.imgur.com/CNAPK4r.png" align="right" width="40%">

A tool to preview, export, and re-import files within Alien: Isolation's PAK archives.

The latest stable version can be downloaded [by clicking here](https://github.com/MattFiler/AlienPAK/raw/master/AlienPAK.exe).

On launch, the toolkit will automatically alert you if a new version is available.

***

This PAK Tool is available within the [Alien: Isolation Mod Tools](https://github.com/MattFiler/LegendPlugin) as of V0.6.3.0! Future updates will be ported, as development continues on this repo for the extended standalone version.


## Currently supported

- PAK2 (UI.PAK, ANIMATIONS.PAK)
  - Open archive
  - Add files
  - Replace files
  - Export files
  - Remove files
  
- Texture PAK (LEVEL_TEXTURES.ALL.PAK, GLOBAL_TEXTURES.ALL.PAK)
  - Open archive
  - Import files [experimental!]
  - Export files

- Models PAK (LEVEL_MODELS.PAK, GLOBAL_MODELS.PAK)
	- Open archive

- Scripts PAK (COMMANDS.PAK)
	- Open archive

- Material Mappings PAK (MATERIAL_MAPPINGS.PAK)
	- Open archive


## Coming soon

Intended functionality for upcoming versions includes: 
- Ability to configure material mappings (still looking at how the system works).
- Ability to export models (potential import support too, will see).
- Ability to export/import scripts (lots of work still to do).
- Ability to preview images when selecting them.


## Recommended tools

 * [JPEXS Flash Decompiler](https://github.com/jindrapetrik/jpexs-decompiler) is recommended for editing exported UI .GFX files.
 * [Pico Pixel](https://pixelandpolygon.com/) is recommended for viewing exported texture .DDS files.
 * [DirectXTex](https://github.com/microsoft/DirectXTex/releases) compiled binary is recommended for converting to/from .DDS formats.
 * [io_scene_aliens](https://forum.xentax.com/viewtopic.php?t=12079&start=90#p103131) Blender plugin is recommended for viewing exported models.


## Final mentions

 * The DDS header compiler used to export textures was created by [Cra0kalo](https://github.com/cra0kalo) and [Volfin](https://github.com/volfin). 
