#if UNITY_EDITOR
using System.Security.Cryptography;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.Island
{
    [Serializable]
    public class IslandCacheObject
    {
        public byte[] Hash;
        public List<Island> Islands;

        public IslandCacheObject(byte[] hash, List<Island> islands)
        {
            Hash = hash;
            Islands = islands;
        }
        public IslandCacheObject(List<TriangleIndex> Triangle, List<Vector2> UV, List<Island> Island)
        {
            SetData(Triangle, UV, Island);
        }
        public void SetData(List<TriangleIndex> Triangle, List<Vector2> UV, List<Island> Island)
        {
            Islands = Island;

            Hash = GenerateHash(Triangle, UV);
        }

        public static byte[] GenerateHash(IReadOnlyList<TriangleIndex> Triangle, IReadOnlyList<Vector2> UV)
        {
            var dataJson = JsonUtility.ToJson(new TriangleAndUVpairs(new List<TriangleIndex>(Triangle), UV));
            byte[] data = System.Text.Encoding.UTF8.GetBytes(dataJson);

            return SHA1.Create().ComputeHash(data);
        }

        [Serializable]
        class TriangleAndUVpairs
        {
            public List<TriangleIndex> Triangle;
            public List<Vector2> UV;

            public TriangleAndUVpairs(List<TriangleIndex> triangle, List<Vector2> uV)
            {
                Triangle = triangle;
                UV = uV;
            }
            public TriangleAndUVpairs(IReadOnlyList<TriangleIndex> triangle, IReadOnlyList<Vector2> uV)
            {
                Triangle = triangle.ToList();
                UV = uV.ToList();
            }


        }
    }


    public class IslandCache : ScriptableObject
    {
        public IslandCacheObject CacheObject;
    }
}
#endif
