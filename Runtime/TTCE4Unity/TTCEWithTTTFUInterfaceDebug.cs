#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public class TTCEWithTTT4UInterfaceDebug : TTCEInterfaceDebug, ITexTransToolForUnity
    {
        ITexTransToolForUnity _texTransToolForUnity;
        public TTCEWithTTT4UInterfaceDebug(ITexTransToolForUnity texTransToolForUnity, Action<string> debugCallBack) : base(texTransToolForUnity, debugCallBack)
        {
            _texTransToolForUnity = texTransToolForUnity;
        }

        public ITTBlendKey QueryBlendKey(string blendKeyName)
        {
            return _texTransToolForUnity.QueryBlendKey(blendKeyName);
        }
        public ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            return Tracing(_texTransToolForUnity.Wrapping(texture2D));
        }
        public ITTDiskTexture Wrapping(TTTImportedImage imported)
        {
            return Tracing(_texTransToolForUnity.Wrapping(imported));
        }
        public ITTRenderTexture UploadTexture(RenderTexture renderTexture)
        {
            return Tracing(_texTransToolForUnity.UploadTexture(renderTexture));
        }
        public RenderTexture GetReferenceRenderTexture(ITTRenderTexture renderTexture)
        {
            return _texTransToolForUnity.GetReferenceRenderTexture(PassingTracer(renderTexture));
        }

        // できる場合はいい感じに 内部で使用されている物を雑に投げつけて
        public TexTransCoreTextureFormat PrimaryTextureFormat { get => _texTransToolForUnity.PrimaryTextureFormat; }
    }
    public class TTDiskUtilInterfaceDebug : ITexTransUnityDiskUtil
    {
        ITexTransUnityDiskUtil _texTransUnityDiskUtil;
        Action<string> _debugCall;
        public TTDiskUtilInterfaceDebug(ITexTransUnityDiskUtil texTransUnityDiskUtil, Action<string> debugCall)
        {
            _texTransUnityDiskUtil = texTransUnityDiskUtil;
            _debugCall = debugCall;
        }

        public void LoadTexture(ITexTransToolForUnity ttce4u, ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
        {
            using var debugged = new TTCEWithTTT4UInterfaceDebug(ttce4u, _debugCall);
            _texTransUnityDiskUtil.LoadTexture(debugged, writeTarget, diskTexture);
        }

        public ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            return _texTransUnityDiskUtil.Wrapping(texture2D);
        }

        public ITTDiskTexture Wrapping(TTTImportedImage texture2D)
        {
            return _texTransUnityDiskUtil.Wrapping(texture2D);
        }
    }
}
