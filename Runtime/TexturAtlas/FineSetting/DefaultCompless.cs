#if UNITY_EDITOR

using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas.FineSettng
{
    public struct DefaultCompless : IFineSetting
    {
        public int Order => 1;

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            var format = Compless.GetTextureFormat(Compless.FromatQuality.Normal);
            foreach (var target in propAndTextures)
            {
                if (target.Texture2D.format == format) { continue; }
                UnityEditor.EditorUtility.CompressTexture(target.Texture2D, format, UnityEditor.TextureCompressionQuality.Normal);
            }

        }
    }


}
#endif
