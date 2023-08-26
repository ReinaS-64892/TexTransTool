#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransTool.ShaderSupport;

namespace net.rs64.TexTransTool.TexturAtlas.FineSettng
{
    public interface IFineSetting
    {
        int Order { get; }
        void FineSetting(List<PropAndTexture2D> propAndTextures);
    }

    public enum PropatySelect
    {
        Equal,
        NotEqual,
    }

    public static class FineSettingUtil
    {
        public static IEnumerable<PropAndTexture2D> FiltTarget(string PropatyNames, PropatySelect select, List<PropAndTexture2D> propAndTextures)
        {
            var PropatyNameList = PropatyNames.Split(' ');
            switch (select)
            {
                default:
                case PropatySelect.Equal:
                    {
                        return propAndTextures.Where(x => PropatyNameList.Contains(x.PropertyName));

                    }
                case PropatySelect.NotEqual:
                    {
                        return propAndTextures.Where(x => !PropatyNameList.Contains(x.PropertyName));

                    }
            }
        }
    }


}
#endif