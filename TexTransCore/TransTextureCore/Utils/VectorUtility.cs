
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class VectorUtility
    {

        public static Vector4 CrossTriangle(List<Vector2> triangle, Vector2 targetPoint)
        {
            var w = Vector3.Cross(triangle[2] - triangle[1], targetPoint - triangle[1]).z;
            var u = Vector3.Cross(triangle[0] - triangle[2], targetPoint - triangle[2]).z;
            var v = Vector3.Cross(triangle[1] - triangle[0], targetPoint - triangle[0]).z;
            var wuv = TriangleArea(triangle);
            return new (w, u, v, wuv);
        }
        public static float TriangleArea(List<Vector2> triangle)
        {
            return Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]).z;
        }

        public static Vector3 ToBarycentricCoordinateSystem(Vector4 crossT)
        {
            var a = crossT.x / crossT.w;
            var b = crossT.y / crossT.w;
            var c = crossT.z / crossT.w;

            return new (a, b, c);
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
            return new (w, u, v);
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
            return new (distanceA, distanceB, distanceC);
        }

        public static float MinVector(Vector3 vector)
        {
            return Mathf.Min(vector.x, Mathf.Min(vector.y, vector.z));
        }
        public static (Vector2, Vector2) BoxCal(List<Vector2> triangles)
        {
            Vector2 min = new ();
            Vector2 max = new ();
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
    }
}