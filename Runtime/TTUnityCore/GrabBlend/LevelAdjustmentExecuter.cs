using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;

namespace net.rs64.TexTransUnityCore
{
    public class LevelAdjustmentExecuter : IGrabBlendingExecuter
    {
        public Type ExecutionTarget => typeof(LevelAdjustment);

        void IGrabBlendingExecuter.GrabExecute(TTUnityCoreEngine engin, RenderTexture rt, TTGrabBlending grabBlending)
        {
            var gbUnity = (TTGrabBlendingUnityObject)grabBlending.ComputeKey;
            var cs = gbUnity.Compute;
            var level = (LevelAdjustment)grabBlending;

            using (new UsingColoSpace(rt, gbUnity.IsLinerRequired))
            {
                cs.SetTexture(0, "Tex", rt);
                SetLevelData(cs, level.RGB);
                SetChannel(cs, true, true, true);
                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);

                cs.SetTexture(0, "Tex", rt);
                SetLevelData(cs, level.R);
                SetChannel(cs, true, false, false);
                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);

                cs.SetTexture(0, "Tex", rt);
                SetLevelData(cs, level.G);
                SetChannel(cs, false, true, false);
                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);

                cs.SetTexture(0, "Tex", rt);
                SetLevelData(cs, level.B);
                SetChannel(cs, false, false, true);
                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
            }
        }

        private static void SetLevelData(ComputeShader cs, LevelData ld)
        {
            cs.SetFloat(nameof(LevelData.InputFloor), ld.InputFloor);
            cs.SetFloat(nameof(LevelData.InputCeiling), ld.InputCeiling);
            cs.SetFloat(nameof(LevelData.Gamma), ld.Gamma);
            cs.SetFloat(nameof(LevelData.OutputFloor), ld.OutputFloor);
            cs.SetFloat(nameof(LevelData.OutputCeiling), ld.OutputCeiling);
        }
        private static void SetChannel(ComputeShader cs, bool r, bool g, bool b)
        {
            cs.SetFloat("R", r ? 1 : 0);
            cs.SetFloat("G", g ? 1 : 0);
            cs.SetFloat("B", b ? 1 : 0);
        }
    }
}
