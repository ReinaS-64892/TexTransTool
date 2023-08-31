# About AvatarDomainDefinition

## Overview of this component

This component is a function that prevents the total number of materials (not material slots) and textures in the avatar from increasing, since only the materials of the renderer that each [TextureTransformer](TextureTransformer.md) is targeting can be changed. In the case of VRChatAvatar, it also functions as a marker to specify the [TexTransGroup](TexTransGroup.md) to be applied during Build.

This makes a big difference for [TextureBlender] (TextureBlender.md) and other renderers, as it will affect other renderers as well, whereas it could only affect the renderer of each target.

## Usage

### Getting Started

You must first add [TexTransGroup](TexTransGroup.md) or [TexTransParentGroup](TexTransParentGroup.md) to your GameObject.

From AvatarDomainDefinition.cs in TexTransTool/Runtime/Build,
or from the Inspector component in the additional TexTransTool/AvatarDomainDefinition.
You can add it to your game object.

## Properties

### Generate Custom Mip Map

This property generates a custom mip map that does not bleed color at UV borders.

However, the MipMap is not generated very quickly, so check this box only if you are concerned about blurring.

### PreviewAvatar

This property specifies the GameObject that will be the avatar's range when previewing.
