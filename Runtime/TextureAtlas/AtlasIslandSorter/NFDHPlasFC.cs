using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class NFDHPlasFC : IAtlasIslandSorter
    {
        public const string NDFHPlasFCName = "NextFitDecreasingHeightPlusFloorCeiling";
        public string SorterName => NDFHPlasFCName;
        public bool RectTangleMove => true;

        public Dictionary<AtlasIslandID, IslandRect> Sorting(Dictionary<AtlasIslandID, IslandRect> atlasIslands, IReadOnlyDictionary<AtlasIslandID, AtlasIsland> atlasIslandReference, bool useUpScaling, float padding)
        {
            IslandPoolNextFitDecreasingHeightPlusFloorCeiling(atlasIslands, useUpScaling, padding);
            return atlasIslands;
        }
        public static Dictionary<ID, IslandRect> IslandPoolNextFitDecreasingHeightPlusFloorCeiling<ID>(
            Dictionary<ID, IslandRect> islands,
            bool useUpScaling = true,
            float islandPadding = 0.01f,
            float crawlingStep = 0.01f,
            int safetyCount = 256)
        {
            var idList = islands.Keys.ToList();
            if (!islands.Any()) return islands;
            foreach (var id in idList) { if (islands[id].Size.y > islands[id].Size.x) { islands[id].Rotate90(); } }

            idList.Sort((lId, rId) => Mathf.RoundToInt((islands[rId].Size.y - islands[lId].Size.y) * 1073741824));

            var validateHeight = islands[idList[0]].Size.y;
            foreach (var id in idList)
            {
                if (validateHeight >= islands[id].Size.y)
                {
                    validateHeight = islands[id].Size.y;
                }
                else
                {
                    TTTRuntimeLog.Warning("NFDHPlusFC : The islands are not sorted correctly according to height. It is possible that undesirable reordering is being done.");
                    break;
                }
            }


            bool success = false;
            float nawScale = 1f;
            int loopCount = -1;
            bool isBigger = false;
            bool nextSuccessEnd = false;

            while (!success && safetyCount > loopCount)
            {
                loopCount += 1;
                success = true;

                var boxList = new List<UVWithBox<IslandRect>>();


                foreach (var islandId in idList)
                {
                    var Result = false;
                    foreach (var withBox in boxList)
                    {
                        var island = islands[islandId];
                        Result = withBox.TrySetBox(island, out var pivot);
                        island.Pivot = pivot;
                        islands[islandId] = island;

                        if (Result) { break; }
                    }
                    if (!Result)
                    {
                        var Floor = boxList.Any() ? boxList.Last().Ceil + islandPadding : islandPadding;
                        var Ceil = islands[islandId].Size.y + Floor;
                        var newWithBox = new UVWithBox<IslandRect>(Ceil, Floor, islandPadding);

                        var island = islands[islandId];
                        var res = newWithBox.TrySetBox(island, out var pivot);
                        island.Pivot = pivot;
                        islands[islandId] = island;

                        boxList.Add(newWithBox);
                    }
                }

                var lastHeight = boxList.Last().Ceil + islandPadding;
                success = lastHeight < 1;

                if (!success)
                {
                    if (isBigger) { nextSuccessEnd = true; }
                    ScaleApply(1 - crawlingStep);

                }
                else
                {
                    if (!nextSuccessEnd && useUpScaling)
                    {
                        success = false;
                        isBigger = true;
                        ScaleApply(1 + crawlingStep);
                    }
                }


            }
            if (safetyCount == loopCount) { TTTRuntimeLog.Warning("NextFitDecreasingHeightPlusFloorCeiling : Safetyによりループが中断された可能性があり、アイランドの再配置が正常に行われていない可能性があります"); }

            return islands;

            void ScaleApply(float Scale)
            {
                foreach (var islandId in idList)
                {
                    if ((islands[islandId].Size.x * Scale) > (0.999f - islandPadding)) { continue; }

                    var island = islands[islandId];
                    island.Size *= Scale;
                    islands[islandId] = island;
                }
                nawScale *= Scale;
            }
        }


        private class UVWithBox<TIslandRect> where TIslandRect : IIslandRect
        {
            public float with = 1;
            public float Padding;
            public float Ceil;
            public float Floor;
            public float Height => Ceil - Floor;
            public List<TIslandRect> Upper = new();
            public List<TIslandRect> Lower = new();

            public UVWithBox(float height, float floor, float padding)
            {
                Ceil = height;
                Floor = floor;
                Padding = padding;
            }

            public bool TrySetBox(TIslandRect islandRect, out Vector2 outPivot)
            {
                outPivot = Vector2.zero;
                if (Height + 0.01f < islandRect.Size.y) return false;


                var withMin = Lower.Any() ? Lower.Last().GetMaxPos().x : 0;
                var withMax = GetCeilWithEmpty(Mathf.Clamp(Floor + islandRect.Size.y + Padding, Floor, Ceil));
                var withSize = withMax - withMin;
                if (withSize > Padding + islandRect.Size.x + Padding)
                {
                    islandRect.Pivot = outPivot = new Vector2(withMin + Padding, Floor);
                    Lower.Add(islandRect);
                    return true;
                }


                withMin = GetFloorWithEmpty(Mathf.Clamp(Ceil - islandRect.Size.y - Padding, Floor, Ceil));
                withMax = Upper.Any() ? Upper.Last().Pivot.x : with;
                withSize = withMax - withMin;
                if (withSize > Padding + islandRect.Size.x + Padding)
                {
                    islandRect.Pivot = outPivot = new Vector2(withMax - islandRect.Size.x - Padding, Ceil - islandRect.Size.y);
                    Upper.Add(islandRect);
                    return true;
                }


                return false;
            }

            public float GetFloorWithEmpty(float targetHeight)
            {
                if (!VectorUtility.InRange(Floor, Ceil, targetHeight)) { throw new Exception("TargetHeight is not in range!"); }

                var minWith = 0f;

                foreach (var box in Lower)
                {
                    var island = box;
                    if (VectorUtility.InRange(island.Pivot.y, island.GetMaxPos().y, targetHeight))
                    {
                        if (minWith < island.GetMaxPos().x) { minWith = island.GetMaxPos().x; }
                    }
                }



                return minWith;
            }
            public float GetCeilWithEmpty(float targetHeight)
            {
                if (!VectorUtility.InRange(Floor, Ceil, targetHeight)) throw new Exception("TargetHeight is not in range!");

                var maxWith = with;

                foreach (var Box in Upper)
                {
                    var island = Box;
                    if (VectorUtility.InRange(island.Pivot.y, island.GetMaxPos().y, targetHeight))
                    {
                        if (island.GetMaxPos().x < maxWith) { maxWith = island.GetMaxPos().x; }
                    }
                }

                return maxWith;
            }
        }
    }
}
