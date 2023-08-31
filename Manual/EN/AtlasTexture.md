# About AtlasTexture

## Overview of this component

This component allows multiple textures to be combined into a single texture, and when a costume is pulled from various places and there are many unused parts of the texture, only the necessary parts can be combined to reduce VRAM space without significantly affecting the appearance of the costume. The purpose of this component is to

## Usage

### Getting Started

From AtlasTexture.cs in TexTransTool/Runtime/TextureAtlas,
or from the Inspector component in the additional TexTransTool/AtlasTexture
You can add it to your game object.

### Basic Settings

- Add the parent of the object you want to atlas to TargetRoot
- Check the list of materials whose textures you want to atlas.
- Set the size offset for each material automatically with Automatic OffSet Setting or adjust manually
- Adjust AtlasSettings accordingly.

### About the target to be atlased.

All SkinnedMeshRenderer and MeshRenderer materials with IsTarget checked that are children of the game object added to TargetRoot will be targeted and atlased.

## Notes.

This component's main function is to reduce VRAM space, but it can increase it in some cases and settings, so handling it is a bit advanced.

This component is very slow and will freeze up the first time you do an Apply or Avatar build with UseIslandCash (see below) turned off.

Depending on the weight of the target avatar's mesh, a long one may take a minute or more.

## Properties

### TargetRoot

Property to set the parent object of the renderer.

### MaterialSelector And Offset, Channel

Selection of whether the checkbox under IsTarget should be atlased or not.

If you see a material that should not exist, or if there is no material that should exist, perform "ResearchRenders".

### Offset

Offset is a value to offset the size of the texture from the material. It is used to adjust the size of the texture if it is to be displayed small or if the original texture is small.

The "Automatic OffSet Setting" shown above will set the ratio based on the resolution of those textures and is recommended to be used if there is no particular reason to use it.

### Channel

This is like an atlasing channel, and is used to atlas two textures from those materials.

It is usually used for combining materials into one piece, so it may not be used very often.

### UseIslandCache

This property determines whether or not a cache is used when calculating UVs (islands) from UVs and triangles of a mesh,
Basically, on is recommended, but if you have problems with using the cache, turn it off.

If you want to remove the cache rather than not use it, delete the object named "IslandCash ~" in Assets/TexTransToolGenerates.


### AtlasTextureSize

The resolution of the atlased texture, where X is the width Y the height.

### IsMergeMaterial

Check this box to force a merge of materials into one.

Please note that it can reduce the number of materials, thus reducing SetPassCall, etc., but it cannot reduce the number of material slots.
If you want to reduce the number of slots, it is recommended to use with [Anatawa's AvatarOptimizer](https://github.com/anatawa12/AvatarOptimizer).

Please note that this is for advanced users as it has a large impact on the appearance of the image.

#### Property Bake Setting

When merging materials, if the material's color change differs from material to material, this setting allows the color change to be baked into the texture, making it easier to merge materials while maintaining their appearance.

- NotBake Do nothing, this is always the case when IsMergeMaterial is unchecked.
- Bake Normal setting, baking is performed to merge properties to the extent that no new textures are created.
- BakeAllProperty Set to merge all properties as much as possible, generating new textures to keep the look as much as possible. However, care should be taken as this will likely increase VRAM capacity.

#### MergeReferenceMaterial

A property that allows you to specify a new material to be assigned.

### ForceSetTexture

If checked, forces all atlased textures to be set for the material.

When unchecked, if a material's properties already have a texture, the properties will be set to the atlased texture.

### Padding

Distance between UVs.

### SortingType

Specifies how to sort the auto-generated UVs when creating an atlas.

- EvenlySpaced - Arranges the UVs in the proper order and according to the grid. (deprecated)
- NextFitDecreasingHeight - lay them out in order of height, starting from the bottom left. (deprecated)
- NextFitDecreasingHeightPlusFloorCeiling - Lays out the height in descending order, starting from the bottom left, and when the bottom left is no longer needed, fill in the top right as much as possible. (Recommended)

If you can implement a more efficient UV reordering algorithm, please submit an Issue or PullRequest.

### FineSettings

[FineSettingManual](AtlasTextureFineSetting.md)
