using net.rs64.TexTransCore;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace net.rs64.TexTransTool.Unsafe
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

        public static unsafe void MemCpy<T>(NativeArray<T> d, NativeArray<T> s, int count)
        where T : struct
        {
            if (Mathf.Min(s.Length, d.Length) > count) { throw new System.ArgumentOutOfRangeException(); }
            UnsafeUtility.MemCpy(d.GetUnsafePtr(), s.GetUnsafePtr(), (long)count * UnsafeUtility.SizeOf<T>());
        }

        public static unsafe NativeArray<TriangleIndex> GetTriangleIndices(Mesh.MeshData mainMesh, int subMeshIndex, Allocator allocator = Allocator.TempJob)
        {
            System.Diagnostics.Debug.Assert(0 <= subMeshIndex && subMeshIndex < mainMesh.subMeshCount);
            var desc = mainMesh.GetSubMesh(subMeshIndex);
            System.Diagnostics.Debug.Assert(desc.topology == MeshTopology.Triangles);
            unsafe
            {
                var triangleBuffer = new NativeArray<TriangleIndex>(desc.indexCount / 3, allocator, NativeArrayOptions.UninitializedMemory);
                var indexes = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(triangleBuffer.GetUnsafePtr(), desc.indexCount, Allocator.None);
#if UNITY_EDITOR
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref indexes, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(triangleBuffer));
#endif
                mainMesh.GetIndices(indexes, subMeshIndex);
                return triangleBuffer;
            }
        }
    }


}
