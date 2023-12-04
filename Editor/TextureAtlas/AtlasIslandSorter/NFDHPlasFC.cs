#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public class NFDHPlasFC : IAtlasIslandSorter
    {
        public const string NDFHPlasFCName = "NextFitDecreasingHeightPlusFloorCeiling";
        public string SorterName => NDFHPlasFCName;
        public bool RectTangleMove => true;

        public Dictionary<AtlasIslandID, AtlasIsland> Sorting(Dictionary<AtlasIslandID, AtlasIsland> atlasIslands, float Padding)
        {
            IslandPoolNextFitDecreasingHeightPlusFloorCeiling(atlasIslands.Values.ToList(), Padding);
            return atlasIslands;
        }
        public static List<TIsland> IslandPoolNextFitDecreasingHeightPlusFloorCeiling<TIsland>(
            List<TIsland> TargetPool,
            float IslandPadding = 0.01f,
            float CrawlingStep = 0.01f,
            int MaxLoopCount = 128)
            where TIsland : Island
        {
            var Islands = TargetPool;
            if (!Islands.Any()) return TargetPool;
            foreach (var Island in Islands) { if (Island.Size.y > Island.Size.x) { Island.Rotate90(); } }
            Islands.Sort((l, r) => Mathf.RoundToInt((r.Size.y - l.Size.y) * 100));
            bool success = false;
            float nawScale = 1f;
            int loopCount = -1;
            bool isBigger = false;
            bool nextSuccessEnd = false;

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
                        var Ceil = islandAndTag.Size.y + Floor;
                        var newWithBox = new UVWithBox(Ceil, Floor, IslandPadding);
                        var res = newWithBox.TrySetBox(islandAndTag);
                        boxList.Add(newWithBox);
                    }
                }

                var lastHeight = boxList.Last().Ceil + IslandPadding;
                success = lastHeight < 1;

                if (!success)
                {
                    if (isBigger) { nextSuccessEnd = true; }
                    ScaleApply(1 - CrawlingStep);

                }
                else if (success)
                {
                    if (!nextSuccessEnd)
                    {
                        success = false;
                        isBigger = true;
                        ScaleApply(1 + CrawlingStep);
                    }
                }


            }

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
    }
}
#endif