using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransCore.Island
{
    internal static class IslandUtility
    {
        public static List<Island> UVtoIsland(List<TriangleIndex> triangles, List<Vector2> UV, IIslandCache Caches = null)
        {
            if (Caches != null && Caches.TryCache(UV, triangles, out List<Island> cacheHitIslands))
                return cacheHitIslands;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var Islands = UVToIslandImpl(triangles, UV);
            Debug.Log($"UVtoIsland {triangles.Count} took {stopwatch.Elapsed}: {Islands.Count}");

            if (Caches != null) { Caches.AddCache(UV, triangles, Islands); }

            return Islands;
        }

        private static List<Island> UVToIslandImpl(IEnumerable<TriangleIndex> trianglesIn, List<Vector2> uvs)
        {
            var triangles = new HashSet<TriangleIndex>(trianglesIn);
            var trianglesByUv = new Dictionary<Vector2, HashSet<TriangleIndex>>(uvs.Count);

            // initialize dictionary for less jumps
            foreach (var uv in uvs)
                if (!trianglesByUv.ContainsKey(uv))
                    trianglesByUv.Add(uv, new HashSet<TriangleIndex>());

            // collect all triangles by each triangle
            foreach (var triangle in triangles)
            {
                trianglesByUv[uvs[triangle.zero]].Add(triangle);
                trianglesByUv[uvs[triangle.one]].Add(triangle);
                trianglesByUv[uvs[triangle.two]].Add(triangle);
            }

            var islands = new List<Island>();

            while (triangles.Count != 0)
            {
                var entryPoint = triangles.First();
                triangles.Remove(entryPoint);
                var trianglesOfIsland = new List<TriangleIndex> { entryPoint };

                var proceedUvs = new HashSet<Vector2>();
                var processUvQueue = new Queue<Vector2>();

                if (proceedUvs.Add(uvs[entryPoint.zero])) processUvQueue.Enqueue(uvs[entryPoint.zero]);
                if (proceedUvs.Add(uvs[entryPoint.one])) processUvQueue.Enqueue(uvs[entryPoint.one]);
                if (proceedUvs.Add(uvs[entryPoint.two])) processUvQueue.Enqueue(uvs[entryPoint.two]);

                while (processUvQueue.Count != 0)
                {
                    var uv = processUvQueue.Dequeue();
                    var trianglesCandidate = trianglesByUv[uv];

                    foreach (var triangle in trianglesCandidate)
                    {
                        // already the triangle is proceed
                        if (!triangles.Remove(triangle)) continue;

                        trianglesOfIsland.Add(triangle);
                        if (proceedUvs.Add(uvs[triangle.zero])) processUvQueue.Enqueue(uvs[triangle.zero]);
                        if (proceedUvs.Add(uvs[triangle.one])) processUvQueue.Enqueue(uvs[triangle.one]);
                        if (proceedUvs.Add(uvs[triangle.two])) processUvQueue.Enqueue(uvs[triangle.two]);
                    }
                }

                islands.Add(new Island(trianglesOfIsland));
            }

            foreach (var island in islands)
            {
                island.BoxCalculation(uvs);
            }

            return islands;
        }

        public static void IslandMoveUV(List<Vector2> UV, List<Vector2> MoveUV, Island OriginIsland, Island MovedIsland)
        {
            if (OriginIsland.Is90Rotation == MovedIsland.Is90Rotation)
            {
                var mSize = MovedIsland.Size;
                var nmSize = OriginIsland.Size;

                var relativeScale = new Vector2(mSize.x / nmSize.x, mSize.y / nmSize.y).ValidateNaN();

                foreach (var vertIndex in OriginIsland.GetVertexIndex())
                {
                    var vertPos = UV[vertIndex];
                    var relativeVertPos = vertPos - OriginIsland.Pivot;

                    relativeVertPos.x *= relativeScale.x;
                    relativeVertPos.y *= relativeScale.y;

                    var movedVertPos = MovedIsland.Pivot + relativeVertPos;
                    MoveUV[vertIndex] = movedVertPos;
                }
            }
            else
            {
                var mSize = MovedIsland.Is90Rotation ? new Vector2(MovedIsland.Size.y, MovedIsland.Size.x) : MovedIsland.Size;
                var nmSize = OriginIsland.Is90Rotation ? new Vector2(OriginIsland.Size.y, OriginIsland.Size.x) : OriginIsland.Size;

                var relativeScale = new Vector2(mSize.x / nmSize.x, mSize.y / nmSize.y).ValidateNaN();
                var isRotRight = MovedIsland.Is90Rotation;
                var rotate = Quaternion.Euler(0, 0, isRotRight ? -90 : 90);

                foreach (var vertIndex in OriginIsland.GetVertexIndex())
                {
                    var vertPos = UV[vertIndex];
                    var relativeVertPos = vertPos - OriginIsland.Pivot;

                    relativeVertPos.x *= relativeScale.x;
                    relativeVertPos.y *= relativeScale.y;

                    relativeVertPos = rotate * relativeVertPos;

                    var movedVertPos = MovedIsland.Pivot + relativeVertPos;

                    if (isRotRight) { movedVertPos.y += MovedIsland.Size.y; }
                    else { movedVertPos.x += MovedIsland.Size.x; }

                    MoveUV[vertIndex] = movedVertPos;
                }
            }
        }

        private static Vector2 ValidateNaN(this Vector2 relativeScale)
        {
            relativeScale.x = float.IsNaN(relativeScale.x) ? 0 : relativeScale.x;
            relativeScale.y = float.IsNaN(relativeScale.y) ? 0 : relativeScale.y;
            return relativeScale;
        }

        public static void IslandPoolMoveUV<ID, TIsland>(List<Vector2> UV, List<Vector2> MoveUV, Dictionary<ID, TIsland> OriginPool, Dictionary<ID, TIsland> MovedPool)
        where TIsland : Island
        {
            if (UV.Count != MoveUV.Count) throw new Exception("UV.Count != MoveUV.Count 中身が同一頂点数のUVではありません。");
            foreach (var islandKVP in MovedPool)
            {
                var originIsland = OriginPool[islandKVP.Key];
                IslandMoveUV(UV, MoveUV, originIsland, islandKVP.Value);
            }
        }

    }
    [Serializable]
    internal class Island
    {
        public List<TriangleIndex> triangles = new List<TriangleIndex>();
        public Vector2 Pivot;
        public Vector2 Size;
        public bool Is90Rotation;

        public Vector2 GetMaxPos => Pivot + Size;

        public Island(Island Souse)
        {
            triangles = new List<TriangleIndex>(Souse.triangles);
            Pivot = Souse.Pivot;
            Size = Souse.Size;
            Is90Rotation = Souse.Is90Rotation;
        }
        public Island(TriangleIndex triangleIndex)
        {
            triangles.Add(triangleIndex);
        }
        public Island()
        {

        }

        public Island(List<TriangleIndex> trianglesOfIsland)
        {
            triangles = trianglesOfIsland;
        }

        public List<int> GetVertexIndex()
        {
            var IndexList = new List<int>();
            foreach (var triangle in triangles)
            {
                IndexList.AddRange(triangle.ToArray());
            }
            return IndexList;
        }
        public List<Vector2> GetVertexPos(IReadOnlyList<Vector2> SouseUV)
        {
            var vertIndexes = GetVertexIndex();
            return vertIndexes.ConvertAll<Vector2>(i => SouseUV[i]);
        }
        public void BoxCalculation(IReadOnlyList<Vector2> SouseUV)
        {
            var vertPoss = GetVertexPos(SouseUV);
            var Box = VectorUtility.BoxCal(vertPoss);
            Pivot = Box.Item1;
            Size = Box.Item2 - Box.Item1;
        }

        public bool BoxInOut(Vector2 TargetPos)
        {
            var relativeTargetPos = TargetPos - Pivot;
            return !((relativeTargetPos.x < 0 || relativeTargetPos.y < 0) || (relativeTargetPos.x > Size.x || relativeTargetPos.y > Size.y));
        }
        public List<Vector2> GenerateRectVertexes(float padding = 0)
        {
            padding = Mathf.Abs(padding);
            var quad = new List<Vector2>();
            if (!Is90Rotation)
            {
                quad.Add(Pivot + new Vector2(-padding, -padding));
                quad.Add(new Vector2(Pivot.x, Pivot.y + Size.y) + new Vector2(-padding, padding));
                quad.Add(Pivot + Size + new Vector2(padding, padding));
                quad.Add(new Vector2(Pivot.x + Size.x, Pivot.y) + new Vector2(padding, -padding));
            }
            else
            {
                quad.Add(new Vector2(Pivot.x, Pivot.y + Size.y) + new Vector2(-padding, padding));
                quad.Add(Pivot + Size + new Vector2(padding, padding));
                quad.Add(new Vector2(Pivot.x + Size.x, Pivot.y) + new Vector2(padding, -padding));
                quad.Add(Pivot + new Vector2(-padding, -padding));
            }
            return quad;
        }

        public void Rotate90()
        {
            Is90Rotation = !Is90Rotation;
            (Size.x, Size.y) = (Size.y, Size.x);
        }

    }


    internal static class IslandUtilsDebug
    {
        public static void DrawUV(List<Vector2> UV, Texture2D TargetTexture, Color WriteColor)
        {
            foreach (var uvPos in UV)
            {
                if (0 <= uvPos.x && uvPos.x <= 1 && 0 <= uvPos.y && uvPos.y <= 1) continue;
                int x = Mathf.RoundToInt(uvPos.x * TargetTexture.width);
                int y = Mathf.RoundToInt(uvPos.y * TargetTexture.height);
                TargetTexture.SetPixel(x, y, WriteColor);
            }
        }
    }
}