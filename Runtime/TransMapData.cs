#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace Rs64.TexTransTool
{
    public class TransMapData
    {
        public TowDMap<PosAndDistans> Map;
        public float DefaultPading;

        public PosAndDistans this[int i] => Map.Array[i];

        public TransMapData(TowDMap<PosAndDistans> map, float defaultPading)
        {
            Map = map;
            DefaultPading = defaultPading;
        }
        public TransMapData(float defaultPading, Vector2Int mapSize)
        {
            var arrey = Utils.FilledArray(new PosAndDistans(Vector2.zero, defaultPading), mapSize.x * mapSize.y);
            DefaultPading = defaultPading;
            var mapsize = mapSize;
            Map = new TowDMap<PosAndDistans>(arrey, mapsize);
        }
        public TransMapData(SerialaizableMap Smap)
        {
            Map = new TowDMap<PosAndDistans>(Smap.Map.Select(I => new PosAndDistans(I.x, I.y, I.z)).ToArray(), Smap.MapSize);
            DefaultPading = Smap.DefaultPading;

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
        public struct SerialaizableMap
        {
            public Vector3[] Map;
            public Vector2Int MapSize;
            public float DefaultPading;
        }
        public SerialaizableMap ToSerialaizable()
        {
            return new SerialaizableMap()
            {
                Map = GetVector3s(),
                MapSize = Map.MapSize,
                DefaultPading = DefaultPading
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

        public static explicit operator PropAndTexture(PropAndAtlasTex s)
        {
            return new PropAndTexture(s.PropertyName, s.AtlasTexture.Texture2D);
        }
    }

    [Serializable]
    public class TransTargetTexture
    {
        public Texture2D Texture2D;
        /// <summary>
        /// テクスチャーの本当の加増解像度と同じサイズのマップ
        /// </summary>
        public TowDMap<float> DistansMap;

        public TransTargetTexture(Texture2D texture2D, TowDMap<float> distansMap)
        {
            Texture2D = texture2D;
            DistansMap = distansMap;
        }
        public TransTargetTexture(Vector2Int Size, Color DefautColor, float DefaultPading)
        {
            Texture2D = Utils.CreateFillTexture(Size, DefautColor);
            DistansMap = new TowDMap<float>(DefaultPading, Size);
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

    public struct PosAndDistans
    {
        public Vector2 Pos;
        public float Distans;

        public PosAndDistans(Vector2 pos, float distans)
        {
            Pos = pos;
            Distans = distans;
        }

        public PosAndDistans(float Posx, float Posy, float distans)
        {
            Pos = new Vector2(Posx, Posy);
            Distans = distans;
        }
        public PosAndDistans(Vector3 Valu)
        {
            Pos = new Vector2(Valu.x, Valu.y);
            Distans = Valu.z;
        }

        public static implicit operator Vector3(PosAndDistans v)
        {
            return new Vector3(v.Pos.x, v.Pos.y, v.Distans);
        }
        public static implicit operator PosAndDistans(Vector3 v)
        {
            return new PosAndDistans(v);
        }
    }

}
#endif