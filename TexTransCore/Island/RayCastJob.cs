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
    internal struct RayCastJob : IJobParallelFor
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
            tri.zero = rayMatrix.MultiplyPoint3x4(tri.zero);
            tri.one = rayMatrix.MultiplyPoint3x4(tri.one);
            tri.two = rayMatrix.MultiplyPoint3x4(tri.two);
            var CrossT = tri.Cross(Vector3.zero);

            var TBC = VectorUtility.ToBarycentricCoordinateSystem(CrossT);
            if (float.IsNaN(TBC.x) || float.IsNaN(TBC.y) || float.IsNaN(TBC.z)) { return; }
            var IsIn = VectorUtility.IsInCal(CrossT.x, CrossT.y, CrossT.z);
            HitResult[index] = IsIn;
            Distance[index] = tri.FromBCS(TBC).z;

        }



    }
}
