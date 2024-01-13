using System.Security.Cryptography;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;

namespace net.rs64.TexTransCore.Island
{
    [Serializable]
    internal class IslandCacheObject
    {
        public byte[] Hash;
        public List<Island> Islands;

        public IslandCacheObject(byte[] hash, List<Island> islands)
        {
            Hash = hash;
            Islands = islands;
        }
        public IslandCacheObject(List<TriangleIndex> triangle, List<Vector2> uv, List<Island> island)
        {
            SetData(triangle, uv, island);
        }
        public void SetData(List<TriangleIndex> triangle, List<Vector2> uv, List<Island> island)
        {
            Islands = island;

            Hash = GenerateHash(triangle, uv);
        }

        public static byte[] GenerateHash(IReadOnlyList<TriangleIndex> triangle, IReadOnlyList<Vector2> uv)
        {
            var dataJson = JsonUtility.ToJson(new TriangleAndUVpairs(new List<TriangleIndex>(triangle), uv));
            byte[] data = System.Text.Encoding.UTF8.GetBytes(dataJson);

            return SHA1.Create().ComputeHash(data);
        }

        [Serializable]
        class TriangleAndUVpairs
        {
            public List<TriangleIndex> Triangle;
            public List<Vector2> UV;

            public TriangleAndUVpairs(List<TriangleIndex> triangle, List<Vector2> uv)
            {
                Triangle = triangle;
                UV = uv;
            }
            public TriangleAndUVpairs(IReadOnlyList<TriangleIndex> triangle, IReadOnlyList<Vector2> uv)
            {
                Triangle = triangle.ToList();
                UV = uv.ToList();
            }


        }
    }


}
