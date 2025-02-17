#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.TTMathUtil
{
    public static class VectorUtility
    {
        public static float Vector2Cross(Vector2 tri1, Vector2 tri2)
        {
            return Vector3.Cross(new(tri1, 0f), new(tri2, 0f)).Z;
        }
        public static TTVector4 CrossTriangle(Span<Vector2> triangle, Vector2 targetPoint)
        {
            var u = Vector2Cross(triangle[2] - triangle[1], targetPoint - triangle[1]);
            var v = Vector2Cross(triangle[0] - triangle[2], targetPoint - triangle[2]);
            var w = Vector2Cross(triangle[1] - triangle[0], targetPoint - triangle[0]);
            var uvw = TriangleArea(triangle);
            return new(u, v, w, uvw);
        }
        public static float TriangleArea(Span<Vector2> triangle)
        {
            return Vector2Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]);
        }

        public static Vector3 ToBarycentricCoordinateSystem(TTVector4 crossT)
        {
            var a = crossT.X / crossT.W;
            var b = crossT.Y / crossT.W;
            var c = crossT.Z / crossT.W;

            return new(a, b, c);
        }

        public static Vector2 FromBarycentricCoordinateSystem(List<Vector2> triangle, Vector3 sourceTBC)
        {
            var conversionPos = Vector2.Zero;
            conversionPos += triangle[0] * sourceTBC.X;
            conversionPos += triangle[1] * sourceTBC.Y;
            conversionPos += triangle[2] * sourceTBC.Z;
            return conversionPos;
        }
        public static Vector3 FromBarycentricCoordinateSystem(List<Vector3> Triangle, Vector3 SourceTBC)
        {
            var ConversionPos = Vector3.Zero;
            ConversionPos += Triangle[0] * SourceTBC.X;
            ConversionPos += Triangle[1] * SourceTBC.Y;
            ConversionPos += Triangle[2] * SourceTBC.Z;
            return ConversionPos;
        }

        public static (Vector2, Vector2) TriangleToBoundingBox(Span<Vector2> triangle)
        {
            Vector2 min = triangle[0];
            Vector2 max = triangle[0];

            min = Vector2.Min(min, triangle[1]);
            max = Vector2.Max(max, triangle[1]);

            min = Vector2.Min(min, triangle[2]);
            max = Vector2.Max(max, triangle[2]);
            return (min, max);
        }

        public static Vector3 DistanceVertBase(Span<Vector2> triangle, Vector2 targetPoint)
        {
            var u = Vector2.Distance(triangle[0], targetPoint);
            var v = Vector2.Distance(triangle[1], targetPoint);
            var w = Vector2.Distance(triangle[2], targetPoint);
            return new(u, v, w);
        }

        public static Vector2 NearPointOnLine(Vector2 v1, Vector2 v2, Vector2 p)
        {
            var ab = v2 - v1;
            var length = ab.Length();
            ab = Vector2.Normalize(ab);
            var lp = Math.Clamp(Vector2.Dot(p - v1, ab), 0, length);
            return v1 + (lp * ab);
        }
        public static Vector2 NearPoint(Vector2 v1, Vector2 v2, Vector2 p)
        {
            var ab = v2 - v1;
            ab = Vector2.Normalize(ab);
            var lp = Vector2.Dot(p - v1, ab);
            return v1 + (lp * ab);
        }

        public static Vector3 DistanceEdgeBase(Span<Vector2> triangle, Vector2 targetPoint)
        {
            float distanceA = Vector2.Distance(NearPointOnLine(triangle[0], triangle[1], targetPoint), targetPoint);
            float distanceB = Vector2.Distance(NearPointOnLine(triangle[1], triangle[2], targetPoint), targetPoint);
            float distanceC = Vector2.Distance(NearPointOnLine(triangle[2], triangle[0], targetPoint), targetPoint);
            return new(distanceA, distanceB, distanceC);
        }

        public static float MinVector(Vector3 vector)
        {
            return Math.Min(vector.X, Math.Min(vector.Y, vector.Z));
        }

        public static bool IsInCal(float u, float v, float w)
        {
            return ((u > 0) == (v > 0)) && ((v > 0) == (w > 0));
        }
        public static bool InRange(float min, float max, float target)
        {
            return min <= target && target <= max;
        }
        internal static Vector2 QuadNormalize(ReadOnlySpan<Vector2> quad, Vector2 targetPos)
        {
            var oneNearPoint = NearPoint(quad[0], quad[2], targetPos);
            var oneCross = Vector2Cross(quad[2] - quad[0], targetPos - quad[0]) > 0 ? -1 : 1;

            var twoNearPoint = NearPoint(quad[0], quad[1], targetPos);
            var twoCross = Vector2Cross(quad[1] - quad[0], targetPos - quad[0]) > 0 ? 1 : -1;

            var threeNearPoint = NearPoint(quad[1], quad[3], targetPos);
            var threeCross = Vector2Cross(quad[3] - quad[1], targetPos - quad[1]) > 0 ? 1 : -1;

            var forNearPoint = VectorUtility.NearPoint(quad[2], quad[3], targetPos);
            var forCross = Vector2Cross(quad[3] - quad[2], targetPos - quad[2]) > 0 ? -1 : 1;

            var oneDistance = Vector2.Distance(oneNearPoint, targetPos) * oneCross;
            var towDistance = Vector2.Distance(twoNearPoint, targetPos) * twoCross;
            var threeDistance = Vector2.Distance(threeNearPoint, targetPos) * threeCross;
            var forDistance = Vector2.Distance(forNearPoint, targetPos) * forCross;

            var x = oneDistance / (oneDistance + threeDistance);
            var y = towDistance / (towDistance + forDistance);

            return new Vector2(x, y);
        }
    }
}
