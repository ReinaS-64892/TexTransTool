using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class CollectionsUtility
    {
        public static NativeArray<T> ListToNativeArray<T>(List<T> list, Unity.Collections.Allocator allocator) where T : struct
        {
            var length = list.Count;
            var NativeArray = new Unity.Collections.NativeArray<T>(length, allocator);
            for (var i = 0; length > i; i += 1) { NativeArray[i] = list[i]; }
            return NativeArray;
        }
        public static List<Vector3> ZipListVector3(IReadOnlyList<Vector2> XY, IReadOnlyList<float> Z)
        {
            var count = XY.Count;
            if (count != Z.Count) { throw new System.ArgumentException("XY.Count != Z.Count"); }

            List<Vector3> result = new(count);

            for (int index = 0; index < count; index += 1)
            {
                result.Add(new (XY[index].x, XY[index].y, Z[index]));
            }

            return result;
        }


        public static T[] FilledArray<T>(T DefaultValue, int Length)
        {
            var array = new T[Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = DefaultValue;
            }
            return array;
        }

    }
}