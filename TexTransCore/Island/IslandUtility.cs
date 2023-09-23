using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransCore.Island
{
    public static class IslandUtility
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

        public static void IslandPoolMoveUV<T>(List<Vector2> UV, List<Vector2> MoveUV, TagIslandPool<T> OriginPool, TagIslandPool<T> MovedPool)
        {
            if (UV.Count != MoveUV.Count) throw new Exception("UV.Count != MoveUV.Count 中身が同一頂点数のUVではありません。");
            foreach (var island in MovedPool)
            {
                var originIsland = OriginPool.FindTag(island.tag);
                IslandMoveUV(UV, MoveUV, originIsland.island, island.island);
            }
        }

        public static void IslandPoolSizeOffset<T>(this TagIslandPool<T> IslandPool, float Offset)
        {
            foreach (var islandI in IslandPool)
            {
                var island = islandI.island;
                island.Size *= Offset;
            }
        }



    }
    public static class IslandSorting
    {
        public enum IslandSortingType
        {
            EvenlySpaced,
            NextFitDecreasingHeight,
            NextFitDecreasingHeightPlusFloorCeiling,
        }
        public static void GenerateMovedIslands<T>(IslandSortingType SortingType, TagIslandPool<T> IslandPool, float Padding = 0.01f)
        {
            switch (SortingType)
            {
                case IslandSortingType.EvenlySpaced:
                    {
                        IslandSorting.IslandPoolEvenlySpaced(IslandPool);
                        break;
                    }
                case IslandSortingType.NextFitDecreasingHeight:
                    {
                        IslandSorting.IslandPoolNextFitDecreasingHeight(IslandPool, Padding);
                        break;
                    }
                case IslandSortingType.NextFitDecreasingHeightPlusFloorCeiling:
                    {
                        IslandSorting.IslandPoolNextFitDecreasingHeightPlusFloorCeiling(IslandPool, Padding);
                        break;
                    }

                default: throw new ArgumentException();
            }
        }
        public static TagIslandPool<T> IslandPoolNextFitDecreasingHeight<T>(
            TagIslandPool<T> TargetPool,
            float IslandsPadding = 0.01f,
            float CrawlingStep = 0.01f,
            float MinHeight = 0.75f,
            int MaxLoopCount = 128)//NFDH
        {
            var Islands = TargetPool.Islands;
            if (!Islands.Any()) return TargetPool;
            foreach (var Island in Islands) { if (Island.Size.y > Island.Size.x) { Island.Rotate90(); } }
            Islands.Sort((l, r) => Mathf.RoundToInt((r.Size.y - l.Size.y) * 100));
            bool success = false;
            float NawScale = 1f;
            int loopCount = -1;

            while (!success && MaxLoopCount > loopCount)
            {
                loopCount += 1;
                success = true;

                var nawPos = new Vector2(IslandsPadding, IslandsPadding);
                float firstHeight = Islands[0].island.Size.y;
                var nawHeight = IslandsPadding + firstHeight + IslandsPadding;

                foreach (var islandAndTag in Islands)
                {
                    var nawSize = islandAndTag.Size;
                    var nawMaxPos = nawPos + nawSize;
                    var isOutOfX = (nawMaxPos.x + IslandsPadding) > 1;

                    if (isOutOfX)
                    {
                        nawPos.y = nawHeight;
                        nawPos.x = IslandsPadding;

                        nawHeight += IslandsPadding + nawSize.y;

                        if (nawHeight > 1)
                        {

                            success = false;

                            ScaleApply(1 - CrawlingStep);
                            break;
                        }
                    }

                    islandAndTag.Pivot = nawPos;

                    nawPos.x += IslandsPadding + nawSize.x;
                }

                if (success && MinHeight > nawHeight)
                {
                    success = false;
                    ScaleApply(1 + CrawlingStep);
                }

            }

            return TargetPool;

            void ScaleApply(float Scale)
            {
                foreach (var islandAndTag in Islands)
                {
                    islandAndTag.Size *= Scale;
                }
                NawScale *= Scale;
            }
        }

        public static TagIslandPool<T> IslandPoolNextFitDecreasingHeightPlusFloorCeiling<T>(
            TagIslandPool<T> TargetPool,
            float IslandPadding = 0.01f,
            float CrawlingStep = 0.01f,
            float MinHeight = 0.75f,
            int MaxLoopCount = 128)//NFDH
        {
            var Islands = TargetPool.Islands;
            if (!Islands.Any()) return TargetPool;
            foreach (var Island in Islands) { if (Island.Size.y > Island.Size.x) { Island.Rotate90(); } }
            Islands.Sort((l, r) => Mathf.RoundToInt((r.Size.y - l.Size.y) * 100));
            bool success = false;
            float nawScale = 1f;
            int loopCount = -1;

            while (!success && MaxLoopCount > loopCount)
            {
                loopCount += 1;
                success = true;

                var boxList = new List<UVWithBox>();


                foreach (var islandAndTag in Islands)
                {
                    var Result = false;
                    foreach (var withBox in boxList)
                    {
                        Result = withBox.TrySetBox(islandAndTag);
                        if (Result) { break; }

                    }
                    if (!Result)
                    {
                        var Floor = boxList.Any() ? boxList.Last().Ceil + IslandPadding : IslandPadding;
                        var Ceil = islandAndTag.island.Size.y + Floor;
                        var newWithBox = new UVWithBox(Ceil, Floor, IslandPadding);
                        var res = newWithBox.TrySetBox(islandAndTag);
                        boxList.Add(newWithBox);
                    }
                }

                var lastHeight = boxList.Last().Ceil + IslandPadding;
                success = lastHeight < 1;

                if (!success)
                {
                    ScaleApply(1 - CrawlingStep);
                }

                if (success && MinHeight > lastHeight)
                {
                    success = false;
                    ScaleApply(1 + CrawlingStep);
                }


            }
            Debug.Log(loopCount);
            return TargetPool;

            void ScaleApply(float Scale)
            {
                foreach (var islandAndTag in Islands)
                {
                    if ((islandAndTag.Size.x * Scale) > (0.999f - IslandPadding)) { continue; }
                    islandAndTag.Size *= Scale;
                }
                nawScale *= Scale;
            }
        }

        private class UVWithBox
        {
            public float with = 1;
            public float Padding;
            public float Ceil;
            public float Floor;
            public float Height => Ceil - Floor;
            public List<Island> Upper = new List<Island>();
            public List<Island> Lower = new List<Island>();

            public UVWithBox(float height, float floor, float padding)
            {
                Ceil = height;
                Floor = floor;
                Padding = padding;
            }

            public bool TrySetBox(Island Box)
            {
                var Island = Box;
                if (Height + 0.01f < Island.Size.y) return false;


                var withMin = Lower.Any() ? Lower.Last().GetMaxPos.x : 0;
                var withMax = GetCeilWithEmpty(Mathf.Clamp(Floor + Island.Size.y + Padding, Floor, Ceil));
                var withSize = withMax - withMin;
                if (withSize > Padding + Island.Size.x + Padding)
                {
                    Island.Pivot = new Vector2(withMin + Padding, Floor);
                    Lower.Add(Box);
                    return true;
                }


                withMin = GetFloorWithEmpty(Mathf.Clamp(Ceil - Island.Size.y - Padding, Floor, Ceil));
                withMax = Upper.Any() ? Upper.Last().Pivot.x : with;
                withSize = withMax - withMin;
                if (withSize > Padding + Island.Size.x + Padding)
                {
                    Island.Pivot = new Vector2(withMax - Island.Size.x - Padding, Ceil - Island.Size.y);
                    Upper.Add(Box);
                    return true;
                }


                return false;
            }

            public float GetFloorWithEmpty(float TargetHeight)
            {
                if (!VectorUtility.InRange(Floor, Ceil, TargetHeight)) { throw new Exception("TargetHeight is not in range!"); }

                var MinWith = 0f;

                foreach (var Box in Lower)
                {
                    var Island = Box;
                    if (VectorUtility.InRange(Island.Pivot.y, Island.GetMaxPos.y, TargetHeight))
                    {
                        if (MinWith < Island.GetMaxPos.x) { MinWith = Island.GetMaxPos.x; }
                    }
                }



                return MinWith;
            }
            public float GetCeilWithEmpty(float TargetHeight)
            {
                if (!VectorUtility.InRange(Floor, Ceil, TargetHeight)) throw new Exception("TargetHeight is not in range!");

                var MaxWith = with;

                foreach (var Box in Upper)
                {
                    var Island = Box;
                    if (VectorUtility.InRange(Island.Pivot.y, Island.GetMaxPos.y, TargetHeight))
                    {
                        if (Island.GetMaxPos.x < MaxWith) { MaxWith = Island.GetMaxPos.x; }
                    }
                }

                return MaxWith;
            }
        }

        public static TagIslandPool<T> IslandPoolEvenlySpaced<T>(TagIslandPool<T> TargetPool)
        {
            Vector2 maxIslandSize = TargetPool.GetLargest().island.Size;
            var gridSize = Mathf.CeilToInt(Mathf.Sqrt(TargetPool.Islands.Count));
            var cellSize = 1f / gridSize;
            int count = 0;
            foreach (var CellIndex in DimensionIndexUtility.Range2d(new Vector2Int(gridSize, gridSize)))
            {
                var CellPos = (Vector2)CellIndex / gridSize;
                Island island;
                if (TargetPool.Islands.Count > count)
                {
                    var Target = TargetPool.Islands[count];
                    island = Target.island;
                }
                else
                {
                    break;
                }

                var islandBox = island.Size;
                island.Pivot = CellPos;

                var islandMaxRange = islandBox.y < islandBox.x ? islandBox.x : islandBox.y;
                if (islandMaxRange > cellSize)
                {
                    islandBox *= (cellSize / islandMaxRange);
                    islandBox *= 0.95f;
                }
                island.Size = islandBox;

                count += 1;
            }
            return TargetPool;
        }
    }
    public class TagIslandPool<Tag> : IEnumerable<TagIsland<Tag>>
    {
        public List<TagIsland<Tag>> Islands = new List<TagIsland<Tag>>();

        public TagIsland<Tag> this[int index] => Islands[index];

        public HashSet<Tag> GetTag()
        {
            HashSet<Tag> tags = new HashSet<Tag>();
            foreach (var island in Islands)
            {
                tags.Add(island.tag);
            }
            return tags;
        }
        public void AddIsland(TagIsland<Tag> item)
        {
            Islands.Add(item);
        }
        public void AddIsland(Island item, Tag tag)
        {
            Islands.Add(new TagIsland<Tag>(item, tag, false));
        }
        public void AddRangeIsland(List<TagIsland<Tag>> items)
        {
            foreach (var item in items)
            {
                AddIsland(item);
            }
        }
        public void AddRangeIsland(List<Island> items, Tag tag)
        {
            foreach (var item in items)
            {
                AddIsland(item, tag);
            }
        }
        public void AddRangeIsland(TagIslandPool<Tag> AddPool)
        {
            foreach (var item in AddPool)
            {
                AddIsland(item);
            }
        }
        public int RemoveAll(Tag tag)
        {
            return Islands.RemoveAll(I => I.tag.Equals(tag));
        }
        public TagIsland<Tag> FindTag(Tag tag)
        {
            return Islands.Find(I => I.tag.Equals(tag));
        }
        public IEnumerator<TagIsland<Tag>> GetEnumerator()
        {
            return Islands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Islands.GetEnumerator();
        }

        public TagIslandPool<Tag> Clone()
        {
            var NewPool = new TagIslandPool<Tag>();
            foreach (var island in Islands)
            {
                NewPool.AddIsland(new TagIsland<Tag>(island.island, island.tag, true));
            }
            return NewPool;
        }


        public TagIsland<Tag> GetLargest()
        {
            int getIndex = -1;
            int count = -1;
            Vector2 cache = new Vector2(0, 0);
            foreach (var islandAndTag in Islands)
            {
                count += 1;
                var Island = islandAndTag;
                if (cache.sqrMagnitude < Island.Size.sqrMagnitude)
                {
                    cache = islandAndTag.Size;
                    getIndex = count;
                }
            }
            if (getIndex != -1)
            {
                return Islands[getIndex];
            }
            else
            {
                return null;
            }
        }


    }
    public class TagIsland<Tag> : Island
    {
        public Tag tag;
        /// <summary>
        /// 互換性のため
        /// </summary>
        public Island island => this;
        public TagIsland(Island Souse, Tag tag, bool DeepClone = false)
        {
            if (DeepClone)
            {
                triangles = new List<TriangleIndex>(Souse.triangles);
            }
            else
            {
                triangles = Souse.triangles;
            }
            Pivot = Souse.Pivot;
            Size = Souse.Size;
            this.tag = tag;
        }
        public TagIsland(TagIsland<Tag> Souse, bool DeepClone = false)
        {
            if (DeepClone)
            {
                triangles = new List<TriangleIndex>(Souse.triangles);
            }
            else
            {
                triangles = Souse.triangles;
            }
            Pivot = Souse.Pivot;
            Size = Souse.Size;
            tag = Souse.tag;
        }
        public TagIsland()
        {

        }
    }

    [Serializable]
    public class Island
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


    public static class IslandUtilsDebug
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
        public static void DrawerIslandBox<T>(TagIslandPool<T> Pool, Texture2D TargetTexture, Color WriteColor)
        {
            foreach (var island in Pool.Islands)
            {
                var minPos = new Vector2Int(Mathf.RoundToInt(island.island.Pivot.x * TargetTexture.width), Mathf.RoundToInt(island.island.Pivot.y * TargetTexture.height));
                var maxPos = new Vector2Int(Mathf.RoundToInt(island.island.GetMaxPos.x * TargetTexture.width), Mathf.RoundToInt(island.island.GetMaxPos.y * TargetTexture.height));
                Vector2Int pos = minPos;
                while (maxPos.x > pos.x)
                {
                    TargetTexture.SetPixel(pos.x, pos.y, WriteColor);
                    pos.x += 1;
                }
                pos.x = minPos.x;
                pos.y = maxPos.y;
                while (maxPos.x > pos.x)
                {
                    TargetTexture.SetPixel(pos.x, pos.y, WriteColor);
                    pos.x += 1;
                }

                pos = minPos;
                while (maxPos.y > pos.y)
                {
                    TargetTexture.SetPixel(pos.x, pos.y, WriteColor);
                    pos.y += 1;
                }
                pos.x = maxPos.x;
                pos.y = minPos.y;
                while (maxPos.y > pos.y)
                {
                    TargetTexture.SetPixel(pos.x, pos.y, WriteColor);
                    pos.y += 1;
                }
            }
        }
    }
}