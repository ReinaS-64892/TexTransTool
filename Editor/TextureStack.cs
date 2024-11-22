using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class ImmediateStackManager
    {
        Dictionary<Texture2D, ImmediateTextureStack> _textureStacks = new();
        ITextureManager _textureManager;
        ITexTransToolForUnity _ttce4U;
        public ImmediateStackManager(ITexTransToolForUnity ttce4U, ITextureManager textureManager)
        {
            _textureManager = textureManager;
            _ttce4U = ttce4U;
        }
        public void AddTextureStack(Texture2D dist, ITTRenderTexture add, ITTBlendKey blendKey)
        {
            if (dist == null) { return; }
            if (_textureStacks.TryGetValue(dist, out var stack))
            {
                stack.AddStack(add, blendKey);
            }
            else
            {
                var nweStack = _textureStacks[dist] = new(_ttce4U, dist, _textureManager);
                nweStack.AddStack(add, blendKey);
            }
        }
        public List<MergeResult> MergeStacks()
        {
            var mergeTex = new List<MergeResult>(_textureStacks.Count);
            foreach (var stackKV in _textureStacks)
            {
                var fTex = stackKV.Key;
                using var stack = stackKV.Value;
                mergeTex.Add(new MergeResult(fTex, stack.MergeStack()));
            }
            _textureStacks.Clear();
            return mergeTex;
        }

    }
    internal readonly struct MergeResult
    {
        public readonly Texture2D FirstTexture;
        public readonly Texture2D MergeTexture;

        public MergeResult(Texture2D firstTexture, Texture2D mergeTexture)
        {
            FirstTexture = firstTexture;
            MergeTexture = mergeTexture;
        }
    }



    internal class ImmediateTextureStack : IDisposable
    {
        public readonly Texture2D BaseTexture;

        public readonly ITextureManager TextureManager;
        public readonly ITexTransToolForUnity TTCE4Unity;

        ITTRenderTexture _renderTexture;
        public ImmediateTextureStack(ITexTransToolForUnity ttce4u, Texture2D baseTexture, ITextureManager textureManager)
        {
            BaseTexture = baseTexture;
            TextureManager = textureManager;


            _renderTexture = ttce4u.CreateRenderTexture(BaseTexture.width, BaseTexture.height);
            _renderTexture.Name = $"{BaseTexture.name}:ImmediateTextureStack-{BaseTexture.width}x{BaseTexture.height}";

            TTCE4Unity = ttce4u;

            using var ttDiscTex = ttce4u.Wrapping(BaseTexture);
            ttce4u.LoadTextureWidthAnySize(_renderTexture, ttDiscTex);
        }

        public void AddStack(ITTRenderTexture addTex, ITTBlendKey blendKey) { TTCE4Unity.BlendingWithAnySize(_renderTexture, addTex, blendKey); }

        public Texture2D MergeStack()
        {
            var resultTex2D = new Texture2D(BaseTexture.width, BaseTexture.height, TextureFormat.RGBA32, BaseTexture.mipmapCount > 1, true);
            var map = resultTex2D.GetRawTextureData<byte>().AsSpan().Slice(0, BaseTexture.width * BaseTexture.height * 4);

            TTCE4Unity.GammaToLinear(_renderTexture);

            TTCE4Unity.DownloadTexture(map, TexTransCoreTextureFormat.Byte, _renderTexture);

            resultTex2D.CopyFilWrap2D(BaseTexture);
            TextureManager.DeferredInheritTextureCompress(BaseTexture, resultTex2D);

            resultTex2D.name = BaseTexture.name + "_MergedStack";
            resultTex2D.Apply(true);
            return resultTex2D;
        }
        public void Dispose()
        {
            _renderTexture?.Dispose();
            _renderTexture = null;
        }

    }
}
