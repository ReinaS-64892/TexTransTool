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

        public Dictionary<AtlasIslandID, AtlasIsland> Sorting(Dictionary<AtlasIslandID, AtlasIsland> atlasIslands, bool useUpScaling, float padding)
        {
            IslandPoolNextFitDecreasingHeightPlusFloorCeiling(atlasIslands.Values.ToList(), useUpScaling, padding);
            return atlasIslands;
        }
        public static List<TIsland> IslandPoolNextFitDecreasingHeightPlusFloorCeiling<TIsland>(
            List<TIsland> targetPool,
            bool useUpScaling = true,
            float islandPadding = 0.01f,
            float crawlingStep = 0.01f,
            int safetyCount = 256)
            where TIsland : Island
        {
            var islands = targetPool;
            if (!islands.Any()) return targetPool;
            foreach (var Island in islands) { if (Island.Size.y > Island.Size.x) { Island.Rotate90(); } }

            islands.Sort((l, r) => Mathf.RoundToInt((r.Size.y - l.Size.y) * 1073741824));

            var posValue = 1f;
            foreach (var island in islands)
            {
                if (posValue >= island.Size.y)
                {
                    posValue = island.Size.y;
                }
                else
                {
                    // TTTLog.Warning("NFDHPlusFC : The islands are not sorted correctly according to height. It is possible that undesirable reordering is being done.".GetLocalize());
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

                var boxList = new List<UVWithBox>();


                foreach (var islandAndTag in islands)
                {
                    var Result = false;
                    foreach (var withBox in boxList)
                    {
                        Result = withBox.TrySetBox(islandAndTag);
                        if (Result) { break; }

                    }
                    if (!Result)
                    {
                        var Floor = boxList.Any() ? boxList.Last().Ceil + islandPadding : islandPadding;
                        var Ceil = islandAndTag.Size.y + Floor;
                        var newWithBox = new UVWithBox(Ceil, Floor, islandPadding);
                        var res = newWithBox.TrySetBox(islandAndTag);
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

            return targetPool;

            void ScaleApply(float Scale)
            {
                foreach (var islandAndTag in islands)
                {
                    if ((islandAndTag.Size.x * Scale) > (0.999f - islandPadding)) { continue; }
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

            public bool TrySetBox(Island box)
            {
                var Island = box;
                if (Height + 0.01f < Island.Size.y) return false;


                var withMin = Lower.Any() ? Lower.Last().GetMaxPos.x : 0;
                var withMax = GetCeilWithEmpty(Mathf.Clamp(Floor + Island.Size.y + Padding, Floor, Ceil));
                var withSize = withMax - withMin;
                if (withSize > Padding + Island.Size.x + Padding)
                {
                    Island.Pivot = new Vector2(withMin + Padding, Floor);
                    Lower.Add(box);
                    return true;
                }


                withMin = GetFloorWithEmpty(Mathf.Clamp(Ceil - Island.Size.y - Padding, Floor, Ceil));
                withMax = Upper.Any() ? Upper.Last().Pivot.x : with;
                withSize = withMax - withMin;
                if (withSize > Padding + Island.Size.x + Padding)
                {
                    Island.Pivot = new Vector2(withMax - Island.Size.x - Padding, Ceil - Island.Size.y);
                    Upper.Add(box);
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
                    if (VectorUtility.InRange(island.Pivot.y, island.GetMaxPos.y, targetHeight))
                    {
                        if (minWith < island.GetMaxPos.x) { minWith = island.GetMaxPos.x; }
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
                    if (VectorUtility.InRange(island.Pivot.y, island.GetMaxPos.y, targetHeight))
                    {
                        if (island.GetMaxPos.x < maxWith) { maxWith = island.GetMaxPos.x; }
                    }
                }

                return maxWith;
            }
        }
    }
}
