# About TextureBlender

## Overview of this component

This component is intended to synthesize textures nondestructively, much like the layer function in image editing software.

Please note that this component is not like [SimpleDecal](SimpleDecal.md), which pastes the texture onto a mesh, but simply overlays it, so it is affected by the UVs of the mesh when used.

## Usage

### Getting Started

From TextureBlender.cs in TexTransTool/Runtime
or from TexTransTool/TextureBlender in the Inspector Add Components,
You can add it to your game object.

### How to blend textures

 - Add a component to a suitable GameObject using the above method.
 - Set the TargetRenderer to the renderer that has the material with the original texture you want to blend.
 - Select the material (use the checkbox to the left of the material's display to select it).
 - Set the texture you want to blend to BlendTexture
 - Set [BlendType](BlendType.md) if necessary.

Press the Apply button to preview the result of blending that texture.

## Properties

### TargetRenderer

Property to set the target renderer.

### MaterialSelect

This property selects the material with the target texture,
In the inspector, the material is selected by a checkbox in the material array.

### BlendTexture

This property sets the texture to be blended.

### BlendType

This property selects the blend mode for blending. [Details](BlendType.md)

### TargetPropatyName

This property selects which property of the material the source texture is to be made from.
