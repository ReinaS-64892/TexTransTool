#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace Rs64.TexTransTool.TexturAtlas
{

    public static class IslandUtils
    {

        public static IslandPool IslandPoolNextFitDecreasingHeight(IslandPool TargetPool, float IslanadsPading = 0.01f, float ClorreScaile = 0.01f, float MinHeight = 0.75f, int MaxLoopCount = 128)//NFDH
        {
            var Islands = TargetPool.IslandPoolList;
            if (!Islands.Any()) return TargetPool;
            Islands.Sort((l, r) => Mathf.RoundToInt((r.island.Size.y - l.island.Size.y) * 100));
            bool Success = false;
            float NawScaile = 1f;
            int loopCount = -1;

            while (!Success && MaxLoopCount > loopCount)
            {
                loopCount += 1;
                Success = true;

                var NawPos = new Vector2(IslanadsPading, IslanadsPading);
                float FirstHeight = Islands[0].island.Size.y;
                var NawHeight = IslanadsPading + FirstHeight + IslanadsPading;

                foreach (var islandandIndex in Islands)
                {
                    var Island = islandandIndex.island;
                    var NawSize = Island.Size;
                    var NawMaxPos = NawPos + NawSize;
                    var IsOutOfX = (NawMaxPos.x + IslanadsPading) > 1;

                    if (IsOutOfX)
                    {
                        NawPos.y = NawHeight;
                        NawPos.x = IslanadsPading;

                        NawHeight += IslanadsPading + NawSize.y;

                        if (NawHeight > 1)
                        {

                            Success = false;

                            ScaileAppry(1 - ClorreScaile);
                            break;
                        }
                    }

                    Island.Pivot = NawPos;

                    NawPos.x += IslanadsPading + NawSize.x;
                }

                if (Success && MinHeight > NawHeight)
                {
                    Success = false;
                    ScaileAppry(1 + ClorreScaile);
                }

            }

            return TargetPool;

            void ScaileAppry(float Scaile)
            {
                foreach (var islandandIndex in Islands)
                {
                    var Island = islandandIndex.island;
                    Island.Size *= Scaile;
                }
                NawScaile *= Scaile;
            }
        }

        public static List<Island> UVtoIsland(List<TraiangleIndex> traiangles, List<Vector2> UV)
        {
            var Islands = traiangles.ConvertAll<Island>(i => new Island(i));

            bool Continue = true;
            while (Continue)
            {
                Continue = false;
                Islands = IslandCrawling(Islands, UV, ref Continue);
            }
            Islands.ForEach(i => i.BoxCurriculation(UV));
            return Islands;
        }

        public static List<Island> IslandCrawling(List<Island> IslandPool, List<Vector2> UV, ref bool IsJoin)
        {

            var CrawlingdIslandPool = new List<Island>();

            foreach (var Iland in IslandPool)
            {
                var IslandVartPos = Iland.GetVertexPos(UV);


                int IlandCout = -1;
                int IlandJoinIndex = -1;

                foreach (var CrawlingdIsland in CrawlingdIslandPool)
                {
                    IlandCout += 1;

                    var CrawlingIslandVartPos = CrawlingdIsland.GetVertexPos(UV);


                    if (IslandVartPos.Intersect(CrawlingIslandVartPos).Any())
                    {
                        IlandJoinIndex = IlandCout;
                        break;
                    }

                }

                if (IlandJoinIndex == -1)
                {
                    CrawlingdIslandPool.Add(Iland);
                }
                else
                {
                    CrawlingdIslandPool[IlandJoinIndex].trainagels.AddRange(Iland.trainagels);
                    IsJoin = true;
                }

            }
            return CrawlingdIslandPool;
        }

        public static IslandPool IslandPoolEvenlySpaced(IslandPool TargetPool)
        {
            Vector2 MaxIslandSize = TargetPool.GetLargest().island.Size;
            var GridSize = Mathf.CeilToInt(Mathf.Sqrt(TargetPool.IslandPoolList.Count));
            var CellSize = 1f / GridSize;
            int Count = 0;
            foreach (var CellIndex in Utils.Reange2d(new Vector2Int(GridSize, GridSize)))
            {
                var CellPos = (Vector2)CellIndex / GridSize;
                MeshIndex MapIndex;
                int IslandIndex;
                Island Island;
                if (TargetPool.IslandPoolList.Count > Count)
                {
                    var Target = TargetPool.IslandPoolList[Count];
                    Island = Target.island;
                    MapIndex = Target.MapIndex;
                    IslandIndex = Target.IslandIndex;
                }
                else
                {
                    break;
                }

                var IslandBox = Island.Size;
                Island.Pivot = CellPos;

                var IslandMaxRanege = IslandBox.y < IslandBox.x ? IslandBox.x : IslandBox.y;
                if (IslandMaxRanege > CellSize)
                {
                    IslandBox *= (CellSize / IslandMaxRanege);
                    IslandBox *= 0.95f;
                }
                Island.Size = IslandBox;

                Count += 1;
            }
            return TargetPool;
        }

        public static List<List<Vector2>> UVsMove(List<List<Vector2>> UVs, IslandPool Original, IslandPool Moved)
        {
            List<List<Vector2>> MovedUV = CloneUVs(UVs);

            foreach (var Index in Enumerable.Range(0, Moved.IslandPoolList.Count))
            {
                MoveUV(UVs, Original, Moved, MovedUV, Index);
            }

            return MovedUV;
        }
        public static async Task<List<List<Vector2>>> UVsMoveAsync(List<List<Vector2>> UVs, IslandPool Original, IslandPool Moved)
        {
            List<List<Vector2>> MovedUV = CloneUVs(UVs);
            List<ConfiguredTaskAwaitable> Tasks = new List<ConfiguredTaskAwaitable>();
            foreach (var Index in Enumerable.Range(0, Moved.IslandPoolList.Count))
            {
                var Indexi = Index;
                Tasks.Add(Task.Run(() => MoveUV(UVs, Original, Moved, MovedUV, Indexi)).ConfigureAwait(false));
            }
            foreach (var task in Tasks)
            {
                await task;
            }
            return MovedUV;




        }
        static void MoveUV(List<List<Vector2>> UVs, IslandPool Original, IslandPool Moved, List<List<Vector2>> MovedUV, int Index)
        {
            var MapIndex = Moved.IslandPoolList[Index].MapIndex;
            var MovedIslandI = Moved.IslandPoolList[Index];

            var VertexIndex = MovedIslandI.island.GetVertexIndex();
            var NotMovedIslandI = Original.IslandPoolList.Find(i => i.MapIndex.Index == MovedIslandI.MapIndex.Index && i.IslandIndex == MovedIslandI.IslandIndex);

            var mIsland = MovedIslandI.island;
            var nmIsland = NotMovedIslandI.island;

            var mSize = mIsland.Size;
            var nmSize = nmIsland.Size;
            var RelativeScaile = new Vector2(mSize.x / nmSize.x, mSize.y / nmSize.y);

            foreach (var TrinagleIndex in VertexIndex)
            {
                var VertPos = UVs[MapIndex.Index][TrinagleIndex];
                var RelativeVertPos = VertPos - nmIsland.Pivot;

                RelativeVertPos.x *= RelativeScaile.x;
                RelativeVertPos.y *= RelativeScaile.y;

                var MovedVertPos = mIsland.Pivot + RelativeVertPos;
                MovedUV[MapIndex.Index][TrinagleIndex] = MovedVertPos;
            }
        }

        public static async Task<IslandPool> AsyncGeneretIslandPool(List<Mesh> Data, List<List<Vector2>> UVs, List<MeshIndex> SelectUV)
        {
            var IslandPool = new IslandPool();

            List<ConfiguredTaskAwaitable<List<IslandPool.IslandAndIndex>>> Tesks = new List<ConfiguredTaskAwaitable<List<IslandPool.IslandAndIndex>>>();
            foreach (var index in SelectUV)
            {
                var mapcount = index;//Asyncな奴に投げている関係かこうしないとばぐるたぶん
                var Triangle = Utils.ToList(Data[index.Index].GetTriangles(index.SubMeshIndex));
                Tesks.Add(Task.Run<List<IslandPool.IslandAndIndex>>(() => GeneretIslandAndIndex(UVs[index.Index], Triangle, mapcount)).ConfigureAwait(false));
            }
            foreach (var task in Tesks)
            {
                IslandPool.IslandPoolList.AddRange(await task);
            }

            return IslandPool;

        }
        static List<IslandPool.IslandAndIndex> GeneretIslandAndIndex(List<Vector2> UV, List<TraiangleIndex> traiangles, MeshIndex MapCount)
        {
            var Islanads = IslandUtils.UVtoIsland(traiangles, UV);
            var IslandPoolList = new List<IslandPool.IslandAndIndex>();
            int IlandIndex = -1;
            foreach (var Islnad in Islanads)
            {
                IlandIndex += 1;
                IslandPoolList.Add(new IslandPool.IslandAndIndex(Islnad, MapCount, IlandIndex));
            }
            return IslandPoolList;
        }
        public static List<List<Vector2>> GetUVs(this AtlasCompileData Data, int UVindex = 0)
        {
            var UVs = new List<List<Vector2>>();

            foreach (var Mesh in Data.meshes)
            {
                List<Vector2> uv = new List<Vector2>();
                Mesh.GetUVs(UVindex, uv);
                UVs.Add(uv);
            }
            return UVs;
        }
        public static void SetUVs(this AtlasCompileData Data, List<List<Vector2>> UVs, int UVindex = 0)
        {
            int Count = -1;
            foreach (var Mesh in Data.meshes)
            {
                Count += 1;
                Mesh.SetUVs(UVindex, UVs[Count]);
            }
        }


        public static List<List<Vector2>> CloneUVs(List<List<Vector2>> UVs)
        {
            var Clone = new List<List<Vector2>>();

            foreach (var uv in UVs)
            {
                Clone.Add(new List<Vector2>(uv));
            }
            return Clone;
        }
    }

    public class IslandPool
    {
        public List<IslandAndIndex> IslandPoolList = new List<IslandAndIndex>();

        public IslandPool(List<IslandAndIndex> List)
        {
            IslandPoolList = List;
        }

        public IslandPool()
        {
        }

        public IslandPool(IslandPool targetPool)
        {
            foreach (var island in targetPool.IslandPoolList)
            {
                IslandPoolList.Add(new IslandAndIndex(island));
            }
        }

        public class IslandAndIndex
        {
            public IslandAndIndex(Island island, MeshIndex mapIndex, int islandInx)
            {
                this.island = new Island(island);
                MapIndex = mapIndex;
                IslandIndex = islandInx;
            }

            public IslandAndIndex(IslandAndIndex Souse)
            {
                this.island = new Island(Souse.island);
                MapIndex = Souse.MapIndex;
                IslandIndex = Souse.IslandIndex;
            }

            public Island island { get; set; }
            public MeshIndex MapIndex { get; set; }
            public int IslandIndex { get; set; }
        }

        public IslandAndIndex GetLargest()
        {
            int GetIndex = -1;
            int Count = -1;
            Vector2 Cash = new Vector2(0, 0);
            foreach (var islandandI in IslandPoolList)
            {
                Count += 1;
                var Island = islandandI.island;
                if (Cash.sqrMagnitude < Island.Size.sqrMagnitude)
                {
                    Cash = islandandI.island.Size;
                    GetIndex = Count;
                }
            }
            if (GetIndex != -1)
            {
                return IslandPoolList[GetIndex];
            }
            else
            {
                return null;
            }
        }
    }
    public class Island
    {
        public List<TraiangleIndex> trainagels = new List<TraiangleIndex>();
        public Vector2 Pivot;
        public Vector2 Size;

        public Vector2 GetMaxPos => (Pivot + Size);

        public Island(Island Souse)
        {
            trainagels = new List<TraiangleIndex>(Souse.trainagels);
            Pivot = Souse.Pivot;
            Size = Souse.Size;
        }
        public Island(TraiangleIndex traiangleIndex)
        {
            trainagels.Add(traiangleIndex);
        }
        public Island()
        {

        }
        public List<int> GetVertexIndex()
        {
            var IndexList = new List<int>();
            foreach (var traiangle in trainagels)
            {
                IndexList.AddRange(traiangle.ToArray());
            }
            return IndexList;
        }
        public List<Vector2> GetVertexPos(List<Vector2> SouseUV)
        {
            var VIndexs = GetVertexIndex();
            return VIndexs.ConvertAll<Vector2>(i => SouseUV[i]);
        }
        public void BoxCurriculation(List<Vector2> SouseUV)
        {
            var VartPoss = GetVertexPos(SouseUV);
            var Box = TransMapper.BoxCal(VartPoss);
            Pivot = Box.Item1;
            Size = Box.Item2 - Box.Item1;
        }

        public bool BoxInOut(Vector2 TargetPos)
        {
            var RelaTargetPos = TargetPos - Pivot;
            return !((RelaTargetPos.x < 0 || RelaTargetPos.y < 0) || (RelaTargetPos.x > Size.x || RelaTargetPos.y > Size.y));
        }

    }


    public static class IslandUtilsDebug
    {
        public static void DorwUV(List<Vector2> UV, Texture2D TargetTextur, Color WriteColor)
        {
            foreach (var uvpos in UV)
            {
                if (0 <= uvpos.x && uvpos.x <= 1 && 0 <= uvpos.y && uvpos.y <= 1) continue;
                int x = Mathf.RoundToInt(uvpos.x * TargetTextur.width);
                int y = Mathf.RoundToInt(uvpos.y * TargetTextur.height);
                TargetTextur.SetPixel(x, y, WriteColor);
            }
        }
        public static void DrowIlandBox(IslandPool Pool, Texture2D TargetTextur, Color WriteColor)
        {
            foreach (var island in Pool.IslandPoolList)
            {
                var minpos = new Vector2Int(Mathf.RoundToInt(island.island.Pivot.x * TargetTextur.width), Mathf.RoundToInt(island.island.Pivot.y * TargetTextur.height));
                var maxpos = new Vector2Int(Mathf.RoundToInt(island.island.GetMaxPos.x * TargetTextur.width), Mathf.RoundToInt(island.island.GetMaxPos.y * TargetTextur.height));
                Vector2Int pos = minpos;
                while (maxpos.x > pos.x)
                {
                    TargetTextur.SetPixel(pos.x, pos.y, WriteColor);
                    pos.x += 1;
                }
                pos.x = minpos.x;
                pos.y = maxpos.y;
                while (maxpos.x > pos.x)
                {
                    TargetTextur.SetPixel(pos.x, pos.y, WriteColor);
                    pos.x += 1;
                }

                pos = minpos;
                while (maxpos.y > pos.y)
                {
                    TargetTextur.SetPixel(pos.x, pos.y, WriteColor);
                    pos.y += 1;
                }
                pos.x = maxpos.x;
                pos.y = minpos.y;
                while (maxpos.y > pos.y)
                {
                    TargetTextur.SetPixel(pos.x, pos.y, WriteColor);
                    pos.y += 1;
                }
            }
        }
    }
}
#endif