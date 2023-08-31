# About SimpleDecal

## Overview of this component

This component is intended for nondestructively applying a coat of arms or a mark to a mesh, and can do a wide range of things depending on the texture to which it is applied.

## Usage

### Getting Started

From SimpleDecalAvatarTag.cs in TexTransTool/Runtime/Decal,
or from additional TexTransTool/SimpleDecal components in the Inspector
You can add it to your game object.

### How to apply decals

 - Add a component to the appropriate GameObject using the above method.
 - Set TargetRenderer to the renderer whose mesh you want to apply decals to.
 - Set the texture you want to apply to DecalTexture.
 - Adjust the GameObject's position, Scale, MaxDistans, etc.
 - Press the Compile button to generate the attached texture.

Press the Apply button to preview the decal.

### Real-time preview

This function allows you to see texture changes, blend mode changes, position, orientation, and size changes in real time without having to Compile/Apply. EnableRealTimePreview".

Note that in some cases, the preview in blend mode may show a large difference from the Compile/Apply result due to a small amount of color error caused by the Shader display.

## Properties

### TargetRenderer

Property to set the target renderer.

### DecalTexture

Property to set the decal to be pasted.

### Scale

A property that allows you to adjust the size of the decal to be pasted, changing the X,Y of the Scale of the transforㇺ of that game object.

### MaxDistans

This property allows you to adjust the maximum depth of the decal to be applied, and changes the Z value of the game object's transform's Scale.
This property can be used to prevent decals from being accidentally attached to the white eye mesh when attaching decals to cheeks, etc.

### AdvancedMode

This is a check for a mode that allows detailed settings.

Below this are the properties that are displayed when AdvancedMode is checked.

### TargetRenderer(array)

When applying decals across multiple renderers, the frame can be increased and set by pressing the + button.

### FixedAspect

If checked, the Scale value will be a float and the decal will be applied in the same ratio as the aspect ratio of the image.

If unchecked, the Scale value will be two floats, ignoring the aspect ratio of the image, and the decal will be applied with X as the width and Y as the height.

This is mainly used when applying a gradient to hair.

If AdvancedMode is unchecked, this property will be checked.

### BlendType

This property selects the blend mode when the decal is blended with the original image. [Details](BlendType.md)

If AdvancedMode is not checked, this property is set to Normal.

### TargetPropertyName

This property selects which property of the material the decal will be applied to.

### PolygonCulling

This property allows you to adjust the conditions under which the polygon is culled.
Vartex is recommended if there is no particular reason.

 - Vartex Vertex-based culling.
 - Edge Edge-based culling. Edge Culls edge-based culling when no vertex is within the area to be decaled.
 - EdgeAndCenterRey In addition to edge-based culling, a pseudo ray-cast is performed from the center. EdgeAndCenterRey Culls edge-based culling with a pseudo ray-cast from the center.

If AdvancedMode is unchecked, this property is set to Vartex.

### SideChek
If checked, the polygon will be culled if it is the back side of the area to be decaled, and the decal will not be applied.

If not checked, the decal will be applied to the back side.

Unchecking the checkbox when applying a gradient to the hair is a good idea.

If AdvancedMode is unchecked, this property will be checked.
