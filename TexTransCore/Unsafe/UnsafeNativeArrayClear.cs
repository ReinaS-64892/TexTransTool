using System;
using System.Drawing.Imaging;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace net.rs64.TexTransCore.Unsafe
{
    internal static class UnsafeNativeArrayClear
    {
        public static unsafe void ClearMemory<T>(NativeArray<T> array) where T : struct
        {
            UnsafeUtility.MemClear(array.GetUnsafePtr(), (long)array.Length * UnsafeUtility.SizeOf<T>());
        }
        public static unsafe void ClearMemoryOnColor(NativeArray<Color32> array, byte val)
        {
            UnsafeUtility.MemSet(array.GetUnsafePtr(), val, (long)array.Length * UnsafeUtility.SizeOf<Color32>());
        }

    }


}
