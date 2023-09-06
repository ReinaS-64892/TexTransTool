using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    public static class CollectionsUtility
    {
        public static List<Vector3> ZipListVector3(IReadOnlyList<Vector2> XY, IReadOnlyList<float> Z)
        {
            var count = XY.Count;
            if (count != Z.Count) { throw new System.ArgumentException("XY.Count != Z.Count"); }

            List<Vector3> result = new List<Vector3>(count);

            for (int index = 0; index < count; index += 1)
            {
                result.Add(new Vector3(XY[index].x, XY[index].y, Z[index]));
            }

            return result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(IReadOnlyDictionary<T, List<T2>> Souse, IReadOnlyDictionary<T, List<T2>> Add)
        {
            var result = ReadOnlyDictClone(Souse);
            foreach (var key in Add.Keys)
            {
                if (result.ContainsKey(key))
                {
                    result[key].AddRange(Add[key]);
                }
                else
                {
                    result.Add(key, Add[key]);
                }
            }
            return result;
        }

        public static Dictionary<T, T2> ReadOnlyDictClone<T, T2>(IReadOnlyDictionary<T, T2> Souse)
        {
            var result = new Dictionary<T, T2>();
            foreach (var keyValue in Souse)
            {
                result.Add(keyValue.Key, keyValue.Value);
            }
            return result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(IReadOnlyList<Dictionary<T, List<T2>>> Target)
        {
            var result = new Dictionary<T, List<T2>>();
            foreach (var zipTargetDict in Target)
            {
                foreach (var key in zipTargetDict.Keys)
                {
                    if (result.ContainsKey(key))
                    {
                        result[key].AddRange(zipTargetDict[key]);
                    }
                    else
                    {
                        result.Add(key, zipTargetDict[key]);
                    }
                }
            }
            return result;
        }

        public static List<Texture2D> GenerateTexturesList(IReadOnlyList<Material> SouseMaterials, IReadOnlyDictionary<Material, Texture2D> MatAndTexDict)
        {
            List<Texture2D> result = new List<Texture2D>();
            foreach (var mat in SouseMaterials)
            {
                if (MatAndTexDict.ContainsKey(mat))
                {
                    result.Add(MatAndTexDict[mat]);
                }
                else
                {
                    result.Add(null);
                }
            }
            return result;
        }

        public static Dictionary<T, T2> GenerateFromKvP<T, T2>(IReadOnlyList<KeyValuePair<T, T2>> KvPList)
        {
            Dictionary<T, T2> result = new Dictionary<T, T2>();
            foreach (var KvP in KvPList)
            {
                result.Add(KvP.Key, KvP.Value);
            }
            return result;
        }

                public static List<int> AllIndexOf<T>(this List<T> Meshes, T Mesh)
        {
            List<int> indexes = new List<int>();
            int I = 0;
            foreach (var findTargetMesh in Meshes)
            {
                if (findTargetMesh.Equals(Mesh)) indexes.Add(I);
                I += 1;
            }

            return indexes;
        }



    }
}