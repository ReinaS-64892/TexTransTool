#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineSettng
{
    public struct Initialize : IFineSetting
    {
        public int Order => -1024;
        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in propAndTextures)
            {
                var Editabletex = new Texture2D(target.Texture2D.width, target.Texture2D.height, TextureFormat.RGBA32, true);
                Editabletex.SetPixels32(target.Texture2D.GetPixels32());
                Editabletex.Apply(true);
                Editabletex.name = target.Texture2D.name;
                target.Texture2D = Editabletex;
            }
        }
    }


}
#endif
