using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    internal class NFDHPlasFC : IAtlasIslandRelocator
    {
        public bool RectTangleMove => true;

        public bool UseUpScaling { set => _useUpScaling = value; }
        public float Padding { set => _padding = value; }

        public bool _useUpScaling;
        public float _padding;

        public Dictionary<AtlasIslandID, IslandRect> Relocation(Dictionary<AtlasIslandID, IslandRect> atlasIslands, IReadOnlyDictionary<AtlasIslandID, AtlasIsland> atlasIslandReference)
        {
            IslandPoolNextFitDecreasingHeightPlusFloorCeiling(atlasIslands, _useUpScaling, _padding);
            return atlasIslands;
        }
        public static Dictionary<ID, IslandRect> IslandPoolNextFitDecreasingHeightPlusFloorCeiling<ID>(Dictionary<ID, IslandRect> islands, bool useUpScaling = true, float islandPadding = 0.01f, float scaleStep = 0.99f, int safetyCount = 512)
        {
            if (!islands.Any()) return islands;

            var idList = islands.Keys.ToList();
            foreach (var id in idList) { if (islands[id].Size.y > islands[id].Size.x) { var island = islands[id]; island.Rotate90(); islands[id] = island; } }
            idList.Sort((lId, rId) => Mathf.RoundToInt((islands[rId].Size.y - islands[lId].Size.y) * 1073741824));

            var sortedIslandArray = idList.Select(i => islands[i]).ToArray();

            ValidateDeceasing(sortedIslandArray);

            if (TryNFDHPlasFC(sortedIslandArray, islandPadding) && !useUpScaling) { ApplyDictionary(); return islands; }

            ScaleApply(Mathf.Sqrt(1 / IslandRectUtility.CalculateAllAreaSum(islands.Values)));

            var stepCount = 0;
            while (!TryNFDHPlasFC(sortedIslandArray, islandPadding) && safetyCount > stepCount) { ScaleApply(scaleStep); stepCount += 1; }
            if (stepCount == safetyCount) { TTTRuntimeLog.Warning("NextFitDecreasingHeightPlusFloorCeiling : Safetyによりループが中断された可能性があり、アイランドの再配置が正常に行われていない可能性があります"); }

            ApplyDictionary();
            return islands;

            void ScaleApply(float scale)
            {
                for (var i = 0; sortedIslandArray.Length > i; i += 1) { sortedIslandArray[i].Size *= scale; }
            }
            void ApplyDictionary()
            {
                for (var i = 0; sortedIslandArray.Length > i; i += 1) { islands[idList[i]] = sortedIslandArray[i]; }
            }
        }


        internal static bool ValidateDeceasing(IslandRect[] rectArray)
        {
            var validateHeight = rectArray[0].Size.y;
            foreach (var rect in rectArray)
            {
                if (validateHeight >= rect.Size.y) { validateHeight = rect.Size.y; }
                else
                {
                    TTTRuntimeLog.Warning("NFDHPlusFC : The islands are not sorted correctly according to height. It is possible that undesirable reordering is being done.");
                    return true;
                }
            }
            return false;
        }

        static bool TryNFDHPlasFC(IslandRect[] sortedIslands, float islandPadding = 0.01f)
        {
            var uvWidthBox = new List<UVWidthBox<IslandRect>>();

            for (var i = 0; sortedIslands.Length > i; i += 1)
            {
                if (TrySetUVBoxList(i)) { continue; }

                var Floor = uvWidthBox.Any() ? uvWidthBox.Last().Ceil + islandPadding : islandPadding;
                var Ceil = sortedIslands[i].Size.y + Floor;
                var newWithBox = new UVWidthBox<IslandRect>(Ceil, Floor, islandPadding);

                var pivot = newWithBox.TrySetBox(sortedIslands[i]);
                if (pivot is null) { return false; }

                sortedIslands[i].Pivot = pivot.Value;

                uvWidthBox.Add(newWithBox);
            }

            var lastHeight = uvWidthBox.Last().Ceil + islandPadding;
            return lastHeight <= 1;

            bool TrySetUVBoxList(int index)
            {
                foreach (var withBox in uvWidthBox)
                {
                    var pivot = withBox.TrySetBox(sortedIslands[index]);
                    if (pivot is not null)
                    {
                        sortedIslands[index].Pivot = pivot.Value;
                        return true;
                    }
                }
                return false;
            }

        }


        private readonly struct UVWidthBox<TIslandRect> where TIslandRect : IIslandRect
        {
            public readonly float Width;
            public readonly float Padding;
            public readonly float Ceil;
            public readonly float Floor;
            public float Height => Ceil - Floor;
            public readonly List<TIslandRect> Upper;
            public readonly List<TIslandRect> Lower;

            public UVWidthBox(float height, float floor, float padding, float width = 1)
            {
                Width = width;

                Ceil = height;
                Floor = floor;
                Padding = padding;

                Upper = new();
                Lower = new();
            }

            public Vector2? TrySetBox(TIslandRect islandRect)
            {
                if (Height + 0.01f < islandRect.Size.y) return null;


                var widthMin = Lower.Any() ? Lower.Last().GetMaxPos().x : 0;
                var widthMax = GetCeilWithEmpty(Mathf.Clamp(Floor + islandRect.Size.y + Padding, Floor, Ceil));
                var widthSize = widthMax - widthMin;
                if (widthSize > Padding + islandRect.Size.x + Padding)
                {
                    islandRect.Pivot = new Vector2(widthMin + Padding, Floor);
                    Lower.Add(islandRect);
                    return islandRect.Pivot;
                }


                widthMin = GetFloorWithEmpty(Mathf.Clamp(Ceil - islandRect.Size.y - Padding, Floor, Ceil));
                widthMax = Upper.Any() ? Upper.Last().Pivot.x : Width;
                widthSize = widthMax - widthMin;
                if (widthSize > Padding + islandRect.Size.x + Padding)
                {
                    islandRect.Pivot = new Vector2(widthMax - islandRect.Size.x - Padding, Ceil - islandRect.Size.y);
                    Upper.Add(islandRect);
                    return islandRect.Pivot;
                }


                return null;
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

                var maxWith = Width;

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
