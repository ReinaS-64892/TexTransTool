#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;


namespace net.rs64.TexTransTool.Decal
{
    [System.Serializable]
    public class DecalDataContainer : TTDataContainer
    {
        [SerializeField] List<Texture2D> _DecaleBlendTexteres;

        public List<Texture2D> DecaleBlendTexteres
        {
            set => _DecaleBlendTexteres = value;
            get => _DecaleBlendTexteres;
        }

    }


}
#endif