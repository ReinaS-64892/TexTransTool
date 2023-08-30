#if UNITY_EDITOR

using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas.AdvancedSetting
{
    public struct DefaultCompress : IAdvancedSetting
    {
        public int Order => 1;

        public void AdvancedSetting(List<PropAndTexture2D> propAndTextures)
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
#endif
