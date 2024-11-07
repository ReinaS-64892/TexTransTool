#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity.MipMap;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using UnityEngine;
using Color = net.rs64.TexTransCore.Color;

namespace net.rs64.TexTransCoreEngineForUnity
{
    internal class TTCEUnity : ITexTransCoreEngine
    {
        protected TTCoreEnginTypeUtil.DelegateWithLoadTexture _loadTexture;
        protected TTCoreEnginTypeUtil.DelegateWithPreloadAndTextureSizeForTex2D _preloadAndTextureSize;
        public TTCEUnity(TTCoreEnginTypeUtil.DelegateWithLoadTexture loadTexture, TTCoreEnginTypeUtil.DelegateWithPreloadAndTextureSizeForTex2D preloadAndTextureSize)
        {
            _loadTexture = loadTexture;
            _preloadAndTextureSize = preloadAndTextureSize;
        }

        public ITTRenderTexture CreateRenderTexture(int width, int height, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            return new UnityRenderTexture(width, height, channel);
        }

        public void CopyRenderTexture(ITTRenderTexture target, ITTRenderTexture source)
        {
            if (source.Width != target.Width || source.Hight != target.Hight) { throw new ArgumentException("Texture size is not equal!"); }
            Graphics.CopyTexture(source.Unwrap(), target.Unwrap());
        }


        public void LoadTexture(ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
        {
            if (diskTexture.Width != writeTarget.Width || diskTexture.Hight != writeTarget.Hight) { throw new ArgumentException("WriteTarget Is not equal"); }
            _loadTexture(diskTexture, writeTarget);
        }


        public ITTComputeHandler GetComputeHandler(ITTComputeKey computeKey)
        {
            return new TTUnityComputeHandler(((TTComputeUnityObject)computeKey).Compute);
        }

        public static bool IsLinerRenderTexture = false;//基本的にガンマだ


        public bool RenderTextureColorSpaceIsLinear { get => IsLinerRenderTexture; }


        ITexTransStandardComputeKey? _standardComputeKey = null;
        public ITexTransStandardComputeKey StandardComputeKey => _standardComputeKey ??= new TextureBlend.UnityStandardComputeKeyHolder();

        ITexTransComputeKeyDictionary<string>? _grabBlend = null;
        public ITexTransComputeKeyDictionary<string> GrabBlend => _grabBlend ??= new GrabBlendQuery();

        ITexTransComputeKeyDictionary<ITTBlendKey>? _blendKey = null;
        public ITexTransComputeKeyDictionary<ITTBlendKey> BlendKey => _blendKey ??= new BlendKeyUnWrapper();


        class BlendKeyUnWrapper : ITexTransComputeKeyDictionary<ITTBlendKey>
        {
            public ITTComputeKey this[ITTBlendKey key] => key.Unwrap();
        }

        class GrabBlendQuery : ITexTransComputeKeyDictionary<string>
        {
            public ITTComputeKey this[string key] => GrabBlending.GrabBlendObjects[key];
        }

    }


    internal class UnityRenderTexture : ITTRenderTexture
    {
        internal RenderTexture RenderTexture;
        public UnityRenderTexture(int width, int height, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            RenderTexture = TTRt.Get(width, height, channel: channel);
        }
        public bool IsDepthAndStencil => RenderTexture.depth != 0;

        public int Width => RenderTexture.width;

        public int Hight => RenderTexture.height;

        public string Name { get => RenderTexture.name; set => RenderTexture.name = value; }

        public TexTransCoreTextureChannel ContainsChannel => throw new NotImplementedException();

        public void Dispose() { TTRt.Rel(RenderTexture); }
    }

    internal class UnityDiskTexture : ITTDiskTexture
    {
        internal Texture2D Texture;
        internal (int x, int y) LoadableTextureSize;
        public UnityDiskTexture(Texture2D texture, (int x, int y) loadableTextureSize)
        {
            Texture = texture;
            LoadableTextureSize = loadableTextureSize;
        }
        public int Width => LoadableTextureSize.x;

        public int Hight => LoadableTextureSize.y;


        public string Name { get => Texture.name; set => Texture.name = value; }

        public void Dispose() { }
    }

    internal static class TTCoreEnginTypeUtil
    {
        public delegate (int x, int y) DelegateWithPreloadAndTextureSizeForTex2D(Texture2D diskTexture);
        public delegate void DelegateWithLoadTexture(ITTDiskTexture diskTexture, ITTRenderTexture writeTarget);

        public static UnityEngine.Color ToUnity(this Color color) { return new(color.R, color.G, color.B, color.A); }
        public static Color ToTTCore(this UnityEngine.Color color) { return new(color.r, color.g, color.b, color.a); }
        public static UnityEngine.Color ToUnity(this ColorWOAlpha color, float alpha = 1f) { return new(color.R, color.G, color.B, alpha); }
        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 vec) { return new(vec.X, vec.Y); }
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 vec) { return new(vec.X, vec.Y, vec.Z); }
        public static UnityEngine.Vector4 ToUnity(this System.Numerics.Vector4 vec) { return new(vec.X, vec.Y, vec.Z, vec.W); }
        public static System.Numerics.Vector4 ToTTCore(this UnityEngine.Vector4 vec) { return new(vec.x, vec.y, vec.z, vec.w); }

        public static RenderTexture Unwrap(this ITTRenderTexture renderTexture) { return ((UnityRenderTexture)renderTexture).RenderTexture; }
        public static Texture2D Unwrap(this ITTDiskTexture diskTexture) { return ((UnityDiskTexture)diskTexture).Texture; }
        public static TTBlendingComputeShader Unwrap(this ITTBlendKey key) { return (TTBlendingComputeShader)key; }
        public static TTBlendingComputeShader Wrapping(this string key) { return TextureBlend.BlendObjects[key]; }
        public static float[] ToArray(this System.Numerics.Vector4 vec) { return new[] { vec.X, vec.Y, vec.Z, vec.W }; }
        public static float[] ToArray(this System.Numerics.Vector3 vec) { return new[] { vec.X, vec.Y, vec.Z }; }

    }
}
