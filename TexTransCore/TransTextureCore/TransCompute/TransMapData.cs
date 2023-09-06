#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using net.rs64.TexTransCore.TransTextureCore.Utils;
namespace net.rs64.TexTransCore.TransTextureCore.TransCompute
{
    public class TransMapData
    {
        public TwoDimensionalMap<TransPixel> Map;
        public float DefaultPadding;

        public TransPixel this[int i] => Map.Array[i];

        public TransMapData(TwoDimensionalMap<TransPixel> map, float defaultPadding)
        {
            Map = map;
            DefaultPadding = defaultPadding;
        }
        public TransMapData(float defaultPadding, Vector2Int mapSize)
        {
            var array = CoreUtility.FilledArray(new TransPixel(Vector2.zero, defaultPadding), mapSize.x * mapSize.y);
            DefaultPadding = defaultPadding;
            Map = new TwoDimensionalMap<TransPixel>(array, mapSize);
        }
        public TransMapData(SerializableMap sMap)
        {
            Map = new TwoDimensionalMap<TransPixel>(sMap.Map.Select(I => new TransPixel(I.x, I.y, I.z)).ToArray(), sMap.MapSize);
            DefaultPadding = sMap.DefaultPadding;

        }
        public TransMapData()
        {
        }

        public Vector3[] GetVector3s()
        {
            var Ret = new Vector3[Map.Array.Length];
            for (int i = 0; i < Map.Array.Length; i += 1)
            {
                Ret[i] = Map.Array[i];
            }
            return Ret;
        }
        public void SetVector3s(Vector3[] vector3s)
        {
            for (int i = 0; i < vector3s.Length; i += 1)
            {
                Map.Array[i] = vector3s[i];
            }
        }
        [Serializable]
        public struct SerializableMap
        {
            public Vector3[] Map;
            public Vector2Int MapSize;
            public float DefaultPadding;
        }
        public SerializableMap ToSerializable()
        {
            return new SerializableMap()
            {
                Map = GetVector3s(),
                MapSize = Map.MapSize,
                DefaultPadding = DefaultPadding
            };
        }
    }

    public class TwoDimensionalMap<T>
    {
        public T[] Array;
        public Vector2Int MapSize;

        public T this[int i] { get => Array[i]; set => Array[i] = value; }
        public T this[int x, int y] { get => Array[GetIndexOn1D(new Vector2Int(x, y))]; set => Array[GetIndexOn1D(new Vector2Int(x, y))] = value; }

        public TwoDimensionalMap(T[] array, Vector2Int mapSize)
        {
            Array = array;
            MapSize = mapSize;
        }
        public TwoDimensionalMap(T defaultValue, Vector2Int mapSize)
        {
            Array = CoreUtility.FilledArray(defaultValue, mapSize.x * mapSize.y);
            MapSize = mapSize;
        }
        public TwoDimensionalMap(Vector2Int mapSize)
        {
            Array = new T[mapSize.x * mapSize.y];
            MapSize = mapSize;
        }
        public TwoDimensionalMap()
        {
        }

        public Vector2Int GetPosOn2D(int i)
        {
            return DimensionIndexUtility.ConvertIndex2D(i, MapSize.x);
        }
        public int GetIndexOn1D(Vector2Int pos)
        {
            return DimensionIndexUtility.TwoDToOneDIndex(pos, MapSize.x);
        }

        public T GetOn2DIndex(Vector2Int pos)
        {
            return Array[GetIndexOn1D(pos)];
        }
    }

    public struct TransPixel
    {
        public Vector2 Pos;
        public float Distance;

        public TransPixel(Vector2 pos, float distance)
        {
            Pos = pos;
            Distance = distance;
        }

        public TransPixel(float PosX, float PosY, float distance)
        {
            Pos = new Vector2(PosX, PosY);
            Distance = distance;
        }
        public TransPixel(Vector3 Value)
        {
            Pos = new Vector2(Value.x, Value.y);
            Distance = Value.z;
        }

        public static implicit operator Vector3(TransPixel v)
        {
            return new Vector3(v.Pos.x, v.Pos.y, v.Distance);
        }
        public static implicit operator TransPixel(Vector3 v)
        {
            return new TransPixel(v);
        }
    }
    public class TransColor
    {
        public Color Color;
        public float Distance;

        public TransColor(Color color, float distance)
        {
            Color = color;
            Distance = distance;
        }

        public static float[] GetDistanceArray(TransColor[] map)
        {
            var distanceArray = new float[map.Length];
            for (int i = 0; i < map.Length; i += 1)
            {
                distanceArray[i] = map[i].Distance;
            }
            return distanceArray;
        }
        public static void SetDistanceArray(TransColor[] map, float[] distanceArray)
        {
            if (map.Length != distanceArray.Length) { return; }
            for (int i = 0; i < map.Length; i += 1)
            {
                map[i].Distance = distanceArray[i];
            }
        }
        public static Color[] GetColorArray(TransColor[] map)
        {
            var colorArray = new Color[map.Length];
            for (int i = 0; i < map.Length; i += 1)
            {
                colorArray[i] = map[i].Color;
            }
            return colorArray;
        }
        public static void SetColorArray(TransColor[] map, Color[] colorArray)
        {
            if (colorArray.Length != map.Length) { return; }
            for (int i = 0; i < map.Length; i += 1)
            {
                map[i].Color = colorArray[i];
            }
        }

        public static Texture2D ConvertTexture2D(TwoDimensionalMap<TransColor> twoDimensionalMap)
        {
            var newTex = new Texture2D(twoDimensionalMap.MapSize.x, twoDimensionalMap.MapSize.y);
            newTex.SetPixels(GetColorArray(twoDimensionalMap.Array));
            newTex.Apply();
            return newTex;
        }


    }

}
#endif
