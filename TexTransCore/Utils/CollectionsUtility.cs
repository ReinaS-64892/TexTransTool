using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransCore.Utils
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
        public static List<Vector3> ZipListVector3(IReadOnlyList<Vector2> xy, IReadOnlyList<float> z)
        {
            var count = xy.Count;
            if (count != z.Count) { throw new System.ArgumentException("XY.Count != Z.Count"); }

            List<Vector3> result = new(count);

            for (int index = 0; index < count; index += 1)
            {
                result.Add(new (xy[index].x, xy[index].y, z[index]));
            }

            return result;
        }


        public static T[] FilledArray<T>(T defaultValue, int length)
        {
            var array = new T[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultValue;
            }
            return array;
        }

    }
}
