using System;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace net.rs64.TexTransCore.Island
{
    internal static class IslandUtility
    {
        /// <summary>
        /// Union-FindアルゴリズムのためのNode Structureです。細かいアロケーションの負荷を避けるために、配列で管理する想定で、
        /// ポインターではなくインデックスで親ノードを指定します。
        ///
        /// グループの代表でない限り、parentIndex以外の値は無視されます（古いデータが入る場合があります）
        /// </summary>
        internal struct VertNode
        {
            public int parentIndex;
            
            public (Vector2, Vector2) boundingBox;
            
            public int depth;
            public int triCount;

            public Island island;
            
            public VertNode(int i, Vector2 uv)
            {
                parentIndex = i;
                boundingBox = (uv, uv);
                depth = 0;
                island = null;
                triCount = 0;
            }

            /// <summary>
            /// 指定したインデックスのノードのグループの代表ノードを調べる
            /// </summary>
            /// <param name="arr"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public static int Find(VertNode[] arr, int index)
            {
                if (arr[index].parentIndex == index) return index;
                
                return arr[index].parentIndex = Find(arr, arr[index].parentIndex);
            }

            /// <summary>
            /// 指定したふたつのノードを結合する
            /// </summary>
            /// <param name="arr"></param>
            /// <param name="a"></param>
            /// <param name="b"></param>
            public static void Merge(VertNode[] arr, int a, int b)
            {
                a = Find(arr, a);
                b = Find(arr, b);

                if (a == b) return;
                
                if (arr[a].depth < arr[b].depth)
                {
                    (a, b) = (b, a);
                }
                
                if (arr[a].depth == arr[b].depth) arr[a].depth++;
                arr[b].parentIndex = a;
                
                arr[a].boundingBox = (Vector2.Min(arr[a].boundingBox.Item1, arr[b].boundingBox.Item1),
                    Vector2.Max(arr[a].boundingBox.Item2, arr[b].boundingBox.Item2));
                arr[a].triCount += arr[b].triCount;
            }

            /// <summary>
            /// このグループに該当するIslandに三角面を追加します。Islandが存在しない場合は作成しislandListに追加します。
            /// </summary>
            /// <param name="idx"></param>
            /// <param name="islandList"></param>
            public void AddTriangle(TriangleIndex idx, List<Island> islandList)
            {
                if (island == null)
                {
                    islandList.Add(island = new Island());
                    island.triangles.Capacity = triCount;
                    
                    var min = boundingBox.Item1;
                    var max = boundingBox.Item2;
                    
                    island.Size = new Vector2(max.x - min.x, max.y - min.y);
                    island.Pivot = min;
                }
                island.triangles.Add(idx);
            }
        }

        public static List<Island> UVtoIsland(MeshData meshData)
        {
            return UVtoIsland(meshData.CombinedTriangleIndex.AsList(), meshData.VertexUV.AsList());
        }
        
        public static List<Island> UVtoIsland(IList<TriangleIndex> triIndexes, IList<Vector2> vertexUV)
        {
            Profiler.BeginSample("UVtoIsland");
            var islands = UVToIslandImpl(triIndexes, vertexUV);
            Profiler.EndSample();
         
            return islands;
        }

        private static List<Island> UVToIslandImpl(IList<TriangleIndex> triIndexes, IList<Vector2> vertexUV)
        {
            int uniqueUv = 0;
            var vertCount = vertexUV.Count;
            List<Vector2> indexToUv = new List<Vector2>(vertCount);
            Dictionary<Vector2, int> uvToIndex = new Dictionary<Vector2, int>(vertCount);
            List<int> inputVertToUniqueIndex = new List<int>(vertCount);

            // 同一の位置にある頂点をまず調べて、共通のインデックスを割り当てます
            Profiler.BeginSample("Preprocess vertices");
            foreach (var uv in vertexUV)
            {
                if (!uvToIndex.TryGetValue(uv, out var uvVert))
                {
                    uvVert = uvToIndex[uv] = uniqueUv++;
                    indexToUv.Add(uv);
                }
                
                inputVertToUniqueIndex.Add(uvVert);
            }
            Profiler.EndSample();
            
            VertNode[] nodes = new VertNode[uniqueUv];

            // Union-Find用のデータストラクチャーを初期化
            Profiler.BeginSample("Init vertNodes");
            for (int i = 0; i < uniqueUv; i++)
            {
                nodes[i] = new VertNode(i, indexToUv[i]);
            }
            Profiler.EndSample();

            Profiler.BeginSample("Merge vertices");
            foreach (var tri in triIndexes)
            {
                int idx_a = inputVertToUniqueIndex[tri.zero];
                int idx_b = inputVertToUniqueIndex[tri.one];
                int idx_c = inputVertToUniqueIndex[tri.two];
                
                // 三角面に該当するノードを併合
                VertNode.Merge(nodes, idx_a, idx_b);
                VertNode.Merge(nodes, idx_b, idx_c);
                
                // 際アロケーションを避けるために三角面を数える
                nodes[VertNode.Find(nodes, idx_a)].triCount++;
            }
            Profiler.EndSample();
            
            var islands = new List<Island>();
            
            // この時点で代表が決まっているので、三角を追加していきます。
            Profiler.BeginSample("Add triangles to islands");
            foreach (var tri in triIndexes)
            {
                int idx = inputVertToUniqueIndex[tri.zero];

                nodes[VertNode.Find(nodes, idx)].AddTriangle(tri, islands);
            }
            Profiler.EndSample();

            return islands;
        }
        public static void IslandMoveUV<TIsland>(List<Vector2> uv, List<Vector2> moveUV, Island originIsland, TIsland movedIsland, bool keepIslandUVTile = false) where TIsland : IIslandRect
        {
            if (originIsland.Is90Rotation) { throw new ArgumentException("originIsland.Is90Rotation is true"); }

            var tempList = ListPool<int>.Get();
            var rotate = Quaternion.Euler(0, 0, -90);

            var mSize = movedIsland.Is90Rotation ? new(movedIsland.Size.y, movedIsland.Size.x) : movedIsland.Size;
            var nmSize = originIsland.Size;

            var relativeScale = new Vector2(mSize.x / nmSize.x, mSize.y / nmSize.y).NaNtoZero();

            foreach (var vertIndex in originIsland.GetVertexIndex(tempList))
            {
                var relativeVertPos = uv[vertIndex] - originIsland.Pivot;

                relativeVertPos.x *= relativeScale.x;
                relativeVertPos.y *= relativeScale.y;

                if (movedIsland.Is90Rotation) { relativeVertPos = rotate * relativeVertPos; relativeVertPos.y += movedIsland.Size.y; }

                var movedPos = movedIsland.Pivot + relativeVertPos;
                if (keepIslandUVTile) { movedPos.x += Mathf.Floor(originIsland.Pivot.x); movedPos.y += Mathf.Floor(originIsland.Pivot.y); }
                moveUV[vertIndex] = movedPos;
            }

            ListPool<int>.Release(tempList);
        }

        private static Vector2 NaNtoZero(this Vector2 relativeScale)
        {
            relativeScale.x = float.IsNaN(relativeScale.x) ? 0 : relativeScale.x;
            relativeScale.y = float.IsNaN(relativeScale.y) ? 0 : relativeScale.y;
            return relativeScale;
        }

        public static void IslandPoolMoveUV<ID, TIsland, TIslandRect>(List<Vector2> uv, List<Vector2> moveUV, Dictionary<ID, TIsland> originPool, Dictionary<ID, TIslandRect> movedPool)
        where TIsland : Island
        where TIslandRect : IIslandRect
        {
            if (uv.Count != moveUV.Count) throw new ArgumentException("UV.Count != MoveUV.Count 中身が同一頂点数のUVではありません。");
            foreach (var islandKVP in movedPool)
            {
                var originIsland = originPool[islandKVP.Key];
                IslandMoveUV(uv, moveUV, originIsland, islandKVP.Value, true);
            }
        }

    }
    [Serializable]
    public class Island : IIslandRect
    {
        public List<TriangleIndex> triangles;
        public Vector2 Pivot;
        public Vector2 Size;
        public bool Is90Rotation;

        public Vector2 GetMaxPos => Pivot + Size;

        Vector2 IIslandRect.Pivot { get => Pivot; set => Pivot = value; }
        Vector2 IIslandRect.Size { get => Size; set => Size = value; }
        bool IIslandRect.Is90Rotation { get => Is90Rotation; set => Is90Rotation = value; }

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

        public void Rotate90()
        {
            Is90Rotation = !Is90Rotation;
            (Size.x, Size.y) = (Size.y, Size.x);
        }

    }

    internal interface IIslandRect
    {
        public Vector2 Pivot { set; get; }
        public Vector2 Size { set; get; }
        public bool Is90Rotation { set; get; }
    }

    internal static class IslandRectUtility
    {
        internal static float CalculateAllAreaSum<TIslandRect>(IEnumerable<TIslandRect> islandRect) where TIslandRect : IIslandRect
        {
            var sum = 0f;
            foreach (var rect in islandRect) { sum += rect.Size.x * rect.Size.y; }
            return sum;
        }
        internal static float CalculateIslandsMaxHeight<TIslandRect>(IEnumerable<TIslandRect> islandRectPool) where TIslandRect : IIslandRect
        {
            var height = 0f;
            foreach (var islandRect in islandRectPool) { height = Mathf.Max(height, islandRect.Pivot.y + islandRect.Size.y); }
            return height;
        }
        public static Vector2 GetMaxPos<TIslandRect>(this TIslandRect islandRect) where TIslandRect : IIslandRect { return islandRect.Pivot + islandRect.Size; }
        public static float UVScaleToRectScale<TIslandRect>(this TIslandRect islandRect, float uvScaleValue) where TIslandRect : IIslandRect
        {
            return new Vector2(uvScaleValue, uvScaleValue).magnitude / islandRect.Size.magnitude;
        }

        public static IEnumerable<Vector2> GenerateRectVertexes<TIslandRect>(this TIslandRect islandRect, float rectScalePadding = 0.1f, List<Vector2> outPutQuad = null)
        where TIslandRect : IIslandRect
        {
            outPutQuad?.Clear(); outPutQuad ??= new();

            var rectScale = Mathf.Abs(rectScalePadding) * islandRect.Size.magnitude;
            var paddingVector = Vector2.ClampMagnitude(Vector2.one, rectScale);

            var leftDown = islandRect.Pivot;
            var rightUp = islandRect.Pivot + islandRect.Size;

            if (!islandRect.Is90Rotation)
            {
                yield return (new(leftDown.x - paddingVector.x, leftDown.y - paddingVector.y));
                yield return (new(leftDown.x - paddingVector.x, rightUp.y + paddingVector.y));
                yield return (new(rightUp.x + paddingVector.x, rightUp.y + paddingVector.y));
                yield return (new(rightUp.x + paddingVector.x, leftDown.y - paddingVector.y));
            }
            else
            {
                yield return (new(leftDown.x - paddingVector.x, rightUp.y + paddingVector.y));
                yield return (new(rightUp.x + paddingVector.x, rightUp.y + paddingVector.y));
                yield return (new(rightUp.x + paddingVector.x, leftDown.y - paddingVector.y));
                yield return (new(leftDown.x - paddingVector.x, leftDown.y - paddingVector.y));
            }
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
