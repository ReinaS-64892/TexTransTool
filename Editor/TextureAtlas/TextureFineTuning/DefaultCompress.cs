using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    public struct DefaultCompress : ITextureFineTuning
    {
        public int Order => 1;

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            var format = Compress.GetTextureFormat(Compress.FormatQuality.Normal);
            foreach (var target in propAndTextures)
            {
                if (target.Texture2D.format == format) { continue; }
                UnityEditor.EditorUtility.CompressTexture(target.Texture2D, format, UnityEditor.TextureCompressionQuality.Normal);
            }

        }
    }


}