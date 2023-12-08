#if UNITY_EDITOR
using System;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [Serializable]
    internal class PropAndTexture
    {
        public string PropertyName;
        public Texture Texture;

        public PropAndTexture(string propertyName, Texture textures)
        {
            PropertyName = propertyName;
            Texture = textures;
        }
    }

}

#endif
