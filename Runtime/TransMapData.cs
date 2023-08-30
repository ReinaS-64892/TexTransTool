#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace net.rs64.TexTransTool
{
    public class TransMapData
    {
        public TowDMap<PosAndDistance> Map;
        public float DefaultPadding;

        public PosAndDistance this[int i] => Map.Array[i];

        public TransMapData(TowDMap<PosAndDistance> map, float defaultPadding)
        {
            Map = map;
            DefaultPadding = defaultPadding;
        }
        public TransMapData(float defaultPadding, Vector2Int mapSize)
        {
            var array = Utils.FilledArray(new PosAndDistance(Vector2.zero, defaultPadding), mapSize.x * mapSize.y);
            DefaultPadding = defaultPadding;
            Map = new TowDMap<PosAndDistance>(array, mapSize);
        }
        public TransMapData(SerializableMap sMap)
        {
            Map = new TowDMap<PosAndDistance>(sMap.Map.Select(I => new PosAndDistance(I.x, I.y, I.z)).ToArray(), sMap.MapSize);
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

    [Serializable]
    public class PropAndAtlasTex
    {
        public string PropertyName;
        public TransTargetTexture AtlasTexture;

        public PropAndAtlasTex(TransTargetTexture texture2D, string propertyName)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }
        public PropAndAtlasTex(string propertyName, TransTargetTexture texture2D)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }

        public static explicit operator PropAndTexture2D(PropAndAtlasTex s)
        {
            return new PropAndTexture2D(s.PropertyName, s.AtlasTexture.Texture2D);
        }
    }

    [Serializable]
    public class TransTargetTexture
    {
        public Texture2D Texture2D;
        /// <summary>
        /// テクスチャーの本当の加増解像度と同じサイズのマップ
        /// </summary>
        public TowDMap<float> DistanceMap;

        public TransTargetTexture(Texture2D texture2D, TowDMap<float> distanceMap)
        {
            Texture2D = texture2D;
            DistanceMap = distanceMap;
        }
        public TransTargetTexture(Vector2Int Size, Color DefaultColor, float DefaultPadding)
        {
            Texture2D = Utils.CreateFillTexture(Size, DefaultColor);
            DistanceMap = new TowDMap<float>(DefaultPadding, Size);
        }

    }

    public class TowDMap<T>
    {
        public T[] Array;
        public Vector2Int MapSize;

        public T this[int i] { get => Array[i]; set => Array[i] = value; }
        public T this[int x, int y] { get => Array[GetIndexOn1D(new Vector2Int(x, y))]; set => Array[GetIndexOn1D(new Vector2Int(x, y))] = value; }

        public TowDMap(T[] array, Vector2Int mapSize)
        {
            Array = array;
            MapSize = mapSize;
        }
        public TowDMap(T defaultValue, Vector2Int mapSize)
        {
            Array = Utils.FilledArray(defaultValue, mapSize.x * mapSize.y);
            MapSize = mapSize;
        }
        public TowDMap( Vector2Int mapSize)
        {
            Array = new T[mapSize.x * mapSize.y];
            MapSize = mapSize;
        }
        public TowDMap()
        {
        }

        public Vector2Int GetPosOn2D(int i)
        {
            return Utils.ConvertIndex2D(i, MapSize.x);
        }
        public int GetIndexOn1D(Vector2Int pos)
        {
            return Utils.TwoDToOneDIndex(pos, MapSize.x);
        }

        public T GetOn2DIndex(Vector2Int pos)
        {
            return Array[GetIndexOn1D(pos)];
        }
    }

    public struct PosAndDistance
    {
        public Vector2 Pos;
        public float Distance;

        public PosAndDistance(Vector2 pos, float distance)
        {
            Pos = pos;
            Distance = distance;
        }

        public PosAndDistance(float PosX, float PosY, float distance)
        {
            Pos = new Vector2(PosX, PosY);
            Distance = distance;
        }
        public PosAndDistance(Vector3 Value)
        {
            Pos = new Vector2(Value.x, Value.y);
            Distance = Value.z;
        }

        public static implicit operator Vector3(PosAndDistance v)
        {
            return new Vector3(v.Pos.x, v.Pos.y, v.Distance);
        }
        public static implicit operator PosAndDistance(Vector3 v)
        {
            return new PosAndDistance(v);
        }
    }

}
#endif
