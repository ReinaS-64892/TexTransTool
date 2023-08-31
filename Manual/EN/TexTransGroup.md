# About TexTransGroup

## Overview of this component

This component compiles and applies TextureTransformers in TextureTransformers at once.
This component can compile and apply TextureTransformers in TextureTransformers at once, and since they are executed in order from the top, they can be executed after TextureTransformers that modify UVs (e.g. AtlasSet).

## Usage

### Getting Started

From TexTransGroupAvatarTag.cs in TexTransTool/Runtime,
or from TexTransTool/TexTransGroup in the Inspector of Additions
You can add this to your game object.

### How to do it all at once

- Set the TextureTransformers you want to execute all at once to the properties of an array called TextureTransformers

Compile all of them in order from the top,
Apply to preview them all.

## Properties

### TextureTransformers

This is an array of TextureTransformers, which will be used for compile and apply.

However, if GameObjects with those TextureTransformers are disabled, they will be ignored.
