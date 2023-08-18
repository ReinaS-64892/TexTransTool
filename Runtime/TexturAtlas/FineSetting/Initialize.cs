#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Rs64.TexTransTool.TexturAtlas.FineSettng
{
    public struct Initialize : IFineSetting
    {
        public int Order => -1024;
        public void FineSetting(List<PropAndTexture> propAndTextures)
        {
            foreach (var target in propAndTextures)
            {
                target.Texture2D = UnityEngine.Object.Instantiate<Texture2D>(target.Texture2D);
            }
        }
    }


}
#endif