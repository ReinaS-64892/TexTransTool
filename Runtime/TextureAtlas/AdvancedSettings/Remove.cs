#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.AdvancedSetting
{
    public class Remove : IAdvancedSetting
    {
        public int Order => -1025;
        public string PropertyNames;
        public PropertySelect Select;

        public Remove(string propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;
        }

        public void AdvancedSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in AdvancedSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures).ToArray())
            {
                propAndTextures.Remove(target);
            }
        }

    }


}
#endif
