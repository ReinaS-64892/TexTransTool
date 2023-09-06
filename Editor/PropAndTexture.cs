#if UNITY_EDITOR
using System;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [Serializable]
    public class PropAndTexture
    {
        public string PropertyName;
        public Texture Texture2D;

        public PropAndTexture(string propertyName, Texture textures)
        {
            PropertyName = propertyName;
            Texture2D = textures;
        }
    }

}

#endif
