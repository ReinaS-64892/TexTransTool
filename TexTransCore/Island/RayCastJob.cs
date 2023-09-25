using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using Unity.Jobs;
using Unity.Collections;
using net.rs64.TexTransCore.TransTextureCore;
using System.Collections.Generic;
using Unity.Burst;
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore.Island
{
    [BurstCompile]
    public struct RayCastJob : IJobParallelFor
    {
        [ReadOnly]
        public Matrix4x4 rayMatrix;
        [ReadOnly]
        public NativeArray<Triangle> triangles;

        [WriteOnly]
        public NativeArray<float> Distance;
        [WriteOnly]
        public NativeArray<bool> HitResult;
        public void Execute(int index)
        {
            var tri = triangles[index];
            tri.Zero = rayMatrix.MultiplyPoint3x4(tri.Zero);
            tri.One = rayMatrix.MultiplyPoint3x4(tri.One);
            tri.Two = rayMatrix.MultiplyPoint3x4(tri.Two);
            var CrossT = tri.Cross(Vector3.zero);

            var TBC = VectorUtility.ToBarycentricCoordinateSystem(CrossT);
            if (float.IsNaN(TBC.x) || float.IsNaN(TBC.y) || float.IsNaN(TBC.z)) { return; }
            var IsIn = VectorUtility.IsInCal(CrossT.x, CrossT.y, CrossT.z);
            HitResult[index] = IsIn;
            Distance[index] = tri.FromBCS(TBC).z;

        }



    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Triangle
    {
        [FieldOffset(0)] public Vector3 Zero;
        [FieldOffset(12)] public Vector3 One;
        [FieldOffset(24)] public Vector3 Two;

        public Triangle(TriangleIndex TriIndex, List<Vector3> vector3s)
        {
            Zero = vector3s[TriIndex.zero];
            One = vector3s[TriIndex.one];
            Two = vector3s[TriIndex.two];
        }

        public Vector4 Cross(Vector3 TargetPoint)
        {
            var w = Vector3.Cross(Two - One, TargetPoint - One).z;
            var u = Vector3.Cross(Zero - Two, TargetPoint - Two).z;
            var v = Vector3.Cross(One - Zero, TargetPoint - Zero).z;
            var wuv = Vector3.Cross(One - Zero, Two - Zero).z;
            return new Vector4(w, u, v, wuv);
        }

        public Vector3 FromBCS(Vector3 SourceTBC)
        {
            var ConversionPos = Vector3.zero;
            ConversionPos += Zero * SourceTBC.x;
            ConversionPos += One * SourceTBC.y;
            ConversionPos += Two * SourceTBC.z;
            return ConversionPos;
        }
    }
}
