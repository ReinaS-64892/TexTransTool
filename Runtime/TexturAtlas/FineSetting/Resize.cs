#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Rs64.TexTransTool.TexturAtlas.FineSettng
{
    public struct Resize : IFineSetting
    {
        public int Order => -64;
        public int Size;
        public string PropatyNames;
        public PropatySelect select;

        public Resize(int resize_Size, string resize_PropatyNames, PropatySelect resize_select)
        {
            Size = resize_Size;
            PropatyNames = resize_PropatyNames;
            select = resize_select;

        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FiltTarget(PropatyNames, select, propAndTextures))
            {
                target.Texture2D = TextureLayerUtil.ResizeTexture(target.Texture2D, new Vector2Int(Size, Size));
            }
        }
    }


}
#endif