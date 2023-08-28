#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.TextureAtlas.FineSettng
{
    public struct MipMapRemove : IFineSetting
    {
        public int Order => -32;
        public string PropertyNames;
        public PropertySelect select;

        public MipMapRemove(string mipMapRemove_PropertyNames, PropertySelect mipMapRemove_select)
        {
            PropertyNames = mipMapRemove_PropertyNames;
            select = mipMapRemove_select;

        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FiltTarget(PropertyNames, select, propAndTextures))
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
#endif
