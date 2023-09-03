# About TextureFineTuning

## Significance

There are many problems when you use an atlased texture with AtlasTexture as it is.

For example, VRAM space will be increased, mipmaps will be generated for textures that do not need mipmaps, etc.

This is for making such minor adjustments.

## Common settings

### TargetPropertyName

Select the properties of the material for which you want to make those settings.

This is a special setting for FineSetting only, but you can use UseCustomProperty to specify multiple properties separated by spaces.

### Select;.

If it is Equal, it can be applied to the property specified in TargetPropertyName, and if it is Not Equal, it can be applied to the properties other than those specified in TargetPropertyName.

## Resize

Resize the specified texture.

This setting is intended to be used in such cases, since textures other than the main texture often have no problem even if they do not have a very large resolution, so reducing the texture size will also compress VRAM capacity.

- Size The size after resizing, to the power of two, such as 512 or 128, whichever is appropriate.

## Compress

A setting that allows you to specify compression for a given texture.

The atlas textures are formatted and compressed with NormalQuality by default, but it is assumed that the main texture is set to high quality and other textures to low quality, etc. The compression setting greatly affects VRAM capacity, so be careful when handling it.

### FormatQuality , CompressionQuality

The settings are not exactly the same, but they are similar to the import settings of Unity's Texture2D. [see](https://docs.unity3d.com/ja/2019.4/Manual/class-TextureImporterOverride.html)

## ReferenceCopy

This setting specifies source and target and assigns the source's texture to target.

If you assign the main texture to the outline texture, you can save VRAM for the outline texture... This setting is used in cases such as

### Source Property Name , Target Property Name

The source and target property names, respectively.

Note that this setting does not allow multiple space-separated properties to be specified.

## Remove

This setting removes the specified texture.

If textures are removed, VRAM can be reduced. If you want to use this setting, make sure that you have carefully considered whether it is really safe to remove the texture before setting it.

## MipMapRemove

Removes the mipmap of the specified texture.

Atlasized textures have a mipmap by default, but in many cases, such as mask textures, the mipmap does not affect the appearance without the mipmap, so it is better to remove it to save VRAM.
