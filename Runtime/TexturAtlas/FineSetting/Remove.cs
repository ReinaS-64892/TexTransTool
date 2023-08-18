#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace Rs64.TexTransTool.TexturAtlas.FineSettng
{
    public class Remove : IFineSetting
    {
        public int Order => -1025;
        public string PropatyNames;
        public PropatySelect select;

        public Remove(string remove_PropatyNames, PropatySelect remove_select)
        {
            PropatyNames = remove_PropatyNames;
            select = remove_select;
        }

        public void FineSetting(List<PropAndTexture> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FiltTarget(PropatyNames, select, propAndTextures).ToArray())
            {
                propAndTextures.Remove(target);
            }
        }

    }


}
#endif