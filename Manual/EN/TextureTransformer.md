# About TextureTransformer

First of all, there is no TextureTransformer for the available components, but most of the components in this tool inherit this TextureTransformer and are treated as TextureTransformers.

This means that most of the components of the tool are treated as TextureTransformers.

## One TextureTransformer per GameObject,

For example, [SimpleDecal](SimpleDecal.md) may directly adjust the scale of the attached game object. In this case, other Decal-like components will not behave properly, and [TexTransParentGroup] (TexTransParentGroup.md) will only execute the first TextureTransformer of the GameObject.
