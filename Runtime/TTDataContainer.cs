#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rs64.TexTransTool
{
    public class TTDataContainer : ScriptableObject
    {
        [SerializeField] List<Mesh> _DistMeshs = new List<Mesh>();
        [SerializeField] List<Mesh> _GenereatMeshs = new List<Mesh>();

        [SerializeField] List<MatPea> _GenereatMatPears = new List<MatPea>();

        public List<Mesh> DistMeshs
        {
            set => _DistMeshs = value;
            get => _DistMeshs;
        }
        public List<Mesh> GenereatMeshs
        {
            set
            {
                if (_GenereatMeshs != null) AssetSaveHelper.DeletSubAssets(_GenereatMeshs);
                _GenereatMeshs = value;
                AssetSaveHelper.SaveSubAssets(this, _GenereatMeshs);
            }
            get => _GenereatMeshs;
        }
        public List<MatPea> GenereatMaterials
        {
            set
            {
                if (_GenereatMatPears != null) AssetSaveHelper.DeletSubAssets(_GenereatMatPears.ConvertAll(x => x.SecndMaterial));
                _GenereatMatPears = value;
                AssetSaveHelper.SaveSubAssets(this, _GenereatMatPears.ConvertAll(x => x.SecndMaterial));
            }
            get => _GenereatMatPears;
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
    [System.Serializable]
    public struct MatPea
    {
        public Material Material;
        public Material SecndMaterial;

        public MatPea(Material material, Material secndMaterial)
        {
            Material = material;
            SecndMaterial = secndMaterial;
        }

        public static List<MatPea> GeneratMatPeaList(Dictionary<Material, Material> MatDict)
        {
            var ret = new List<MatPea>();
            foreach (var item in MatDict)
            {
                ret.Add(new MatPea(item.Key, item.Value));
            }
            return ret;
        }
        public static Dictionary<Material, Material> GeneratMatDict(List<MatPea> MatPeas)
        {
            var ret = new Dictionary<Material, Material>();
            foreach (var item in MatPeas)
            {
                ret.Add(item.Material, item.SecndMaterial);
            }
            return ret;
        }
        public static List<MatPea> SwitchingdList(List<MatPea> MatPeas)
        {
            var ret = new List<MatPea>();
            foreach (var item in MatPeas)
            {
                ret.Add(new MatPea(item.SecndMaterial, item.Material));
            }
            return ret;
        }
    }
}
#endif