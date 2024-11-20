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
        public static Dictionary<string, TTGeneralComputeOperator> GeneralComputeObjects;//TODO : 何とかする
        public static Dictionary<string, TTSamplerComputeShader> SamplerComputeShaders;//TODO : 何とかする
        [TexTransInitialize]
        public static void GrabBlendingInit()
        {
            GrabBlendObjects = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTGrabBlendingComputeShader)).Cast<TTGrabBlendingComputeShader>().ToDictionary(i => i.name, i => i);
            GeneralComputeObjects = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTGeneralComputeOperator)).Cast<TTGeneralComputeOperator>().ToDictionary(i => i.name, i => i);
            SamplerComputeShaders = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTSamplerComputeShader)).Cast<TTSamplerComputeShader>().ToDictionary(i => i.name, i => i);
        }

        //HSLAdjustment
        //LevelAdjustment
        //SelectiveColorAdjustment
        //LuminanceMapping
        //Colorize

    }
}
