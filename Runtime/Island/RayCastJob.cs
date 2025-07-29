#nullable enable
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using net.rs64.TexTransCoreEngineForUnity;
using Unity.Burst;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.TTMathUtil;

namespace net.rs64.TexTransTool.UVIsland
{
    [BurstCompile]
    internal struct RayCastJob : IJobParallelFor
    {
        public RayIntersect.Ray ray;
        [ReadOnly]
        public NativeArray<Triangle> triangles;

        [WriteOnly]
        public NativeArray<float> Distance;
        [WriteOnly]
        public NativeArray<bool> HitResult;
        public void Execute(int index)
        {
            HitResult[index] = RayIntersect.Intersect(ray, triangles[index], out var result);
            Distance[index] = result.Distance;

            // tri.zero = rayMatrix.MultiplyPoint3x4(tri.zero.ToUnity()).ToTTCore();
            // tri.one = rayMatrix.MultiplyPoint3x4(tri.one.ToUnity()).ToTTCore();
            // tri.two = rayMatrix.MultiplyPoint3x4(tri.two.ToUnity()).ToTTCore();
            // var CrossT = tri.Cross(new(0, 0, 0));

            // var tbc = VectorUtility.ToBarycentricCoordinateSystem(CrossT);
            // if (float.IsNaN(tbc.X) || float.IsNaN(tbc.Y) || float.IsNaN(tbc.Z)) { return; }
            // var IsIn = VectorUtility.IsInCal(CrossT.X, CrossT.Y, CrossT.Z);
            // HitResult[index] = IsIn;
            // Distance[index] = tri.FromBCS(tbc).Z;
        }
    }
    [BurstCompile]
    internal struct RayCastJob2 : IJobParallelFor
    {
        public RayIntersect.Ray ray;
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<TriangleVertexIndices> Triangles;
        [ReadOnly]
        public NativeArray<Vector3> Position;

        [WriteOnly]
        public NativeArray<float> Distance;
        [WriteOnly]
        public NativeArray<bool> HitResult;
        public void Execute(int index)
        {
            var trIndex = Triangles[index];
            var tri = new Triangle() { zero = Position[trIndex[0]].ToSysNum(), one = Position[trIndex[1]].ToSysNum(), two = Position[trIndex[2]].ToSysNum() };

            HitResult[index] = RayIntersect.Intersect(ray, tri, out var result);
            Distance[index] = result.Distance;
        }

    }
}
