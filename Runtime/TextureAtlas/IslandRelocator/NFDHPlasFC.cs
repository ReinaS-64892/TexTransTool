using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    internal class NFDHPlasFC : IAtlasIslandRelocator
    {
        public bool RectTangleMove => true;
        public float Padding { set => _padding = value; }

        public float _padding;

        public bool Relocation(IslandRect[] atlasIslands)
        {
            Profiler.BeginSample("NFDHPlasFC");
            var res = IslandPoolNextFitDecreasingHeightPlusFloorCeiling(atlasIslands, _padding);
            Profiler.EndSample();
            return res;
        }
        public static bool IslandPoolNextFitDecreasingHeightPlusFloorCeiling(IslandRect[] islands, float islandPadding = 0.01f)
        {
            if (islands.Length == 0) { return false; }

            Profiler.BeginSample("KeyArrayInit");
            var idKey = Enumerable.Range(0, islands.Length).ToArray();
            foreach (var id in idKey) { if (islands[id].Size.y > islands[id].Size.x) { var island = islands[id]; island.Rotate90(); islands[id] = island; } }
            Array.Sort(idKey, (lId, rId) => Mathf.RoundToInt((islands[rId].Size.y - islands[lId].Size.y) * 1073741824));
            Profiler.EndSample();

            using (var sortedIA = new NativeArray<IslandRect>(islands.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
            {
                var sortedIASpan = sortedIA.AsSpan();

                Profiler.BeginSample("CopyAndValidate");
                for (var i = 0; idKey.Length > i; i += 1) { sortedIASpan[i] = islands[idKey[i]]; }
                ValidateDeceasing(sortedIA);
                Profiler.EndSample();

                Profiler.BeginSample("TryNFDHPlasFC");
                if (TryNFDHPlasFC(sortedIASpan, islandPadding))
                {
                    Profiler.EndSample();
                    for (var i = 0; sortedIASpan.Length > i; i += 1) { islands[idKey[i]] = sortedIASpan[i]; }
                    return true;
                }
                Profiler.EndSample();
                return false;
            }
        }


        internal static bool ValidateDeceasing(Span<IslandRect> rectArray)
        {
            var validateHeight = rectArray[0].Size.y;
            foreach (var rect in rectArray)
            {
                if (validateHeight >= rect.Size.y) { validateHeight = rect.Size.y; }
                else
                {
                    if (!Mathf.Approximately(validateHeight, rect.Size.y))
                    {
                        TTTRuntimeLog.Warning("NFDHPlusFC : The islands are not sorted correctly according to height. It is possible that undesirable reordering is being done.");
                        return true;
                    }
                }
            }
            return false;
        }

        static bool TryNFDHPlasFC(Span<IslandRect> sortedIslands, float islandPadding = 0.01f)
        {
            var uvWidthBox = new LinkedList<UVWidthBox<IslandRect>>();

            for (var i = 0; sortedIslands.Length > i; i += 1)
            {
                Profiler.BeginSample("TrySet");
                if (TrySetUVBoxList(sortedIslands, i)) { Profiler.EndSample(); continue; }
                Profiler.EndSample();

                Profiler.BeginSample("NewBox");
                var Floor = uvWidthBox.Any() ? uvWidthBox.Last.Value.Ceil + islandPadding : islandPadding;
                var Ceil = sortedIslands[i].Size.y + Floor;
                var newWithBox = new UVWidthBox<IslandRect>(Ceil, Floor, islandPadding);

                var pivot = newWithBox.TrySetBox(sortedIslands[i]);
                if (pivot is null) { Profiler.EndSample(); return false; }

                sortedIslands[i].Pivot = pivot.Value;

                uvWidthBox.AddLast(newWithBox);
                Profiler.EndSample();
            }

            var lastHeight = uvWidthBox.Last.Value.Ceil + islandPadding;
            return lastHeight <= 1;

            bool TrySetUVBoxList(Span<IslandRect> sortedIslands, int index)
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
            public readonly LinkedList<TIslandRect> Upper;
            public readonly LinkedList<TIslandRect> Lower;

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


                var widthMin = Lower.Any() ? Lower.Last.Value.GetMaxPos().x : 0;
                var widthMax = GetCeilWithEmpty(Mathf.Clamp(Floor + islandRect.Size.y + Padding, Floor, Ceil));
                var widthSize = widthMax - widthMin;
                if (widthSize > Padding + islandRect.Size.x + Padding)
                {
                    islandRect.Pivot = new Vector2(widthMin + Padding, Floor);
                    Lower.AddLast(islandRect);
                    return islandRect.Pivot;
                }


                widthMin = GetFloorWithEmpty(Mathf.Clamp(Ceil - islandRect.Size.y - Padding, Floor, Ceil));
                widthMax = Upper.Any() ? Upper.Last.Value.Pivot.x : Width;
                widthSize = widthMax - widthMin;
                if (widthSize > Padding + islandRect.Size.x + Padding)
                {
                    islandRect.Pivot = new Vector2(widthMax - islandRect.Size.x - Padding, Ceil - islandRect.Size.y);
                    Upper.AddLast(islandRect);
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
