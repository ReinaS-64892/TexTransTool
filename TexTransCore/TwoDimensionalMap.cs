using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using net.rs64.TexTransCore.Utils;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace net.rs64.TexTransCore
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2Int GetPosOn2D(int i)
        {
            return ConvertIndex2D(i, MapSize.x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndexOn1D(Vector2Int pos)
        {
            return TwoDToOneDIndex(pos, MapSize.x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TwoDToOneDIndex(Vector2Int TowDIndex, int Size)
        {
            return (TowDIndex.y * Size) + TowDIndex.x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int ConvertIndex2D(int Index1D, int width)
        {
            return new Vector2Int(Index1D % width, Index1D / width);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetOn2DIndex(Vector2Int pos)
        {
            return Array[GetIndexOn1D(pos)];
        }
    }

    internal readonly struct LowMap<T> : IDisposable
    where T : struct
    {
        public readonly NativeArray<T> Array;
        public readonly int Width;
        public readonly int Height;
        public LowMap(NativeArray<T> array, int width, int height)
        {
            Array = array;
            Width = width;
            Height = height;
        }

        public Vector2Int MapSize => new Vector2Int(Width, Height);

        public void Dispose()
        {
            Array.Dispose();
        }
    }
}
