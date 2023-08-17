#if UNITY_EDITOR
using System.Collections.Generic;

namespace Rs64.TexTransTool.TexturAtlas.FineSettng
{
    public class RefarensCopy : IFineSetting
    {
        public int Order => 1024;

        public string SousePropatyName;
        public string TargetPropatyName;

        public void FineSetting(List<PropAndTexture> propAndTextures)
        {
            var Texture = propAndTextures.Find(x => x.PropertyName == SousePropatyName);
            if (Texture == null) return;
            propAndTextures.Find(x => x.PropertyName == TargetPropatyName).Texture2D = Texture.Texture2D;
        }
    }


}
#endif