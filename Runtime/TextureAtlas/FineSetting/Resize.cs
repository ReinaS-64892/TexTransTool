#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    public struct Resize : IFineSetting
    {
        public int Order => -64;
        public int Size;
        public string PropertyNames;
        public PropertySelect Select;

        public Resize(int size, string propertyNames, PropertySelect select)
        {
            Size = size;
            PropertyNames = propertyNames;
            Select = select;

        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                target.Texture2D = TextureLayerUtil.ResizeTexture(target.Texture2D, new Vector2Int(Size, Size));
            }
        }
    }


}
#endif
