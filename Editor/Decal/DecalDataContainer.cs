using System.Collections.Generic;
using UnityEngine;


namespace net.rs64.TexTransTool.Decal
{
    [System.Serializable]
    public class DecalDataContainer : TTDataContainer
    {
        [SerializeField] List<Texture2D> _DecalBlendTextures;

        public List<Texture2D> DecalBlendTextures
        {
            set => _DecalBlendTextures = value;
            get => _DecalBlendTextures;
        }

    }


}