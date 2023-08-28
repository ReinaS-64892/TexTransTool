#if UNITY_EDITOR
using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas.FineSettng
{
    public class RefarensCopy : IFineSetting
    {
        public int Order => 1024;

        public string SousePropertyName;
        public string TargetPropertyName;

        public RefarensCopy(string refarensCopy_SousePropertyName, string refarensCopy_TargetPropertyName)
        {
            SousePropertyName = refarensCopy_SousePropertyName;
            TargetPropertyName = refarensCopy_TargetPropertyName;
        }

        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            var Texture = propAndTextures.Find(x => x.PropertyName == SousePropertyName);
            if (Texture == null) return;
            var propAndTex = propAndTextures.Find(x => x.PropertyName == TargetPropertyName);
            if (propAndTex == null) propAndTex = new PropAndTexture2D(TargetPropertyName, null);
            propAndTex.Texture2D = Texture.Texture2D;
        }
    }


}
#endif
