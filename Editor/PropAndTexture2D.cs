#if UNITY_EDITOR
using System;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [Serializable]
    internal class PropAndTexture2D
    {
        public string PropertyName;
        public Texture2D Texture2D;

        public PropAndTexture2D(string propertyName, Texture2D textures)
        {
            PropertyName = propertyName;
            Texture2D = textures;
        }
    }

}

#endif
