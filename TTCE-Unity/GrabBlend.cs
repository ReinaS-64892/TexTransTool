using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using UnityEngine;
namespace net.rs64.TexTransCoreEngineForUnity
{
    internal static class GrabBlending
    {
        public static Dictionary<string, TTGrabBlendingComputeShader> GrabBlendObjects;
        [TexTransInitialize]
        public static void GrabBlendingInit()
        {
            GrabBlendObjects = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTGrabBlendingComputeShader)).Cast<TTGrabBlendingComputeShader>().ToDictionary(i => i.name, i => i);
        }

        //HSLAdjustment
        //LevelAdjustment
        //SelectiveColorAdjustment
        //LuminanceMapping
        //Colorize

    }
}
