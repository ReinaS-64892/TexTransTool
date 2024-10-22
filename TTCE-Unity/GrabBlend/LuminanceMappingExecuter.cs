using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class LuminanceMappingExecuter : IGrabBlendingExecuter
    {
        public Type ExecutionTarget => typeof(LuminanceMapping);

        void IGrabBlendingExecuter.GrabExecute(TTCEForUnity engin, RenderTexture rt, TTGrabBlendingUnityObject gbUnity, ITTGrabBlending grabBlending)
        {
            var cs = gbUnity.Compute;
            var lumMap = (LuminanceMapping)grabBlending;

            using (new UsingColoSpace(rt, gbUnity.IsLinerRequired))
            {
                cs.SetTexture(0, "Tex", rt);
                cs.SetTexture(0, "Gradient", LuminanceMappingGradientTempTexture.Get(lumMap.Gradient));

                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
            }
        }

    }
    static class LuminanceMappingGradientTempTexture
    {
        static Texture2D s_GradientTempTexture;
        internal static Texture2D Get(ILuminanceMappingGradient gradient)
        {
            if (s_GradientTempTexture == null) { s_GradientTempTexture = new Texture2D(256, 1, TextureFormat.RGBAFloat, false); }

            using (var colorArray = new NativeArray<TexTransCore.Color>(256, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
            {
                var writeSpan = colorArray.AsSpan();
                gradient.WriteGradient(writeSpan);
                s_GradientTempTexture.LoadRawTextureData(colorArray);
            }
            s_GradientTempTexture.Apply(true);

            return s_GradientTempTexture;
        }

    }

}
