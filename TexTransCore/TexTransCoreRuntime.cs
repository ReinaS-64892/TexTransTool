using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore.BlendTexture;
using UnityEditor;

namespace net.rs64.TexTransCore
{
    public static class TexTransCoreRuntime
    {
        public static void Initialize()//シェーダー等がロードさている状態を想定している。
        {
            TextureBlend.BlendShadersInit();
            FrameMemoEvents.Init();
            TransTextureCore.Utils.TextureUtility.Init();
            TransTextureCore.TransTexture.Init();
        }
        public static Action Update = () => { NextUpdateCall?.Invoke(); NextUpdateCall = null; };
        public static Action NextUpdateCall;
        public static Func<string, UnityEngine.Object> LoadAsset;

    }
}
