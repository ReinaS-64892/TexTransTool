#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;


namespace Rs64.TexTransTool.Decal
{
    [System.Serializable]
    public class DecalDataContainer : TTDataContainer
    {
        [SerializeField] List<Texture2D> _DecalCompiledTextures;
        [SerializeField] List<Texture2D> _DecaleBlendTexteres;

        public List<Texture2D> DecalCompiledTextures
        {
            set
            {
                foreach (var item in _DecalCompiledTextures)
                {
                    if (item != null)
                    {
                        item.Apply();
                    }
                }
                _DecalCompiledTextures = value;
            }
            get => _DecalCompiledTextures;
        }
        public List<Texture2D> DecaleBlendTexteres
        {
            set
            {
                if (_DecaleBlendTexteres != null) { AssetSaveHelper.DeletAssets(_DecaleBlendTexteres); }
                _DecaleBlendTexteres = value;
            }
            get => _DecaleBlendTexteres;
        }

    }


}
#endif