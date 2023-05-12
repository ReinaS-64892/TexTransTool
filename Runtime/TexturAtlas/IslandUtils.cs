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

        public static IslandPool IslandPoolNextFitDecreasingHeight(IslandPool TargetPool, float IslanadsPading = 0.01f, float ClorreScaile = 0.99f, float UpClorreScaile = 1.01f, float MinHeight = 0.75f, int MaxLoopCount = 128)//NFDH
        {
            var ClonedPool = new IslandPool(TargetPool);
            var Islands = ClonedPool.IslandPoolList;
            Islands.Sort((l, r) => Mathf.RoundToInt((r.island.GetSize.y - l.island.GetSize.y) * 100));
            bool Success = false;
            float NawScaile = 1f;
            int loopCount = -1;
            while (!Success)
            {
                loopCount += 1;
                if (loopCount > MaxLoopCount) break;
                Success = true;
                var NawPos = new Vector2(IslanadsPading, IslanadsPading);
                float FirstHeight = 0;
                if (Islands.Any()) FirstHeight = Islands[0].island.GetSize.y * NawScaile;
                var NawMaxHigt = IslanadsPading + FirstHeight + IslanadsPading;
                foreach (var islandandIndex in Islands)
                {
                    var NawSize = islandandIndex.island.GetSize;
                    var NawMaxPos = NawPos + NawSize;
                    var IsOutOfX = (NawMaxPos.x * (NawScaile > 1 ? NawScaile : 1) + IslanadsPading) > 1;

                    if (IsOutOfX)
                    {
                        NawPos.y = NawMaxHigt;
                        NawPos.x = IslanadsPading;

                        NawMaxHigt += IslanadsPading + NawSize.y * NawScaile;

                        if (NawMaxHigt > 1)
                        {

                            Success = false;

                            Islands.ForEach(i => i.island.MaxIlandBox = i.island.MinIlandBox + (i.island.GetSize * ClorreScaile));
                            NawScaile *= ClorreScaile;
                            break;
                        }
                    }
                    islandandIndex.island.MinIlandBox = NawPos;
                    islandandIndex.island.MaxIlandBox = NawPos + NawSize;

                    NawPos.x += IslanadsPading + NawSize.x * NawScaile;
                    //高さの更新や現在の位置の移動の今の時のスケールをかけないとうまくいかない理由は何もわからない...
                    //GetSize周りがなぜかうまくいっていないのだろうか...いろいろ試しても何もわからないので一回あきらめよう...

                }
                if (MinHeight > NawMaxHigt)
                {
                    Success = false;
                    Islands.ForEach(i => i.island.MaxIlandBox = i.island.MinIlandBox + (i.island.GetSize * UpClorreScaile));
                    NawScaile *= UpClorreScaile;
                }
            }
            //Debug.Log(loopCount + " " + NawScaile + " " + Success);
            return ClonedPool;
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
            var MovedIslandPool = new IslandPool();
            Vector2 MaxIslandSize = TargetPool.GetLargest().island.GetSize;
            var GridSize = Mathf.CeilToInt(Mathf.Sqrt(TargetPool.IslandPoolList.Count));
            var CellSize = 1f / GridSize;
            int Count = 0;
            foreach (var CellIndex in Utils.Reange2d(new Vector2Int(GridSize, GridSize)))
            {
                var CellPos = (Vector2)CellIndex / GridSize;
                int MapIndex;
                int IslandIndex;
                Island Island;
                if (TargetPool.IslandPoolList.Count > Count)
                {
                    var Target = TargetPool.IslandPoolList[Count];
                    Island = new Island(Target.island);
                    MapIndex = Target.MapIndex;
                    IslandIndex = Target.IslandIndex;
                }
                else
                {
                    break;
                }

                var IslandBox = Island.GetSize;
                Island.MinIlandBox = CellPos;

                var IslandMaxRanege = IslandBox.y < IslandBox.x ? IslandBox.x : IslandBox.y;
                if (IslandMaxRanege > CellSize)
                {
                    IslandBox *= (CellSize / IslandMaxRanege);
                    IslandBox *= 0.95f;
                }
                Island.MaxIlandBox = CellPos + IslandBox;

                MovedIslandPool.IslandPoolList.Add(new IslandPool.IslandAndIndex(Island, MapIndex, IslandIndex));
                Count += 1;
            }
            return MovedIslandPool;
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
                Tasks.Add(Task.Run(() => MoveUV(UVs, Original, Moved, MovedUV, Index)).ConfigureAwait(false));
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
            var MovedIsland = Moved.IslandPoolList[Index];

            var VertexIndex = MovedIsland.island.GetVertexIndex();
            var NotMovedIsland = Original.IslandPoolList.Find(i => i.MapIndex == MovedIsland.MapIndex && i.IslandIndex == MovedIsland.IslandIndex);


            float RelativeScaile = MovedIsland.island.GetSize.sqrMagnitude / NotMovedIsland.island.GetSize.sqrMagnitude;

            foreach (var TrinagleIndex in VertexIndex)
            {
                var VertPos = UVs[MapIndex][TrinagleIndex];
                var RelativeVertPos = VertPos - NotMovedIsland.island.MinIlandBox;
                RelativeVertPos *= RelativeScaile;
                var MovedVertPos = MovedIsland.island.MinIlandBox + RelativeVertPos;
                MovedUV[MapIndex][TrinagleIndex] = MovedVertPos;
            }
        }
        [Obsolete]
        public static IslandPool GeneretIslandPool(this AtlasCompileData Data)
        {
            return GeneretIslandPool(Data.meshes);
        }
        [Obsolete]
        public static IslandPool GeneretIslandPool(List<Mesh> Data)
        {
            var IslandPool = new IslandPool();

            int MapCount = -1;
            foreach (var data in Data)
            {
                MapCount += 1;
                var UV = new List<Vector2>();
                data.GetUVs(0, UV);
                var Triangle = Utils.ToList(data.triangles);
                IslandPool.IslandPoolList.AddRange(GeneretIslandAndIndex(UV, Triangle, MapCount));
            }
            return IslandPool;
        }

        public static async Task<IslandPool> AsyncGeneretIslandPool(List<Mesh> Data, List<List<Vector2>> UVs, List<MeshIndex> SelectUV)
        {
            var IslandPool = new IslandPool();

            List<ConfiguredTaskAwaitable<List<IslandPool.IslandAndIndex>>> Tesks = new List<ConfiguredTaskAwaitable<List<IslandPool.IslandAndIndex>>>();
            foreach (var index in SelectUV)
            {
                var mapcount = index.Index;//Asyncな奴に投げている関係かこうしないとばぐるたぶん
                var Triangle = Utils.ToList(Data[index.Index].GetTriangles(index.SubMeshIndex));
                Tesks.Add(Task.Run<List<IslandPool.IslandAndIndex>>(() => GeneretIslandAndIndex(UVs[index.Index], Triangle, mapcount)).ConfigureAwait(false));
            }
            foreach (var task in Tesks)
            {
                IslandPool.IslandPoolList.AddRange(await task);
            }

            return IslandPool;

        }
        static List<IslandPool.IslandAndIndex> GeneretIslandAndIndex(List<Vector2> UV, List<TraiangleIndex> traiangles, int MapCount)
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
            public IslandAndIndex(Island island, int mapIndex, int islandInx)
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
            public int MapIndex { get; set; }
            public int IslandIndex { get; set; }
        }

        public IslandAndIndex GetLargest()
        {
            int GetIndex = -1;
            int Count = -1;
            Vector2 Cash = new Vector2(0, 0);
            foreach (var islandandi in IslandPoolList)
            {
                Count += 1;
                if (Cash.sqrMagnitude < islandandi.island.GetSize.sqrMagnitude)
                {
                    Cash = islandandi.island.GetSize;
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
        public Vector2 MinIlandBox;
        public Vector2 MaxIlandBox;
        public Vector2 GetSize { get => MaxIlandBox - MinIlandBox; }

        public Island(Island Souse)
        {
            trainagels = new List<TraiangleIndex>(Souse.trainagels);
            MinIlandBox = Souse.MinIlandBox;
            MaxIlandBox = Souse.MaxIlandBox;
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
            MinIlandBox = Box.Item1;
            MaxIlandBox = Box.Item2;
        }

        public bool BoxInOut(Vector2 TargetPos)
        {
            var InOutX = MinIlandBox.x < TargetPos.x && TargetPos.x < MaxIlandBox.x;
            var InOutY = MinIlandBox.y < TargetPos.y && TargetPos.y < MaxIlandBox.y;
            return InOutX && InOutY;
        }

    }


    public static class IlandUtilsDebug
    {
        public static void DorwUV(List<Vector2> UV, Texture2D TargetTextur, Color WriteColor)
        {
            foreach (var uvpos in UV)
            {
                int x = Mathf.RoundToInt(uvpos.x * TargetTextur.width);
                int y = Mathf.RoundToInt(uvpos.y * TargetTextur.height);
                TargetTextur.SetPixel(x, y, WriteColor);
            }
        }
        public static void DrowIlandBox(IslandPool Pool, Texture2D TargetTextur, Color WriteColor)
        {
            foreach (var island in Pool.IslandPoolList)
            {
                var minpos = new Vector2Int(Mathf.RoundToInt(island.island.MinIlandBox.x * TargetTextur.width), Mathf.RoundToInt(island.island.MinIlandBox.y * TargetTextur.height));
                var maxpos = new Vector2Int(Mathf.RoundToInt(island.island.MaxIlandBox.x * TargetTextur.width), Mathf.RoundToInt(island.island.MaxIlandBox.y * TargetTextur.height));
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