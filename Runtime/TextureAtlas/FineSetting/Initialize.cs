#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AdvancedSetting
{
    public struct Initialize : IAdvancedSetting
    {
        public int Order => -1024;
        public void AdvancedSetting(List<PropAndTexture2D> propAndTextures)
        {
            foreach (var target in propAndTextures)
            {
                var EditableTex = new Texture2D(target.Texture2D.width, target.Texture2D.height, TextureFormat.RGBA32, true);
                EditableTex.SetPixels32(target.Texture2D.GetPixels32());
                EditableTex.Apply(true);
                EditableTex.name = target.Texture2D.name;
                target.Texture2D = EditableTex;
            }
        }
    }


}
#endif
