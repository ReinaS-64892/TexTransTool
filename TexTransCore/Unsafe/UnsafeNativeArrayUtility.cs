using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace net.rs64.TexTransCore.Unsafe
{
    internal static class UnsafeNativeArrayUtility
    {
        public static unsafe void ClearMemory<T>(NativeArray<T> array) where T : struct
        {
            UnsafeUtility.MemClear(array.GetUnsafePtr(), (long)array.Length * UnsafeUtility.SizeOf<T>());
        }
        public static unsafe void ClearMemoryOnColor(NativeArray<Color32> array, byte val)
        {
            UnsafeUtility.MemSet(array.GetUnsafePtr(), val, (long)array.Length * UnsafeUtility.SizeOf<Color32>());
        }

        public static unsafe void MemCpy<T>(NativeArray<T> s, NativeArray<T> d, int count)
        where T : struct
        {
            if (Mathf.Min(s.Length, d.Length) > count) { throw new System.ArgumentOutOfRangeException(); }
            UnsafeUtility.MemCpy(d.GetUnsafePtr(), s.GetUnsafePtr(), (long)count * UnsafeUtility.SizeOf<T>());
        }

    }


}
