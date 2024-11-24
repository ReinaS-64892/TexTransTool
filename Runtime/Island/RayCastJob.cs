using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using net.rs64.TexTransCoreEngineForUnity;
using Unity.Burst;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.UVIsland
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
            tri.zero = rayMatrix.MultiplyPoint3x4(tri.zero.ToUnity()).ToTTCore();
            tri.one = rayMatrix.MultiplyPoint3x4(tri.one.ToUnity()).ToTTCore();
            tri.two = rayMatrix.MultiplyPoint3x4(tri.two.ToUnity()).ToTTCore();
            var CrossT = tri.Cross(new(0, 0, 0)).ToUnity();

            var TBC = VectorUtility.ToBarycentricCoordinateSystem(CrossT);
            if (float.IsNaN(TBC.x) || float.IsNaN(TBC.y) || float.IsNaN(TBC.z)) { return; }
            var IsIn = VectorUtility.IsInCal(CrossT.x, CrossT.y, CrossT.z);
            HitResult[index] = IsIn;
            Distance[index] = tri.FromBCS(TBC.ToTTCore()).Z;

        }



    }
    [BurstCompile]
    internal struct RayCastJob2 : IJobParallelFor
    {
        [ReadOnly]
        public Matrix4x4 rayMatrix;
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<TriangleIndex> Triangles;
        [ReadOnly]
        public NativeArray<Vector3> Position;

        [WriteOnly]
        public NativeArray<float> Distance;
        [WriteOnly]
        public NativeArray<bool> HitResult;
        public void Execute(int index)
        {
            var trIndex = Triangles[index];
            var tri = new Triangle();
            tri.zero = rayMatrix.MultiplyPoint3x4(Position[trIndex.zero]).ToTTCore();
            tri.one = rayMatrix.MultiplyPoint3x4(Position[trIndex.one]).ToTTCore();
            tri.two = rayMatrix.MultiplyPoint3x4(Position[trIndex.two]).ToTTCore();
            var CrossT = tri.Cross(new(0, 0, 0)).ToUnity();

            var TBC = VectorUtility.ToBarycentricCoordinateSystem(CrossT);
            if (float.IsNaN(TBC.x) || float.IsNaN(TBC.y) || float.IsNaN(TBC.z)) { return; }
            var IsIn = VectorUtility.IsInCal(CrossT.x, CrossT.y, CrossT.z);
            HitResult[index] = IsIn;
            Distance[index] = tri.FromBCS(TBC.ToTTCore()).Z;

        }



    }
}
