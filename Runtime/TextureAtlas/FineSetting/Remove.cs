#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.FineSettng
{
    public class Remove : IFineSetting
    {
        public int Order => -1025;
        public string PropertyNames;
        public PropertySelect select;

        public Remove(string remove_PropertyNames, PropertySelect remove_select)
        {
            PropertyNames = remove_PropertyNames;
            select = remove_select;
        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FiltTarget(PropertyNames, select, propAndTextures).ToArray())
            {
                propAndTextures.Remove(target);
            }
        }

    }


}
#endif
