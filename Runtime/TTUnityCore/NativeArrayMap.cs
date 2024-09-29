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
    internal readonly struct NativeArrayMap<T> : IDisposable
    where T : struct
    {
        public readonly NativeArray<T> Array;
        public readonly int Width;
        public readonly int Height;
        public NativeArrayMap(NativeArray<T> array, int width, int height)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Convert1D(int x, int y, int Size)
        {
            return (y * Size) + x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int x, int y) Convert2D(int i, int width)
        {
            return (i % width, i / width);
        }

    }
}
