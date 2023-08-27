#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineSettng
{
    public struct Resize : IFineSetting
    {
        public int Order => -64;
        public int Size;
        public string PropertyNames;
        public PropertySelect select;

        public Resize(int resize_Size, string resize_PropertyNames, PropertySelect resize_select)
        {
            Size = resize_Size;
            PropertyNames = resize_PropertyNames;
            select = resize_select;

        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FiltTarget(PropertyNames, select, propAndTextures))
            {
                target.Texture2D = TextureLayerUtil.ResizeTexture(target.Texture2D, new Vector2Int(Size, Size));
            }
        }
    }


}
#endif
