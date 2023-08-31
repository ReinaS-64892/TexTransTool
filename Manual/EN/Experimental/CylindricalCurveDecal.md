# About CylindricalCurveDecal

# **This component is under development and experimental**.

## Overview of this component

Derived from SimpleDecal, this component can stretch decals in a curved shape, suitable for cylindrical objects such as arms, and conversely, please note that cylinders will explode if they are plugged into places where they are not.

## Usage

### Getting Started

From CylindricalCurveDecal.cs in TexTransTool/Runtime/Decal/Curve/Cylindrical,
or from additional TexTransTool/Experimental/CylindricalCurveDecal components in the Inspector
You can add it to your game object.

### How to apply a curve decal

- Set the target TargetRenderer
- Set DecalTexture
- If necessary, check UseFirsAndEnd and set each of them.
- Set [CylindricalCoordinatesSystem](CylindricalCoordinatesSystem.md) placed at the center of the arm etc.
- Create [Segment](CurevSegment.md) which will be the curve and add it to Segments, adjusting the position and roll of the segment
- Set size and loop count to desired values

Press Apply button to preview the curve decal.

## Properties

### TargetRenderer

Property to set the target renderer.

### UseFirstAndEnd

Check this box to change the decal texture only at the beginning and end of the curve decal.

#### FirstTexture

Property to set the first texture to be used.

#### EndTexture

Property that sets the last texture to be used.

### DecalTexture

Property to set the decal to be pasted.

### TargetPropertyName

This property selects which property of the material the decal will be applied to.

### Segments

Array property to set the [CurveSegment](CurveSegment.md) that will be the segment of the Bezier curve.

### CylindricalCoordinatesSystem

Property of reference to [CylindricalCoordinatesSystem](CylindricalCoordinatesSystem.md) that defines the cylindrical coordinate system.

### Size

Property of the size of a square of the decal that follows the curve.

### Loop Count

Property for the number of decal squares that follow the curve.

### Roll Mode

This property specifies how the slope of the curve is calculated.

### Draw Gizmo Always

Property to check to show gizmos of beige curves and squares even when they are not currently selected.

It is easier to see when checking the check box, for example, when adjusting the position of a segment.
