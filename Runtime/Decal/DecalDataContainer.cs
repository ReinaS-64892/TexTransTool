using System.Collections.Generic;
using UnityEngine;


namespace Rs64.TexTransTool.Decal
{
    public class DecalDataContainer : TTDataContainer
    {
        [SerializeField] List<MatAndTex> _DecalCompiledTextures;
        [SerializeField] List<MatAndTex> _DecaleBlendTexteres;

        public List<MatAndTex> DecalCompiledTextures
        {
            set
            {
                if (_DecalCompiledTextures != null) AssetSaveHelper.DeletAssets(_DecalCompiledTextures.ConvertAll(i => i.Texture));
                _DecalCompiledTextures = value;
                MatAndTex.TextureSet(_DecalCompiledTextures, AssetSaveHelper.SaveAssets(_DecalCompiledTextures.ConvertAll(i => i.Texture)));
            }
            get => _DecalCompiledTextures;
        }
        public List<MatAndTex> DecaleBlendTexteres
        {
            set
            {
                if (_DecaleBlendTexteres != null) AssetSaveHelper.DeletAssets(_DecaleBlendTexteres.ConvertAll(i => i.Texture));
                _DecaleBlendTexteres = value;
                MatAndTex.TextureSet(_DecaleBlendTexteres, AssetSaveHelper.SaveAssets(_DecaleBlendTexteres.ConvertAll(i => i.Texture)));
            }
            get => _DecaleBlendTexteres;
        }

    }

    [System.Serializable]
    public class MatAndTex
    {
        public Material Material;
        public Texture2D Texture;

        public MatAndTex()
        {
        }

        public MatAndTex(Material material, Texture2D texture)
        {
            Material = material;
            Texture = texture;
        }

        public static List<MatAndTex> TextureSet(List<MatAndTex> Target, List<Texture2D> Textures)
        {
            if (Target.Count != Textures.Count) throw new System.Exception("Target.Count != Textures.Count");
            var ret = new List<MatAndTex>(Target);

            for (int i = 0; i < ret.Count; i++)
            {
                ret[i].Texture = Textures[i];
            }

            return ret;
        }
    }
}