using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    public struct MipMapRemove : ITextureFineTuning
    {
        public int Order => -32;
        public string PropertyNames;
        public PropertySelect Select;

        public MipMapRemove(string propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;

        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var newTex = new Texture2D(target.Texture2D.width, target.Texture2D.height, TextureFormat.RGBA32, false);
                newTex.SetPixels32(target.Texture2D.GetPixels32());
                newTex.Apply();
                newTex.name = target.Texture2D.name;
                target.Texture2D = newTex;
            }

        }
    }


}