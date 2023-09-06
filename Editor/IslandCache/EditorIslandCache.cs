using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using System.Security.Cryptography;

namespace net.rs64.TexTransTool.EditorIsland
{
    public class EditorIslandCache : IIslandCache, IDisposable
    {
        private List<IslandCacheObject> CacheIslands;
        private readonly List<IslandCacheObject> diffCacheIslands;

        public EditorIslandCache()
        {
            CacheIslands = AssetSaveHelper.LoadAssets<IslandCache>().ConvertAll(i => i.CacheObject);
            diffCacheIslands = new List<IslandCacheObject>(CacheIslands);
        }
        public bool TryCache(List<Vector2> UV, List<TriangleIndex> Triangle, out List<Island> island)
        {
            var NawHash = IslandCacheObject.GenerateHash(Triangle, UV);

            foreach (var Cache in CacheIslands)
            {
                if (Cache.Hash.SequenceEqual(NawHash))
                {
                    island = Cache.Islands;
                    return true;
                }
            }

            island = null;
            return false;
        }
        public void AddCache(List<Vector2> UV, List<TriangleIndex> Triangle, List<Island> island)
        {
            CacheIslands.Add(new IslandCacheObject(Triangle, UV, island));
        }

        public void Dispose()
        {
            AssetSaveHelper.SaveAssets(CacheIslands.Except(diffCacheIslands).Select(i =>
            {
                var NI = ScriptableObject.CreateInstance<IslandCache>();
                NI.CacheObject = i; NI.name = "IslandCache";
                return NI;
            }));
        }

    }


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
}