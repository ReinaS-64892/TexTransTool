using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class ColorizeExecuter : IGrabBlendingExecuter
    {
        public Type ExecutionTarget => typeof(Colorize);

        void IGrabBlendingExecuter.GrabExecute(TTCoreEngineForUnity engin, RenderTexture rt, TTGrabBlending grabBlending)
        {
            var gbUnity = (TTGrabBlendingUnityObject)grabBlending.ComputeKey;
            var cs = gbUnity.Compute;
            var colorize = (Colorize)grabBlending;

            using (new UsingColoSpace(rt, gbUnity.IsLinerRequired))
            {
                cs.SetFloats(nameof(Colorize.Color), colorize.Color.ToArray());
                cs.SetTexture(0, "Tex", rt);

                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
            }
        }
    }
}
