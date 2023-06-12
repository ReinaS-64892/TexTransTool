#if UNITY_EDITOR
using System.Security.Cryptography;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Rs64.TexTransTool.TexturAtlas
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
        public IslandCacheObject(List<TraiangleIndex> Triange, List<Vector2> UV, List<Island> Island)
        {
            SetData(Triange, UV, Island);
        }
        public void SetData(List<TraiangleIndex> Triange, List<Vector2> UV, List<Island> Island)
        {
            Islands = Island;

            Hash = GenereatHash(Triange, UV);
        }

        public static byte[] GenereatHash(List<TraiangleIndex> Triange, List<Vector2> UV)
        {
            var datajson = JsonUtility.ToJson(new TrainagleAndUVpeas(Triange, UV));
            byte[] data = System.Text.Encoding.UTF8.GetBytes(datajson);

            return SHA1.Create().ComputeHash(data);
        }

        [Serializable]
        class TrainagleAndUVpeas
        {
            public List<TraiangleIndex> Triange;
            public List<Vector2> UV;

            public TrainagleAndUVpeas(List<TraiangleIndex> triange, List<Vector2> uV)
            {
                Triange = triange;
                UV = uV;
            }
        }
    }


    public class IslandCache : ScriptableObject
    {
        public IslandCacheObject CacheObject;
    }
}
#endif