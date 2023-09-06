using System;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;

namespace net.rs64.TexTransTool
{
    [Serializable]
    public class PropAndAtlasTex
    {
        public string PropertyName;
        public TwoDimensionalMap<TransColor> AtlasTexture;

        public PropAndAtlasTex(TwoDimensionalMap<TransColor> texture2D, string propertyName)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }
        public PropAndAtlasTex(string propertyName, TwoDimensionalMap<TransColor> texture2D)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }

        public static explicit operator PropAndTexture2D(PropAndAtlasTex s)
        {
            return new PropAndTexture2D(s.PropertyName, TransColor.ConvertTexture2D(s.AtlasTexture));
        }
    }

}