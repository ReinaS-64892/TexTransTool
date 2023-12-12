using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine.Pool;

namespace net.rs64.TexTransCore.Island
{
    internal static class IslandUtility
    {
        public static List<Island> UVtoIsland(List<TriangleIndex> triangles, List<Vector2> uv, IIslandCache caches = null)
        {
            if (caches != null && caches.TryCache(uv, triangles, out List<Island> cacheHitIslands))
                return cacheHitIslands;

            // var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var islands = UVToIslandImpl(triangles, uv);
            // Debug.Log($"UVtoIsland {triangles.Count} took {stopwatch.Elapsed}: {islands.Count}");

            if (caches != null) { caches.AddCache(uv, triangles, islands); }

            return islands;
        }

        private static List<Island> UVToIslandImpl(IEnumerable<TriangleIndex> trianglesIn, List<Vector2> uvs)
        {
            var triangles = new HashSet<TriangleIndex>(trianglesIn);
            var trianglesByUv = new Dictionary<Vector2, HashSet<TriangleIndex>>(uvs.Count);

            // initialize dictionary for less jumps
            foreach (var uv in uvs)
                if (!trianglesByUv.ContainsKey(uv))
                    trianglesByUv.Add(uv, new());

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

                islands.Add(new(trianglesOfIsland));
            }

            foreach (var island in islands)
            {
                island.BoxCalculation(uvs);
            }

            return islands;
        }

        public static void IslandMoveUV(List<Vector2> uv, List<Vector2> moveUV, Island originIsland, Island movedIsland)
        {
            var tempList = ListPool<int>.Get();
            if (originIsland.Is90Rotation == movedIsland.Is90Rotation)
            {
                var mSize = movedIsland.Size;
                var nmSize = originIsland.Size;

                var relativeScale = new Vector2(mSize.x / nmSize.x, mSize.y / nmSize.y).ValidateNaN();
                foreach (var vertIndex in originIsland.GetVertexIndex(tempList))
                {
                    var vertPos = uv[vertIndex];
                    var relativeVertPos = vertPos - originIsland.Pivot;

                    relativeVertPos.x *= relativeScale.x;
                    relativeVertPos.y *= relativeScale.y;

                    var movedVertPos = movedIsland.Pivot + relativeVertPos;
                    moveUV[vertIndex] = movedVertPos;
                }
            }
            else
            {
                var mSize = movedIsland.Is90Rotation ? new(movedIsland.Size.y, movedIsland.Size.x) : movedIsland.Size;
                var nmSize = originIsland.Is90Rotation ? new(originIsland.Size.y, originIsland.Size.x) : originIsland.Size;

                var relativeScale = new Vector2(mSize.x / nmSize.x, mSize.y / nmSize.y).ValidateNaN();
                var isRotRight = movedIsland.Is90Rotation;
                var rotate = Quaternion.Euler(0, 0, isRotRight ? -90 : 90);

                foreach (var vertIndex in originIsland.GetVertexIndex(tempList))
                {
                    var vertPos = uv[vertIndex];
                    var relativeVertPos = vertPos - originIsland.Pivot;

                    relativeVertPos.x *= relativeScale.x;
                    relativeVertPos.y *= relativeScale.y;

                    relativeVertPos = rotate * relativeVertPos;

                    var movedVertPos = movedIsland.Pivot + relativeVertPos;

                    if (isRotRight) { movedVertPos.y += movedIsland.Size.y; }
                    else { movedVertPos.x += movedIsland.Size.x; }

                    moveUV[vertIndex] = movedVertPos;
                }
            }
            ListPool<int>.Release(tempList);
        }

        private static Vector2 ValidateNaN(this Vector2 relativeScale)
        {
            relativeScale.x = float.IsNaN(relativeScale.x) ? 0 : relativeScale.x;
            relativeScale.y = float.IsNaN(relativeScale.y) ? 0 : relativeScale.y;
            return relativeScale;
        }

        public static void IslandPoolMoveUV<ID, TIsland>(List<Vector2> uv, List<Vector2> moveUV, Dictionary<ID, TIsland> originPool, Dictionary<ID, TIsland> movedPool)
        where TIsland : Island
        {
            if (uv.Count != moveUV.Count) throw new Exception("UV.Count != MoveUV.Count 中身が同一頂点数のUVではありません。");
            foreach (var islandKVP in movedPool)
            {
                var originIsland = originPool[islandKVP.Key];
                IslandMoveUV(uv, moveUV, originIsland, islandKVP.Value);
            }
        }

    }
    [Serializable]
    public class Island
    {
        public List<TriangleIndex> triangles;
        public Vector2 Pivot;
        public Vector2 Size;
        public bool Is90Rotation;

        public Vector2 GetMaxPos => Pivot + Size;

        public Island(Island souse)
        {
            triangles = new List<TriangleIndex>(souse.triangles);
            Pivot = souse.Pivot;
            Size = souse.Size;
            Is90Rotation = souse.Is90Rotation;
        }
        public Island(TriangleIndex triangleIndex)
        {
            triangles = new List<TriangleIndex> { triangleIndex };
        }
        public Island()
        {
            triangles = new List<TriangleIndex>();
        }

        public Island(List<TriangleIndex> trianglesOfIsland)
        {
            triangles = trianglesOfIsland;
        }

        public List<int> GetVertexIndex(List<int> output = null)
        {
            output?.Clear(); output ??= new();
            foreach (var triangle in triangles)
            {
                output.AddRange(triangle);
            }
            return output;
        }
        public List<Vector2> GetVertexPos(IReadOnlyList<Vector2> souseUV)
        {
            var vertIndexes = GetVertexIndex();
            return vertIndexes.ConvertAll<Vector2>(i => souseUV[i]);
        }
        public void BoxCalculation(IReadOnlyList<Vector2> souseUV)
        {
            var vertPoss = GetVertexPos(souseUV);
            var Box = VectorUtility.BoxCal(vertPoss);
            Pivot = Box.min;
            Size = Box.max - Box.min;
        }

        public bool BoxInOut(Vector2 targetPos)
        {
            var relativeTargetPos = targetPos - Pivot;
            return !((relativeTargetPos.x < 0 || relativeTargetPos.y < 0) || (relativeTargetPos.x > Size.x || relativeTargetPos.y > Size.y));
        }
        public List<Vector2> GenerateRectVertexes(float padding = 0, List<Vector2> outPutQuad = null)
        {
            padding = Mathf.Abs(padding);
            outPutQuad?.Clear(); outPutQuad ??= new();
            if (!Is90Rotation)
            {
                outPutQuad.Add(Pivot + new Vector2(-padding, -padding));
                outPutQuad.Add(new Vector2(Pivot.x, Pivot.y + Size.y) + new Vector2(-padding, padding));
                outPutQuad.Add(Pivot + Size + new Vector2(padding, padding));
                outPutQuad.Add(new Vector2(Pivot.x + Size.x, Pivot.y) + new Vector2(padding, -padding));
            }
            else
            {
                outPutQuad.Add(new Vector2(Pivot.x, Pivot.y + Size.y) + new Vector2(-padding, padding));
                outPutQuad.Add(Pivot + Size + new Vector2(padding, padding));
                outPutQuad.Add(new Vector2(Pivot.x + Size.x, Pivot.y) + new Vector2(padding, -padding));
                outPutQuad.Add(Pivot + new Vector2(-padding, -padding));
            }
            return outPutQuad;
        }

        public void Rotate90()
        {
            Is90Rotation = !Is90Rotation;
            (Size.x, Size.y) = (Size.y, Size.x);
        }

    }


    internal static class IslandUtilsDebug
    {
        public static void DrawUV(List<Vector2> uv, Texture2D targetTexture, Color writeColor)
        {
            foreach (var uvPos in uv)
            {
                if (0 <= uvPos.x && uvPos.x <= 1 && 0 <= uvPos.y && uvPos.y <= 1) continue;
                int x = Mathf.RoundToInt(uvPos.x * targetTexture.width);
                int y = Mathf.RoundToInt(uvPos.y * targetTexture.height);
                targetTexture.SetPixel(x, y, writeColor);
            }
        }
    }
}