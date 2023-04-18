#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Linq;
namespace Rs.TexturAtlasCompiler
{

    //[CreateAssetMenu(fileName = "AtlasMapObject", menuName = "RsProductEdit/AtlasMapObject", order = 0)]
    [Serializable]
    public class AtlasMapData //: ScriptableObject
    {
        public Vector2[,] Map;
        public float[,] DistansMap;
        public float DefaultPading;
        public Vector2Int MapSize;

        public AtlasMapData(Vector2[,] map, float[,] distansMap, float defaultPading, Vector2Int mapSize)
        {
            //var NewI = CreateInstance<AtlasMapData>();
            Map = map;
            DistansMap = distansMap;
            DefaultPading = defaultPading;
            MapSize = mapSize;
        }
        public AtlasMapData(float Pading, Vector2Int mapSize)
        {
            //var NewI = CreateInstance<AtlasMapData>();
            Map = new Vector2[mapSize.x, mapSize.y];
            DistansMap = new float[mapSize.x, mapSize.y];
            DefaultPading = Pading;
            foreach (var index in Utils.Reange2d(mapSize))
            {
                DistansMap[index.x, index.y] = Pading;
            }
            MapSize = mapSize;
        }
        public AtlasMapData()
        {
        }
    }
    [Serializable]
    public class TextureAndDistansMap
    {
        public Texture2D Texture2D;
        public float[,] DistansMap;

        public TextureAndDistansMap(Texture2D texture2D, float[,] distansMap)
        {
            this.Texture2D = texture2D;
            DistansMap = distansMap;
        }
        public TextureAndDistansMap(Texture2D texture2D, float DefoultDistans)
        {
            this.Texture2D = texture2D;
            DistansMap = new float[texture2D.width, texture2D.height];
            foreach (var index in Utils.Reange2d(new Vector2Int(texture2D.width, texture2D.height)))
            {
                DistansMap[index.x, index.y] = DefoultDistans;
            }
        }
    }

    public struct TraiangleIndex
    {
        public int zero;
        public int one;
        public int two;

        public TraiangleIndex(int zero, int one, int two)
        {
            this.zero = zero;
            this.one = one;
            this.two = two;
        }

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: { return zero; }
                    case 1: { return one; }
                    case 2: { return two; }
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0: { zero = value; break; }
                    case 1: { one = value; break; }
                    case 2: { two = value; break; }
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public int[] ToArray()
        {
            return new int[3] { zero, one, two };
        }
    }
}
#endif