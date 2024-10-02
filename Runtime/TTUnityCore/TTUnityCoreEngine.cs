#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransUnityCore.MipMap;
using net.rs64.TexTransUnityCore.Utils;
using UnityEngine;
using Color = net.rs64.TexTransCore.Color;

namespace net.rs64.TexTransUnityCore
{
    internal class TTUnityCoreEngine : ITTEngine
    {
        TTCoreTypeUtil.DelegateWithLoadTexture _loadTexture;
        public TTUnityCoreEngine(TTCoreTypeUtil.DelegateWithLoadTexture loadTexture)
        {
            _loadTexture = loadTexture;
        }
        public ITTRenderTexture CreateRenderTexture(int width, int height, bool mipMap = false, bool depthAndStencil = false)
        {
            return new UnityRenderTexture(width, height, mipMap, depthAndStencil);
        }

        public void ClearRenderTexture(ITTRenderTexture renderTexture, Color fillColor)
        {
            ((UnityRenderTexture)renderTexture).RenderTexture.ClearWithColor(fillColor.ToUnity());
        }

        public void CopyRenderTexture(ITTRenderTexture source, ITTRenderTexture target)
        {
            if (source.Width != target.Width || source.Hight != target.Hight) { throw new ArgumentException("Texture size is not equal!"); }
            Graphics.CopyTexture(source.ToUnity(), target.ToUnity());
        }


        public void LoadTexture(ITTDiskTexture diskTexture, ITTRenderTexture writeTarget)
        {
            if (diskTexture.Width != writeTarget.Width || diskTexture.Hight != writeTarget.Hight) { throw new ArgumentException("WriteTarget Is not equal"); }
            _loadTexture(diskTexture, writeTarget);
        }



        public void CopyAlpha(ITTRenderTexture source, ITTRenderTexture target)
        {
            if (source.Width != target.Width || source.Hight != target.Hight) { throw new ArgumentException("Texture size is not equal!"); }
            BlendTexture.TextureBlend.AlphaCopy(source.ToUnity(), target.ToUnity());
        }
        public void FillAlpha(ITTRenderTexture renderTexture, float alpha)
        {
            BlendTexture.TextureBlend.AlphaFill(renderTexture.ToUnity(), alpha);
        }
        public void MulAlpha(ITTRenderTexture renderTexture, float value)
        {
            BlendTexture.TextureBlend.MultipleRenderTexture(renderTexture.ToUnity(), new(1, 1, 1, value));
        }

        public void MulAlpha(ITTRenderTexture dist, ITTRenderTexture add)
        {
            if (dist.Width != add.Width || dist.Hight != add.Hight) { throw new ArgumentException("Texture size is not equal!"); }
            BlendTexture.TextureBlend.MaskDrawRenderTexture(dist.ToUnity(), add.ToUnity());
        }



        public void DownScale(ITTRenderTexture source, ITTRenderTexture target, ITTDownScalingKey? downScalingKey = null)
        {
            if (source.Width == target.Width && source.Hight == target.Hight) { throw new ArgumentException("Texture size is equal!"); }
            if (downScalingKey is not null) { throw new NotImplementedException(); }

            Graphics.Blit(source.ToUnity(), target.ToUnity());
        }
        public void UpScale(ITTRenderTexture source, ITTRenderTexture target, ITTUpScalingKey? upScalingKey = null)
        {
            if (source.Width == target.Width && source.Hight == target.Hight) { throw new ArgumentException("Texture size is equal!"); }
            if (upScalingKey is not null) { throw new NotImplementedException(); }

            Graphics.Blit(source.ToUnity(), target.ToUnity());
        }

        public void GenerateMipMap(ITTRenderTexture renderTexture, ITTDownScalingKey? downScalingKey = null)
        {
            if (renderTexture.MipMap is false) { throw new ArgumentException("MipMap is false!"); }
            if (downScalingKey is not null) { throw new NotImplementedException(); }

            MipMapUtility.GenerateMips(renderTexture.ToUnity(), DownScalingAlgorithm.Average);
        }


        public void TextureBlend(ITTRenderTexture dist, ITTRenderTexture add, ITTBlendKey blendKey)
        {
            if (dist.Width != add.Width || dist.Hight != add.Hight) { throw new ArgumentException("Texture size is not equal!"); }
            dist.ToUnity().BlendBlit(add.ToUnity(), blendKey.ToUnity());
        }

    }


    internal class UnityRenderTexture : ITTRenderTexture
    {
        internal RenderTexture RenderTexture;
        public UnityRenderTexture(int width, int height, bool mipMap, bool isDepthAndStencil)
        {
            RenderTexture = TTRt.G(width, height, false, isDepthAndStencil, mipMap, true);
        }
        public bool IsDepthAndStencil => RenderTexture.depth != 0;

        public int Width => RenderTexture.width;

        public int Hight => RenderTexture.height;

        public bool MipMap => RenderTexture.useMipMap;

        public string Name { get => RenderTexture.name; set => RenderTexture.name = value; }

        public void Dispose() { TTRt.R(RenderTexture); }
    }

    internal class UnityDiskTexture : ITTDiskTexture
    {
        public int Width => throw new NotImplementedException();

        public int Hight => throw new NotImplementedException();

        public bool MipMap => throw new NotImplementedException();

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    internal class TTTBlendTypeKey : ITTBlendKey
    {
        public string BlendTypeKey;

        public TTTBlendTypeKey(string blendTypeKey) { BlendTypeKey = blendTypeKey; }
    }

    internal static class TTCoreTypeUtil
    {
        public delegate void DelegateWithLoadTexture(ITTDiskTexture diskTexture, ITTRenderTexture writeTarget);

        public static UnityEngine.Color ToUnity(this Color color) { return new(color.R, color.G, color.B, color.A); }
        public static UnityEngine.Color ToUnity(this ColorWOAlpha color, float alpha = 1f) { return new(color.R, color.G, color.B, alpha); }
        public static UnityEngine.Vector2 ToUnity(this TexTransCore.Vector2 vec) { return new(vec.X, vec.Y); }
        public static UnityEngine.Vector3 ToUnity(this TexTransCore.Vector3 vec) { return new(vec.X, vec.Y, vec.Z); }
        public static UnityEngine.Vector4 ToUnity(this TexTransCore.Vector4 vec) { return new(vec.X, vec.Y, vec.Z, vec.W); }

        public static RenderTexture ToUnity(this ITTRenderTexture renderTexture)
        {
            return ((UnityRenderTexture)renderTexture).RenderTexture;
        }
        public static string ToUnity(this ITTBlendKey key)
        {
            return ((TTTBlendTypeKey)key).BlendTypeKey;
        }
    }
}
