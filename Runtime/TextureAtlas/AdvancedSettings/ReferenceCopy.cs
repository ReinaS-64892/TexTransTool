#if UNITY_EDITOR
using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas.AdvancedSetting
{
    public class ReferenceCopy : IAdvancedSetting
    {
        public int Order => 1024;

        public string SousePropertyName;
        public string TargetPropertyName;

        public ReferenceCopy(string sousePropertyName, string targetPropertyName)
        {
            SousePropertyName = sousePropertyName;
            TargetPropertyName = targetPropertyName;
        }

        public void AdvancedSetting(List<PropAndTexture2D> propAndTextures)
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
