
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    public static class VectorUtility
    {

        public static Vector4 CrossTriangle(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            var w = Vector3.Cross(Triangle[2] - Triangle[1], TargetPoint - Triangle[1]).z;
            var u = Vector3.Cross(Triangle[0] - Triangle[2], TargetPoint - Triangle[2]).z;
            var v = Vector3.Cross(Triangle[1] - Triangle[0], TargetPoint - Triangle[0]).z;
            var wuv = TriangleArea(Triangle);
            return new Vector4(w, u, v, wuv);
        }
        public static float TriangleArea(List<Vector2> Triangle)
        {
            return Vector3.Cross(Triangle[1] - Triangle[0], Triangle[2] - Triangle[0]).z;
        }

        public static Vector3 ToBarycentricCoordinateSystem(Vector4 CrossT)
        {
            var a = CrossT.x / CrossT.w;
            var b = CrossT.y / CrossT.w;
            var c = CrossT.z / CrossT.w;

            return new Vector3(a, b, c);
        }

        public static Vector2 FromBarycentricCoordinateSystem(List<Vector2> Triangle, Vector3 SourceTBC)
        {
            var ConversionPos = Vector2.zero;
            ConversionPos += Triangle[0] * SourceTBC.x;
            ConversionPos += Triangle[1] * SourceTBC.y;
            ConversionPos += Triangle[2] * SourceTBC.z;
            return ConversionPos;
        }
        public static Vector3 FromBarycentricCoordinateSystem(List<Vector3> Triangle, Vector3 SourceTBC)
        {
            var ConversionPos = Vector3.zero;
            ConversionPos += Triangle[0] * SourceTBC.x;
            ConversionPos += Triangle[1] * SourceTBC.y;
            ConversionPos += Triangle[2] * SourceTBC.z;
            return ConversionPos;
        }

        public static (Vector2, Vector2) TriangleToBoundingBox(List<Vector2> Triangle)
        {
            Vector2 min = Triangle[0];
            Vector2 max = Triangle[0];

            min = Vector2.Min(min, Triangle[1]);
            max = Vector2.Max(max, Triangle[1]);

            min = Vector2.Min(min, Triangle[2]);
            max = Vector2.Max(max, Triangle[2]);
            return (min, max);
        }

        public static Vector3 DistanceVertBase(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            var w = Vector2.Distance(Triangle[0], TargetPoint);
            var u = Vector2.Distance(Triangle[1], TargetPoint);
            var v = Vector2.Distance(Triangle[2], TargetPoint);
            return new Vector3(w, u, v);
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

        public static Vector3 DistanceEdgeBase(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            float DistanceA = Vector2.Distance(NearPointOnLine(Triangle[0], Triangle[1], TargetPoint), TargetPoint);
            float DistanceB = Vector2.Distance(NearPointOnLine(Triangle[1], Triangle[2], TargetPoint), TargetPoint);
            float DistanceC = Vector2.Distance(NearPointOnLine(Triangle[2], Triangle[0], TargetPoint), TargetPoint);
            return new Vector3(DistanceA, DistanceB, DistanceC);
        }

        public static float MinVector(Vector3 Vector)

        {
            return Mathf.Min(Vector.x, Mathf.Min(Vector.y, Vector.z));
        }
        public static (Vector2, Vector2) BoxCal(List<Vector2> Triangles)
        {
            Vector2 min = new Vector2();
            Vector2 max = new Vector2();
            bool Farst = true;
            foreach (var tri in Triangles)
            {
                if (Farst)
                {
                    min = tri;
                    max = tri;
                    Farst = false;
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