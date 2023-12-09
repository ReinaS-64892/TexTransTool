using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using net.rs64.TexTransCore.TransTextureCore.Utils;
namespace net.rs64.TexTransCore.TransTextureCore
{
    internal struct TwoDimensionalMap<T>
    {
        public readonly T[] Array;
        public readonly Vector2Int MapSize;

        public T this[int i] { get => Array[i]; set => Array[i] = value; }
        public T this[int x, int y] { get => Array[GetIndexOn1D(new Vector2Int(x, y))]; set => Array[GetIndexOn1D(new Vector2Int(x, y))] = value; }

        public TwoDimensionalMap(T[] array, Vector2Int mapSize)
        {
            Array = array;
            MapSize = mapSize;
        }
        public TwoDimensionalMap(T defaultValue, Vector2Int mapSize)
        {
            Array = CollectionsUtility.FilledArray(defaultValue, mapSize.x * mapSize.y);
            MapSize = mapSize;
        }
        public TwoDimensionalMap(Vector2Int mapSize)
        {
            Array = new T[mapSize.x * mapSize.y];
            MapSize = mapSize;
        }

        public Vector2Int GetPosOn2D(int i)
        {
            return ConvertIndex2D(i, MapSize.x);
        }
        public int GetIndexOn1D(Vector2Int pos)
        {
            return TwoDToOneDIndex(pos, MapSize.x);
        }
        public static int TwoDToOneDIndex(Vector2Int TowDIndex, int Size)
        {
            return (TowDIndex.y * Size) + TowDIndex.x;
        }
        public static Vector2Int ConvertIndex2D(int Index1D, int width)
        {
            return new Vector2Int(Index1D % width, Index1D / width);
        }
        public T GetOn2DIndex(Vector2Int pos)
        {
            return Array[GetIndexOn1D(pos)];
        }
    }
}