#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AdvancedSetting
{
    public struct Resize : IAdvancedSetting
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

        public void AdvancedSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in AdvancedSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                target.Texture2D = TextureLayerUtil.ResizeTexture(target.Texture2D, new Vector2Int(Size, Size));
            }
        }
    }


}
#endif
