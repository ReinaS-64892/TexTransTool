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
        public void FineSetting(List<PropAndTexture> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FiltTarget(PropatyNames, select, propAndTextures))
            {
                target.Texture2D = TextureLayerUtil.ResizeTexture(target.Texture2D, new Vector2Int(Size, Size));
            }
        }
    }


}
#endif