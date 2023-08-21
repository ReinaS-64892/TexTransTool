#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool.TexturAtlas.FineSettng
{
    public struct MipMapRemove : IFineSetting
    {
        public int Order => -32;
        public string PropatyNames;
        public PropatySelect select;

        public MipMapRemove(string mipMapRemove_PropatyNames, PropatySelect mipMapRemove_select)
        {
            PropatyNames = mipMapRemove_PropatyNames;
            select = mipMapRemove_select;

        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FiltTarget(PropatyNames, select, propAndTextures))
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