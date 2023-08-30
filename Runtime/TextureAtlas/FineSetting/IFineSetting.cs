#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransTool.ShaderSupport;

namespace net.rs64.TexTransTool.TextureAtlas.AdvancedSetting
{
    public interface IAdvancedSetting
    {
        int Order { get; }
        void AdvancedSetting(List<PropAndTexture2D> propAndTextures);
    }

    public enum PropertySelect
    {
        Equal,
        NotEqual,
    }

    public static class AdvancedSettingUtil
    {
        public static IEnumerable<PropAndTexture2D> FilteredTarget(string PropertyNames, PropertySelect select, List<PropAndTexture2D> propAndTextures)
        {
            var PropertyNameList = PropertyNames.Split(' ');
            switch (select)
            {
                default:
                case PropertySelect.Equal:
                    {
                        return propAndTextures.Where(x => PropertyNameList.Contains(x.PropertyName));

                    }
                case PropertySelect.NotEqual:
                    {
                        return propAndTextures.Where(x => !PropertyNameList.Contains(x.PropertyName));

                    }
            }
        }
    }


}
#endif
