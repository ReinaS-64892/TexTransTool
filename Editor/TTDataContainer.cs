#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool
{
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
    public struct MatPair
    {
        public Material Material;
        public Material SecondMaterial;

        public MatPair(Material material, Material secondMaterial)
        {
            Material = material;
            SecondMaterial = secondMaterial;
        }

        public static List<MatPair> ConvertMatPairList(Dictionary<Material, Material> MatDict)
        {
            var ret = new List<MatPair>();
            foreach (var item in MatDict)
            {
                ret.Add(new MatPair(item.Key, item.Value));
            }
            return ret;
        }
        public static Dictionary<Material, Material> ConvertMatDict(List<MatPair> MatPairs)
        {
            var ret = new Dictionary<Material, Material>();
            foreach (var item in MatPairs)
            {
                ret.Add(item.Material, item.SecondMaterial);
            }
            return ret;
        }
        public static List<MatPair> SwitchingList(List<MatPair> MatPairs)
        {
            var ret = new List<MatPair>();
            foreach (var item in MatPairs)
            {
                ret.Add(new MatPair(item.SecondMaterial, item.Material));
            }
            return ret;
        }
    }
}
#endif
