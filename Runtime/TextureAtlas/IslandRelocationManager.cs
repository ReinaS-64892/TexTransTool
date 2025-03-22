#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;
using Vector2Sys = System.Numerics.Vector2;

namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    internal class IslandRelocationManager
    {
        IIslandRelocator _islandRelocator;

        public bool ForceSizePriority { get; set; } = false;
        public float Height { get; set; } = 1.0f;
        public float Padding { get; set; } = 0.01f;
        public IslandRelocationManager(IIslandRelocator islandRelocator)
        {
            _islandRelocator = islandRelocator;
        }



        public void RelocateLoop(AtlasContext atlasContext, IslandTransform[] targetVirtualIslands, float[] sizePriority, out RelocateResult relocateResult)
        {
            relocateResult = new();

            var virtualIslandBackUp = new IslandTransform[targetVirtualIslands.Length];
            for (var i = 0; virtualIslandBackUp.Length > i; i += 1) { virtualIslandBackUp[i] = targetVirtualIslands[i].Clone(); }

            var workVirtualIslands = targetVirtualIslands;
            var workVirtualIslandsOrderBackUp = targetVirtualIslands.ToArray();
            var workRectToIndex = new Dictionary<IslandTransform, int>();
            {
                var index = 0;
                foreach (var v in targetVirtualIslands)
                {
                    workRectToIndex[v] = index;
                    index += 1;
                }
            }
            var islandReference = new RelocationContext.IslandReference(
                workVirtualIsland =>
                {
                    return atlasContext.SourceVirtualIsland2OriginRefaces[atlasContext.SourceVirtualIslands[workRectToIndex[workVirtualIsland]]].mergedTriangles;
                }
            );
            var relocationContext = new RelocationContext(Height, Padding, islandReference);



            if (ForceSizePriority is false)
            {
                relocateResult.IslandRelocationResult = _islandRelocator.Relocation(relocationContext, workVirtualIslands);
                RestoreWorkVirtualIslandOrder();
                if (relocateResult.IslandRelocationResult.IsSuccess) { return; }
            }

            var originOrPrioryMinimizedIslands = virtualIslandBackUp;
            if (sizePriority.Any(f => TTMath.Approximately(1, f) is false))
            {
                var minPrioritySizeRect = GetPriorityMinSizeRectArray(virtualIslandBackUp, sizePriority);
                if (ForceSizePriority)
                {
                    relocateResult.PriorityDownScale = 0;
                    VirtualIslandCopy(workVirtualIslands, minPrioritySizeRect);

                    relocateResult.IslandRelocationResult = _islandRelocator.Relocation(relocationContext, workVirtualIslands);
                    RestoreWorkVirtualIslandOrder();
                    if (relocateResult.IslandRelocationResult.IsSuccess) { return; }
                }
                else
                {
                    for (var lerpValue = 1f; 0 < lerpValue; lerpValue -= 0.05f)
                    {
                        VirtualIslandLerp(workVirtualIslands, minPrioritySizeRect, virtualIslandBackUp, lerpValue);

                        relocateResult.IslandRelocationResult = _islandRelocator.Relocation(relocationContext, workVirtualIslands);
                        RestoreWorkVirtualIslandOrder();
                        if (relocateResult.IslandRelocationResult.IsSuccess)
                        {
                            relocateResult.PriorityDownScale = lerpValue;
                            return;
                        }
                    }
                    relocateResult.PriorityDownScale = 0;
                }
                originOrPrioryMinimizedIslands = minPrioritySizeRect;
            }

            var initialRectArea = CalculateAllAreaSum(originOrPrioryMinimizedIslands);

            // 全体のサイズを 0.5倍にして、そこからぎりぎりになるまで拡大するのだが
            // 0.5倍では大きい場合に、倍率を下げてやり直すループ
            for (var size = 0.5f; size >= 0; size -= 0.01f)
            {
                relocateResult.TotalRelocateCount += 1;
                VirtualIslandCopy(workVirtualIslands, originOrPrioryMinimizedIslands);
                RectAllScaleDown(workVirtualIslands, (float)Mathf.Sqrt(size / initialRectArea));

                var expandResult = ExpandLoop(out var loopCount, out var relocationResult);
                relocateResult.IslandRelocationResult = relocationResult;

                relocateResult.TotalRelocateCount += loopCount;
                if (expandResult)
                {
                    relocateResult.OverallDownScale = CalculateAllAreaSum(workVirtualIslands) / initialRectArea;
                    break;
                }
            }

            // exit !

            bool ExpandLoop(out int loopCount, out RelocationResult relocationResult)
            {
                var lastRelocated = new IslandTransform[workVirtualIslands.Length];
                for (var i = 0; lastRelocated.Length > i; i += 1) { lastRelocated[i] = workVirtualIslands[i].Clone(); }
                loopCount = 0;

                var lastRelocationResult = _islandRelocator.Relocation(relocationContext, workVirtualIslands);
                RestoreWorkVirtualIslandOrder();
                relocationResult = lastRelocationResult;

                if (lastRelocationResult.IsSuccess is false) { relocationResult = lastRelocationResult; return false; }
                while (lastRelocationResult.IsSuccess && loopCount < 2048)//失敗するかセーフティにかかるまで続けて、失敗したら前回の物を使用する方針
                {
                    loopCount += 1;
                    VirtualIslandCopy(lastRelocated, workVirtualIslands);
                    RectAllScaleUp(workVirtualIslands, 1.01f);
                    relocationResult = lastRelocationResult;

                    lastRelocationResult = _islandRelocator.Relocation(relocationContext, workVirtualIslands);
                    RestoreWorkVirtualIslandOrder();
                }
                VirtualIslandCopy(workVirtualIslands, lastRelocated);
                // relocationResult には前回の内容が入っているから問題ない
                return true;
            }

            void RestoreWorkVirtualIslandOrder()
            {
                for (var i = 0; workVirtualIslands.Length > i; i += 1) { workVirtualIslands[i] = workVirtualIslandsOrderBackUp[i]; }
            }
        }

        static void RectAllScaleDown(IslandTransform[] rect, float scaleDownStep)
        {
            for (int i = 0; rect.Length > i; i++)
            {
                rect[i].Size *= scaleDownStep;
            }
        }
        void RectAllScaleUp(IslandTransform[] rect, float scaleUpStep)
        {
            for (int i = 0; rect.Length > i; i++)
            {
                var size = rect[i].Size *= scaleUpStep;

                var maxLengthX = 1.0f - Padding - Padding - 0.001f;// 0.001f は誤差許容の念の為 ... 多分いらないとは思うけど
                var maxLengthY = Height - Padding - Padding - 0.001f;
                var targetArea = maxLengthX * maxLengthY;
                if (size.X > maxLengthX || size.Y > maxLengthY)//一つ大きいのがあるとすべて使いきれなくなってしまうために、これは必要。
                {
                    var max = Mathf.Max(size.X, size.Y);
                    size *= targetArea / max;
                    rect[i].Size = size;
                }
            }
        }
        internal static float CalculateAllAreaSum(IslandTransform[] virtualIslands)
        {
            var sum = 0f;
            foreach (var island in virtualIslands) { sum += island.Size.X * island.Size.Y; }
            return sum;
        }
        internal static float CalculateIslandsMaxHeight(IslandTransform[] virtualIslands)
        {
            var height = 0f;
            foreach (var island in virtualIslands)
            {
                height = Mathf.Max(height, island.Position.Y);
                height = Mathf.Max(height, island.GetRotatedMaxPos().Y);
                height = Mathf.Max(height, (island.Position + IslandTransform.RotateVector(new(0f, island.Size.Y), island.Rotation)).Y);
                height = Mathf.Max(height, (island.Position + IslandTransform.RotateVector(new(island.Size.X, 0f), island.Rotation)).Y);
            }
            return height;
        }
        private static void VirtualIslandCopy(IslandTransform[] target, IslandTransform[] source)
        {
            if (source.Length != target.Length) { throw new ArgumentException(); }
            for (int i = 0; source.Length > i; i += 1) { target[i].CopyFrom(source[i]); }
        }
        private static void VirtualIslandLerp(IslandTransform[] target, IslandTransform[] min, IslandTransform[] max, float lerpValue)
        {
            for (var i = 0; target.Length > i; i += 1)
            {
                var size = Vector2Sys.Lerp(min[i].Size, max[i].Size, lerpValue);
                target[i].Size = size;
            }
        }

        private static IslandTransform[] GetPriorityMinSizeRectArray(IslandTransform[] vIslands, float[] sizePriority)
        {
            var priorityMinSize = new IslandTransform[vIslands.Length];
            for (var i = 0; priorityMinSize.Length > i; i += 1) { priorityMinSize[i] = vIslands[i].Clone(); }
            for (var i = 0; priorityMinSize.Length > i; i += 1) { priorityMinSize[i].Size *= sizePriority[i]; }
            return priorityMinSize;
        }

        internal record RelocateResult
        {
            public float PriorityDownScale = 1;
            public float OverallDownScale = 1;
            public int TotalRelocateCount = 0;
            public RelocationResult? IslandRelocationResult;
        }

    }
}
