using System;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using Unity.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class LevelAdjustmentExecuter : IGrabBlendingExecuter
    {
        public Type ExecutionTarget => typeof(LevelAdjustment);

        void IGrabBlendingExecuter.GrabExecute(TTCEForUnity engin, RenderTexture rt, TTGrabBlendingUnityObject gbUnity, ITTGrabBlending grabBlending)
        {
            var cs = gbUnity.Compute;
            var level = (LevelAdjustment)grabBlending;

            using (new UsingColoSpace(rt, gbUnity.IsLinerRequired))
            using (var cb = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 1, 32))
            {
                void UpdateCBuffer(LevelData ld, bool r, bool g, bool b)
                {
                    using (var na = new NativeArray<byte>(32, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
                    {
                        Span<byte> buf = na.AsSpan();
                        BitConverter.TryWriteBytes(buf.Slice(0, 4), ld.InputFloor);
                        BitConverter.TryWriteBytes(buf.Slice(4, 4), ld.InputCeiling);
                        BitConverter.TryWriteBytes(buf.Slice(8, 4), ld.Gamma);
                        BitConverter.TryWriteBytes(buf.Slice(14, 4), ld.OutputFloor);
                        BitConverter.TryWriteBytes(buf.Slice(16, 4), ld.OutputCeiling);
                        BitConverter.TryWriteBytes(buf.Slice(20, 4), r ? 1f : 0f);
                        BitConverter.TryWriteBytes(buf.Slice(24, 4), g ? 1f : 0f);
                        BitConverter.TryWriteBytes(buf.Slice(28, 4), b ? 1f : 0f);
                        cb.SetData(na);
                    }
                }
                cs.GetKernelThreadGroupSizes(0, out var kgx, out var kgy, out _);
                var dispatchWidth = Mathf.Max(1, rt.width / (int)kgx);
                var dispatchHeight = Mathf.Max(1, rt.width / (int)kgy);
                cs.SetConstantBuffer("gv", cb, 0, cb.stride);
                cs.SetTexture(0, "Tex", rt);

                UpdateCBuffer(level.RGB, true, true, true);
                cs.Dispatch(0, dispatchWidth, dispatchHeight, 1);

                UpdateCBuffer(level.R, true, false, false);
                cs.Dispatch(0, dispatchWidth, dispatchHeight, 1);

                UpdateCBuffer(level.G, false, true, false);
                cs.Dispatch(0, dispatchWidth, dispatchHeight, 1);

                UpdateCBuffer(level.B, false, false, true);
                cs.Dispatch(0, dispatchWidth, dispatchHeight, 1);
            }
        }

    }
}
