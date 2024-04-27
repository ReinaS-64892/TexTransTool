
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.Decal;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class VectorUtility
    {

        public static Vector4 CrossTriangle(Span<Vector2> triangle, Vector2 targetPoint)
        {
            var w = Vector3.Cross(triangle[2] - triangle[1], targetPoint - triangle[1]).z;
            var u = Vector3.Cross(triangle[0] - triangle[2], targetPoint - triangle[2]).z;
            var v = Vector3.Cross(triangle[1] - triangle[0], targetPoint - triangle[0]).z;
            var wuv = TriangleArea(triangle);
            return new(w, u, v, wuv);
        }
        public static float TriangleArea(Span<Vector2> triangle)
        {
            return Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]).z;
        }

        public static Vector3 ToBarycentricCoordinateSystem(Vector4 crossT)
        {
            var a = crossT.x / crossT.w;
            var b = crossT.y / crossT.w;
            var c = crossT.z / crossT.w;

            return new(a, b, c);
        }

        public static Vector2 FromBarycentricCoordinateSystem(List<Vector2> triangle, Vector3 sourceTBC)
        {
            var conversionPos = Vector2.zero;
            conversionPos += triangle[0] * sourceTBC.x;
            conversionPos += triangle[1] * sourceTBC.y;
            conversionPos += triangle[2] * sourceTBC.z;
            return conversionPos;
        }
        public static Vector3 FromBarycentricCoordinateSystem(List<Vector3> Triangle, Vector3 SourceTBC)
        {
            var ConversionPos = Vector3.zero;
            ConversionPos += Triangle[0] * SourceTBC.x;
            ConversionPos += Triangle[1] * SourceTBC.y;
            ConversionPos += Triangle[2] * SourceTBC.z;
            return ConversionPos;
        }

        public static (Vector2, Vector2) TriangleToBoundingBox(List<Vector2> triangle)
        {
            Vector2 min = triangle[0];
            Vector2 max = triangle[0];

            min = Vector2.Min(min, triangle[1]);
            max = Vector2.Max(max, triangle[1]);

            min = Vector2.Min(min, triangle[2]);
            max = Vector2.Max(max, triangle[2]);
            return (min, max);
        }

        public static Vector3 DistanceVertBase(List<Vector2> triangle, Vector2 targetPoint)
        {
            var w = Vector2.Distance(triangle[0], targetPoint);
            var u = Vector2.Distance(triangle[1], targetPoint);
            var v = Vector2.Distance(triangle[2], targetPoint);
            return new(w, u, v);
        }

        public static Vector2 NearPointOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float length = ab.magnitude;
            ab.Normalize();
            float lp = Mathf.Clamp(Vector2.Dot(p - a, ab), 0, length);
            return a + (lp * ab);
        }
        public static Vector2 NearPoint(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            ab.Normalize();
            float lp = Vector2.Dot(p - a, ab);
            return a + (lp * ab);
        }

        public static Vector3 DistanceEdgeBase(List<Vector2> triangle, Vector2 targetPoint)
        {
            float distanceA = Vector2.Distance(NearPointOnLine(triangle[0], triangle[1], targetPoint), targetPoint);
            float distanceB = Vector2.Distance(NearPointOnLine(triangle[1], triangle[2], targetPoint), targetPoint);
            float distanceC = Vector2.Distance(NearPointOnLine(triangle[2], triangle[0], targetPoint), targetPoint);
            return new(distanceA, distanceB, distanceC);
        }

        public static float MinVector(Vector3 vector)
        {
            return Mathf.Min(vector.x, Mathf.Min(vector.y, vector.z));
        }
        public static (Vector2 min, Vector2 max) BoxCal(List<Vector2> triangles)
        {
            Vector2 min = new();
            Vector2 max = new();
            bool first = true;
            foreach (var tri in triangles)
            {
                if (first)
                {
                    min = tri;
                    max = tri;
                    first = false;
                    continue;
                }
                min = Vector2.Min(min, tri);
                max = Vector2.Max(max, tri);
            }
            return (min, max);
        }

        public static bool IsInCal(float w, float u, float v)
        {
            return ((w > 0) == (u > 0)) && ((u > 0) == (v > 0));
        }
        public static bool InRange(float min, float max, float target)
        {
            return (min <= target && target <= max);
        }

        internal static void ConvertVerticesInMatrix(Matrix4x4 matrix, List<Vector3> vertices, Vector3 Offset)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = matrix.MultiplyPoint3x4(vertices[i]) + Offset;
            }
        }
        internal static Vector2 QuadNormalize(IReadOnlyList<Vector2> quad, Vector2 targetPos)
        {
            var oneNearPoint = VectorUtility.NearPoint(quad[0], quad[2], targetPos);
            var oneCross = Vector3.Cross(quad[2] - quad[0], targetPos - quad[0]).z > 0 ? -1 : 1;

            var twoNearPoint = VectorUtility.NearPoint(quad[0], quad[1], targetPos);
            var twoCross = Vector3.Cross(quad[1] - quad[0], targetPos - quad[0]).z > 0 ? 1 : -1;

            var threeNearPoint = VectorUtility.NearPoint(quad[1], quad[3], targetPos);
            var threeCross = Vector3.Cross(quad[3] - quad[1], targetPos - quad[1]).z > 0 ? 1 : -1;

            var forNearPoint = VectorUtility.NearPoint(quad[2], quad[3], targetPos);
            var forCross = Vector3.Cross(quad[3] - quad[2], targetPos - quad[2]).z > 0 ? -1 : 1;

            var oneDistance = Vector2.Distance(oneNearPoint, targetPos) * oneCross;
            var towDistance = Vector2.Distance(twoNearPoint, targetPos) * twoCross;
            var threeDistance = Vector2.Distance(threeNearPoint, targetPos) * threeCross;
            var forDistance = Vector2.Distance(forNearPoint, targetPos) * forCross;

            var x = oneDistance / (oneDistance + threeDistance);
            var y = towDistance / (towDistance + forDistance);

            return new Vector2(x, y);
        }
        internal static List<Vector2> QuadNormalize(IReadOnlyList<Vector2> quad, List<Vector2> targetPoss, List<Vector2> outPut = null)
        {
            outPut?.Clear(); outPut ??= new List<Vector2>(targetPoss.Count);
            foreach (var targetPos in targetPoss)
            {
                outPut.Add(QuadNormalize(quad, targetPos));
            }
            return outPut;
        }

        internal static NativeArray<Vector3> ConvertVerticesInMatrix(Matrix4x4 matrix, MeshData meshData, Vector3 offset, out JobHandle jobHandle)
        {
            var array = new NativeArray<Vector3>(meshData.Vertices.Length, Allocator.TempJob);

            jobHandle = new ConvertVerticesJob()
            {
                InputVertices = meshData.Vertices,
                OutputVertices = array,
                Matrix = matrix,
                Offset = offset
            }.Schedule(meshData.Vertices.Length, 64);

            meshData.AddJobDependency(jobHandle);

            return array;
        }

        [BurstCompile]
        private struct ConvertVerticesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> InputVertices;
            [WriteOnly] public NativeArray<Vector3> OutputVertices;
            public Matrix4x4 Matrix;
            public Vector3 Offset;

            public void Execute(int index)
            {
                OutputVertices[index] = Matrix.MultiplyPoint3x4(InputVertices[index]) + Offset;
            }
        }


    }
}
