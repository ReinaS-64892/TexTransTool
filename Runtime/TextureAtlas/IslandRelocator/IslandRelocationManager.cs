
using System;
using System.Linq;
using net.rs64.TexTransUnityCore.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    internal class IslandRelocationManager
    {
        IAtlasIslandRelocator _islandRelocator;

        float _padding;
        public float Padding
        {
            get => _padding;
            set { _islandRelocator.Padding = _padding = value; }
        }
        public bool ForceSizePriority { private get; set; }
        public IslandRelocationManager(IAtlasIslandRelocator islandRelocator)
        {
            _islandRelocator = islandRelocator;
            Padding = 0;
            ForceSizePriority = false;
        }


        public IslandRect[] RelocateLoop(IslandRect[] originalRectArray, float[] sizePriority, out RelocateResult relocateResult)
        {
            relocateResult = new();
            var workRect = originalRectArray.ToArray();
            var originalRatioMedicatedReference = originalRectArray;


            if (_islandRelocator.Relocation(workRect)) { relocateResult.IsRelocateSuccess = true; return workRect; }

            if (sizePriority.Any(f => !Mathf.Approximately(1, f)))
            {
                var minPrioritySizeRect = GetPriorityMinSizeRectArray(originalRectArray, sizePriority);
                if (ForceSizePriority)
                {
                    relocateResult.PriorityDownScale = 0;
                    RectCopy(workRect, minPrioritySizeRect);
                    if (_islandRelocator.Relocation(workRect))
                    {
                        relocateResult.IsRelocateSuccess = true;
                        return workRect;
                    }
                }
                else
                {
                    for (var lerpValue = 1f; 0 < lerpValue; lerpValue -= 0.05f)
                    {
                        RectLerp(workRect, minPrioritySizeRect, originalRectArray, lerpValue);

                        if (_islandRelocator.Relocation(workRect))
                        {
                            relocateResult.PriorityDownScale = lerpValue;
                            relocateResult.IsRelocateSuccess = true;
                            return workRect;
                        }
                    }
                    relocateResult.PriorityDownScale = 0;
                }
                originalRatioMedicatedReference = minPrioritySizeRect;
            }

            var initialRectArea = IslandRectUtility.CalculateAllAreaSum(originalRatioMedicatedReference);
            var lastWorkedRect = workRect.ToArray();

            // 全体のサイズを 0.5倍にして、そこからぎりぎりになるまで拡大するのだが
            // 0.5倍では大きい場合に、倍率を下げてやり直すループ
            for (var size = 0.5f; size >= 0; size -= 0.01f)
            {
                relocateResult.TotalRelocateCount += 1;
                RectCopy(workRect, originalRatioMedicatedReference);
                RectAllScaleDown(workRect, (float)Mathf.Sqrt(size / initialRectArea));

                var expandResult = ExpandLoop(workRect, lastWorkedRect, out var loopCount);

                relocateResult.TotalRelocateCount += loopCount;
                if (expandResult)
                {
                    relocateResult.OverallDownScale = IslandRectUtility.CalculateAllAreaSum(lastWorkedRect) / initialRectArea;
                    relocateResult.IsRelocateSuccess = true;
                    break;
                }
            }

            return lastWorkedRect;

            bool ExpandLoop(IslandRect[] relocatedRect, IslandRect[] lastRelocated, out int loopCount)
            {
                loopCount = 0;
                if (!_islandRelocator.Relocation(relocatedRect)) { return false; }
                while (_islandRelocator.Relocation(relocatedRect) && loopCount < 2048)//失敗するかセーフティにかかるまで続けて、失敗したら前回の物を使用する方針
                {
                    loopCount += 1;
                    RectCopy(lastRelocated, relocatedRect);
                    RectAllScaleUp(relocatedRect, 1.01f);
                }
                return true;
            }
        }

        static void RectAllScaleDown(IslandRect[] rect, float scaleDownStep)
        {
            for (int i = 0; rect.Length > i; i++)
            {
                rect[i].Size *= scaleDownStep;
            }
        }
        void RectAllScaleUp(IslandRect[] rect, float scaleUpStep)
        {
            for (int i = 0; rect.Length > i; i++)
            {
                var size = rect[i].Size *= scaleUpStep;


                var maxLength = 0.99f - Padding - Padding;
                if (size.x > maxLength || size.y > maxLength)//一つ大きいのがあるとすべて使いきれなくなってしまうために、これは必要。
                {
                    var max = Mathf.Max(size.x, size.y);
                    size *= maxLength / max;
                    rect[i].Size = size;
                }
            }
        }
        private static void RectCopy(IslandRect[] target, IslandRect[] source)
        {
            if (source.Length != target.Length) { throw new ArgumentException(); }
            for (int i = 0; source.Length > i; i++) { target[i] = source[i]; }
        }
        private static void RectLerp(IslandRect[] target, IslandRect[] min, IslandRect[] max, float lerpValue)
        {
            for (var i = 0; max.Length > i; i += 1)
            {
                var size = Vector3.Lerp(min[i].Size, max[i].Size, lerpValue);
                if (target[i].Is90Rotation) { (size.x, size.y) = (size.y, size.x); }
                target[i].Size = size;
            }
        }

        private static IslandRect[] GetPriorityMinSizeRectArray(IslandRect[] rectArray, float[] sizePriority)
        {
            var priorityMinSize = rectArray.ToArray();
            for (var i = 0; priorityMinSize.Length > i; i += 1) { priorityMinSize[i].Size *= sizePriority[i]; }

            return priorityMinSize;
        }

        internal record RelocateResult
        {
            public float PriorityDownScale = 1;
            public float OverallDownScale = 1;
            public int TotalRelocateCount = 0;
            public bool IsRelocateSuccess = false;
        }

    }
}
