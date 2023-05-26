using System.Collections.Generic;
using UnityEngine;


namespace Rs64.TexTransTool.Decal
{
    public class DecalDataContainer : TTDataContainer
    {
        [SerializeField] List<Texture2D> _DecalCompiledTextures;
        [SerializeField] List<Texture2D> _DecaleBlendTexteres;

        public List<Texture2D> DecalCompiledTextures
        {
            set
            {
                if (_DecalCompiledTextures != null) AssetSaveHelper.DeletAssets(_DecalCompiledTextures);
                _DecalCompiledTextures = AssetSaveHelper.SaveAssets(value);
            }
            get => _DecalCompiledTextures;
        }
        public List<Texture2D> DecaleBlendTexteres
        {
            set
            {
                if (_DecaleBlendTexteres != null) AssetSaveHelper.DeletAssets(_DecaleBlendTexteres);
                _DecaleBlendTexteres = value;
            }
            get => _DecaleBlendTexteres;
        }

    }


}