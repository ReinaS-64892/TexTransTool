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
    internal class TTCEUnityWithTTT4Unity : TTCEUnity, ITexTransToolForUnity
    {
        ITexTransUnityDiskUtil _diskUtil;

        public TexTransCoreTextureFormat PrimaryTextureFormat => TTRt2.GetRGBAFormat();

        public TTCEUnityWithTTT4Unity(ITexTransUnityDiskUtil diskUtil)
        {
            _diskUtil = diskUtil;
        }

        public virtual ITTRenderTexture UploadTexture(RenderTexture renderTexture)
        {
            var rt = CreateRenderTexture(renderTexture.width, renderTexture.height);
            Graphics.CopyTexture(renderTexture, GetReferenceRenderTexture(rt));
            return rt;
        }

        public ITTBlendKey QueryBlendKey(string blendKeyName)
        {
            return ComputeObjectUtility.BlendingObject[blendKeyName];
        }

        public ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            return _diskUtil.Wrapping(texture2D);
        }

        public ITTDiskTexture Wrapping(TTTImportedImage importImage)
        {
            return _diskUtil.Wrapping(importImage);
        }

        public void LoadTexture(ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
        {
            _diskUtil.LoadTexture(this, writeTarget, diskTexture);
        }

        public RenderTexture GetReferenceRenderTexture(ITTRenderTexture renderTexture)
        {
            if (renderTexture is not UnityRenderTexture urt) { throw new InvalidOperationException(); }
            return urt.RenderTexture;
        }
    }
}
