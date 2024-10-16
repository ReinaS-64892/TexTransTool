#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity.MipMap;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using UnityEngine;
using Color = net.rs64.TexTransCore.Color;

namespace net.rs64.TexTransCoreEngineForUnity
{
    internal class TTCoreEngineForUnity : ITexTransCoreEngine, ITexTransToolEngine
    {
        TTCoreEnginTypeUtil.DelegateWithLoadTexture _loadTexture;
        public TTCoreEngineForUnity(TTCoreEnginTypeUtil.DelegateWithLoadTexture loadTexture)
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
            TexTransCoreEngineForUnity.TextureBlend.AlphaCopy(source.ToUnity(), target.ToUnity());
        }
        public void FillAlpha(ITTRenderTexture renderTexture, float alpha)
        {
            TexTransCoreEngineForUnity.TextureBlend.AlphaFill(renderTexture.ToUnity(), alpha);
        }
        public void MulAlpha(ITTRenderTexture renderTexture, float value)
        {
            TexTransCoreEngineForUnity.TextureBlend.ColorMultiply(renderTexture.ToUnity(), new(1, 1, 1, value));
        }

        public void MulAlpha(ITTRenderTexture dist, ITTRenderTexture add)
        {
            if (dist.Width != add.Width || dist.Hight != add.Hight) { throw new ArgumentException("Texture size is not equal!"); }
            TexTransCoreEngineForUnity.TextureBlend.AlphaMultiplyWithTexture(dist.ToUnity(), add.ToUnity());
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


        public void GrabBlending(ITTRenderTexture grabTexture, TTGrabBlending grabCompute)
        {
            TexTransCoreEngineForUnity.GrabBlending.GrabBlendingExecuters[grabCompute.GetType()].GrabExecute(this, grabTexture.ToUnity(), grabCompute);
        }



        public static bool IsLinerRenderTexture = false;//基本的にガンマだ
        public ITTBlendKey QueryBlendKey(string keyName) { return TexTransCoreEngineForUnity.TextureBlend.BlendObjects[keyName]; }
        public ITTComputeKey QueryComputeKey(string ComputeKeyName) { return TexTransCoreEngineForUnity.GrabBlending.GrabBlendObjects[ComputeKeyName]; }


        public bool RenderTextureColorSpaceIsLinear { get => IsLinerRenderTexture; }
    }


    internal class UnityRenderTexture : ITTRenderTexture
    {
        internal RenderTexture RenderTexture;
        public UnityRenderTexture(int width, int height, bool mipMap, bool isDepthAndStencil)
        {
            RenderTexture = TTRt.Get(width, height, isDepthAndStencil, mipMap);
        }
        public bool IsDepthAndStencil => RenderTexture.depth != 0;

        public int Width => RenderTexture.width;

        public int Hight => RenderTexture.height;

        public bool MipMap => RenderTexture.useMipMap;

        public string Name { get => RenderTexture.name; set => RenderTexture.name = value; }

        public void Dispose() { TTRt.Rel(RenderTexture); }
    }

    internal class UnityDiskTexture : ITTDiskTexture
    {
        internal Texture2D Texture;
        public UnityDiskTexture(Texture2D texture)
        {
            Texture = texture;
        }
        public int Width => Texture.width;

        public int Hight => Texture.height;

        public bool MipMap => Texture.mipmapCount > 0;

        public string Name { get => Texture.name; set => Texture.name = value; }

        public void Dispose() { }
    }

    internal static class TTCoreEnginTypeUtil
    {
        public delegate void DelegateWithLoadTexture(ITTDiskTexture diskTexture, ITTRenderTexture writeTarget);

        public static UnityEngine.Color ToUnity(this Color color) { return new(color.R, color.G, color.B, color.A); }
        public static Color ToTTCore(this UnityEngine.Color color) { return new(color.r, color.g, color.b, color.a); }
        public static UnityEngine.Color ToUnity(this ColorWOAlpha color, float alpha = 1f) { return new(color.R, color.G, color.B, alpha); }
        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 vec) { return new(vec.X, vec.Y); }
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 vec) { return new(vec.X, vec.Y, vec.Z); }
        public static UnityEngine.Vector4 ToUnity(this System.Numerics.Vector4 vec) { return new(vec.X, vec.Y, vec.Z, vec.W); }
        public static System.Numerics.Vector4 ToTTCore(this UnityEngine.Vector4 vec) { return new(vec.x, vec.y, vec.z, vec.w); }

        public static RenderTexture ToUnity(this ITTRenderTexture renderTexture) { return ((UnityRenderTexture)renderTexture).RenderTexture; }
        public static Texture2D ToUnity(this ITTDiskTexture diskTexture) { return ((UnityDiskTexture)diskTexture).Texture; }
        public static TTBlendUnityObject ToUnity(this ITTBlendKey key) { return (TTBlendUnityObject)key; }
        public static TTBlendUnityObject ToTTUnityEngin(this string key) { return TextureBlend.BlendObjects[key]; }
        public static float[] ToArray(this System.Numerics.Vector4 vec) { return new[] { vec.X, vec.Y, vec.Z, vec.W }; }
        public static float[] ToArray(this System.Numerics.Vector3 vec) { return new[] { vec.X, vec.Y, vec.Z }; }

    }
}
