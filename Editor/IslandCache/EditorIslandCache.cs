#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using System.Security.Cryptography;

namespace net.rs64.TexTransTool.EditorIsland
{
    internal class EditorIslandCache : IIslandCache
    {
        private static List<IslandCacheObject> s_cacheIslands;

        public EditorIslandCache()
        {
            s_cacheIslands ??= AssetSaveHelper.LoadAssets<IslandCache>().ConvertAll(i => i.CacheObject);
        }
        public bool TryCache(List<Vector2> uv, List<TriangleIndex> triangle, out List<Island> island)
        {
            var nawHash = IslandCacheObject.GenerateHash(triangle, uv);

            foreach (var cache in s_cacheIslands)
            {
                if (cache.Hash.SequenceEqual(nawHash))
                {
                    island = cache.Islands;
                    return true;
                }
            }

            island = null;
            return false;
        }
        public void AddCache(List<Vector2> uv, List<TriangleIndex> triangle, List<Island> island)
        {
            var newCache = new IslandCacheObject(triangle, uv, island);
            s_cacheIslands.Add(newCache);

            var serializableNewCache = ScriptableObject.CreateInstance<IslandCache>();
            serializableNewCache.CacheObject = newCache; serializableNewCache.name = "IslandCache";
            AssetSaveHelper.SaveAsset(serializableNewCache);
        }


    }


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
#endif