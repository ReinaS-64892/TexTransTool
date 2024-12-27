#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.TTMathUtil
{
    public static class RayIntersect
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Ray
        {
            public Vector3 Position;
            public Vector3 Direction;
        }
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct RayIntersectResult
        {
            public readonly Vector3 TBC;
            public readonly float Distance;
            public RayIntersectResult(Vector3 tbc, float distance)
            {
                TBC = tbc;
                Distance = distance;
            }
        }
        public static bool Intersect(Ray ray, Triangle triangle, out RayIntersectResult result)
        {
            result = new();
            var v0 = triangle[0] - ray.Position;
            var v1 = triangle[1] - ray.Position;
            var v2 = triangle[2] - ray.Position;

            var e0 = v2 - v1;
            var e1 = v0 - v2;
            var e2 = v1 - v0;


            // Ray 原点 と 辺 から垂直なベクトルを生成し、dot により、三角形のどのあたりを Ray が向いているかを調べることができるようだ。
            var u = Vector3.Dot(Vector3.Cross(e0, v2 + v1), ray.Direction);
            var v = Vector3.Dot(Vector3.Cross(e1, v0 + v2), ray.Direction);
            var w = Vector3.Dot(Vector3.Cross(e2, v1 + v0), ray.Direction);

            var uvw = u + v + w;
            var rcp = 1f / uvw;

            if (VectorUtility.IsInCal(u, v, w) is false) { return false; }

            // 通常三角形と逆方向の法線を取得する
            var ngt = Vector3.Cross(e0, e1);
            var ng = ngt + ngt;
            var den = Vector3.Dot(ng, ray.Direction);

            if (den == 0f) { return false; }

            // おそらく ... このあたりで三角形に衝突する長さが取得できるとみてよいだろう、なぜだかは私にはわからないけど。
            // もし私 (ReinaSakiria) に対して完全にこのあたりの原理を説明できる人がいたらおしえてほしい！
            var t = Vector3.Dot(v0, ng) / den;

            if (t < 0f) { return false; }

            result = new(new Vector3(u, v, w) * rcp, t);
            return true;
        }
    }
}
